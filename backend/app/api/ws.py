# !/backend/app/api/ws.py 
# WebSocket API endpoint
from fastapi import APIRouter, WebSocket, WebSocketDisconnect, Query, Depends
from datetime import datetime, timezone
from sqlalchemy.orm import Session
from jose import JWTError
from app.core.database import get_db, SessionLocal
from app.core.security import decode_token
from app.models.user import User
from app.models.conversation import Conversation
from app.models.message import Message
import json
import uuid

router = APIRouter(tags=["websocket"])

# In-memory connection store: 
# {
#   "user_id": WebSocket
# }
active: dict[str, WebSocket] = {}

@router.websocket("/ws")
async def websocket_endpoint(
    ws: WebSocket,
    token: str = Query(...),
):
    # Authenticate via token query param
    try:
        payload = decode_token(token)
        user_id = payload.get("sub")
        if not user_id:
            await ws.close(code=4001)
            return
    except JWTError:
        await ws.close(code=4001)
        return

    await ws.accept()
    active[user_id] = ws

    db: Session = SessionLocal()
    try:
        while True:
            raw = await ws.receive_text()
            try:
                data = json.loads(raw)
                conv_id = data.get("conversation_id")
                content = data.get("content", "").strip()
                if not conv_id or not content:
                    continue

                # Validate membership
                conv = db.query(Conversation).filter(
                    Conversation.id == conv_id
                ).first()
                if not conv:
                    continue

                member_ids = [str(m.id) for m in conv.members]
                if user_id not in member_ids:
                    continue

                # Persist message
                sender = db.query(User).filter(User.id == user_id).first()
                msg = Message(
                    conversation_id=conv_id,
                    sender_id=user_id,
                    content=content,
                )
                db.add(msg)
                db.commit()
                db.refresh(msg)

                # Fan out to all members who are online
                envelope = json.dumps({
                    "type": "message",
                    "id": str(msg.id),
                    "conversation_id": str(msg.conversation_id),
                    "sender_id": str(msg.sender_id),
                    "sender_username": sender.username,
                    "content": msg.content,
                    "created_at": msg.created_at.isoformat(),
                })
                for mid in member_ids:
                    peer = active.get(mid)
                    if peer:
                        try:
                            await peer.send_text(envelope)
                        except Exception:
                            active.pop(mid, None)

            except (json.JSONDecodeError, Exception):
                continue

    except WebSocketDisconnect:
        active.pop(user_id, None)
    finally:
        db.close()