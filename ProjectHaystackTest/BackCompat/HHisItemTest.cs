using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest.BackCompat
{
    [TestClass]
    public class HHisItemTest : HValTest
    {
        [TestMethod]
        public void gridToItems()
        {
            // Arrange.
            var gridBuilder = new HGridBuilder();
            gridBuilder.addCol("ts");
            gridBuilder.addCol("val");
            gridBuilder.addRow(HDateTime.make(2020, 1, 1, 0, 0, 0, HTimeZone.UTC), HNum.make(10));
            var grid = gridBuilder.toGrid();

            // Act.
            var hisItems = HHisItem.gridToItems(grid);

            // Assert.
            Assert.AreEqual(1, hisItems.Length);
            Assert.AreEqual(HDateTime.make(2020, 1, 1, 0, 0, 0, HTimeZone.UTC), hisItems[0].TimeStamp);
            Assert.AreEqual(HNum.make(10), hisItems[0].hsVal);
        }
    }
}