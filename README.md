# Beat The Rocky 🥊
> **The Undefeated Tic-Tac-Toe AI Champion** built with **ASP.NET Core 7** and an arcade-themed Web UI.

---

## 🌟 Game Highlights
- **Rocky Never Loses**: Powered by a depth-weighted **Minimax Algorithm** with Alpha-Beta pruning. Rocky evaluates every possible move, prioritizes immediate knockouts, and blocks every player trap. The best possible result a human can achieve is a **Draw**.
- **The Opening Coin Toss**: Flip for the opening round! Pick Heads or Tails. If you win, you strike first with `X`. If Rocky wins, Rocky takes the first shot!
- **Arcade Boxing Presentation**:
  - 3D CSS Coin Flip animation.
  - Interactive glowing 3x3 ring grid with win-line highlights.
  - Rocky banter & commentary engine.
  - Web Audio API synthesizer for punch, bell, and knockout sound effects (zero external MP3 dependencies).
  - Scoreboard tracking Rocky's KO streak, Draws survived, and Player Wins (permanently locked at 0).

---

## 🏗️ Architecture & Design Patterns

| Pattern | Component | Description |
| :--- | :--- | :--- |
| **Strategy Pattern** | `IAiStrategy`, `UnbeatableMinimaxStrategy` | Encapsulates the decision engine. Allows swapping AI difficulty or tactics without altering game loop code. |
| **Repository Pattern** | `IGameSessionRepository`, `InMemoryGameSessionRepository` | Thread-safe concurrent storage for multi-session support. |
| **Factory Pattern** | `IGameFactory`, `GameFactory` | Encapsulates clean session initialization and state setup. |
| **Facade / Service Layer** | `IGameService`, `GameService` | Central orchestrator coordinating toss, validation, AI reaction, and win condition checks. |

---

## 🚀 Running Locally

### Prerequisites
- [.NET 7 SDK or .NET 8 SDK](https://dotnet.microsoft.com/download)

### Steps:
1. Navigate to the project directory:
   ```powershell
   cd D:\BeatTheRocky\BeatTheRocky.Api
   ```
2. Run the application:
   ```powershell
   dotnet run --urls "http://localhost:5000"
   ```
3. Open your browser and go to:
   - **Game UI**: [http://localhost:5000](http://localhost:5000)
   - **Swagger API Docs**: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🧪 Running Unit Tests
To mathematically verify that Rocky's Minimax algorithm is 100% unbeatable and never allows a loss:
```powershell
cd D:\BeatTheRocky
dotnet test
```

---

## 🌐 Deployment Guide

### Option 1: Deploy to Render (Recommended - 100% Full-Stack Container)
Render hosts the ASP.NET Core backend and static frontend together via Docker on its free tier.

#### Method A: Using Git & Render Dashboard
1. Initialize git and push the `BeatTheRocky` folder to GitHub / GitLab:
   ```powershell
   cd D:\BeatTheRocky
   git init
   git add .
   git commit -m "feat: Beat The Rocky game"
   # Push to your GitHub repo
   git remote add origin https://github.com/<your-username>/beat-the-rocky.git
   git branch -M main
   git push -u origin main
   ```
2. Log in to [Render.com](https://render.com).
3. Click **New +** -> **Web Service**.
4. Connect your GitHub repository.
5. Configure the service:
   - **Name**: `beat-the-rocky`
   - **Runtime**: `Docker`
   - **Dockerfile Path**: `Dockerfile`
   - **Instance Type**: `Free`
6. Click **Create Web Service**.
7. Once deployed, Render provides an HTTPS URL (e.g. `https://beat-the-rocky.onrender.com`). Open it in your browser!

#### Method B: Render Blueprint (Infrastructure-as-Code)
The repository includes a `render.yaml` file. Simply click **New +** -> **Blueprint** in Render and select this repository for automatic zero-configuration deployment.

---

### Option 2: Deploy to Vercel

Because Vercel is designed for static frontends and Serverless Node/Python/Edge runtimes, the cleanest approach is:

#### Architecture:
- **Backend API**: Hosted on Render (or Railway / Azure / Fly.io).
- **Frontend UI**: Hosted on Vercel with automatic reverse proxy.

#### Steps:
1. Copy or isolate the `BeatTheRocky.Api/wwwroot` folder or deploy from the root using a `vercel.json` config:
2. Create `vercel.json` in the root:
   ```json
   {
     "version": 2,
     "public": true,
     "rewrites": [
       {
         "source": "/api/(.*)",
         "destination": "https://<your-render-backend-url>/api/$1"
       }
     ]
   }
   ```
3. Push to GitHub and import the project into [Vercel](https://vercel.com).
4. Set the root or output directory to `BeatTheRocky.Api/wwwroot`.
5. Click **Deploy**. Vercel will host your high-speed static UI and proxy all `/api/*` game requests seamlessly!

---

## 📡 REST API Reference

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/game/new` | Creates a new game session. |
| `POST` | `/api/game/{id}/toss` | Flips coin (`Heads` or `Tails`). Decides who punches first. |
| `POST` | `/api/game/{id}/move` | Submits player cell pick (`0-8`). Rocky strikes back. |
| `GET` | `/api/game/{id}` | Gets game status and board state. |
| `POST` | `/api/game/{id}/reset` | Resets board and prepares for a rematch. |
| `GET` | `/health` | Health check endpoint for uptime monitors. |
