using static PizzaPlace.Models.Types.PizzaRecipeType;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    public Menu GetMenu(DateTimeOffset menuDate)
    {
        var lunchtimeStart = new DateTimeOffset(2030, 10, 12, 11, 0, 0, TimeSpan.Zero);
        var lunchtimeEnd = new DateTimeOffset(2030, 10, 12, 14, 0, 0, TimeSpan.Zero);
        var menu = new Menu("Placeholder", []);

        // Lunchtime is 11.00.00-14.00.00, 'not-lunch' is 14.00.01-10.59.59
        if (menuDate.Hour >= lunchtimeStart.Hour && (menuDate.Hour < lunchtimeEnd.Hour || (menuDate.Hour == lunchtimeEnd.Hour && menuDate.Minute == 0 && menuDate.Second == 0)))
        {
            // Lunchtime Menu
            menu = new Menu("12 you can eat!", [
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
        }
        else
        {
            // Standard menu
            menu = new Menu("Many Toppings", [
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
        }
        return menu;
    }
}