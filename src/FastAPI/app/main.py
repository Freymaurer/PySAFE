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

connected_websockets = set()
timer_task = None

async def send_timer_updates(websocket: WebSocket):
    try:
        while True:
            current_time = datetime.now().strftime("%H:%M:%S")
            await websocket.send_text(f"Current time: {current_time}")
            await asyncio.sleep(1)  # Sends updates every second
    except asyncio.CancelledError:
        pass


@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    global timer_task  # Declare timer_task as global here
    await websocket.accept()
    connected_websockets.add(websocket)
    try:
        while True:
            data = await websocket.receive_text()
            print(data)
            if data.lower() == "start":
                if timer_task is None:
                    timer_task = asyncio.create_task(send_timer_updates(websocket))
            elif data.lower() == "stop":
                if timer_task:
                    timer_task.cancel()
                    timer_task = None
    finally:
        connected_websockets.remove(websocket)
