# SocietyMS — FAST Societies Management System

Centralized desktop platform for managing university societies, memberships, events, and announcements at **FAST-NUCES**. Built as a Software Engineering & Design (SED) course project.

> Connecting students · Empowering societies

---

## Overview

**SocietyMS** is a role-based Windows desktop application that streamlines how students join societies, how society heads run their organizations, and how administrators oversee the entire ecosystem. The app automatically creates its database schema and seed data on first launch.

## Features

### Student
- Register and log in with a student account
- Browse active societies and apply for membership
- View and register for approved events
- Track membership requests and event tickets
- See announcements from joined societies and university-wide posts

### Society Head
- Manage society profile and details
- Approve or reject membership requests
- Create and manage society events
- Assign tasks to members
- Post society announcements
- View society-level reports and dashboard stats

### Admin
- Manage users and society records
- Approve or suspend societies
- Review and approve pending events
- View system-wide reports and statistics
- Monitor pending memberships and registrations

## Tech Stack

| Layer | Technology |
|-------|------------|
| Language | C# |
| Framework | .NET Framework 4.8 |
| UI | Windows Forms (WinForms) |
| Database | Microsoft SQL Server (LocalDB / Express) |
| Data Access | ADO.NET (`SqlClient`) |

## Prerequisites

Before running the project, make sure you have:

- **Windows** (required for WinForms)
- **Visual Studio 2019 or later** with the **.NET desktop development** workload
- **SQL Server LocalDB** or **SQL Server Express**

Install LocalDB: [https://aka.ms/sqllocaldb](https://aka.ms/sqllocaldb)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/<repo-name>.git
cd SocietyMS
```

### 2. Open the project

Open `SocietyMS.csproj` in Visual Studio.

### 3. Build and run

1. Set the build configuration to **Debug** or **Release**
2. Press **F5** or click **Start**
3. On first launch, the app will:
   - Connect to SQL Server
   - Create the `SocietyMS` database tables if they do not exist
   - Seed a default admin account and sample societies

### 4. Log in

Use the default admin account for initial access:

| Field | Value |
|-------|-------|
| Email | `admin@fast.edu.pk` |
| Password | `Admin@123` |

Students can register new accounts from the login screen.

## Database

The app uses the following default connection string:

```
Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SocietyMS;Integrated Security=True;
```

If LocalDB is unavailable, the app automatically tries these alternatives:

- `.\SQLEXPRESS`
- `localhost`
- `(local)`

You can change the connection string in `Database/DatabaseManager.cs` if needed.

### Database Tables

- `Users` — students, society heads, and admins
- `Societies` — society profiles and status
- `Memberships` — join requests and approved members
- `Events` — society events
- `EventRegistrations` — event sign-ups and ticket numbers
- `SocietyTasks` — tasks assigned to members
- `Announcements` — society and global announcements

## Project Structure

```
SocietyMS/
├── Program.cs              # Application entry point
├── Database/
│   └── DatabaseManager.cs  # DB connection, queries, schema setup
├── Models/                 # User, Society, Event, Membership, Task, Announcement
├── Forms/                  # WinForms UI (login, dashboards, management screens)
├── Helpers/                # Session, validation, UI theme utilities
└── Properties/
    └── AssemblyInfo.cs
```

## Team

| Roll Number | Role |
|-------------|------|
| i222500 | Team Member |
| i222638 | Team Member |
| i222537 | Team Member |

**Course:** Software Engineering & Design (SED)  
**Institution:** FAST-NUCES  
**Year:** 2026

## Screenshots

_Add screenshots of the login page, student dashboard, and admin panel here after uploading to GitHub._

## License

This project was developed for academic purposes at FAST-NUCES.

---

**SocietyMS** — *FAST Societies Management System*
