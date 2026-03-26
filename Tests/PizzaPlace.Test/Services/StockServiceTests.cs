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
    // ASSUMPTION: The method follows expediency - checks ingredients sequentially and returns
    //             immediately upon finding the first insufficient ingredient, without checking the rest.
    //             This test sets up Dough as insufficient (checked first), so Tomatoes may not be queried.
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
        // Note: VerifyAll() is intentionally omitted because Tomatoes may not be checked
        // due to early termination when Dough is found insufficient (expediency principle)
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

    // PURPOSE: Verifies that GetStock returns correct stock levels for all order ingredients.
    // ASSUMPTION: The method queries stock for each ingredient required by the order's pizzas.
    // EXPECTATION: Returns stock levels for all ingredients needed by the order.
    [TestMethod]
    public async Task GetStock_ReturnsCorrectStockLevels()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1)
        });

        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 5));

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(s => s.StockType == StockType.Dough && s.Amount == 10));
        Assert.IsTrue(result.Any(s => s.StockType == StockType.Tomatoes && s.Amount == 5));
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that GetStock returns empty list for empty order.
    // ASSUMPTION: When no pizzas are ordered, no ingredients are needed, so returns empty list.
    // EXPECTATION: GetStock returns an empty ComparableList for empty orders.
    [TestMethod]
    public async Task GetStock_EmptyOrder_ReturnsEmptyList()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>());

        var recipes = new ComparableList<PizzaRecipeDto>();

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    // PURPOSE: Verifies that GetStock returns only ingredients needed by the order.
    // ASSUMPTION: The method should only query stock for ingredients that are actually required.
    // EXPECTATION: Unused ingredients are not included in the returned stock list.
    [TestMethod]
    public async Task GetStock_OnlyRequiredIngredientsReturned()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1)
        });

        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 10));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 5));

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(2, result.Count);
        // Should not include BellPeppers or other unused ingredients
        stockRepository.Verify(x => x.GetStock(StockType.BellPeppers), Times.Never);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies GetStock handles multiple pizza types correctly.
    // ASSUMPTION: Each pizza type has its own recipe with different ingredients.
    // EXPECTATION: Returns stock for all unique ingredients across all pizza types.
    [TestMethod]
    public async Task GetStock_MultiplePizzaTypes_ReturnsAllIngredients()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1)
        });

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
        var result = await service.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(4, result.Count);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that TakeStock reduces stock for all order ingredients.
    // ASSUMPTION: This is a write operation that calls stockRepository.TakeStock for each ingredient.
    // EXPECTATION: Stock is reduced for all ingredients required by the order.
    [TestMethod]
    public async Task TakeStock_ReducesStockForAllIngredients()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 2)
        });

        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.TakeStock(StockType.Dough, 4)) // 2 * 2
            .ReturnsAsync(new StockDto(StockType.Dough, 6));
        stockRepository.Setup(x => x.TakeStock(StockType.Tomatoes, 2)) // 2 * 1
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 3));

        var service = GetService(stockRepository.Object);

        // Act
        await service.TakeStock(order, recipes);

        // Assert
        stockRepository.Verify(x => x.TakeStock(StockType.Dough, 4), Times.Once);
        stockRepository.Verify(x => x.TakeStock(StockType.Tomatoes, 2), Times.Once);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that TakeStock handles empty order (no stock changes).
    // ASSUMPTION: When no pizzas are ordered, no stock operations should occur.
    // EXPECTATION: No calls to stockRepository.TakeStock for empty orders.
    [TestMethod]
    public async Task TakeStock_EmptyOrder_NoStockChanges()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>());
        var recipes = new ComparableList<PizzaRecipeDto>();

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);

        var service = GetService(stockRepository.Object);

        // Act
        await service.TakeStock(order, recipes);

        // Assert
        stockRepository.Verify(x => x.TakeStock(It.IsAny<StockType>(), It.IsAny<int>()), Times.Never);
    }

    // PURPOSE: Verifies that TakeStock handles multiple pizza types correctly.
    // ASSUMPTION: Ingredients from multiple pizza types should all be deducted.
    // EXPECTATION: All unique ingredients across pizza types are reduced.
    [TestMethod]
    public async Task TakeStock_MultiplePizzaTypes_DeductsAllIngredients()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1)
        });

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

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.TakeStock(StockType.Dough, 2))
            .ReturnsAsync(new StockDto(StockType.Dough, 8));
        stockRepository.Setup(x => x.TakeStock(StockType.Tomatoes, 1))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 4));
        stockRepository.Setup(x => x.TakeStock(StockType.UnicornDust, 1))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 2));
        stockRepository.Setup(x => x.TakeStock(StockType.BellPeppers, 2))
            .ReturnsAsync(new StockDto(StockType.BellPeppers, 2));

        var service = GetService(stockRepository.Object);

        // Act
        await service.TakeStock(order, recipes);

        // Assert
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that TakeStock aggregates duplicate pizza types correctly.
    // ASSUMPTION: When same pizza type appears multiple times, quantities should be summed.
    // EXPECTATION: Total ingredient deduction equals sum of all quantities.
    [TestMethod]
    public async Task TakeStock_DuplicatePizzaTypes_AggregatesQuantities()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 2),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 3)
        });

        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, new ComparableList<StockDto>
            {
                new StockDto(StockType.Dough, 2)
            }, 10)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        // Total: (2+3) * 2 = 10 Dough
        stockRepository.Setup(x => x.TakeStock(StockType.Dough, 10))
            .ReturnsAsync(new StockDto(StockType.Dough, 0));

        var service = GetService(stockRepository.Object);

        // Act
        await service.TakeStock(order, recipes);

        // Assert
        stockRepository.Verify(x => x.TakeStock(StockType.Dough, 10), Times.Once);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that TakeStock ignores zero-quantity pizzas.
    // ASSUMPTION: Pizzas with amount 0 should not contribute to stock deduction.
    // EXPECTATION: Zero-quantity pizzas do not affect stock reduction.
    [TestMethod]
    public async Task TakeStock_ZeroQuantityPizza_Ignored()
    {
        // Arrange
        var order = new PizzaOrder(new ComparableList<PizzaAmount>
        {
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 0)
        });

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

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.TakeStock(StockType.Dough, 2))
            .ReturnsAsync(new StockDto(StockType.Dough, 8));
        stockRepository.Setup(x => x.TakeStock(StockType.Tomatoes, 1))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 4));

        var service = GetService(stockRepository.Object);

        // Act
        await service.TakeStock(order, recipes);

        // Assert
        // UnicornDust and BellPeppers should NOT be called (quantity 0)
        stockRepository.Verify(x => x.TakeStock(StockType.UnicornDust, It.IsAny<int>()), Times.Never);
        stockRepository.Verify(x => x.TakeStock(StockType.BellPeppers, It.IsAny<int>()), Times.Never);
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that Restock returns updated stock for a single item.
    // ASSUMPTION: The method calls repository.AddToStock and returns the updated DTO.
    // EXPECTATION: Returns list with the updated stock item.
    [TestMethod]
    public async Task Restock_SingleItem_ReturnsUpdatedStock()
    {
        // Arrange
        var stockList = new ComparableList<StockDto> { new StockDto(StockType.Dough, 10) };
        var updatedStock = new StockDto(StockType.Dough, 20);

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.AddToStock(It.Is<StockDto>(s => s.StockType == StockType.Dough && s.Amount == 10)))
            .ReturnsAsync(updatedStock);

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.Restock(stockList);

        // Assert
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(updatedStock, result.First());
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that Restock handles multiple items and returns all updated stocks.
    // ASSUMPTION: The method processes each item in order and collects results.
    // EXPECTATION: Returns list with all updated stock items in order.
    [TestMethod]
    public async Task Restock_MultipleItems_ReturnsAllUpdated()
    {
        // Arrange
        var stockList = new ComparableList<StockDto>
        {
            new StockDto(StockType.Dough, 10),
            new StockDto(StockType.Tomatoes, 5)
        };
        var updatedDough = new StockDto(StockType.Dough, 20);
        var updatedTomatoes = new StockDto(StockType.Tomatoes, 10);

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.AddToStock(It.Is<StockDto>(s => s.StockType == StockType.Dough && s.Amount == 10)))
            .ReturnsAsync(updatedDough);
        stockRepository.Setup(x => x.AddToStock(It.Is<StockDto>(s => s.StockType == StockType.Tomatoes && s.Amount == 5)))
            .ReturnsAsync(updatedTomatoes);

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.Restock(stockList);

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(updatedDough, result.ElementAt(0));
        Assert.AreEqual(updatedTomatoes, result.ElementAt(1));
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that Restock returns empty list for empty input.
    // ASSUMPTION: When no stock items are provided, no operations occur.
    // EXPECTATION: Returns empty enumerable.
    [TestMethod]
    public async Task Restock_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var stockList = new ComparableList<StockDto>();

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);

        var service = GetService(stockRepository.Object);

        // Act
        var result = await service.Restock(stockList);

        // Assert
        Assert.AreEqual(0, result.Count());
        stockRepository.VerifyAll();
    }

    // PURPOSE: Verifies that Restock calls AddToStock with correct parameters for each item.
    // ASSUMPTION: The method passes each StockDto directly to repository.AddToStock.
    // EXPECTATION: Repository.AddToStock is called exactly once per item with matching parameters.
    [TestMethod]
    public async Task Restock_AddToStockCalledCorrectly()
    {
        // Arrange
        var stockList = new ComparableList<StockDto>
        {
            new StockDto(StockType.Dough, 10),
            new StockDto(StockType.Tomatoes, 5)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.AddToStock(It.IsAny<StockDto>()))
            .ReturnsAsync((StockDto s) => s); // Echo back the input

        var service = GetService(stockRepository.Object);

        // Act
        await service.Restock(stockList);

        // Assert
        stockRepository.Verify(x => x.AddToStock(It.Is<StockDto>(s => s.StockType == StockType.Dough && s.Amount == 10)), Times.Once);
        stockRepository.Verify(x => x.AddToStock(It.Is<StockDto>(s => s.StockType == StockType.Tomatoes && s.Amount == 5)), Times.Once);
        stockRepository.VerifyAll();
    }
}
