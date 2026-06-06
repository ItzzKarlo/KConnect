# !/backend/app/schemas/chat.py
from pydantic import BaseModel
from uuid import UUID
from datetime import datetime

class ConversationCreate(BaseModel):
    # Receiver
    user_id: UUID 

class MessageOut(BaseModel):
    id: UUID
    conversation_id: UUID
    sender_id: UUID
    sender_username: str
    content: str
    created_at: datetime

    model_config = {"from_attributes": True}

class ConversationOut(BaseModel):
    id: UUID
    created_at: datetime
    member_usernames: list[str]
    last_message: MessageOut | None = None

    model_config = {"from_attributes": True}

# WebSocket Message Envelope
class WsMessageIn(BaseModel):
    conversation_id: str
    content: str

class WsMessageOut(BaseModel):
    type: str = "message"
    id: str
    conversation_id: str
    sender_id: str
    sender_username: str
    content: str
    created_at: str

