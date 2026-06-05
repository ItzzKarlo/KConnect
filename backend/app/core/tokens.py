# !/backend/app/core/tokens.py
from itsdangerous import URLSafeSerializer, SignatureExpired, BadSignature
from app.core.config import settings

serializer = URLSafeSerializer(settings.SECRET_KEY)

def generate_email_token(email: str, salt: str = "email-verify") -> str:
    return serializer.dump(email, salt=salt)

def verify_email_token(token: str, salt: str = "email-verify", max_age: int = 86400) -> str | None:
    """Returns the email if valid, None if expired or tampered."""
    try:
        return serializer.loads(token, salt=salt, max_age=max_age)
    except (SignatureExpired, BadSignature):
        return None