using static PizzaPlace.Models.Types.PizzaRecipeType;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTests
{
    [TestMethod]
	public void GetMenuStandard()
    // Test the GetMenu method on the MenuService class outside lunch hours.
	{
        // Arrange

        // set up a hypothetical time of 2030-11-29 17:30:05
        // this is outside of lunch hours, so we expect the standard menu to be returned
        // dates should not matter
        var time = new DateTimeOffset(2030, 11, 29, 17, 30, 5, TimeSpan.Zero);

        // construct a test menu; title + items with name, type and price
        var validMenu = new Menu("Many Toppings", [
            new MenuItem("Pizza of the day", ExtremelyTastyPizza, 9.50),
            new MenuItem("Caesar Salad", OddPizza, 30.75),
            new MenuItem("Margharita", StandardPizza, 10.11),
            new MenuItem("Hawaii", StandardPizza, 12.03),
            new MenuItem("Mortadella", StandardPizza, 10.80),
            new MenuItem("Five Seasons", OddPizza, 14.00),
            new MenuItem("Mexicana", HorseRadishPizza, 11.09),
            new MenuItem("Chycken Pizza", StandardPizza, 8.00),
            new MenuItem("Named Meat Pizza", RarePizza, 12.00),
            new MenuItem("Unnamed Meat Pizza", StandardPizza, 8.00),
            new MenuItem("Marius", RarePizza, 20.00),
            new MenuItem("Wolpertinger", RarePizza, 30.00),
        ]);

        // call up a MenuService
        var menuService = new MenuService();

        // Act

        // call MenuService to get hardcoded standard menu
        var returnedMenu = menuService.GetMenu(time);

        // Assert
        Assert.IsInstanceOfType<Menu>(returnedMenu);
        Assert.AreEqual(validMenu, returnedMenu);
    }

    [TestMethod]
	public void GetLunchMenu()
    // Test the GetMenu method on the MenuService class inside lunch hours.
	{
        // Arrange

        // set up a hypothetical time of 2029-08-04 12:30:05
        // this is during lunch hours, so we expect the lunch menu to be returned
        // dates should not matter
        var time = new DateTimeOffset(2029, 8, 4, 12, 30, 5, TimeSpan.Zero);

        // construct a test menu; title + items with name, type and price
        var validMenu = new Menu("12 you can eat!", [
            new MenuItem("Luxury Family Size Hawaiian Style Pizza", StandardPizza, 19.25),
            new MenuItem("Pigeon Feast", RarePizza, 2.22),
            new MenuItem("Chef's Assorted", OddPizza, 32.35),
            new MenuItem("Nippon Style", HorseRadishPizza, 13.24),
            new MenuItem("I have a bad feeling about this", EmptyPizza, 0.40),
            new MenuItem("6th time's the charnel!", ExtremelyTastyPizza, 30.30),
            new MenuItem("Ham and Cucumber", StandardPizza, 90.23),
            new MenuItem("Roasted MewMew", RarePizza, 15.3),
            new MenuItem("Diet and Progress", EmptyPizza, 11.95),
            new MenuItem("Now with Meat", StandardPizza, 20.45),
            new MenuItem("Do you come here often?", StandardPizza, 11.21),
            new MenuItem("Don't Worry, Be Happy", OddPizza, 5.43),
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

    [TestMethod]
    public void GetMenuStandardAtLunchEdge()
    // Test the GetMenu method on the MenuService class (just) outside lunch hours.
    {
        // Arrange

        // set up a hypothetical time of 2031-09-08 14:00:05
        // this is just outside of lunch hours, so we expect the standard menu to be returned
        // dates should not matter
        var time = new DateTimeOffset(2031, 9, 8, 14, 0, 5, TimeSpan.Zero);

        // construct a test menu; title + items with name, type and price
        var validMenu = new Menu("Many Toppings", [
            new MenuItem("Pizza of the day", ExtremelyTastyPizza, 9.50),
            new MenuItem("Caesar Salad", OddPizza, 30.75),
            new MenuItem("Margharita", StandardPizza, 10.11),
            new MenuItem("Hawaii", StandardPizza, 12.03),
            new MenuItem("Mortadella", StandardPizza, 10.80),
            new MenuItem("Five Seasons", OddPizza, 14.00),
            new MenuItem("Mexicana", HorseRadishPizza, 11.09),
            new MenuItem("Chycken Pizza", StandardPizza, 8.00),
            new MenuItem("Named Meat Pizza", RarePizza, 12.00),
            new MenuItem("Unnamed Meat Pizza", StandardPizza, 8.00),
            new MenuItem("Marius", RarePizza, 20.00),
            new MenuItem("Wolpertinger", RarePizza, 30.00),
        ]);

        // call up a MenuService
        var menuService = new MenuService();

        // Act

        // call MenuService to get hardcoded standard menu
        var returnedMenu = menuService.GetMenu(time);

        // Assert
        Assert.IsInstanceOfType<Menu>(returnedMenu);
        Assert.AreEqual(validMenu, returnedMenu);
    }
}