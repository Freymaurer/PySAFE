from pickle import TRUE
from fastapi import FastAPI, WebSocket
from datetime import datetime
import asyncio
# from typing import Union
# from pydantic import BaseModel

app = FastAPI(debug=True)

@app.get("/")
async def root():
    return {"message": "Hello World"}

# connected_websockets = set()
# timer_task = None

async def send_counter(websocket):
    counter = 0;  # Declare counter here
    try:
        while counter <= 10:
            await websocket.send_text(str(counter))  # Send the counter value
            print("[WS] Sent:", counter)
            counter += 1  # Increment counter
            await asyncio.sleep(1)  # Wait for 1 second
        await websocket.send_text("EXIT")  
        await websocket.close()
    except Exception as e:
        print("[WS] Error:", e)


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    global timer_task  # Declare timer_task as global here
    print("[WS] Connected")
    await websocket.accept()
    data = await websocket.receive()
    print("[WS]", data)
    await send_counter(websocket)  # Start sending counter asynchronously

