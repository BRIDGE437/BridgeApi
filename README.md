# Bridge API

Bridge API is a robust backend solution built with **ASP.NET Core**, adhering to **Clean Architecture** principles. It serves as the central data and business logic layer for the Bridge application ecosystem.

## 🚀 Key Features & Technologies
- **.NET 8** (C#)
- **Clean Architecture** (Separation of Domain, Application, Infrastructure, and Presentation)
- **Caching:** Distributed Redis Caching integrated with Docker
- **Logging & Monitoring:** Serilog & Seq for advanced, structured log handling
- **Real-time Communication:** SignalR for instant updates
- **Security & Performance:** Strict rate limiting, and Role-based JWT Authentication
- **Data & Storage:** Entity Framework Core (SQL) and custom File Storage operations

## 📂 Project Structure

```text
BridgeApi/
├── Core
│   ├── BridgeApi.Domain (Core Entities, Enums, Exceptions)
│   └── BridgeApi.Application (CQRS, Interfaces, DTOs, Task/Business Rules)
├── Infrastructure
│   ├── BridgeApi.Infrastructure (External Services, File Storage System)
│   ├── BridgeApi.Persistence (Entity Framework Subsystem, DbContext, Migrations)
│   └── BridgeApi.RealtimeCommunication (SignalR endpoints)
└── Presentation
    └── BridgeApi.API (Controllers, Middlewares, API Endpoints)
```

## 🛠 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) *(for local Redis and Seq instances)*

### Running Locally
1. Run the necessary backing services (e.g., Redis) using Docker Compose located at the root of the project:
   ```bash
   docker-compose up -d
   ```
2. Navigate to the API running directory:
   ```bash
   cd Presentation/BridgeApi.API
   ```
3. Update the database schema with Entity Framework:
   ```bash
   dotnet ef database update --project ../../Infrastructure/BridgeApi.Persistence
   ```
4. Start the application:
   ```bash
   dotnet run
   ```

*(Ensure you have set the appropriate connection strings in your `appsettings.Development.json` prior to execution.)*

---
*Maintained by the Bridge Project Team.*
