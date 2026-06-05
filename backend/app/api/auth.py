# ~/backend/app/api/auth.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.core.database import get_db
from app.core.security import hash_password, verify_password, create_access_token, create_refresh_token, decode_token
from app.models.user import User
from app.schemas.auth import RegisterRequest, LoginRequest, TokenResponse, RefreshRequest, MeResponse
from app.api.deps import get_current_user
from jose import JWTError
import pyotp

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

    return {
        "message": "Account successfully created",
        "user_id": str(user.id)
    }

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