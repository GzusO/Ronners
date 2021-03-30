using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ronners.Loot;

namespace Ronners.Test
{
    [TestClass]
    public class LootTests
    {
        [TestMethod]
        public void LootGenTest()
        {
            var lg = new Ronners.Loot.LootGenerator("TestData/");
            Assert.IsNotNull(lg.Generate());
        }

    }
}
