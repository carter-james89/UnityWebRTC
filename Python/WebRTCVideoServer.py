# WebRTCVideoServer.py
import asyncio
from aiohttp import web
from aiortc import RTCPeerConnection, RTCSessionDescription
from aiortc.contrib.media import MediaPlayer

pcs = set()

async def offer(request):
    params = await request.json()
    offer = RTCSessionDescription(sdp=params["sdp"], type=params["type"])
    print("[Server] Received offer")

    pc = RTCPeerConnection()
    pcs.add(pc)

    # load and loop your file
    player = MediaPlayer("Stereo Video.mp4", loop=True)
    if player.video:
        pc.addTrack(player.video)
        print("[Server] Video track added")
    else:
        print("[Server] ERROR: no video track")

    # apply the client's SDP
    await pc.setRemoteDescription(offer)
    print("[Server] Remote description set")

    # build our answer
    answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)
    print("[Server] Local description set")

    return web.json_response({
        "sdp":  pc.localDescription.sdp,
        "type": pc.localDescription.type
    })

async def on_shutdown(app):
    await asyncio.gather(*[pc.close() for pc in pcs])
    pcs.clear()

app = web.Application()
app.router.add_post("/offer", offer)
app.on_shutdown.append(on_shutdown)

print("Server running at http://0.0.0.0:8080")
web.run_app(app, port=8080)
