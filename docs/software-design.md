qq# PizzaSolution Software Design

This document describes the key architectural patterns used in the PizzaSolution project, their general purpose, and their pros/cons specific to this implementation.

---

## Table of Contents

1. [Service Layer Pattern](#1-service-layer-pattern)
2. [Repository Pattern](#2-repository-pattern)
3. [Factory Pattern](#3-factory-pattern)
4. [Dependency Injection](#4-dependency-injection)
5. [Controller Pattern](#5-controller-pattern)
6. [Value Objects (Records)](#6-value-objects-records)
7. [Custom Collection (ComparableList)](#7-custom-collection-comparablelist)

---

## 1. Service Layer Pattern

### General Use
The Service Layer pattern separates business logic from presentation and data access concerns. Services contain core application logic and orchestrate operations across repositories and other services. They provide a clean API for controllers and other consumers.

### Project Implementation
- **OrderingService** - Handles pizza order processing, coordinates stock validation and pizza preparation
- **StockService** - Manages inventory checks and stock calculations
- **RecipeService** - Provides access to pizza recipes
- **MenuService** - Handles menu operations

Each service implements an interface (e.g., `IOrderingService`) enabling loose coupling and testability.

### Pros
| Pro | Description |
|-----|-------------|
| **Separation of concerns** | Business logic is isolated from controllers and repositories |
| **Testability** | Services can be easily unit tested with mock dependencies |
| **Reusability** | Services can be consumed by multiple controllers or clients |
| **Maintainability** | Changes to business logic are isolated to service classes |

### Cons
| Con | Description |
|-----|-------------|
| **Additional abstraction** | Extra interface/class for every service adds complexity |
| **Potential over-abstraction** | Small projects may not need this level of separation |
| **Dependency management** | Services can become coupled to many dependencies |

---

## 2. Repository Pattern

### General Use
The Repository pattern abstracts data access, providing a clean interface for retrieving and persisting domain objects. It hides the details of the underlying data store (database, API, file system) from the rest of the application.

### Project Implementation
- **IRecipeRepository** / **RecipeRepository** - Access to pizza recipes (abstracted interface with placeholder implementation)
- **IStockRepository** / **StockRepository** - Access to inventory data
- **FakeRecipeRepository** / **FakeStockRepository** - In-memory implementations for testing

### Pros
| Pro | Description |
|-----|-------------|
| **Abstraction** | Data source can be swapped without changing business logic |
| **Testability** | Fake repositories enable isolated unit testing |
| **Single responsibility** | Data access logic is centralized |
| **Test doubles** | Easy to create in-memory fakes for testing |

### Cons
| Con | Description |
|-----|-------------|
| **Interface overhead** | Each repository needs an interface and implementation |
| **Real implementations missing** | RecipeRepository and StockRepository throw NotImplementedException - only fake implementations exist |
| **Potential for anemia** | Risk of repositories becoming just CRUD wrappers without domain logic |

---

## 3. Factory Pattern

### General Use
The Factory pattern encapsulates object creation logic, providing an interface for creating objects without specifying their exact classes. This is useful when object creation involves complex logic or when you need multiple implementations.

### Project Implementation
- **IPizzaOven** - Interface for pizza cooking operations
- **PizzaOven** - Base abstract class with common oven logic
- **NormalPizzaOven** - Sequential pizza production (capacity: 4)
- **GiantRevolvingPizzaOven** - Large batch cooking (capacity: 120)
- **AssemblyLinePizzaOven** - Optimized for repeated same-type pizzas (setup + per-unit time)

### Pros
| Pro | Description |
|-----|-------------|
| **Flexible implementation** | Swap ovens at runtime (e.g., Normal vs AssemblyLine) |
| **Encapsulated creation** | Complex cooking logic is hidden from consumers |
| **Open/Closed principle** | Add new oven types without modifying existing code |
| **Testing** | Easy to mock different oven behaviors |

### Cons
| Con | Description |
|-----|-------------|
| **Class explosion** | Each oven variant requires a separate class |
| **Complexity** | Base class inheritance adds complexity |
| **Not fully implemented** | GiantRevolvingPizzaOven and AssemblyLinePizzaOven throw NotImplementedException |
| **Over-engineering risk** | For simple use cases, a single implementation may suffice |

---

## 4. Dependency Injection

### General Use
Dependency Injection (DI) is a technique where dependencies are provided to a class rather than being created by it. In .NET, this is typically implemented via constructor injection with a DI container that manages object lifecycle.

### Project Implementation
- **Primary constructors** - All services and controllers use primary constructor syntax for DI
- **Program.cs** - Registers all services, repositories, and factories with the DI container

Example from `OrderingService`:
```csharp
public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven) : IOrderingService
```

### Pros
| Pro | Description |
|-----|-------------|
| **Loose coupling** | Classes depend on abstractions, not concretions |
| **Testability** | Easy to inject mocks/fakes in tests |
| **Lifecycle management** | Container manages singleton/transient scopes |
| **Modern .NET support** | Native support via Microsoft.Extensions.DependencyInjection |

### Cons
| Con | Description |
|-----|-------------|
| **Learning curve** | Understanding scopes and lifetimes requires knowledge |
| **Debugging complexity** | Tracing through multiple abstraction layers can be difficult |
| **Implicit dependencies** | Constructor doesn't show dependency requirements at a glance |
| **Over-injection** | Classes can become "god classes" with too many dependencies |

---

## 5. Controller Pattern

### General Use
The Controller pattern in ASP.NET Core handles HTTP requests, maps them to business logic, and returns HTTP responses. Controllers are the entry points for API requests.

### Project Implementation
- **OrderingController** - POST /api/order - Processes pizza orders
- **MenuController** - Handles menu-related endpoints
- **RestockingController** - Manages inventory restocking
- **WelcomeController** - Basic welcome endpoint

### Pros
| Pro | Description |
|-----|-------------|
| **Request/response isolation** | HTTP concerns separated from business logic |
| **Attribute routing** | Clear URL mapping via [Route] and [HttpPost] |
| **Action result flexibility** - | Returns IActionResult for flexible HTTP responses |
| **Integration with DI** | Controllers can inject services directly |

### Cons
| Con | Description |
|-----|-------------|
| **Thin controller risk** | Controllers can become pass-throughs without logic |
| **Multiple endpoints** | Can lead to many small controller classes |
| **Testing complexity** | Full integration tests needed for HTTP layer |

---

## 6. Value Objects (Records)

### General Use
Value objects are immutable types that are defined by their values rather than identity. In C#, `record` types provide built-in equality based on property values rather than reference.

### Project Implementation
- **PizzaOrder** - Immutable order container with requested pizzas
- **PizzaAmount** - Pizza type and quantity pair
- **PizzaRecipeDto** - Recipe data with ingredients and cooking time
- **Pizza** (abstract) - Base for concrete pizza types
- **StockDto** - Stock type and amount

### Pros
| Pro | Description |
|-----|-------------|
| **Immutability** | Records are inherently immutable, thread-safe |
| **Value equality** | Two records with same values are equal |
| **Pattern matching** - | Records support deconstruction and pattern matching |
| **Less boilerplate** | Much less code than equivalent class with Equals/HashCode |

### Cons
| Con | Description |
|-----|-------------|
| **Performance** | Value equality checks can be slower than reference equality for large objects |
| **Mutation required** - | Creating "modified" versions requires new instances |
| **Learning curve** | Understanding when to use records vs classes requires experience |

---

## 7. Custom Collection (ComparableList)

### General Use
A custom collection provides specialized behavior not available in standard collections. `ComparableList<T>` provides value-based equality for collections - two lists are equal if they contain the same elements in the same order.

### Project Implementation
```csharp
public class ComparableList<T> : IList<T>
{
    public override bool Equals(object? obj) =>
        obj is ComparableList<T> otherList &&
        Count == otherList.Count &&
        _list.Zip(otherList).All(x => x.First?.Equals(x.Second) ?? x.Second is null);
}
```

### Pros
| Pro | Description |
|-----|-------------|
| **Value equality for collections** | Collections can be compared by content, not reference |
| **Test assertions** | Easy to assert expected vs actual collections in tests |
| **Immutability-friendly** | Works well with immutable record types |

### Cons
| Con | Description |
|-----|-------------|
| **Custom code** | Requires maintaining custom collection type |
| **Performance** | Element-by-element comparison is O(n) vs O(1) reference check |
| **Limited functionality** | Inherits from IList<T> but doesn't override all LINQ methods |
| **Alternative exists** | Could potentially use built-in immutable collections instead |

---

## Summary

| Pattern | Primary Benefit | Primary Risk |
|---------|-----------------|--------------|
| Service Layer | Business logic isolation | Over-abstraction |
| Repository | Data source abstraction | Real implementations missing |
| Factory | Flexible object creation | Class explosion |
| Dependency Injection | Loose coupling & testability | Complexity |
| Controller | HTTP request handling | Thin controller syndrome |
| Records (Value Objects) | Immutability & equality | Performance overhead |
| ComparableList | Value-based collection equality | Custom code maintenance |

The PizzaSolution demonstrates solid architectural practices with clear separation of concerns, dependency injection throughout, and appropriate use of interfaces and abstractions. Key areas for improvement include completing the NotImplementedException stubs in Repository and Factory implementations and evaluating whether ComparableList provides sufficient benefit over built-in alternatives.