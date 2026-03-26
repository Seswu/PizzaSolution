using PizzaPlace.Models;

namespace PizzaPlace.Services;

public interface IStockService
{
    Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos);

    Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos);

    // PURPOSE: Deducts stock for all ingredients required by an order.
    // ASSUMPTION: This is a write operation that modifies stock levels.
    //             TakeStock naming follows IStockRepository pattern (Take prefix = modify stock).
    // EXPECTATION: Reduces stock for each ingredient; throws if insufficient stock.
    Task TakeStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos);

    // PURPOSE: Adds stock for the provided items.
    // ASSUMPTION: Stock addition is idempotent and handles concurrency via repository.
    // EXPECTATION: Updates stock levels and returns the updated stock DTOs.
    Task<IEnumerable<StockDto>> Restock(ComparableList<StockDto> stock);
}
