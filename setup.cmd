CALL dotnet tool restore

CALL python -m venv .venv

CALL .\.venv\Scripts\python.exe -m pip install -r .\src\FastAPI\requirements.txt