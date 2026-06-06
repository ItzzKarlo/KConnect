from sqlalchemy import Column, DateTime, ForeignKey, func, Table
from sqlalchemy.dialects.postgresql import UUID
import uuid
from sqlalchemy.orm import relationship
from app.core.database import Base

# Many-to-many: user <-> conversaion
conversation_members = Table(
    "conversation_members",
    Base.metadata,
    Column("conversation_id", UUID(as_uuid=True), ForeignKey("conversations.id"), primary_key=True),
    Column("user_id",         UUID(as_uuid=True), ForeignKey("users.id"),         primary_key=True),
)

class Conversation(Base):
    __tablename__ = "conversations"

    id         = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    members  = relationship("User",    secondary=conversation_members, backref="conversations")
    messages = relationship("Message", back_populates="conversation",  order_by="Message.created_at")
