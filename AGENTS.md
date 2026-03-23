# AGENTS.md - PizzaSolution

This document provides context for AI coding agents working in this repository.

## Project Structure

```
/home/uw/solo/repositories/public/PizzaSolution
├── PizzaSolution.sln           # Solution file
├── PizzaPlace/                 # Main ASP.NET Core project
│   ├── Controllers/            # API controllers
│   ├── Models/                 # DTOs and domain models
│   │   └── Types/              # Enum types
│   ├── Repositories/           # Data access layer
│   ├── Services/               # Business logic services
│   ├── Factories/              # Factory pattern implementations
│   ├── Pizzas/                 # Pizza domain objects
│   └── Extensions/             # Extension methods
└── Tests/
    └── PizzaPlace.Test/        # MSTest project
```

## Build & Test Commands

### Build
```bash
dotnet build
```

### Run All Tests
```bash
dotnet test
```

### Run Single Test (by fully qualified name)
```bash
dotnet test --filter "FullyQualifiedName~OrderingServiceTests.HandlePizzaOrder"
```

### Run Single Test (by test method name)
```bash
dotnet test --filter "Name=HandlePizzaOrder"
```

### Verbose Test Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Code Style Guidelines

### General
- Target Framework: .NET 8.0
- Nullable reference types enabled
- Implicit usings enabled
- File-scoped namespaces preferred

### Naming Conventions
- **Types/Methods/Properties**: PascalCase (`OrderingService`)
- **Local variables**: camelCase (`order`, `pizzas`)
- **Interfaces**: Prefix with `I` (`IOrderingService`)
- **Records**: Suffix with meaningful name (`PizzaOrder`, `PizzaAmount`)

### File Organization
```csharp
using PizzaPlace.Factories;
using PizzaPlace.Models;

namespace PizzaPlace.Services;

public class OrderingService { }
```

### Using Statements
- System namespaces first, then project namespaces
- Use explicit namespaces (no global aliases unless in GlobalUsings.cs)

### Types & Records
- Use `record` for immutable DTOs
- Use `class` for services and controllers
- Use primary constructors for DI

```csharp
public record PizzaOrder(ComparableList<PizzaAmount> RequestedOrder);

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService) : IOrderingService { }
```

### Collections
- Use `ComparableList<T>` instead of `List<T>` for equality comparison (provides value-based equality for collections)
- Use collection expressions `[]` for initialization
- Convert to/from with `.ToComparableList()` and LINQ

### Error Handling
- Throw custom `PizzaException` for domain errors
```csharp
throw new PizzaException("Unable to take in order. Insufficient stock.");
```

### Testing Patterns
- Use MSTest with `[TestClass]` and `[TestMethod]`
- Use Moq with `MockBehavior.Strict`
- Use `VerifyAll()` to verify mock expectations
- Follow Arrange/Act/Assert structure

### Controllers
- Use attribute routing `[Route("api/order")]`
- Use `ControllerBase` with `IActionResult` returns
- Inject services via primary constructors

### Global Usings
Pre-imported: System.Collections, System.Linq, System.Text.Json

## Domain Model Types

### Enums
- `PizzaRecipeType`: StandardPizza, ExtremelyTastyPizza
- `StockType`: Dough, Tomatoes, UnicornDust, BellPeppers, Anchovies

### Key DTOs
- `PizzaOrder`: Contains requested pizzas
- `PizzaAmount`: PizzaType + Amount
- `PizzaRecipeDto`: Recipe with ingredients and prep time
- `StockDto`: Stock type and quantity
- `Pizza`: Abstract record for pizza instances

### Services
- `IOrderingService`: Handles pizza orders
- `IStockService`: Manages inventory
- `IRecipeService`: Provides pizza recipes
- `IMenuService`: Menu operations

## Common Patterns

### Repository Pattern
```csharp
public interface IRecipeRepository
{
    Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType);
}
```

### Factory Pattern
```csharp
public interface IPizzaOven
{
    Task<IEnumerable<Pizza>> PreparePizzas(ComparableList<PizzaPrepareOrder> orders, ComparableList<StockDto> stock);
}
```

## Notes
- Fake implementations exist in `Repositories/` for testing
- Custom `ComparableList<T>` provides equality comparison
- Project uses NSwag for API documentation
