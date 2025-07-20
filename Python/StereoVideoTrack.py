import asyncio
import logging
from aiohttp import web
from aiortc import RTCPeerConnection, RTCSessionDescription
from aiortc.contrib.media import MediaPlayer

pcs = set()

# Middleware for CORS
@web.middleware
async def cors_middleware(request, handler):
    if request.method == "OPTIONS":
        return web.Response(status=200, headers={
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "POST, GET, OPTIONS",
            "Access-Control-Allow-Headers": "*"
        })
    response = await handler(request)
    response.headers["Access-Control-Allow-Origin"] = "*"
    return response

# Serve video track
class StereoVideoTrack:
    def __init__(self):
        self.player = MediaPlayer("Stereo Video.mp4")

    def get_video_track(self):
        return self.player.video

async def offer(request):
    params = await request.json()
    offer = RTCSessionDescription(sdp=params["sdp"], type=params["type"])

    pc = RTCPeerConnection()
    pcs.add(pc)
    print("[Server] Received offer")

    video = StereoVideoTrack().get_video_track()
    pc.addTrack(video)

    await pc.setRemoteDescription(offer)
    print("[Server] Set remote description")

    answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)
    print("[Server] Created and set local description")

    return web.json_response(
        {
            "sdp": pc.localDescription.sdp,
            "type": pc.localDescription.type
        }
    )

async def on_shutdown(app):
    print("[Server] Shutting down connections...")
    coros = [pc.close() for pc in pcs]
    await asyncio.gather(*coros)
    pcs.clear()

app = web.Application(middlewares=[cors_middleware])
app.router.add_post("/offer", offer)
app.on_shutdown.append(on_shutdown)

print("Starting WebRTC server on http://localhost:8080")
web.run_app(app, host="localhost", port=8080)
