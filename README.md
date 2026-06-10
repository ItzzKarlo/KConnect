# KConnect

Open-source real-time messaging platform featuring:

* **FastAPI** backend
* **Vue 3 + Vite** web application
* **PostgreSQL** database
* **Redis** for caching and real-time functionality
* **C# Windows Client**
* WebSocket-powered live chat functionality

## 🌐 Live Application

Production Application:

https://kconnect.karlo-cavlovic.dev/

## 📚 Documentation & Community

Developer Documentation:

https://docs.karlo-cavlovic.dev

Community Portal:

https://dev-with-me.karlo-cavlovic.dev/community/#

---

# Features

* User Authentication
* JWT Security
* Multi-Factor Authentication (MFA)
* Password Reset Support
* Real-Time Messaging
* WebSocket Communication
* User Management
* Conversation Management
* Message History
* Email Integration via Resend
* PostgreSQL Database Support
* Redis Integration
* Cross-Platform Architecture

---

# Project Structure

```text
KConnect/
│
├── backend/              # FastAPI Backend
│   ├── app/
│   ├── alembic/
│   ├── requirements.txt
│   └── .env.example
│
├── web-app/              # Vue 3 Frontend
│   ├── src/
│   ├── package.json
│   └── vite.config.js
│
├── windows-client/       # C# Desktop Client
│
├── infra/
│   └── docker-compose.yml
│
└── README.md
```

---

# Requirements

## Backend

* Python 3.12+
* PostgreSQL 16+
* Redis 7+

## Frontend

* Node.js 20+
* npm 10+

## Windows Client

* Visual Studio 2022+
* .NET SDK

---

# Quick Start

## 1. Clone Repository

```bash
git clone <repository-url>
cd KConnect
```

---

# Infrastructure Setup

Start PostgreSQL and Redis using Docker:

```bash
cd infra
docker compose up -d
```

Services started:

| Service    | Port |
| ---------- | ---- |
| PostgreSQL | 5432 |
| Redis      | 6379 |

---

# Backend Setup

## Install Dependencies

```bash
cd backend

python -m venv .venv

# Windows
.venv\Scripts\activate

# Linux / macOS
source .venv/bin/activate

pip install -r requirements.txt
```

---

## Configure Environment

Copy the example configuration:

```bash
cp .env.example .env
```

Update the following values:

```env
DATABASE_URL=postgresql://username:password@host:port/database_name

REDIS_URL=redis://host:port

SECRET_KEY=your-generated-secret

ALGORITHM=HS256

ACCESS_TOKEN_EXPIRE_MINUTES=30

RESEND_API_KEY=re_xxxxxxxxxxxxxxxxxxxxx

EMAIL_FROM=no-reply@your-domain.com
```

### Generate a Secure Secret Key

```python
import secrets
print(secrets.token_urlsafe(32))
```

---

## Database Migrations

Run Alembic migrations:

```bash
alembic upgrade head
```

Create a new migration:

```bash
alembic revision --autogenerate -m "description"
```

---

## Start Backend

```bash
uvicorn app.main:app --reload
```

Backend API:

```text
http://localhost:8000
```

Health Endpoint:

```text
http://localhost:8000/health
```

---

# Frontend Setup

## Install Dependencies

```bash
cd web-app
npm install
```

---

## Development Mode

```bash
npm run dev
```

Default URL:

```text
http://localhost:5173
```

---

## Production Build

```bash
npm run build
```

Generated files:

```text
web-app/dist
```

---

## Preview Production Build

```bash
npm run preview
```

---

# Modifying the Frontend

The frontend is built with:

* Vue 3
* Vue Router
* Pinia
* Axios
* Vite

Install additional packages:

```bash
npm install package-name
```

Example:

```bash
npm install vue-i18n
```

---

## Adding Routes

Edit:

```text
web-app/src/router/
```

Example:

```javascript
{
  path: "/settings",
  component: SettingsView
}
```

---

## Adding State Management

Edit or create stores in:

```text
web-app/src/stores/
```

Using Pinia:

```javascript
import { defineStore } from "pinia";

export const useUserStore = defineStore("user", {
  state: () => ({
    user: null
  })
});
```

---

# Modifying the Backend

Backend uses:

* FastAPI
* SQLAlchemy
* Alembic
* JWT Authentication
* Redis
* PostgreSQL

---

## Adding API Endpoints

Create or modify routes in:

```text
backend/app/api/
```

Example:

```python
from fastapi import APIRouter

router = APIRouter()

@router.get("/hello")
def hello():
    return {"message": "Hello World"}
```

Register the router:

```python
app.include_router(router)
```

---

## Adding Database Models

Create new models inside:

```text
backend/app/models/
```

Example:

```python
class Example(Base):
    __tablename__ = "examples"
```

Generate migration:

```bash
alembic revision --autogenerate -m "create examples table"
```

Apply migration:

```bash
alembic upgrade head
```

---

# Running the Entire Stack

## Terminal 1

```bash
cd infra
docker compose up -d
```

## Terminal 2

```bash
cd backend
uvicorn app.main:app --reload
```

## Terminal 3

```bash
cd web-app
npm run dev
```

Application URLs:

```text
Frontend: http://localhost:5173
Backend:  http://localhost:8000
```

---

# Deployment

## Frontend

Build:

```bash
npm run build
```

Deploy contents of:

```text
web-app/dist
```

to:

* Nginx
* Apache
* Cloudflare Pages
* Vercel
* Netlify

---

## Backend

Recommended:

* Docker
* Linux VPS
* Kubernetes
* Cloud Run
* Azure App Service

Production command:

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

---

# Security Recommendations

Before deploying:

* Generate a new SECRET_KEY
* Use HTTPS
* Restrict CORS origins
* Use strong PostgreSQL credentials
* Use authenticated Redis instances
* Protect API keys
* Rotate secrets regularly

---

# Contributing

1. Fork the repository
2. Create a feature branch

```bash
git checkout -b feature/my-feature
```

3. Commit changes

```bash
git commit -m "Add new feature"
```

4. Push branch

```bash
git push origin feature/my-feature
```

5. Create a Pull Request

---

# License

This project is licensed under the terms defined in the LICENSE file.

---

# Links

Documentation:
https://docs.karlo-cavlovic.dev

Community:
https://dev-with-me.karlo-cavlovic.dev/community/#

Application:
https://kconnect.karlo-cavlovic.dev/
