// VideoClient.cs
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.WebRTC;

public class VideoClient : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImage;

    [Tooltip("Width x Height in screen pixels")]
    public Vector2 rawImageSize = new Vector2(800, 450);

    [Header("Signaling")]
    public string signalingUrl = "http://192.168.x.y:8080/offer";

    private RTCPeerConnection pc;

    [System.Serializable]
    private class SdpData
    {
        public string sdp;
        public string type;
    }

    IEnumerator Start()
    {
        pc = new RTCPeerConnection();
        pc.OnIceCandidate = c => Debug.Log("ICE: " + c.Candidate);
        pc.OnTrack = OnTrack;
        pc.AddTransceiver(TrackKind.Video);

        // offer
        var op = pc.CreateOffer();
        yield return op;
        var offerDesc = op.Desc;
        yield return pc.SetLocalDescription(ref offerDesc);

        // send to server
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
                Debug.LogError("Signal failed: " + req.error);
                yield break;
            }
            var answer = JsonUtility.FromJson<SdpData>(req.downloadHandler.text);
            var answerDesc = new RTCSessionDescription { sdp = answer.sdp, type = RTCSdpType.Answer };
            yield return pc.SetRemoteDescription(ref answerDesc);
            Debug.Log("WebRTC connected");
        }
    }

    private void OnTrack(RTCTrackEvent e)
    {
        if (e.Track is VideoStreamTrack vt)
        {
            // CPU path: get a Texture2D for every frame
            vt.OnVideoReceived += texture =>
            {
                rawImage.texture = texture;
                rawImage.rectTransform.sizeDelta = rawImageSize;
            };
        }
    }

    void Update()
    {
        WebRTC.Update();
    }

    void OnDestroy()
    {
        pc.Close();
        pc.Dispose();
    }
}
