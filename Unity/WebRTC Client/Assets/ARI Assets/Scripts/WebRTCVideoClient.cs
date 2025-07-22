// Required namespaces for Unity components, networking, and WebRTC
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Unity.WebRTC;

public class WebRTCVideoClient : MonoBehaviour
{
    // References to the two RawImage UI components to show split video
    public RawImage leftImage;
    public RawImage rightImage;
    // Add this line near the other RawImages:
    public RawImage fullImage;


    // Size of the video area (800x450 screen pixels by default)
    public Vector2 rawImageSize = new Vector2(800, 450);

    // The signaling server URL (your Python server's /offer endpoint)
    public string signalingUrl = "http://localhost:8080/offer";

    // Peer connection for WebRTC communication
    private RTCPeerConnection pc;

    // Used to ensure WebRTC.Update() coroutine only starts once
    private bool updateStarted;

    // A simple class to hold the SDP data for serialization
    [System.Serializable]
    private class SdpData
    {
        public string sdp; // Session Description Protocol string
        public string type; // Type: "offer" or "answer"
    }

    // Unity coroutine that starts the WebRTC connection
    IEnumerator Start()
    {
        // Create the peer connection
        pc = new RTCPeerConnection();

        // Register a callback for ICE candidates (for NAT traversal)
        pc.OnIceCandidate = c => Debug.Log("ICE candidate: " + c.Candidate);

        // Register a callback when remote media tracks arrive
        pc.OnTrack = OnTrack;

        // Add a transceiver to receive only video before starting negotiation
        var recvInit = new RTCRtpTransceiverInit { direction = RTCRtpTransceiverDirection.RecvOnly };
        pc.AddTransceiver(TrackKind.Video, recvInit);

        // Create an offer from this client
        var offerOp = pc.CreateOffer();
        yield return offerOp;

        // Store the offer description
        var offerDesc = offerOp.Desc;

        // Set this offer as the local description
        yield return pc.SetLocalDescription(ref offerDesc);

        // Convert the offer to JSON and send to signaling server
        var json = JsonUtility.ToJson(new SdpData { sdp = offerDesc.sdp, type = "offer" });

        // Build the HTTP POST request to send the offer to Python server
        using (var req = new UnityWebRequest(signalingUrl, "POST"))
        {
            var body = Encoding.UTF8.GetBytes(json); // Encode payload as UTF-8
            req.uploadHandler = new UploadHandlerRaw(body); // Set upload handler
            req.downloadHandler = new DownloadHandlerBuffer(); // Set download handler
            req.SetRequestHeader("Content-Type", "application/json"); // Add header

            yield return req.SendWebRequest(); // Send the HTTP request

            // If the request failed, stop and log error
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Signaling failed: " + req.error);
                yield break;
            }

            // Parse the SDP answer from the server
            var answer = JsonUtility.FromJson<SdpData>(req.downloadHandler.text);

            // Build RTCSessionDescription from answer
            var answerDesc = new RTCSessionDescription
            {
                sdp = answer.sdp,
                type = RTCSdpType.Answer
            };

            // Set the remote description to complete handshake
            yield return pc.SetRemoteDescription(ref answerDesc);
            Debug.Log("WebRTC connected");
        }
    }

    // Callback triggered when a remote track is received
    void OnTrack(RTCTrackEvent e)
    {
        // Ensure the track is a video stream
        if (e.Track is VideoStreamTrack vt)
        {
            // Subscribe to the video frame callback
            vt.OnVideoReceived += tex2D =>
            {
                Debug.Log("Received frame");

                // Set both RawImages to use the same texture
                leftImage.texture = tex2D;
                rightImage.texture = tex2D;
                fullImage.texture = tex2D;

                // Resize each image to be half the full width
                leftImage.rectTransform.sizeDelta = rawImageSize / 2f;
                rightImage.rectTransform.sizeDelta = rawImageSize / 2f;
                fullImage.rectTransform.sizeDelta = rawImageSize;

                // Move the right image to the right of the left image
                //rightImage.rectTransform.anchoredPosition =
                //    leftImage.rectTransform.anchoredPosition + new Vector2(rawImageSize.x / 2f, 0);

                // Display only the left half of the texture
                leftImage.uvRect = new Rect(0f, 0f, 0.5f, 1f);

                // Display only the right half of the texture
                rightImage.uvRect = new Rect(0.5f, 0f, 0.5f, 1f);

                fullImage.uvRect = new Rect(0f, 0f, 1f, 1f);

                // Start the WebRTC update loop if not already started
                if (!updateStarted)
                {
                    StartCoroutine(WebRTC.Update());
                    updateStarted = true;
                }
            };
        }
    }

    // Cleanup when the MonoBehaviour is destroyed
    void OnDestroy()
    {
        pc.Close();    // Close the peer connection
        pc.Dispose();  // Release memory/resources
    }
}
