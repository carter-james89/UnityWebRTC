// WebRTCStereoVideoDisplay.cs
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;

public class WebRTCStereoVideoDisplay : MonoBehaviour
{
    // Material that uses the stereo-splitting shader
    public Material stereoMaterial;

    // The quad or mesh renderer that displays the stereo video
    public Renderer quadRenderer;

    // The signaling server URL (your Python server's /offer endpoint)
    public string signalingUrl = "http://localhost:8080/offer";

    private RTCPeerConnection pc;
    private bool updateStarted;

    [System.Serializable]
    private class SdpData
    {
        public string sdp;
        public string type;
    }

    IEnumerator Start()
    {
        // Create peer connection
        pc = new RTCPeerConnection();
        pc.OnIceCandidate = c => Debug.Log("ICE candidate: " + c.Candidate);
        pc.OnTrack = OnTrack;

        // Add recv-only video transceiver
        var recvInit = new RTCRtpTransceiverInit { direction = RTCRtpTransceiverDirection.RecvOnly };
        pc.AddTransceiver(TrackKind.Video, recvInit);

        // Create offer
        var offerOp = pc.CreateOffer();
        yield return offerOp;
        var offerDesc = offerOp.Desc;
        yield return pc.SetLocalDescription(ref offerDesc);

        // Send offer to server
        var json = JsonUtility.ToJson(new SdpData { sdp = offerDesc.sdp, type = "offer" });
        using (var req = new UnityWebRequest(signalingUrl, "POST"))
        {
            var body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Signaling failed: " + req.error);
                yield break;
            }

            var answer = JsonUtility.FromJson<SdpData>(req.downloadHandler.text);
            var answerDesc = new RTCSessionDescription
            {
                sdp = answer.sdp,
                type = RTCSdpType.Answer
            };
            yield return pc.SetRemoteDescription(ref answerDesc);
            Debug.Log("WebRTC connected");
        }
    }

    void OnTrack(RTCTrackEvent e)
    {
        if (e.Track is VideoStreamTrack vt)
        {
            vt.OnVideoReceived += tex2D =>
            {
                Debug.Log("Received frame");

                // Assign the texture to the stereo material
                stereoMaterial.mainTexture = tex2D;

                // Apply the material to the quad
                quadRenderer.material = stereoMaterial;

                // Start WebRTC update loop
                if (!updateStarted)
                {
                    StartCoroutine(WebRTC.Update());
                    updateStarted = true;
                }
            };
        }
    }

    void OnDestroy()
    {
        pc.Close();
        pc.Dispose();
    }
}
