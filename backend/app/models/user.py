# ~/backend/app/models/user.py
from sqlalchemy import Column, String, Boolean, DateTime, func
from sqlalchemy.dialects.postgresql import UUID
import uuid
from app.core.database import Base

class User(Base):
    __tablename__ = "users"

    # General User Info
    id            = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    username      = Column(String(32), unique=True, nullable=False, index=True)
    email         = Column(String(255), unique=True, nullable=False, index=True)
    phone         = Column(String(20), unique=True, nullable=True)
    password_hash = Column(String, nullable=False)

    # MFA
    mfa_enabled   = Column(Boolean, default=False)
    mfa_secret    = Column(String, nullable=True)

    # Reset Pwd Token
    password_reset_token = Column(String, nullable=True)
    
    # Status
    is_active     = Column(Boolean, default=True)
    is_verified   = Column(Boolean, default=False)

    # Administrative Fields
    created_at    = Column(DateTime(timezone=True), server_default=func.now())
    updated_at    = Column(DateTime(timezone=True), onupdate=func.now())