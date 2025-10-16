# BilbasenV2

A full-stack vehicle marketplace web application built with **ASP.NET MVC (C#)**, inspired by Bilbasen.dk.  
The system allows users to register, authenticate, and manage vehicle listings with full CRUD functionality, server-side validation, and image handling.

---

## 🧩 Tech Stack

- **Backend:** ASP.NET MVC (C#), Entity Framework Core (Code-First)  
- **Frontend:** Razor Views, HTML5, CSS3, JavaScript (Vanilla / jQuery)  
- **Database:** Microsoft SQL Server  
- **Architecture:** MVC pattern with layered separation (Controllers → Services → Repositories → Data Context)  
- **ORM:** Entity Framework Core with LINQ and data annotations  
- **Authentication:** ASP.NET Identity (role-based access control)  
- **Hosting / Runtime:** .NET 6.0  

---

## 🚀 Core Features

- 🔐 **User Authentication & Authorization** — ASP.NET Identity with roles for admin and standard users  
- 🚗 **Vehicle Management** — Create, read, update, and delete vehicle listings  
- 🔍 **Advanced Filtering** — Search by brand, model, price range, year, and fuel type  
- 🖼️ **Image Uploads** — Multi-image upload using `IFormFile` and server-side storage  
- 🧱 **Clean Architecture** — Business logic isolated in `Services`, persistence handled by `Repositories`  
- 🧩 **Entity Framework Integration** — Code-First migrations and relationship mapping  
- ⚙️ **Dependency Injection** — Configured via `Program.cs` for services and repositories 
