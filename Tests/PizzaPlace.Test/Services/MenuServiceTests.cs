using static PizzaPlace.Models.Types.PizzaRecipeType;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTests
{
    //private static MenuService sacrebleu(ClassConstructor var1, Mock<iVar2> predefined) => new(something, predefined.Object)
    // if a Mock object is called for.

    [TestMethod]
	public void GetMenu()
    // Test the GetMenu method on the MenuService class.
	{
        // Arrange

        // set up a hypothetical time of 2030-10-12 12:30:05
        var time = new DateTimeOffset(2030, 10, 12, 12, 30, 5, TimeSpan.Zero);

        // construct a test menu; title + items with name, type and price
        var validMenu = new Menu("Many Toppings", [
            new MenuItem("Pizza of the day", ExtremelyTastyPizza, 9.50),
            new MenuItem("Caesar Salad", OddPizza, 30.75),
        ]);

        // call up a MenuService
        var menuService = new MenuService();

        // Act

        // call MenuService to get hardcoded menu
        var returnedMenu = menuService.GetMenu(time);

        // Assert
        Assert.IsInstanceOfType<Menu>(returnedMenu);
        Assert.AreEqual(validMenu, returnedMenu);
    }
}