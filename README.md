# 🏋️ Gymbro — AI-Powered Fitness Tracker

A modern WPF desktop application that uses AI to track your nutrition, workouts, and body weight — all in one place.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🤖 **AI Food Analysis** | Drop a photo or type a food name — AI calculates calories and full macros |
| ⚡ **Nutrition Cache** | Previously analysed foods are stored locally so there's no repeated AI call |
| 🏃 **Workout Logger** | Log workouts with instant MET-based calorie estimates, refined by AI in the background |
| 📅 **7-Day History** | View your last 7 days of workouts in a dedicated history tab |
| 💧 **Water Tracker** | Quick-add 250 ml / 500 ml / 1 L buttons with a daily progress bar |
| 🔥 **Streak Counter** | Counts consecutive days you've logged food — keeps you motivated |
| ⚖️ **Weight Chart** | Log your daily weight and see a 30-day trend line chart |
| 🤖 **AI Coach** | Ask the AI anything about your nutrition or training plan |
| 🗓️ **Workout Plan** | Generate a personalised AI workout plan based on your goal and equipment |
| 👤 **Profile & Goals** | Mifflin-St Jeor TDEE formula auto-calculates calorie and macro targets |
| 🌗 **Dark / Light Theme** | Toggle between dark and light mode at any time |

---

## 🛠️ Tech Stack

- **UI** — WPF (.NET 10, C#)
- **Database** — SQL Server (Entity Framework Core 8 + Migrations)
- **AI** — Anthropic Claude API (vision + text)
- **Architecture** — Code-behind with per-operation DbContext, cached views

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or SQL Server Express)
- An [Anthropic API key](https://console.anthropic.com/)

### 1. Clone the repo

```bash
git clone https://github.com/abela69/Teaining_APP.git
cd Teaining_APP
```

### 2. Create your `.env` file

Inside the `Training_APP/` folder (next to the `.csproj`), create a file called `.env`:

```
ANTHROPIC_API_KEY=sk-ant-api03-your-key-here
```

> ⚠️ This file is gitignored and will never be committed.

### 3. Configure the database connection

Open `Training_APP/Data/AppDbContext.cs` and update the connection string to match your SQL Server instance:

```csharp
optionsBuilder.UseSqlServer(
    @"Server=YOUR_SERVER;Database=CalorieMaster;Trusted_Connection=True;TrustServerCertificate=True;"
);
```

### 4. Run migrations

```bash
cd Training_APP
dotnet ef database update
```

### 5. Build and run

```bash
dotnet run
```

---

## 📁 Project Structure

```
Training_APP/
├── Assets/             # Logo and images
├── Data/               # EF Core DbContext
├── Migrations/         # EF Core migration files
├── Model/              # Data models (User, FoodEntry, WorkoutEntry, etc.)
├── Service/            # Business logic (NutritionService, WorkoutService, etc.)
├── Themes/             # Dark and light theme resource dictionaries
├── Views/              # WPF views (Dashboard, FoodLog, Workout, Coach, Profile)
├── App.xaml.cs         # App startup, service initialisation
└── MainWindow.xaml.cs  # Navigation and view caching
```

---

## 🔒 Security

- Your API key is stored in a local `.env` file and **never committed to git**
- The `.env` is loaded at startup by `EnvLoader.cs` from the app's output directory

---

## 📸 Screenshots

> Coming soon

---

## 📄 License

This project is for personal and educational use.
