from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api import auth, mfa, users, chats, ws

app = FastAPI(title="KConnect API", version="0.1.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173", "https://localhost:5173"], # Vue dev server
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"]
)

app.include_router(auth.router)
app.include_router(mfa.router)
app.include_router(users.router)
app.include_router(chats.router)
app.include_router(ws.router)

@app.get("/health")
def health():
    return {"status": "HTTP 200 OK", "app": "KConnect"}