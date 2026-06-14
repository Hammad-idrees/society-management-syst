# SocietyMS — FAST Societies Management System

Centralized desktop platform for managing university societies, memberships, events, and announcements at **FAST-NUCES**. Built for **SE-4011 — Software Measurement and Metrics**.

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

| Layer       | Technology                               |
| ----------- | ---------------------------------------- |
| Language    | C#                                       |
| Framework   | .NET Framework 4.8                       |
| UI          | Windows Forms (WinForms)                 |
| Database    | Microsoft SQL Server (LocalDB / Express) |
| Data Access | ADO.NET (`SqlClient`)                    |

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

## Application Metrics

The app tracks live statistics on each dashboard and in the reports module.

| Role             | Metrics shown                                                                                        |
| ---------------- | ---------------------------------------------------------------------------------------------------- |
| **Admin**        | Active users, active societies, pending events/memberships, total registrations, suspended societies |
| **Student**      | Societies joined, events registered, pending requests, active societies                              |
| **Society Head** | Active members, pending requests, approved events, open tasks                                        |

**Admin Reports** (CSV export): Members Report, Societies Report (members & events per society), Events Report (registrations vs capacity), and a Summary tab with 9 KPIs — total/active users, total/active societies, total/approved events, total/approved memberships, and total event registrations.

**Society Head Reports:** Member list and event registration counts for their society.

## Software Metrics (SE-4011)

Measured as part of the **Software Measurement and Metrics** auditing report (`smm_finalproject_sed_report.pdf`).

| Metric                    | Key result                                                                                                              |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| **Project size**          | 3,771 LOC                                                                                                               |
| **Cyclomatic Complexity** | 32 functions analyzed; avg CC ≈ 4.1; highest = `BtnLogin_Click` (12); best module = `ValidationHelper` (avg 2.67)       |
| **CK metrics**            | Max WMC = 24 (`StudentDashboard`); max CBO = 23 (`LoginForm`); max LCOM = 6 (`AdminReportsForm`); DIT = 7 for all forms |
| **Reliability (Mills)**   | 95.24% confidence for high-CC functions (e.g. login, register); 52.38% for simple utilities (CC = 2)                    |
| **KLM usability**         | Fastest: sidebar nav (3.1 s); slowest: registration (24.4 s)                                                            |
| **COCOMO (Organic)**      | 9.68 person-months, ~6 months, ~2 developers, 389 LOC/person-month                                                      |
| **Documentation ratio**   | 23.57:1 (160 comment lines / 3,771 LOC)                                                                                 |

**Findings:** `ValidationHelper` is the best-structured module; `LoginForm` and `StudentDashboard` are the most complex and are candidates for refactoring.
