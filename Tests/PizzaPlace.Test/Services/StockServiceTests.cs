using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTests
{
    private static StockService GetService(IStockRepository stockRepository) =>
        new(stockRepository);

    // PURPOSE: Verifies that an order with sufficient stock for all ingredients returns false (not insufficient).
    // ASSUMPTION: The method aggregates ingredient requirements from all pizzas in the order,
    //             multiplies each ingredient by the quantity of pizzas, and checks availability.
    // EXPECTATION: When all ingredients are available in sufficient quantities,
    //              HasInsufficientStock returns false, allowing the order to proceed.
    [TestMethod]
    public async Task HasInsufficientStock_SufficientStock_ReturnsFalse()
    {
        // Arrange
        // Create an order for 2 StandardPizzas
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 2)
        });

        // StandardPizza requires: 2 Dough + 1 Tomatoes per pizza
        // Total needed: 4 Dough + 2 Tomatoes
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        // Setup repository to return sufficient stock for all ingredients
        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10)); // Have 10, need 4
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 5)); // Have 5, need 2

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that an order with insufficient stock for any ingredient returns true.
    // ASSUMPTION: The method checks ALL ingredients; running low on even one ingredient
    //             should cause the entire order to be rejected.
    // EXPECTATION: When any single ingredient is insufficient, HasInsufficientStock returns true,
    //              preventing the order from being accepted.
    [TestMethod]
    public async Task HasInsufficientStock_InsufficientStock_ReturnsTrue()
    {
        // Arrange
        // Create an order for 3 StandardPizzas
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 3)
        });

        // StandardPizza requires: 2 Dough + 1 Tomatoes per pizza
        // Total needed: 6 Dough + 3 Tomatoes
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        // Setup repository with insufficient Dough (have 3, need 6)
        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 3)); // Only 3 Dough available, need 6
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 10)); // Sufficient Tomatoes

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsTrue(result);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that an empty order always has sufficient stock.
    // ASSUMPTION: When no pizzas are ordered, no ingredients are consumed, so stock is always sufficient.
    // EXPECTATION: HasInsufficientStock returns false for empty orders.
    [TestMethod]
    public async Task HasInsufficientStock_EmptyOrder_ReturnsFalse()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>());

        // Empty order needs no recipes, but we provide some anyway
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        // No stock calls expected for empty order

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
    }

    // PURPOSE: Verifies correct ingredient aggregation across multiple pizza types.
    // ASSUMPTION: Each pizza type has its own recipe. The method must sum ingredient requirements
    //             from all pizza types before checking stock availability.
    // EXPECTATION: Combined ingredient needs are correctly calculated across different pizza types.
    [TestMethod]
    public async Task HasInsufficientStock_MultiplePizzaTypes_CalculatesCorrectly()
    {
        // Arrange
        // Order 1 StandardPizza and 1 ExtremelyTastyPizza
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1)
        });

        // StandardPizza: 2 Dough + 1 Tomatoes
        // ExtremelyTastyPizza: 1 UnicornDust + 2 BellPeppers
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10),
            new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2)
            }, 15)
        };

        // All ingredients are sufficient
        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 5));
        stockRepository.Setup(x => x.GetStock(StockType.UnicornDust))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 3));
        stockRepository.Setup(x => x.GetStock(StockType.BellPeppers))
            .ReturnsAsync(new StockDto(StockType.BellPeppers, 4));

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that pizzas with zero quantity are ignored in stock calculations.
    // ASSUMPTION: A pizza with amount 0 should not contribute to ingredient requirements.
    // EXPECTATION: Zero-quantity pizzas do not affect the stock check outcome.
    [TestMethod]
    public async Task HasInsufficientStock_ZeroQuantityPizza_Ignored()
    {
        // Arrange
        // Order 1 StandardPizza (real) + 1 with zero quantity (should be ignored)
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 0) // Zero quantity - should be ignored
        });

        // StandardPizza: 2 Dough + 1 Tomatoes
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10),
            new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2)
            }, 15)
        };

        // Stock is only sufficient for StandardPizza ingredients (not for ExtremelyTastyPizza)
        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 5));
        // UnicornDust and BellPeppers are NOT called because ExtremelyTastyPizza has quantity 0

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that missing recipes for ordered pizza types cause appropriate behavior.
    // ASSUMPTION: The method may either throw an exception or skip pizzas with unknown recipes.
    //             This test documents expected behavior - implementation should be defensive.
    // EXPECTATION: HasInsufficientStock handles missing recipes gracefully without crashing.
    [TestMethod]
    public async Task HasInsufficientStock_MissingRecipe_HandlesGracefully()
    {
        // Arrange
        // Order a pizza type for which we don't have a recipe
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1)
        });

        // Empty recipes - no recipe available for StandardPizza
        var recipes = new ComparableList<PizzaRecipeDto>();

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        // Without a recipe, we cannot calculate ingredients, so we should not claim insufficient stock
        Assert.IsFalse(result);
    }

    // PURPOSE: Verifies stock checking with duplicate pizza type entries in an order.
    // ASSUMPTION: The method should correctly aggregate quantities even when the same pizza type
    //             appears multiple times in the order (e.g., two separate line items).
    // EXPECTATION: Quantities for the same pizza type are summed before calculating ingredients.
    [TestMethod]
    public async Task HasInsufficientStock_DuplicatePizzaTypes_AggregatesQuantities()
    {
        // Arrange
        // Order StandardPizza twice (should be treated as 5 total)
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 2),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 3)
        });

        // StandardPizza: 2 Dough per pizza
        // Total needed: (2+3) * 2 = 10 Dough
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2)
            }, 10)
        };

        // Exactly enough Dough for the combined order
        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that an ingredient not used by any pizza in the order is not checked.
    // ASSUMPTION: The method should only query stock for ingredients that are actually needed.
    //             Unused stock types should not trigger repository calls.
    // EXPECTATION: Only required ingredients are checked against available stock.
    [TestMethod]
    public async Task HasInsufficientStock_OnlyRequiredIngredientsChecked()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1)
        });

        // StandardPizza only uses Dough and Tomatoes
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 10));

        var service = GetService(stockRepository.Object);

        // Act
        await service.HasInsufficientStock(order, recipes);

        // Assert - verify only the required stock types were queried
        stockRepository.Verify(x => x.GetStock(StockType.Dough), Times.Once);
        stockRepository.Verify(x => x.GetStock(StockType.Tomatoes), Times.Once);
        // BellPeppers should NOT be called - not in any recipe
        stockRepository.Verify(x => x.GetStock(StockType.BellPeppers), Times.Never);
        stockRepository.VerifyAll();
    }
}
