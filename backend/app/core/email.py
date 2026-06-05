import resend
from app.core.config import settings

resend.api_key = settings.RESEND_API_KEY

def send_verification_email(to: str, username: str, token: str):
    verify_url = f"{settings.FRONTEND_URL}/verify-email?token={token}"
    resend.Emails.send({
        "from": settings.EMAIL_FROM,
        "to": to,
        "subject": "Verify your KConnect account",
        "html": f"""
        <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px;background:#111;color:#fff;border-radius:12px">
            <h1 style="color:#FF0000;margin-bottom:8px">KConnect</h1>
            <p style="color:#CCAC58;font-size:18px">Welcome, {username}!</p>
            <p>Click below to verify your email address. This link expires in <strong>24 hours</strong>.</p>
            <a href="{verify_url}"
               style="display:inline-block;margin-top:16px;padding:12px 28px;background:#FF0000;color:#fff;border-radius:8px;text-decoration:none;font-weight:bold">
                Verify Email
            </a>
            <p style="margin-top:24px;color:#888;font-size:13px">
                If you didn't create a KConnect account, you can safely ignore this email.
            </p>
        </div>
        """,
    })

def send_password_reset_email(to: str, username: str, token: str):
    reset_url = f"{settings.FRONTEND_URL}/reset-password?token={token}"
    resend.Emails.send({
        "from": settings.EMAIL_FROM,
        "to": to,
        "subject": "Reset your KConnect password",
        "html": f"""
        <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px;background:#111;color:#fff;border-radius:12px">
            <h1 style="color:#FF0000;margin-bottom:8px">KConnect</h1>
            <p style="color:#CCAC58;font-size:18px">Password Reset</p>
            <p>Hi {username}, we received a request to reset your password.</p>
            <a href="{reset_url}"
               style="display:inline-block;margin-top:16px;padding:12px 28px;background:#FF0000;color:#fff;border-radius:8px;text-decoration:none;font-weight:bold">
                Reset Password
            </a>
            <p style="margin-top:12px;color:#888;font-size:13px">This link expires in <strong>1 hour</strong>. If you didn't request this, ignore this email.</p>
        </div>
        """,
    })