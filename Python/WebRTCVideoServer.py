# WebRTCVideoServer.py

import asyncio                                         # Python async I/O support
from aiohttp import web                               # HTTP server framework
from aiortc import RTCPeerConnection, RTCSessionDescription  # WebRTC classes
from aiortc.contrib.media import MediaPlayer          # helper to play media files

pcs = set()                                           # keep track of all active PeerConnections

async def offer(request):                             # HTTP POST handler for incoming offers
    params = await request.json()                     # parse JSON body from client
    offer = RTCSessionDescription(                    # build an SDP description object
        sdp=params["sdp"],
        type=params["type"]
    )
    print("[Server] Received offer")                  # log for debugging

    pc = RTCPeerConnection()                          # create a new WebRTC peer connection
    pcs.add(pc)                                       # remember it so we can clean it up later

    # load and loop a local video file as the outgoing media
    player = MediaPlayer("Stereo Video.mp4", loop=True)
    if player.video:                                  # if the file has a video track
        pc.addTrack(player.video)                     # attach the track to our PeerConnection
        print("[Server] Video track added")
    else:
        print("[Server] ERROR: no video track")       # error if no video in file

    # apply the remote description we received from the client
    await pc.setRemoteDescription(offer)
    print("[Server] Remote description set")

    # create an SDP answer based on the remote offer
    answer = await pc.createAnswer()
    # apply our local description (the answer) to the connection
    await pc.setLocalDescription(answer)
    print("[Server] Local description set")

    # respond with our SDP answer in JSON
    return web.json_response({
        "sdp":  pc.localDescription.sdp,
        "type": pc.localDescription.type
    })

async def on_shutdown(app):                           # cleanup hook for server shutdown
    # close all PeerConnections concurrently
    await asyncio.gather(*[pc.close() for pc in pcs])
    pcs.clear()                                       # clear the set

app = web.Application()                               # create aiohttp web app
app.router.add_post("/offer", offer)                  # route POST /offer to our handler
app.on_shutdown.append(on_shutdown)                   # register cleanup callback

print("Server running at http://0.0.0.0:8080")         # startup log
web.run_app(app, port=8080)                           # start the event loop & HTTP server
