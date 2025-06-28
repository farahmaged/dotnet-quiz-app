## Overview

A web-based **quiz maker application** enabling users to:
- Register and manage their profiles
- Create and administer quizzes with a time limit
- Add multiple-choice questions per quiz
- Attempt quizzes with real-time timers
- View quiz results and history

The system is built using **ASP.NET Core MVC** and **Entity Framework Core** with SQL Server for data storage.

---

## Technologies Used

- **ASP.NET Core MVC** — For the application’s backend and server-side logic.
- **Entity Framework Core** — For managing data access and interactions with the database.
- **SQL Server** — For storing application data such as users, quizzes, and results.
- **JavaScript / HTML / CSS** — For the application's frontend.

---

## Setup Instructions

### 1. Clone the repository:
```bash
git clone https://github.com/farahmaged/dotnet-quiz-app.git
cd dotnet-quiz-app
```

### 2. Open the project
Open the solution in Visual Studio or your preferred IDE.

### 3. Apply database migrations
Using the Package Manager Console:

```bash
Update-Database
```

Or using the CLI:

```bash
dotnet ef database update
```

### 4. Run the application
```bash
dotnet run
```