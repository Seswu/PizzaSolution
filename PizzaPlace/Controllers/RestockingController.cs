using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/restocking")]
public class RestockingController(IStockService stockService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Restock([FromBody] ComparableList<StockDto> stock)
    {
        if (stock == null || stock.Count == 0)
        {
            return BadRequest("Stock list cannot be null or empty.");
        }

        var result = await stockService.Restock(stock);
        return Ok(result);
    }
}
