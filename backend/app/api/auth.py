# ~/backend/app/api/auth.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.core.database import get_db
from app.core.security import hash_password, verify_password, create_access_token, create_refresh_token, decode_token
from app.core.email import send_verification_email, send_password_reset_email
from app.core.tokens import generate_email_token, verify_email_token
from app.models.user import User
from app.schemas.auth import (
    RegisterRequest, LoginRequest, TokenResponse, RefreshRequest, MeResponse,
    VerifyEmailRequest, ForgotPasswordRequest, ResetPasswordRequest, ChangePasswordRequest
)
from app.api.deps import get_current_user
import pyotp
from jose import JWTError

router = APIRouter(prefix="/auth", tags=["auth"])

@router.post("/register", status_code=201)
def register(payload: RegisterRequest, db: Session = Depends(get_db)):
    if db.query(User).filter(User.email == payload.email).first():
        raise HTTPException(400, "Email is already registered!")
    if db.query(User).filter(User.username == payload.username).first():
        raise HTTPException(400, "Username is already taken!")
    
    user = User(
        username = payload.username,
        email = payload.email,
        phone = payload.phone,
        password_hash = hash_password(payload.password)
    )

    db.add(user)
    db.commit()
    db.refresh(user)

    token = generate_email_token(user.email)
    send_verification_email(user.email, user.username, token)

    return {
        "message": "Account successfully created. Please check your email to verify your account.",
        "user_id": str(user.id)
    }

@router.post("/verify-email")
def verify_email(payload: VerifyEmailRequest, db: Session = Depends(get_db)):
    email = verify_email_token(payload.token)
    if not email:
        raise HTTPException(400, "Verification link is invalid or has expired.")
    
    user = db.query(User).filter(User.email == email).first()
    if not user:
        raise HTTPException(404, "User not found")
    if user.is_verified:
        return {"message": "Email is already verified"}
    
    user.is_verified = True
    db.commit()
    return {"message": "Email verified successfully."}

@router.post("/login", response_model=TokenResponse)
def login(payload: LoginRequest, db: Session = Depends(get_db)):
    # Find email or username
    user = (
        db.query(User).filter(User.email == payload.identifier).first() or
        db.query(User).filter(User.username == payload.identifier).first()
    )

    if not user or not verify_password(payload.password, user.password_hash):
        raise HTTPException(401, "Invalid credentials")
    if not user.is_active:
        raise HTTPException(403, "Account is disabled.")
    if not user.is_verified:
        raise HTTPException(403, "Please verify your email before logging in.")
    
    # MFA Check
    if user.mfa_enabled:
        if not payload.mfa_code:
            raise HTTPException(403, "MFA code requred.")
        totp = pyotp.TOTP(user.mfa_secret)
        if not totp.verify(payload.mfa_code, valid_window=1):
            raise HTTPException(401, "Invalid MFA code")
    
    # Set token data
    token_data = {"sub": str(user.id)}
    return TokenResponse(
        access_token=create_access_token(token_data),
        refresh_token=create_refresh_token(token_data)
    )

@router.post("/resend-verification")
def resend_verification(payload: ForgotPasswordRequest, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.email == payload.email).first()

    if user and not user.is_verified:
        token = generate_email_token(user.email)
        send_verification_email(user.email, user.username, token)
    return {"message": "If that email exists and is unverified, a new link has been sent for email verification."}

@router.post("/forgot-password")
def forgot_password(payload: ForgotPasswordRequest, db: Session = Depends(get_db)):
    user = db.query(User).filter(User.email == payload.email).first()
    if user and user.is_active:
        token = generate_email_token(user.email, salt="password-reset")
        send_password_reset_email(user.email, user.username, token)
    return {
        "message": "If that email is registered, a reset-password link has been sent."
    }

@router.post("/reset-password")
def reset_password(payload: ResetPasswordRequest, db: Session = Depends(get_db)):
    email = verify_email_token(payload.token, salt="password-reset", max_age=3600)
    if not email:
        raise HTTPException(400, "Reset link is invalid or has expired.")
    
    user = db.query(User).filter(User.email == email).first()
    if not user:
        raise HTTPException(404, "User not found.")
    
    user.password_hash = hash_password(payload.new_password)
    db.commit()
    return {"message": "Password reset successfully."}

@router.post("/change-password")
def change_password(
    payload: ChangePasswordRequest,
    current_user: User = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    if not verify_password(payload.current_password, current_user.password_hash):
        raise HTTPException(401, "Current password is incorrect.")
    
    current_user.password_hash = hash_password(payload.new_password)
    db.commit()
    return {"message": "Password changed successfully"}

@router.post("/refresh", response_model=TokenResponse)
def refresh(payload: RefreshRequest, db: Session = Depends(get_db)):
    try: 
        data = decode_token(payload.refresh_token)
        if data.get("type") != "refresh":
            raise HTTPException(401, "Invalid token type")
    except JWTError:
        raise HTTPException(401, "Invalid or expired refresh token.")
    
    user = db.query(User).filter(User.id == data["sub"]).first()
    if not user or not user.is_active:
        raise HTTPException(401, "User not found")
    
    token_data = {"sub": str(user.id)}
    return TokenResponse(
        access_token=create_access_token(token_data),
        refresh_token=create_refresh_token(token_data)
    )

@router.get("/me", response_model=MeResponse)
def me(current_user: User = Depends(get_current_user)):
    return current_user

@router.post("/logout")
def logout():
    return {"message": "Successfully logged out"}