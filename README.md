# 🗓️ Planora — Smart Weekly Planner

> Plan your days, drag your blocks, and let AI fill the gaps. Your personal time‑management companion, built with .NET 10. 🚀

**Planora** is a full‑stack personal planner application that helps you organize your week with visual calendar blocks, a powerful drag‑and‑drop timeline, and an AI‑powered assistant that automatically schedules your tasks into free time slots. ⏰✨

---

## 📸 Highlights

- 🗓️ **Week & Day Views** — Hour‑accurate timeline (week: 0–24h, day: 00–23h).
- ✂️ **Two‑Pane Day View** — Day view is split into two side‑by‑side halves (00–12 on the left, 12–24 on the right) for comfortable mobile‑style reading.
- 🔍 **Timeline Zoom** — Adjustable hour height (24 → 150 px) with half‑hour and quarter‑hour guides; your zoom level is remembered between sessions.
- 🖱️ **Drag & Drop** — Move blocks anywhere on the timeline with 5‑minute snapping and a live drop preview.
- 🤖 **AI Assistant** — Describe what you want and let Planora build a full week: fixed recurring blocks (e.g. workouts) + tasks placed into free slots, with conflict detection and suggestions.
- ✅ **One‑Click Done** — Tick any block to mark it complete (visual line‑through + faded style), tick again to undo.
- 🗑️ **Safe Deletes** — Trash‑bin icons and a top confirm popup instead of raw alerts.
- 🧩 **Block Builder** — Create blocks with title, description, notes, duration, color (14 palette colors), priority, repeat, and reminder options.
- 🔔 **Toast Notifications** — Non‑intrusive top‑right notifications for every action.
- 🛡️ **Overlap‑Free Guarantee** — Server‑side validation prevents overlapping blocks; AI‑generated plans skip conflicting slots.

---

## 🧱 Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 10) 🟣 |
| Frontend | ASP.NET Core MVC + Vanilla JS + CSS 🎨 |
| ORM | Entity Framework Core 10 (Code First) 🧬 |
| Database | SQL Server LocalDB 🗄️ |
| Mapping | AutoMapper ↔️ |
| API Docs | Swagger / OpenAPI 📖 |
| UI Port | `http://localhost:5200` |
| API Port | `http://localhost:5210` |

### 📦 Solution Structure (Layered Architecture)

```
Planora.sln
├── planora.api              # 🌐 REST API (controllers, DI registration, Swagger)
├── planora.ui               # 🖥️ MVC web app (views, JS, CSS, UI DTOs, mapping)
├── planora.application      # ⚙️ Business logic (AI planner, block service, DTOs, mapping)
├── planora.domain           # 🏗️ Repository interfaces (contracts)
├── planora.entities         # 🧩 Domain entities (User, Block, …)
└── planora.infrastructure   # 🗄️ EF Core context, repositories, migrations
```

---

## ✨ Features in Detail

### 🗓️ Calendar

- **Week view:** all 7 days side by side, 0–24h timeline, every day shows its own blocks. Clicking or dropping a block on a day keeps the exact hour.
- **Day view:** 00–23 hours split into two halves — left pane 00–12, right pane 12–24.
- **Zoom:** `+ / −` buttons scale the hour height from 24 px up to 150 px. Guide lines appear at 90 px (half‑hours) and 130 px (quarter‑hours).

### 🖱️ Drag & Drop Blocks

- Drag a block from the builder sidebar **or** an existing block on the timeline.
- Drop preview shows the exact size the block will occupy.
- 5‑minute snap precision → pixel‑perfect placement.
- Moving a block preserves its duration; day drops keep the hour.

### 🤖 AI Planning

- Natural command style: e.g. *“workout Mon/Wed/Fri 18:00, study 2 hours every weekday”*.
- **Fixed blocks** (recurring, e.g. sport) placed on exact days/times.
- **Tasks** automatically placed into the first free slot (with optional preferred start time and deadlines).
- **Conflict detection** — if a slot is taken, Planora reports the conflict with actionable suggestions.
- **Apply with safety** — applying a plan skips any block that would overlap existing ones.

### ✅ Block Status

- Pending ↔ Done toggle via `PUT /api/blocks/{id}/status`.
- Done blocks are visually distinguished (strikethrough + transparency + green tick).

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) 🧑‍💻
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (usually ships with Visual Studio) 🗄️

### 1️⃣ Create the database

The connection string is configured in `planora.infrastructure/Context/PlanoraDbContext.cs`:

```
Server=(localdb)\planora;Database=Planora;Trusted_Connection=True;
```

Apply the latest migration (from the solution root):

```bash
dotnet ef database update
```

### 2️⃣ Run the API

```bash
dotnet run --project planora.api
```

- API: http://localhost:5210
- Swagger UI: http://localhost:5210/swagger

### 3️⃣ Run the Web UI

```bash
dotnet run --project planora.ui
```

- UI: http://localhost:5200

> 🎯 Tip: Start both projects, then open the UI in your browser.

### 🛠️ VS Code

A `.vscode/tasks.json` is included with convenience tasks (build, run API, run UI, free ports) so you can launch everything with a single press of `F5` / `Ctrl+Shift+B`.

---

## 🔌 API Overview

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/users` | List users |
| `GET` | `/api/blocks` | All blocks |
| `GET` | `/api/blocks/date/{date}` | Blocks for a specific day |
| `GET` | `/api/blocks/range?startDate=&endDate=` | Blocks in a date range |
| `POST` | `/api/blocks` | Create a block (server‑side overlap check 🔒) |
| `PUT` | `/api/blocks/{id}` | Update a block |
| `PUT` | `/api/blocks/{id}/status` | Toggle block status (done/undone) |
| `DELETE` | `/api/blocks/{id}` | Delete a block |
| `POST` | `/api/ai/plan` | Generate an AI‑powered weekly plan 🧠 |
| `POST` | `/api/ai/apply` | Apply proposed AI blocks (overlap‑safe) |
| `POST` | `/api/ai/command` | Natural‑language planning command |

---

## 🗄️ Database Migrations

Migrations live under `planora.infrastructure/Migrations`. To create a new one:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

---

## 🤝 Contributing

Pull requests are welcome! 🎉 For major changes, please open an issue first to discuss what you'd like to change. Make sure to keep the build green:

```bash
dotnet build Planora.sln
```

---

## 📄 License

This project is provided for learning and personal use. 📚
