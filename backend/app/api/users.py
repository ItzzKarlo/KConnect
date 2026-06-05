# !/backend/app/api/users.py
from fastapi import APIRouter, Depends, Query, HTTPException
from sqlalchemy.orm import Session
from sqlalchemy import or_
from app.core.database import get_db
from app.api.deps import get_current_user
from app.models.user import User
from app.schemas.auth import UserSearchResult

router = APIRouter(prefix="/users", tags=["users"])

@router.get("/search", response_model=list[UserSearchResult])
def search_users(
    q: str = Query(min_length=2, max_length=50),
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_user),
):
    """Search users by username, email or phone. Excludes yourself."""
    results = (
        db.query(User)
        .filter(
            User.id != current_user.id,
            User.is_active == True,
            User.is_verified == True,
            or_(
                User.username.ilike(f"%{q}%"),
                User.email.ilike(f"%{q}%"),
                User.phone.ilike(f"%{q}%"),
            )
        )
        .limit(20)
        .all()
    )
    return results

@router.get("/{username}", response_model=UserSearchResult)
def get_user_by_username(
    username: str,
    db: Session = Depends(get_db),
    current_user: User = Depends(get_current_user)
):
    user = db.query(User).filter(
        User.username == username,
        User.is_active == True,
        User.is_verified == True,
    ).first()
    if not user: 
        raise HTTPException(404, "User not found.")
    return user