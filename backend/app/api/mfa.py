# ~/backend/app/api/mfa.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
import pyotp
from app.core.database import get_db
from app.api.deps import get_current_user
from app.models.user import User
from app.schemas.auth import MFASetupResponse, MFAVerifyRequest

router = APIRouter(prefix="/auth/mfa", tags=["mfa"])

@router.post("/enable", response_model=MFASetupResponse)
def enable_mfa(current_user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if current_user.mfa_enabled:
        raise HTTPException(400, "MFA is already enabled for this account.")
    
    secret = pyotp.random_base32()
    totp = pyotp.TOTP(secret)
    uri = totp.provisioning_uri(name=current_user.email, issuer="KConnect")

    current_user.mfa_secret = secret
    db.commit()

    return MFASetupResponse(secret=secret, qr_uri=uri)

@router.post("/verify")
def verify_mfa(payload: MFAVerifyRequest, current_user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not current_user.mfa_secret:
        raise HTTPException(400, "MFA setup not started.")
    
    totp = pyotp.TOTP(current_user.mfa_secret)
    if not totp.verify(payload.code, valid_window=1):
        raise HTTPException(401, "Invalid MFA code.")
    
    current_user.mfa_enabled = True
    db.commit()
    return {"message": "MFA enabled successfully!"}

@router.post("/disable")
def disable_mfa(payload: MFAVerifyRequest, current_user: User = Depends(get_current_user), db: Session = Depends(get_db)):
    if not current_user.mfa_enabled:
        raise HTTPException(400, "MFA is not set up for this account.")
    
    totp = pyotp.TOTP(current_user.mfa_secret)
    if not totp.verify(payload.code, valid_window=1):
        raise HTTPException(401, "Invalid Code - Confirm with your authenticator app to disable MFA for this account.")
    
    current_user.mfa_enabled = False
    current_user.mfa_secret = False
    db.commit()
    return {"message": "MFA disabled successfully"}