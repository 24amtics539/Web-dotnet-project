# CampusDeadline 🎓⏱️ — Student Academic OS
> A modern, responsive, academic task and deadline management platform built with **ASP.NET Core MVC**, **Entity Framework Core (EF Core)**, **MySQL**, and vanilla HTML5 / CSS3 / JavaScript.

---

## 📋 Project Overview & Problem Statement

University and college students often juggle numerous courses, continuous assessments, lab practicals, term projects, and examination schedules across multiple semesters. Without a centralized, deadline-oriented system, students frequently miss submission dates, struggle with task prioritization, and suffer from poor academic time management.

**CampusDeadline** solves this real-world problem by providing a sleek, dark-academic productivity dashboard (Dark Charcoal `#393941`, Deep Panels `#201F29`, and Lavender `#B98AFF` accents) tailored specifically for academic workflow tracking, deadline countdowns, exam reminders, interactive calendars, and performance analytics.

---

## 🌟 Key Requirements Demonstrated

| Requirement | Implementation Details in CampusDeadline |
|---|---|
| **1. MVC Architecture** | Strict separation of concerns with Controllers (`AccountController`, `DashboardController`, `TasksController`, `SubjectsController`, `CalendarController`, `ProgressController`), Models (`User`, `Subject`, `TaskItem`, `TaskStatusHistory`), ViewModels (`TaskFormViewModel`, `SubjectFormViewModel`, `DashboardViewModel`, `ProgressViewModel`), Services (`ITaskService`, `IAnalyticsService`, `IPdfReportService`), and Views (`_Layout.cshtml`, `_Sidebar.cshtml`, Modals, Razor Views). |
| **2. Database Connectivity** | **Entity Framework Core 9.0** integrated with **MySQL 8.0** via `Pomelo.EntityFrameworkCore.MySql`. Automatic database creation and seed migration on startup via `DbInitializer.cs`. Fluent API configuration for relationships, unique indexes, and cascade deletions. |
| **3. CRUD Operations** | **Full CRUD on Academic Tasks**: Create task, Read/Filter tasks, Update task & quick inline status transitions (`pending`, `in_progress`, `completed`), and Delete task with confirmation dialog.<br>**Full CRUD on Course Subjects**: Create course, Read/Search courses with per-subject statistics, Edit course, and Delete course with cascading task warnings. |
| **4. Form Handling** | Razor forms and AJAX form submissions with both standard postbacks and dynamic modal updates. Split date/time pickers combined seamlessly on submission. |
| **5. Model Validation** | Extensive Data Annotations (`[Required]`, `[EmailAddress]`, `[StringLength]`, `[Range(1, 12)]`, `[DataType]`, `[Compare]`) on form models. Both client-side feedback and server-side `ModelState.IsValid` error propagation. |
| **6. State Management** | **Cookie Authentication**: Secure session cookie `.CampusDeadline.Auth` using `CookieAuthenticationDefaults`.<br>**Session State**: `HttpContext.Session` with 2-hour idle timeout for caching user profile metadata (`UserId`, `UserName`, `UserEmail`, `UserRole`).<br>**TempData**: CookieTempDataProvider for flash message toast alerts across redirects (`TempData["Success"]`, `TempData["Error"]`, `TempData["Info"]`).<br>**Query String State**: Preserves multi-factor filtering, search terms, and sort orders. |
| **7. Responsive UI/UX** | Mobile-first and desktop-optimized responsive layout featuring mobile topbar header, off-canvas drawer navigation, responsive 3-column academic dashboard layout, Chart.js graphs, FullCalendar 6 integration, and Lucide icons. |

---

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 10.0 / 9.0 (C#)
- **ORM**: Entity Framework Core 9.0 (`Pomelo.EntityFrameworkCore.MySql`)
- **Database**: MySQL Server 8.0 (`campusdeadline_db`)
- **Security & Auth**: Cookie Authentication, BCrypt password hashing, ASP.NET Core AntiForgery CSRF tokens (`X-CSRFToken`)
- **Reporting**: QuestPDF Community 2026.9 for official academic PDF reports
- **Frontend**: Vanilla HTML5, CSS3 (Custom CSS Grid & Flexbox), Vanilla JavaScript (ES6+)
- **Visualizations**: Chart.js 4.x
- **Calendar**: FullCalendar.js 6.x
- **Icons**: Lucide Icons
- **Typography**: Google Fonts (Inter & Poppins)

---

## 🚀 Quick Start Guide

### 1. Prerequisites
- [.NET SDK 10.0 or 9.0](https://dotnet.microsoft.com/download) installed.
- [MySQL Server 8.0](https://dev.mysql.com/downloads/mysql/) running locally on port `3306`.

### 2. Configure Database Connection
Review or update the connection string in [appsettings.json](file:///C:/Users/Admin/Desktop/CampusDeadline/appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=campusdeadline_db;User=root;Password=ankit@9531;CharSet=utf8mb4;"
  }
}
```

### 3. Run the Application
Open a terminal in the project directory `C:\Users\Admin\Desktop\CampusDeadline` and execute:
```bash
dotnet run
```
The application will automatically:
1. Connect to MySQL.
2. Create the `campusdeadline_db` database and tables if they do not exist.
3. Launch the web server on `http://localhost:5029` (or the port specified in `launchSettings.json`).

### 4. Open in Browser & Create Account
Navigate to:
```
http://localhost:5029
```
Click **"Create an account"** (or navigate to `/register`) to register your student account. Upon registration, default semester subjects and your starter academic onboarding task are initialized automatically.

---

## 🧭 Application Modules & Features

### 1. Academic Dashboard (`/dashboard`)
- **Three-Column Academic Composition**:
  - **Left Workspace (~54%)**:
    - **Academic Activity Heatmap**: Interactive matrix visualizing daily activity and completions across 4, 8, or 12 weeks with purple intensity tiers (`level-0` through `level-4`), tooltip dates and counts.
    - **Four Live Stat Cards**:
      1. *Pending Tasks*: Active task count with 7-day upcoming count badge.
      2. *Completed Tasks*: Total completed count with overall percentage.
      3. *Upcoming Deadlines*: Due within 7 days, highlighting nearest task countdown.
      4. *Overdue Tasks*: Incomplete tasks past deadline with warning badge.
    - **Academic Task Distribution**: Chart.js bar chart breaking down Completed, Pending, and Overdue tasks.
    - **Upcoming Academic Deadlines**: Tasks sorted by nearest deadline with live countdown pill and 1-click status completion toggle.
  - **Middle Column (~18%)**: Collapsible navigation sidebar with student profile card and quick system stats.
  - **Right Workspace (~28%)**:
    - **Prominent Action Button**: Fast modal opening.
    - **Interactive Mini Calendar**: Monthly calendar with clickable dates to schedule deadlines on that specific day.
    - **Upcoming Exam Reminders**: Highlighting exam papers with countdown timer tags.
    - **In-Progress Tracker**: Quick list of currently active projects and labs.

### 2. My Tasks (`/tasks`)
- **Multi-Factor Filtering**: Filter by Subject, Category (*Assignment*, *Practical*, *Examination*, *Project*, *Other*), Priority (*Low*, *Medium*, *High*), and Status (*Pending*, *In Progress*, *Completed*).
- **Quick Status Pills**: 1-click toggling between All, Upcoming, Overdue, and Completed.
- **Live Debounced Search**: Fast search across task titles and descriptions.
- **Flexible Sorting**: Sort by Deadline (nearest/furthest), Priority (high-to-low), and Creation date.
- **Full Task CRUD**: Create, edit, and delete with confirmation dialog; quick inline status updates.

### 3. Academic Calendar (`/calendar`)
- **FullCalendar 6 Integration**: Dark-themed Month, Week, and Day views.
- **Real-Time Dynamic Feed**: Fetches events dynamically from `/api/calendar/events`.
- **Subject & Status Dynamic Filters**: Filter calendar deadlines by subject and status in real-time.
- **Click-to-Create**: Click any date cell on the calendar to open the task creation modal with that date pre-populated.
- **Event Inspection**: Click any deadline event to view details and edit.

### 4. My Subjects (`/subjects`)
- **Course Management**: Add and manage enrolled semester courses (1–12) with custom color themes.
- **Per-Course Analytics**: Total tasks, completed tasks, pending tasks, and completion percentage progress bar.
- **Unique Code Enforcement**: Prevents duplicate subject codes per student.
- **1-Click Navigation**: "View Tasks" button navigates to filtered tasks for any course.
- **Safe Deletion**: Deleting a subject confirms and alerts the user if tasks will be removed.

### 5. Progress & Analytics (`/progress`)
- **Academic Productivity Metrics**: Total tasks, completed tasks, overall completion rate, top performing course.
- **Dynamic Period Filter**: Filter analytics for Last 7 Days, Last 30 Days, Semester, or All Time.
- **Interactive Chart Suite**:
  1. *Doughnut Chart*: Task status distribution (Completed, In Progress, Pending, Overdue).
  2. *Grouped Bar Chart*: Tasks by subject (Completed vs Incomplete).
  3. *Area/Line Chart*: 14-day cumulative completion trend.
  4. *Category Bar Chart*: Task type distribution (Assignments, Practicals, Exams, Projects).
- **Rule-Based Academic Insights**: Dynamic observation cards generated from deadline history.
- **Official Academic PDF Export**: Download official academic performance report as PDF (`/progress/export.pdf`).

### 6. Authentication & Security (`/login`, `/register`, `/logout`)
- **Password Hashing**: Secure password hashing with BCrypt.
- **Anti-Forgery CSRF Protection**: Form inputs and AJAX requests verified with cryptographic tokens (`X-CSRFToken`).
- **Session & Data Isolation**: Every database query is strictly scoped by `User.GetUserId()`.

---

## 📂 Project Directory Structure

```
CampusDeadline/
├── Controllers/
│   ├── AccountController.cs       # Authentication (Login, Register, Logout)
│   ├── DashboardController.cs     # Academic dashboard, stats, heatmap
│   ├── TasksController.cs         # Full Task CRUD, filtering, inline status
│   ├── SubjectsController.cs      # Full Subject CRUD, per-course analytics
│   ├── CalendarController.cs      # FullCalendar integration & events API
│   └── ProgressController.cs      # Analytics charts & PDF report export
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core DbContext with MySQL mapping
│   └── DbInitializer.cs           # Database creation & demo data seeder
├── Extensions/
│   └── ClaimsPrincipalExtensions.cs # Extension methods for authenticated user
├── Models/
│   ├── Entities/
│   │   ├── User.cs                # Student / User entity
│   │   ├── Subject.cs             # Course subject entity
│   │   ├── TaskItem.cs            # Academic task entity
│   │   └── TaskStatusHistory.cs   # Status audit history for heatmap
│   └── ViewModels/
│       ├── AuthViewModels.cs      # Login and registration form models
│       ├── TaskViewModels.cs      # Task forms, DTOs, and list filters
│       ├── SubjectViewModels.cs   # Subject forms, DTOs, and list models
│       ├── DashboardViewModel.cs  # Dashboard stats and heatmap DTOs
│       └── ProgressViewModel.cs   # Analytics metrics, chart data, insights
├── Services/
│   ├── ITaskService.cs & TaskService.cs         # Core academic task business logic
│   ├── IAnalyticsService.cs & AnalyticsService.cs # Metric calculations & trend data
│   └── IPdfReportService.cs & PdfReportService.cs # QuestPDF report generation
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml           # Sleek dark login card
│   │   └── Register.cshtml        # Student registration card
│   ├── Dashboard/
│   │   └── Index.cshtml           # 3-column academic dashboard
│   ├── Tasks/
│   │   └── Index.cshtml           # My Tasks workspace with filters
│   ├── Subjects/
│   │   └── Index.cshtml           # My Subjects cards grid
│   ├── Calendar/
│   │   └── Index.cshtml           # FullCalendar interactive schedule
│   ├── Progress/
│   │   └── Index.cshtml           # Progress & Analytics with 4 Chart.js charts
│   ├── Shared/
│   │   ├── _Layout.cshtml         # Master layout with dark theme
│   │   ├── _Sidebar.cshtml        # Navigation sidebar & user profile
│   │   ├── _Topbar.cshtml         # Mobile responsive header
│   │   ├── _FlashMessages.cshtml  # Flash alert banners
│   │   ├── _TaskModal.cshtml      # Create/Edit Task modal
│   │   ├── _SubjectModal.cshtml   # Create/Edit Subject modal
│   │   └── _ConfirmModal.cshtml   # Delete confirmation modal
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/
│   ├── css/
│   │   ├── style.css              # Dark academic aesthetic styling
│   │   └── responsive.css         # Mobile topbar & responsive grids
│   └── js/
│       ├── app.js                 # Global modals, AJAX helper, drawer
│       ├── dashboard.js           # Heatmap, mini calendar & Chart.js
│       ├── tasks.js               # Tasks CRUD & status updates
│       ├── subjects.js            # Subjects CRUD & cascade warnings
│       ├── calendar.js            # FullCalendar initialization & event feed
│       └── progress.js            # 4 Chart.js analytics & period switcher
├── appsettings.json               # MySQL connection string & configuration
└── Program.cs                     # App entry point, DI, auth & middleware pipeline
```
