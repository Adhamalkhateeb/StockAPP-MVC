# StockApp - Stock Trading Management System

A comprehensive ASP.NET Core MVC application for managing stock trading operations. The application provides real-time stock data integration, buy/sell order management, and portfolio tracking with advanced features like PDF report generation and comprehensive logging.

## 🌟 Features

- **Real-time Stock Data**: Integration with Finnhub API for live market data
- **Order Management**: Create, view, and manage buy/sell orders
- **Portfolio Tracking**: Monitor your stock holdings and trading history
- **Advanced Search**: Search for stocks by symbol or company name
- **PDF Reports**: Generate and download trading reports
- **Comprehensive Logging**: Full request/response logging with Serilog
- **Data Validation**: FluentValidation for robust input validation
- **Responsive UI**: Clean and intuitive user interface
- **Security**: Anti-forgery tokens and secure request handling
- **Testing**: Comprehensive unit and integration tests

## 🏗️ Architecture

The application follows a layered architecture pattern:

```
StockApp/
├── Core/                    # Core DTOs, Validators, Exceptions, Extensions
├── Entities/                # Entity models and DbContext
├── Repositories/            # Data access layer implementations
├── RepositoryContracts/     # Repository interface contracts
├── Services/                # Business logic and service implementations
├── ServiceContracts/        # Service interface contracts
├── StocksApp/              # ASP.NET Core MVC web application
└── Tests/                   # Unit and integration tests
```

### Project Dependencies

- **StocksApp**: Main web application
  - References: Core, Entities, Repositories, RepositoryContracts, ServiceContracts, Services
  - Key Dependencies: EntityFrameworkCore.Design, Rotativa.AspNetCore, Serilog, Rotativa

- **Entities**: Data models and EF Core DbContext
  - References: None
  - Dependencies: EntityFrameworkCore.SqlServer

- **Repositories**: Data access implementations
  - References: RepositoryContracts
  - Dependencies: EntityFrameworkCore

- **Services**: Business logic implementations
  - References: ServiceContracts, RepositoryContracts
  - Dependencies: FluentValidation

- **Core**: Common utilities and validators
  - References: None
  - Dependencies: FluentValidation

- **Tests**: Comprehensive test coverage
  - References: All projects
  - Dependencies: Xunit, Moq, FluentAssertions, HtmlAgilityPack

## 🚀 Getting Started

### Prerequisites

- **.NET 10.0** or higher
- **SQL Server LocalDB** (included with Visual Studio)
- **Visual Studio 2022+** or Visual Studio Code with C# extension

### Local Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd StockApp
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Set up user secrets** (required for API key)
   ```bash
   # Initialize user secrets for the StocksApp project
   cd StocksApp
   dotnet user-secrets init
   
   # Add your Finnhub API key
   dotnet user-secrets set "FinnhubToken" "your_finnhub_api_key"
   
   # Verify the secret was set
   dotnet user-secrets list
   ```

4. **Create and seed the database**
   ```bash
   # Navigate to the Entities project directory
   cd ..\Entities
   
   # Apply migrations
   dotnet ef database update --startup-project ..\StocksApp\StocksApp.csproj
   ```

5. **Configure Serilog (Optional)**
   - The application uses LocalDB for Serilog logs by default
   - For remote logging, configure Seq server URL in `appsettings.json`

6. **Run the application**
   ```bash
   cd ..\StocksApp
   dotnet run
   ```

The application will start at `https://localhost:5001`

## 🔐 Security

### API Key Management

- **Finnhub Token**: Stored securely using .NET User Secrets (never committed to version control)
- Set via: `dotnet user-secrets set "FinnhubToken" "your_key"`
- Accessed at runtime from `IConfiguration["FinnhubToken"]`

### Database Connection

- Uses **Windows Authentication** with LocalDB for local development
- Connection strings are environment-specific (stored in `appsettings.json`)
- Supports configuration override via environment variables or secrets

## 📊 Configuration

### appsettings.json

```json
{
  "TradingOptions": {
    "DefaultStockSymbol": "MSFT",
    "DefaultOrderQuantity": 100,
    "PopularStocks": ["AAPL", "MSFT", "AMZN", ...],
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StockApp;..."
    }
  }
}
```

### Environment-Specific Settings

- **Development**: Full logging, developer exception page
- **Test**: In-memory database, mocked external services
- **Production**: Optimized logging, error handling middleware

## 🧪 Testing

The project includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Tests.csproj

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Test Categories

- **Unit Tests**: Business logic and service layer testing
- **Integration Tests**: End-to-end feature testing with test database
- **Controller Tests**: MVC controller action testing

## 📝 Database

### Schema

- **BuyOrders**: Records of stock purchases
- **SellOrders**: Records of stock sales
- **Logs** (optional): Serilog audit trail

### Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --startup-project StocksApp

# Update database
dotnet ef database update --startup-project StocksApp

# Remove last migration
dotnet ef migrations remove --startup-project StocksApp
```

## 📚 API Integration

### Finnhub API

The application integrates with Finnhub for:
- Stock quotes and price data
- Company profile information
- Stock symbol searches
- Market symbol listings

**Note**: Requires valid Finnhub API key (free tier available at [finnhub.io](https://finnhub.io))

### External APIs

- **Yahoo Finance**: Historical stock price data
- **Finnhub**: Real-time market data

## 🔍 Code Structure

### Controllers
- `TradeController`: Main trading interface and order management

### Services
- **Finnhub Services**: Real-time market data retrieval
- **Stock Services**: Order processing and portfolio management

### Repositories
- **FinnhubRepository**: External API integration
- **StocksRepository**: Database operations for orders

### Models
- Buy/Sell Order Request/Response DTOs
- Company Profile and Stock Quote models
- Trading configuration options

## 📦 Dependencies

Key NuGet packages:
- **ASP.NET Core 10.0**: Web framework
- **Entity Framework Core 10.0.3**: ORM for SQL Server
- **Serilog 10.0.0**: Structured logging
- **FluentValidation 11.x**: Data validation
- **Rotativa 1.4.0**: PDF generation
- **Xunit & Moq**: Testing frameworks

## 🤝 Contributing

When contributing to this project:

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Follow existing code style and patterns
3. Add or update tests as needed
4. Ensure all tests pass: `dotnet test`
5. Commit with clear messages
6. Push and create a pull request

## 📄 License

This project is provided as-is for educational and portfolio purposes.

**Last Updated**: Feb 2026  
**Framework**: ASP.NET Core 10.0  
**Database**: SQL Server LocalDB
