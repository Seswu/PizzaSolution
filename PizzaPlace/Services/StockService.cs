using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    // PURPOSE: Determines if an order can be fulfilled with available stock.
    // ASSUMPTION: The method aggregates ingredient requirements from all pizzas,
    //             then checks each ingredient's stock level sequentially.
    // EXPECTATION: Returns true immediately upon finding any ingredient with insufficient stock,
    //              following the principle of expediency (early termination).
    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        // Calculate total ingredients required for the order by aggregating across all pizzas
        var requiredIngredients = CalculateRequiredIngredients(order, recipeDtos);

        // Check each ingredient's stock level; return true if any is insufficient
        foreach (var (stockType, requiredAmount) in requiredIngredients)
        {
            var currentStock = await stockRepository.GetStock(stockType);
            if (currentStock.Amount < requiredAmount)
            {
                return true; // Insufficient stock found, no need to check further
            }
        }

        return false; // All ingredients have sufficient stock
    }

    // PURPOSE: Retrieves current stock levels for all ingredients needed by an order.
    // ASSUMPTION: This is a read-only operation that only queries stock, does not modify it.
    //             GetStock naming follows the IStockRepository pattern (Get prefix = read-only).
    // EXPECTATION: Returns stock levels for all ingredients required by the order.
    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var requiredIngredients = CalculateRequiredIngredients(order, recipeDtos);
        var stockList = new ComparableList<StockDto>();

        foreach (var stockType in requiredIngredients.Keys)
        {
            var currentStock = await stockRepository.GetStock(stockType);
            stockList.Add(currentStock);
        }

        return stockList;
    }

    // PURPOSE: Deducts stock for all ingredients required by an order.
    // ASSUMPTION: This is a write operation that modifies stock levels by calling stockRepository.TakeStock.
    //             The method calculates total ingredients needed using CalculateRequiredIngredients,
    //             then reduces stock for each ingredient in sequence.
    //             Follows the naming pattern where TakeStock modifies stock (vs GetStock which is read-only).
    // EXPECTATION: Reduces stock for each ingredient required by the order.
    public async Task TakeStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var requiredIngredients = CalculateRequiredIngredients(order, recipeDtos);

        foreach (var (stockType, requiredAmount) in requiredIngredients)
        {
            await stockRepository.TakeStock(stockType, requiredAmount);
        }
    }

    // PURPOSE: Aggregates ingredient requirements from all pizzas in an order.
    // ASSUMPTION: Each pizza type has a recipe specifying its ingredients per unit.
    //             Quantities are summed for duplicate pizza types and zero-quantity pizzas are ignored.
    //             Missing recipes for a pizza type are skipped (treated as having no ingredients).
    // EXPECTATION: Returns a dictionary mapping each StockType to its total required amount.
    private static Dictionary<StockType, int> CalculateRequiredIngredients(
        PizzaOrder order,
        ComparableList<PizzaRecipeDto> recipeDtos)
    {
        // Build a lookup dictionary from recipe type to recipe DTO for efficient access
        var recipeLookup = recipeDtos.ToDictionary(r => r.RecipeType);
        var requiredIngredients = new Dictionary<StockType, int>();

        // Iterate through each pizza in the order
        foreach (var pizza in order.RequestedOrder)
        {
            // Skip pizzas with zero quantity as they don't consume ingredients
            if (pizza.Amount == 0) continue;

            // Find the recipe for this pizza type; skip if not found
            if (!recipeLookup.TryGetValue(pizza.PizzaType, out var recipe)) continue;

            // Add each ingredient multiplied by the pizza quantity
            foreach (var ingredient in recipe.Ingredients)
            {
                if (requiredIngredients.ContainsKey(ingredient.StockType))
                {
                    requiredIngredients[ingredient.StockType] += ingredient.Amount * pizza.Amount;
                }
                else
                {
                    requiredIngredients[ingredient.StockType] = ingredient.Amount * pizza.Amount;
                }
            }
        }

        return requiredIngredients;
    }
}
