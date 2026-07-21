# Cursed Into Oblivion Website

Official website and administration platform for **Cursed Into Oblivion**.

This project combines a modern public-facing website with an internal admin system used to manage concerts, merchandise, Electronic Press Kit (EPK) content, sales, and other core operations.

Built with **ASP.NET Core Blazor** and **PostgreSQL**, the application is designed to be lightweight, maintainable, and scalable as the band grows.

---

## Overview

The solution consists of two primary areas:

- **Public Website** – Presents the band, media, events, and EPK resources.
- **Administration Panel** – Secure internal tools for managing content, products, users, and sales.

---

## Features

### Public Website

- Modern landing page
- Band profile and information
- Electronic Press Kit (EPK)
- Image galleries
- Stage rider resources
- Concert calendar
- Cookie consent and cookie policy
- Fully responsive layout

### Administration Panel

- Secure authentication and authorization
- Dashboard overview
- Concert management
- Calendar event management
- Merchandise/product management
- Sales tracking and statistics
- User management
- Route protection for admin-only pages

---

## Technology Stack

- **Framework:** ASP.NET Core Blazor (.NET 8)
- **Language:** C#
- **UI:** Razor Components, HTML5, CSS3, JavaScript
- **Database:** PostgreSQL
- **Data Access:** Dapper, Npgsql
- **Architecture:** Layered structure with dependency injection

---

## Project Architecture

The project follows a clean layered approach:

```text
Components
│
├── Public Pages
├── Admin Pages
├── Shared Layouts
└── Authentication

Data
│
├── Models
├── Services
└── Authentication

wwwroot
│
├── images
├── css
└── js
```

Business logic is encapsulated in service classes responsible for data access and domain-specific operations.

---

## Database

The application uses **PostgreSQL** as the primary datastore.

Current modules include:

- Users
- Concerts
- Calendar Events
- Products
- Sales
- EPK Access

Data access is implemented through:

- Dapper
- Npgsql
- Dependency Injection

---

## Authentication & Authorization

The administration area uses a custom authentication flow with protected routes.

Implemented capabilities include:

- Login
- Logout
- Authorization checks
- Role-based access control

Only authenticated users can access administrative features.

---

## Getting Started

### Prerequisites

- Visual Studio 2022 (or later)
- .NET 8 SDK
- PostgreSQL

### Clone the Repository

```bash
git clone https://github.com/Bmantaz/CiOHjemmeside.git
cd CiOHjemmeside
```

### Configure Application Settings

Update `appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CiO;Username=postgres;Password=yourpassword"
  }
}
```

### Run the Application

```bash
dotnet run
```

You can also run the project directly through Visual Studio.

---

## Roadmap

Planned enhancements:

- Online merchandise storefront
- Payment gateway integration
- Inventory management
- Booking request workflow
- News publishing section
- Expanded media gallery capabilities
- Additional analytics and reporting
- Extended role/permission management

---

## Screenshots

Screenshots and UI previews will be added as development progresses.

---

## Contributing

This is currently a personal project built for **Cursed Into Oblivion**.  
Suggestions, feedback, and constructive input are welcome.

---

## License

Licensed under the **MIT License**, unless otherwise specified.

---

## Author

**Bjarke Andersen**  
Developer, vocalist, and founder of the Cursed Into Oblivion website project.
