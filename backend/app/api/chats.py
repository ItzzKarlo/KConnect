# !/backend/app/api/chats.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from uuid import UUID
from app.core.database import get_db
from app.api.deps import get_current_user
from app.models.user import User
from app.models.conversation import Conversation
from app.models.message import Message
from app.schemas.chat import ConversationCreate, ConversationOut, MessageOut

router = APIRouter(prefix="/chats", tags=["chats"])

@router.post("", response_model=ConversationOut, status_code=201)
def create_or_get_conversation(
    payload: ConversationCreate,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_user),
):
    other = db.query(User).filter(User.id == payload.user_id, User.is_active == True).first()
    if not other:
        raise HTTPException(404, "User not found")
    if other.id == current_user.id:
        raise HTTPException(400, "Cannot start a conversation with yourself")

    # Check if a DM between these two already exists
    existing = (
        db.query(Conversation)
        .filter(Conversation.members.any(User.id == current_user.id))
        .filter(Conversation.members.any(User.id == other.id))
        .first()
    )
    if existing:
        return _serialize(existing, current_user)

    conv = Conversation(members=[current_user, other])
    db.add(conv)
    db.commit()
    db.refresh(conv)
    return _serialize(conv, current_user)

@router.get("", response_model=list[ConversationOut])
def list_conversations(
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_user),
):
    convs = (
        db.query(Conversation)
        .filter(Conversation.members.any(User.id == current_user.id))
        .order_by(Conversation.created_at.desc())
        .all()
    )
    return [_serialize(c, current_user) for c in convs]

@router.get("/{conv_id}/messages", response_model=list[MessageOut])
def get_messages(
    conv_id: UUID,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_user),
):
    conv = db.query(Conversation).filter(Conversation.id == conv_id).first()
    if not conv:
        raise HTTPException(404, "Conversation not found")
    if current_user not in conv.members:
        raise HTTPException(403, "Not a member of this conversation")

    messages = (
        db.query(Message)
        .filter(Message.conversation_id == conv_id, Message.is_deleted == False)
        .order_by(Message.created_at.asc())
        .limit(100)
        .all()
    )
    return [_msg_out(m) for m in messages]

def _serialize(conv: Conversation, current_user: User) -> dict:
    last = conv.messages[-1] if conv.messages else None
    return {
        "id": conv.id,
        "created_at": conv.created_at,
        "member_usernames": [m.username for m in conv.members if m.id != current_user.id],
        "last_message": _msg_out(last) if last else None,
    }

def _msg_out(m: Message) -> dict:
    return {
        "id": m.id,
        "conversation_id": m.conversation_id,
        "sender_id": m.sender_id,
        "sender_username": m.sender.username,
        "content": m.content,
        "created_at": m.created_at,
    }