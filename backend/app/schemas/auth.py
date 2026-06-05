from pydantic import BaseModel, EmailStr, field_validator
from uuid import UUID
import re

class RegisterRequest(BaseModel):
    username: str
    email: EmailStr
    password: str
    phone: str | None = None

    @field_validator("username")
    @classmethod
    def username_valid(cls, v):
        if not re.match(r"[a-zA-Z0-9_-.]{3,32}$", v):
            raise ValueError("Username must be 3 - 32 chars and may only include letters, numbers, underscores (_), dashes (-) and fullstops (.)!")
        return v
    
    @field_validator("password")
    @classmethod
    def password_strong(cls, v):
        if (len(v) < 8):
            raise ValueError("Password must be atleast 8 characters!")
        return v
    
class LoginRequest(BaseModel):
    identifier: str # Username or mail
    password: str
    mfa_code: str | None = None

class TokenResponse(BaseModel):
    access_token: str
    refresh_token: str
    token_type: str = "bearer"

class RefreshRequest(BaseModel):
    refresh_token: str

class MeResponse(BaseModel):
    id: UUID
    username: str
    email: str
    phone: str | None
    mfa_enabled: bool
    is_verified: bool

    model_config = {"from_attributes": True}

class MFASetupResponse(BaseModel):
    secret: str
    qr_uri: str

class MFAVerifyRequest(BaseModel):
    code: str

class VerifyEmailRequest(BaseModel):
    token: str

class ForgotPasswordRequest(BaseModel):
    email: str

class ResetPasswordRequest(BaseModel):
    token: str
    new_password: str

    @field_validator("new_password")
    @classmethod
    def password_strong(cls, v):
        if len(v) < 8: 
            raise ValueError("New password must be atleast 8 characters.")
        return v
    
class ChangePasswordRequest(BaseModel):
    current_password: str
    new_password: str

class UserSearchResult(BaseModel):
    id: UUID
    username: str
    display_name: str | None = None
    is_verified: bool

    model_config = {"from_attributes": True}