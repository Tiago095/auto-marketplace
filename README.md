<img src="https://capsule-render.vercel.app/api?type=waving&color=0:1a1a1a,100:6e6e6e&height=200&section=header&text=AutoMatch&fontSize=52&fontColor=ffffff&animation=fadeIn&fontAlignY=35&desc=Used%20Vehicle%20Marketplace&descAlignY=55&descSize=16" width="100%"/>

<div align="center">

![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white)
![EF Core](https://img.shields.io/badge/Entity%20Framework-Core-68217A?logo=nuget&logoColor=white)
![Status](https://img.shields.io/badge/Status-Completed-brightgreen)

</div>

> **Course:** Web Applications and Databases Lab (LEI)
> **Institution:** University of Trás-os-Montes e Alto Douro (UTAD)

## Authors

| Name | GitHub |
|---|---|
| Tiago Ribeiro | [@Tiago](https://github.com/Tiago095) |
| Francisco Rodrigues | [@Francisco](https://github.com/FranciscoSR-LEI) |
| Pedro Vieira | [@Pedro](https://github.com/Pedro-Vieira555) |

---

## About the Project

**AutoMatch** is a web application developed for the Web Applications and Databases Lab course. It consists of a **used vehicle marketplace** portal, enabling interaction between different types of users (Buyers, Sellers, and Administrators) for buying, selling, reserving, and visiting cars.

The project was developed across three main phases:

1. **Phase 1: Analysis and Specification** — Functional/Non-Functional Requirements, Conceptual E-R Model, and Functional Model/Use Cases.
2. **Phase 2: Logical and Physical Modeling** — Relational Model, SQL Server Script, Database Diagram, and Figma Mockups.
3. **Phase 3: Practical Implementation** — MVC Architecture in ASP.NET Core, EF Core Integration, Business Logic, Backoffice, and `DbInitializer`.

---

## Main Features by Role

### Unauthenticated Users
- Account registration and login
- Access to platform information ("About Us" and Help)
- Advanced vehicle search with multiple filters (make, model, price, year, mileage, fuel type, transmission, etc.)
- Detailed vehicle listing view

### Buyers
- Personal profile management (updating data and account deletion)
- Notification subscriptions and favorite brand preferences
- Vehicle reservation and visit/test-drive scheduling
- Purchase checkout simulation (cost calculation, fees, insurance, and payment plans)
- Direct communication with sellers through the messaging system
- Access to digital vehicle documents after purchase and order status tracking

### Sellers
- Seller application (subject to administrative approval)
- Creation, editing, pausing, and removal of vehicle listings, including photo and encrypted document management
- Setting and updating listing status (active, reserved, sold, paused)
- Access to reserved/sold vehicle listings and sales statistics

### Administrators (Backoffice)
- Reinforced authentication and global platform management
- Viewing and updating user profiles, account blocking/activation
- Content moderation and listing report management
- Review and approval/rejection of seller applications
- Access to global statistics (user growth, listings overview, and activity reports)

---

## Architecture and Technologies

| Technology | Description |
|---|---|
| **ASP.NET Core** | Web framework, Model-View-Controller (MVC) architecture |
| **Microsoft SQL Server** | Database management system |
| **Entity Framework Core** | Object-Relational Mapping (ORM), with Migrations support |

---

## Test Credentials

The application includes an automatic database initializer (`DbInitializer`) that seeds the system with test data and pre-configured users:

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@gmail.com` | `password123` |
| Regular User | `user@gmail.com` | `password123` |
| Seller 1 | `seller1@gmail.com` | `password123` |
| Seller 2 | `seller2@gmail.com` | `password123` |

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:1a1a1a,100:6e6e6e&height=100&section=footer&animation=fadeIn&reversal=true" width="100%"/>
