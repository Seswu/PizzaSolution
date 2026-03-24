# Sequence Diagrams - PizzaSolution

## Use Case 1: Place Pizza Order

```mermaid
sequenceDiagram
    participant Client
    participant Controller as OrderingController
    participant Service as OrderingService
    participant Stock as StockService
    participant Recipe as RecipeService
    participant Repo as FakeRecipeRepository
    participant StockRepo as FakeStockRepository
    participant Oven as PizzaOven

    Client->>Controller: POST /api/order<br/>{requestedOrder: [...]}
    Controller->>Service: HandlePizzaOrder(pizzaOrder)
    
    rect rgb(240, 248, 255)
        Note over Service,Recipe: Get pizza recipes
        Service->>Recipe: GetPizzaRecipes(order)
        Recipe->>Repo: GetRecipe(StandardPizza)
        Repo-->>Recipe: PizzaRecipeDto
        Recipe->>Repo: GetRecipe(ExtremelyTastyPizza)
        Repo-->>Recipe: PizzaRecipeDto
        Recipe-->>Service: ComparableList<PizzaRecipeDto>
    end
    
    rect rgb(255, 250, 240)
        Note over Service,Stock: Check stock availability
        Service->>Stock: HasInsufficientStock(order, recipes)
        Stock->>StockRepo: GetStock(Dough)
        StockRepo-->>Stock: StockDto
        Stock->>StockRepo: GetStock(Tomatoes)
        StockRepo-->>Stock: StockDto
        Stock->>StockRepo: GetStock(Cheese)
        StockRepo-->>Stock: StockDto
        Stock-->>Service: false (sufficient)
    end
    
    rect rgb(240, 255, 240)
        Note over Service,Stock: Get current stock
        Service->>Stock: GetStock(order, recipes)
        Stock->>StockRepo: GetStock(...)
        StockRepo-->>Stock: ComparableList<StockDto>
        Stock-->>Service: ComparableList<StockDto>
    end
    
    rect rgb(255, 245, 238)
        Note over Service,Oven: Prepare pizzas
        Service->>Oven: PreparePizzas(prepareOrders, stock)
        Oven->>Oven: Validate stock
        Oven->>Oven: PlanPizzaMaking()
        Oven->>Oven: Cook pizzas
        Oven-->>Service: IEnumerable<Pizza>
    end
    
    Service-->>Controller: IEnumerable<Pizza>
    Controller-->>Client: 200 OK<br/>[Pizza, Pizza, ...]
```

## Use Case 2: Get Menu

```mermaid
sequenceDiagram
    participant Client
    participant Controller as MenuController
    participant Service as MenuService
    participant Time as TimeProvider

    Client->>Controller: GET /api/menu
    Controller->>Time: GetUtcNow()
    Time-->>Controller: DateTimeOffset
    Controller->>Service: GetMenu(menuDate)
    
    alt Time between 11:00 and 15:00
        Service-->>Controller: Menu (Lunch Menu)
    else
        Service-->>Controller: Menu (Standard Menu)
    end
    
    Controller-->>Client: 200 OK<br/>{title: "...", items: [...]}
```

## Use Case 3: Stock Management (Check & Take)

```mermaid
sequenceDiagram
    participant Service as StockService
    participant Repo as FakeStockRepository

    Service->>Repo: GetStock(StockType.Dough)
    Repo-->>Service: StockDto (amount: 100)
    
    Service->>Repo: TakeStock(StockType.Dough, 10)
    Repo->>Repo: Find existing stock
    Repo->>Repo: Decrement amount
    Repo->>Repo: Update in database
    Repo-->>Service: StockDto (amount: 90)
    
    Service->>Repo: AddToStock(StockType.Dough, 50)
    Repo->>Repo: Find existing stock
    Repo->>Repo: Increment amount
    Repo->>Repo: Update in database
    Repo-->>Service: StockDto (amount: 140)
```

---

## Pizza Order & Completion Flow

```mermaid
sequenceDiagram
    participant Client
    participant API as OrderingController
    participant Order as OrderingService
    participant Recipe as RecipeService
    participant Stock as StockService
    participant Oven as PizzaOven
    participant StockRepo as FakeStockRepository
    participant RecipeRepo as FakeRecipeRepository

    Note over Client,Oven: Complete Pizza Order Flow

    rect rgb(230, 245, 255)
        Note over Client,API: Step 1: Receive Order
        Client->>API: POST /api/order<br/>{requestedOrder: [{pizzaType: StandardPizza, amount: 2}]}
        API->>API: Validate model
        API->>Order: HandlePizzaOrder(pizzaOrder)
    end

    rect rgb(240, 248, 255)
        Note over Order,Recipe: Step 2: Get Recipes
        Order->>Recipe: GetPizzaRecipes(order)
        Recipe->>Recipe: GetDistinctPizzaTypes()
        loop For each pizza type
            Recipe->>RecipeRepo: GetRecipe(pizzaType)
            RecipeRepo-->>Recipe: PizzaRecipeDto
        end
        Recipe-->>Order: ComparableList<PizzaRecipeDto>
    end

    rect rgb(255, 250, 240)
        Note over Order,Stock: Step 3: Validate Stock
        Order->>Stock: HasInsufficientStock(order, recipes)
        Stock->>Stock: CalculateRequiredIngredients()
        loop For each ingredient
            Stock->>StockRepo: GetStock(ingredientType)
            StockRepo-->>Stock: StockDto
            Stock->>Stock: CheckSufficiency(required, available)
        end
        alt Any ingredient insufficient
            Stock-->>Order: true
            Order-->>API: Throw PizzaException
            API-->>Client: 400 Bad Request<br/>"Insufficient stock"
        else All ingredients sufficient
            Stock-->>Order: false
        end
    end

    rect rgb(240, 255, 240)
        Note over Order,Stock: Step 4: Reserve Stock
        Order->>Stock: GetStock(order, recipes)
        loop For each ingredient
            Stock->>StockRepo: GetStock(ingredientType)
            StockRepo-->>Stock: StockDto
        end
        Stock-->>Order: ComparableList<StockDto>
        Order->>Stock: TakeStock(ingredient, amount)
        Stock->>StockRepo: TakeStock(ingredient, amount)
        StockRepo-->>Stock: Updated StockDto
    end

    rect rgb(255, 245, 238)
        Note over Order,Oven: Step 5: Prepare Pizzas
        Order->>Oven: PreparePizzas(prepareOrders, stock)
        
        Oven->>Oven: Validate stock availability
        Oven->>Oven: CreatePizzaMakingOrders()
        
        rect rgb(255, 240, 245)
            Note over Oven: Cooking Process
            Oven->>Oven: PlanPizzaMaking()
            Oven->>Oven: Initialize cooking queue
            loop While pizzas remaining
                Oven->>Oven: Get next from queue
                Oven->>Oven: Prepare dough
                Oven->>Oven: Add toppings
                Oven->>Oven: Bake
                Oven->>Oven: Mark complete
            end
        end
        
        Oven-->>Order: IEnumerable<Pizza>
    end

    rect rgb(230, 255, 230)
        Note over Order,API: Step 6: Return Result
        Order-->>API: IEnumerable<Pizza>
        API-->>Client: 200 OK<br/>[{type: StandardPizza}, {type: StandardPizza}]
    end
```

## Detailed PizzaOven Preparation Flow

```mermaid
sequenceDiagram
    participant Order as OrderingService
    participant Oven as PizzaOven
    participant Stock as ComparableList<StockDto>

    Order->>Oven: PreparePizzas(prepareOrders, stock)
    
    rect rgb(245, 255, 250)
        Note over Oven: Step A: Validation
        Oven->>Oven: ValidateStock()
        loop For each order
            Oven->>Oven: HasEnoughStock(order, stock)
        end
    end

    rect rgb(250, 245, 255)
        Note over Oven: Step B: Planning
        Oven->>Oven: CreatePizzaMakingOrders()
        Oven->>Oven: Assign order IDs
    end

    rect rgb(255, 245, 250)
        Note over Oven: Step C: Cooking
        Oven->>Oven: PlanPizzaMaking()
        Oven->>Oven: AddToQueue(orders)
        
        loop While queue not empty
            Oven->>Oven: GetNextFromQueue()
            Oven->>Oven: CookPizza()
            Oven->>Oven: Take from oven
            Oven->>Oven: Mark as complete
        end
    end

    rect rgb(245, 250, 255)
        Note over Oven: Step D: Completion
        Oven->>Oven: GetCompletedPizzas()
        Oven-->>Order: IEnumerable<Pizza>
    end
```
