using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class RestockingControllerTests
{
    private static RestockingController GetController(Mock<IStockService> stockService) =>
        new(stockService.Object);

    [TestMethod]
    public async Task Restock_ValidInput_ReturnsOk()
    {
        // Arrange
        var stockList = new ComparableList<StockDto> { new StockDto(StockType.Dough, 10) };
        var expectedResult = new List<StockDto> { new StockDto(StockType.Dough, 20) };

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        stockService.Setup(x => x.Restock(stockList))
            .ReturnsAsync(expectedResult);

        var controller = GetController(stockService);

        // Act
        var actual = await controller.Restock(stockList);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(actual);
        var okResult = actual as OkObjectResult;
        Assert.IsNotNull(okResult);
        CollectionAssert.AreEqual(expectedResult, okResult.Value as List<StockDto>);
        stockService.VerifyAll();
    }

    [TestMethod]
    public async Task Restock_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var stockList = new ComparableList<StockDto>();

        var stockService = new Mock<IStockService>(MockBehavior.Strict);

        var controller = GetController(stockService);

        // Act
        var actual = await controller.Restock(stockList);

        // Assert
        Assert.IsInstanceOfType<BadRequestObjectResult>(actual);
        stockService.VerifyAll();
    }

    [TestMethod]
    public async Task Restock_NullList_ReturnsBadRequest()
    {
        // Arrange
        ComparableList<StockDto> stockList = null;

        var stockService = new Mock<IStockService>(MockBehavior.Strict);

        var controller = GetController(stockService);

        // Act
        var actual = await controller.Restock(stockList);

        // Assert
        Assert.IsInstanceOfType<BadRequestObjectResult>(actual);
        stockService.VerifyAll();
    }
}