# Cursed Into Oblivion Website

The official website and management system for **Cursed Into Oblivion**.

This project combines a modern public-facing website with an internal administration panel used to manage the band's concerts, merchandise, Electronic Press Kit (EPK), sales, and other day-to-day activities.

Built with **ASP.NET Core Blazor** and **PostgreSQL**, the application is designed to be lightweight, maintainable, and easy to expand as the band grows.

---

## Features

### Public Website

- Modern landing page
- Electronic Press Kit (EPK)
- Band information
- Image galleries
- Stage riders
- Concert calendar
- Cookie consent
- Cookie policy
- Responsive design

### Administration Panel

- Secure authentication
- Dashboard
- Concert management
- Calendar management
- Merchandise management
- Sales tracking
- Sales statistics
- User management
- Protected administration pages

---

## Built With

- ASP.NET Core Blazor (.NET 8)
- C#
- Razor Components
- PostgreSQL
- Dapper
- Npgsql
- HTML5
- CSS3
- JavaScript

---

## Architecture

The project follows a simple layered architecture:

```
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
├── Images
├── CSS
└── JavaScript
```

Business logic is separated into service classes responsible for handling database operations and application logic.

---

## Database

The application uses **PostgreSQL** as its primary database.

Data access is implemented using:

- Dapper
- Npgsql
- Dependency Injection

Current modules include:

- Users
- Concerts
- Calendar Events
- Products
- Sales
- EPK Access

---

## Authentication

The admin panel uses a custom authentication system with protected routes.

Features include:

- Login
- Logout
- Authorization
- Role-based access

Only authenticated users can access the administration area.

---

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- PostgreSQL

### Clone the repository

```bash
git clone https://github.com/yourusername/CiOHjemmeside.git
```

### Configure the database

Update your `appsettings.json` with your PostgreSQL connection string.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CiO;Username=postgres;Password=yourpassword"
  }
}
```

### Run the project

```bash
dotnet run
```

or start it directly from Visual Studio.

---

## Roadmap

Planned features include:

- Online merchandise store
- Payment integration
- Inventory management
- Booking request system
- News section
- Media gallery improvements
- Additional analytics
- Expanded role management

---

## Screenshots

Screenshots will be added as the project evolves.

---

## Contributing

This is currently a personal project developed for **Cursed Into Oblivion**, but suggestions and feedback are always welcome.

---

## License

This project is licensed under the MIT License unless stated otherwise.

---

## Author

**Bjarke Andersen**

Developer, singer, and founder of the Cursed Into Oblivion website project.
