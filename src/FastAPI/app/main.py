from fastapi import FastAPI, WebSocket
import asyncio
import json
from dataclasses import dataclass
from typing import List
from enum import Enum

app = FastAPI(debug=True)

@app.get("/")
async def root():
    return {"message": "Hello World"}

class APIMessages(Enum):
    Error = 'Error'
    Exit = 'Exit'
    DataResponse = 'DataResponse'

@dataclass
class DataInputItem:
    Number: int

@dataclass
class DataInputConfig:
    SomeConfig: bool

@dataclass
class DataInput:
    Items: List[DataInputItem]
    Config: DataInputConfig
    

@dataclass
class ResultType:
    Results: List[int]
    Batch: int

@dataclass
class Message:
    API: str
    Data: object

# Sample function to process a single test
def process_test(test: DataInputItem) -> int:
    return test.Number * 2

# Function to parse JSON data into DataInput object
def parse_json(json_data: str) -> DataInput:
    data_dict = json.loads(json_data)
    items = [DataInputItem(**item) for item in data_dict["Items"]]
    config = DataInputConfig(**data_dict["Config"])
    return DataInput(Items=items, Config=config)

exitMsg = Message(APIMessages.Exit.value, None)

def errorMsg(error: str) -> Message:
    return Message(APIMessages.Error.value, error)

async def run_ml(websocket: WebSocket, data_input: DataInput):
    batch_size = 5
    for i in range(0, len(data_input.Items), batch_size):
        batch = data_input.Items[i:i+batch_size]
        print("[WS] Processing: batch -", i)
        processed_results = [process_test(test) for test in batch]  # Assuming process_test is the function to run on each test
        msg = Message(APIMessages.DataResponse.value, ResultType(processed_results, i).__dict__)
        await websocket.send_json(msg.__dict__)
        print("[WS] Sent: batch - ", i)
        await asyncio.sleep(1)  # Wait for 1 second before sending the next batch
    await websocket.send_json(exitMsg.__dict__)
    await websocket.close()


@app.websocket("/dataml")
async def websocket_endpoint(websocket: WebSocket):
    print("[WS] Connected")
    await websocket.accept()
    try:
        data = await websocket.receive_text()
        data_input = parse_json(data)
        await run_ml(websocket, data_input)
    except Exception as e:
        msg = errorMsg(str(e))
        print ("[WS] Error:", msg)
        await websocket.send_json(msg.__dict__)
        await websocket.close(code=1011)

