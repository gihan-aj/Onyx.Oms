Onyx.Oms is a modern Order Management System (OMS) built with .NET 10 and Clean Architecture principles. It provides a robust foundation for managing customers, orders, and products with features like API versioning, CQRS, and Domain-Driven Design.

## 🚀 Features

- **Modern .NET 10**: Built on the latest .NET platform with top-tier performance.
- **Clean Architecture**: Enforces separation of concerns with distinct layers (Domain, Application, Infrastructure, Web).
- **CQRS Pattern**: Command Query Responsibility Segregation for optimized data flow.
- **API Versioning**: Built-in support for API versioning using Asp.Versioning.
- **Domain-Driven Design**: Rich domain entities with value objects and result pattern.
- **Minimal APIs**: High-performance API endpoints with Swagger/OpenAPI documentation.
- **Fluent Validation**: Robust request validation using FluentValidation.
- **MediatR**: In-process messaging for seamless command and query handling.

## 📂 Project Structure

```
Onyx.Oms/
├── src/
│   ├── Onyx.Oms.Core/          # Domain entities, interfaces, and base types
│   ├── Onyx.Oms.Application/   # Application logic, CQRS handlers, and validators
│   ├── Onyx.Oms.Infrastructure/ # EF Core DbContext, repositories, and migrations
│   └── Onyx.Oms.Web/           # API endpoints, configuration, and startup
├── tests/
│   ├── Onyx.Oms.UnitTests/     # Unit tests for domain and application layers
│   └── Onyx.Oms.IntegrationTests/ # Integration tests for the application
└── docs/                       # Documentation and API specifications
```

## 🛠️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Docker for SQL Server)
- [Visual Studio 2024](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Onyx.Oms
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   Update `appsettings.json` in the `Onyx.Oms.Web` project with your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=your-server;Database=OnyxOms;User Id=your-user;Password=your-password;"
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef migrations add InitialCreate --project src/Onyx.Oms.Infrastructure --startup-project src/Onyx.Oms.Web
   dotnet ef database update --project src/Onyx.Oms.Infrastructure --startup-project src/Onyx.Oms.Web
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Onyx.Oms.Web
   ```

## 🧪 Running Tests

### Unit Tests

```bash
dotnet test tests/Onyx.Oms.UnitTests
```

### Integration Tests

```bash
dotnet test tests/Onyx.Oms.IntegrationTests
```

## 📚 Documentation

- [API Documentation](docs/api/swagger.html)
- [Architecture Overview](docs/architecture.md)
- [Domain Model](docs/domain.md)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.