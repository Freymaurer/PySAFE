from fastapi import FastAPI, WebSocket, WebSocketDisconnect
import asyncio
import json
from enum import Enum
from pydantic import BaseModel
import uvicorn
# from prediction import Loc_classifier, prediction

app = FastAPI(debug=True)

class Message(BaseModel):
    API: str
    Data: object

class APIMessages(Enum):
    Error = 'Error'
    Exit = 'Exit'
    DataResponse = 'DataResponse'

exitMsg = Message(API=APIMessages.Exit.value, Data=None)

def errorMsg(error: Exception) -> Message:
    return Message(API=APIMessages.Error.value, Data=repr(error))

@app.get("/")
async def root():
    return {"message": "Hello World"}

# Input data class
# this is what would need to be send if we go for one sequence at a time
class DataInputItem(BaseModel):
    Header      : str
    Sequence    : str

class DataResponseItem(BaseModel):
    Header      : str
    Predictions : list[float]


# this would be the input if we go for multiple sequences at a time
class RequestType(BaseModel):
    Fasta : list[DataInputItem]

class MyResponseType(BaseModel):
    Results: list[DataResponseItem]
    Batch: int


# Input data class
class DataInputItem(BaseModel):
    Header      : str
    Sequence    : str

class DataResponseItem(BaseModel):
    Header      : str
    Predictions : list[float]

# Sample function to process a single test
def process_data(input_data: DataInputItem) -> DataResponseItem:
    return DataResponseItem(Header=input_data.Header, Predictions=[len(input_data.Sequence)])

async def run_ml(websocket: WebSocket, data_input: list[DataInputItem]):
    batch_size = 400
    for i in range(0, len(data_input), batch_size):
        batch = data_input[i:i+batch_size]
        results = [process_data(chunk) for chunk in batch]  # Assuming process_test is the function to run on each test
        response_data = dict(Results=[result.model_dump() for result in results], Batch=i)
        response = MyResponseType(**response_data)
        msg = Message(API=APIMessages.DataResponse.value, Data=response)
        print("[WS] Sending batch --", i, "-- ..")
        await websocket.send_json(msg.model_dump())
    print("[WS] Done. Closing..")
    await websocket.send_json(exitMsg.model_dump())
    print("[WS] Closing message sent")

@app.websocket("/dataml")
async def websocket_endpoint(websocket: WebSocket):
    print("[WS] Connected")
    await websocket.accept()
    try:
        # while True:
        print ("[WS] Receiving data ..")
        data_bytes = await websocket.receive_bytes()
        print ("[WS] bytes received:", len(data_bytes))
        data_json = data_bytes.decode('utf-8')
        print ("[WS] json received ..", data_json[:100], "..")
        data_map = json.loads(data_json)
        data_input = RequestType(**data_map)
        print ("[WS] Starting ml ..")
        await run_ml(websocket, data_input.Fasta)
    except WebSocketDisconnect:
        #on_disconnect
        print("[WS] Disconnected")
        pass
    except Exception as e:
        msg = errorMsg(e)
        print ("[WS] Error:", e)
        await websocket.send_json(msg.model_dump())
        await websocket.close(code=1011)

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info", ws_max_size=None)