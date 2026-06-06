from sqlalchemy import Column, String, DateTime, ForeignKey, func, Boolean
from sqlalchemy.dialects.postgresql import UUID
import uuid
from sqlalchemy.orm import relationship
from app.core.database import Base

class Message(Base):
    __tablename__ = "messages"

    id              = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    conversation_id = Column(UUID(as_uuid=True), ForeignKey("conversations.id"), nullable=False)
    sender_id       = Column(UUID(as_uuid=True), ForeignKey("users.id"),         nullable=False)
    content         = Column(String(4000), nullable=False)
    created_at      = Column(DateTime(timezone=True), server_default=func.now())
    is_deleted      = Column(Boolean, default=False)

    conversation = relationship("Conversation", back_populates="messages")
    sender       = relationship("User")