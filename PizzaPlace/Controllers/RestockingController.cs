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
        throw new NotImplementedException("Restocking has not been implemented.");
    }
}
