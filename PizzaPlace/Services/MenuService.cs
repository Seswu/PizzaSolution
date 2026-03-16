using static PizzaPlace.Models.Types.PizzaRecipeType;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    public Menu GetMenu(DateTimeOffset menuDate)
    {
        //throw new NotImplementedException("No menu has been implemented yet.");
        var menu = new Menu("Many Toppings", [
            new MenuItem("Pizza of the day", ExtremelyTastyPizza, 9.50),
            new MenuItem("Caesar Salad", OddPizza, 30.75),
        ]);
        return menu;
    }
}