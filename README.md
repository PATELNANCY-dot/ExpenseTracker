# ⚙️ Expense Tracker Backend API (.NET Core + Angular)

## 📌 Overview
This is a full-stack Expense Tracker (Expense Book) project built using .NET Core Web API, Angular frontend, and Microsoft SQL Server database.

It allows users to manage income, expenses, balance, profile, and theme settings in a clean dashboard system.

---

## 🚀 Features

### 🔐 Authentication
- User Registration
- User Login

### 💸 Expense Management
- Add Expense
- Update Expense
- Delete Expense
- View Expenses (latest first)

### 💰 Income Management
- Add Income
- Update Income
- Delete Income
- View Income List

### 📊 Dashboard
- Total Income Calculation
- Total Expense Calculation
- Balance (Income - Expense)

### 👤 User Profile
- View Profile
- Update Profile
- Change Password

### 🎨 Theme System
- Dark Mode / Light Mode per user

### 🧹 Data Management
- Clear all user data
- Delete account permanently

---

## 🛠️ Tech Stack
- .NET Core Web API
- ADO.NET (SqlConnection, SqlCommand)
- Microsoft SQL Server (LocalDB / MSSQL)
- REST API
- Angular Frontend
- Swagger API Testing

---

## 📂 Backend Structure
backend/
├── Controllers/
│   └── ExpenseTrackerController.cs
├── Models/
│   ├── UserModel.cs
│   ├── LoginDto.cs
│   ├── ExpenseModel.cs
│   ├── IncomeModel.cs
│   ├── ChangePasswordDto.cs
│   └── ThemeModel.cs
├── appsettings.json
└── Program.cs

---

## ⚙️ Setup Instructions

### 1️⃣  Project
git  https://github.com/PATELNANCY-dot/ExpenseBook-Backend
cd backend

---

### 2️⃣ Configure Database
Update `appsettings.json`:

  ```json id="b3"
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ExpenceBook;Trusted_Connection=True;TrustServerCertificate=True;"
}
```


---

### 3️⃣ Run Project
dotnet restore
dotnet run

---

## 🌐 API Base URL
https://localhost:7042/api/ExpenseTracker

---

## 📌 API Endpoints

### 🔐 Authentication
POST /register
POST /login

---

### 💸 Expense APIs
GET    /get-expenses/{userId}
POST   /add-expense
PUT    /update-expense/{id}
DELETE /delete-expense/{id}

---

### 💰 Income APIs
GET    /get-income/{userId}
POST   /add-income
PUT    /update-income/{id}
DELETE /delete-income/{id}

---

### 📊 Dashboard
GET /dashboard/{userId}

---

### 👤 Profile APIs
GET  /get-user-profile/{id}
PUT  /update-profile
PUT  /change-password

---

### 🎨 Theme APIs
POST /save-theme
GET  /get-theme/{userId}

---

### 🧹 Data Management
DELETE /clear-user-data/{userId}
DELETE /delete-account/{id}

---

## 🗄️ Database Tables

Users:
- Id
- Name
- Email
- Password

Expenses:
- Id
- Title
- Amount
- Category
- ExpenseDate
- Notes
- UserId

Income:
- Id
- Title
- Amount
- IncomeDate
- UserId

UserSettings:
- UserId
- DarkMode

---

## 🔗 Angular Integration
Frontend uses Angular HttpClient to connect with backend APIs.

Example:
this.http.post('/api/ExpenseTracker/add-expense', data)

---

## 📸 Screenshots
Add your UI screenshots here:
- Login Page
- Dashboard
- Expense Page
- Income Page

---

## 🚀 Future Improvements
- JWT Authentication
- Charts (Chart.js)
- PDF Export Reports
- Email Notifications
- Role-based Authorization
- API Security Enhancements

---

## 👩‍💻 Author
Nancy Patel

---

## ⭐ Status
Project Completed ✔  
Backend Ready ✔  
Angular Connected ✔  
