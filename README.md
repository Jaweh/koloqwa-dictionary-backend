
# 📚 The Koloqwa Dictionary (Backend API)

The Koloqwa Dictionary backend is a scalable REST API built with .NET 9 that powers word search, phrase lookup, and community-driven contributions for Liberian local languages.

It follows Clean Architecture principles and supports role-based moderation workflows.

---

## 🚀 Features

### 🌍 Public API
- Search dictionary words
- Search phrase glossary
- Retrieve approved entries

### 👤 Authentication
- User registration & login
- JWT-based authentication
- Role-based access control

### 📝 Contribution System
- Submit new words and phrases
- Store submissions in approval queue
- Track submission status

### 🛡️ Admin System
- Approve/reject submissions
- Edit entries before publishing
- Manage users and roles

---

## 🏗️ Tech Stack

- .NET 9 Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Clean Architecture (Domain, Application, Infrastructure, API)

---

## 📦 Core Modules

- Authentication Module
- Dictionary Module
- Phrase Module
- Submission Workflow Module
- Admin Module

---

## 🧠 Submission Workflow

1. User submits word or phrase
2. Entry is stored in `SubmissionQueue`
3. Admin reviews entry
4. Admin approves/rejects
5. Approved entries are published

---

## 📂 Project Structure
/src
/Domain
/Application
/Infrastructure
/API

---

## 🔐 Roles

- **Guest:** Search content
- **User:** Submit entries
- **Admin:** Moderate content
- **SuperAdmin:** Full system control
