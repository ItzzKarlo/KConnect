# !/backend/app/core/config.py
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    DATABASE_URL: str
    REDIS_URL: str
    SECRET_KEY: str
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 30
    REFRESH_TOKEN_EXPIRE_DAYS: int = 7

    RESEND_API_KEY: str
    EMAIL_FROM: str = "no-reply@karlo-cavlovic.dev"
    FRONTEND_URL: str = "http://localhost:5173"

    class Config:
        env_file = ".env"

settings = Settings()