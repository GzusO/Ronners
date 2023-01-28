using System.ComponentModel;

namespace Ronners.Bot.Models
{
    public enum Food
    {
        [Description("some Banana Hollandaise")]
        BananaHollandaise = 1,
        [Description("a Ramenrito")]
        Ramenrito = 2,
        [Description("some Mac and Cheese")]
        MacAndCheese=3,
        [Description("a Cheese Burger")]
        CheeseBurger=5
    }
}
