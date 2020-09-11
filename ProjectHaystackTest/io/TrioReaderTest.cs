using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class TrioReaderTest
    {
        [TestMethod]
        public void ReadEntities_TwoEntities_IsValid()
        {
            // Arrange.
            var trio = @"dis: ""Site 1""
site
area: 3702ft²
geoAddr: ""100 Main St, Richmond, VA""
geoCoord: C(37.5458,-77.4491)
strTag: OK if unquoted if only safe chars
summary:
  This is a string value which spans multiple
  lines with two or more space characters
---
name: ""Site 2""
site
summary:
  Entities are separated by one or more dashes";
            var reader = new TrioReader(trio);

            // Act.
            var entities = reader.ReadEntities().ToArray();

            // Assert.
            Assert.AreEqual(2, entities.Length);
            Assert.AreEqual(HMarker.VAL, entities[0]["site"]);
            Assert.AreEqual(HStr.make("Entities are separated by one or more dashes"), entities[1]["summary"]);
        }

        [TestMethod]
        public void ReadEntities_NestedGrid_IsValid()
        {
            // Arrange.
            var trio = @"// Trio
type:list
val:[1,2,3]
---
type:dict
val:{ dis:""Dict!"" foo}
---
type:grid
val:Zinc:
  ver:""3.0""
  b,a
  20,10";
            var reader = new TrioReader(trio);

            // Act.
            var entities = reader.ReadEntities().ToArray();

            // Assert.
            Assert.AreEqual(3, entities.Length);
            Assert.AreEqual(HStr.make("list"), entities[0]["type"]);
            Assert.IsTrue(entities[1]["val"] is HDict);
            Assert.IsTrue(entities[2]["val"] is HGrid);
            Assert.AreEqual(10, ((HGrid)entities[2]["val"]).row(0).getInt("a"));
        }

        [TestMethod]
        public void ToGrid_TwoEntities_IsValid()
        {
            // Arrange.
            var trio = @"dis: ""Site 1""
site
area: 3702ft²
geoAddr: ""100 Main St, Richmond, VA""
geoCoord: C(37.5458,-77.4491)
strTag: OK if unquoted if only safe chars
summary:
  This is a string value which spans multiple
  lines with two or more space characters
---
name: ""Site 2""
site
summary:
  Entities are separated by one or more dashes";
            var reader = new TrioReader(trio);

            // Act.
            var grid = reader.ToGrid();

            // Assert.
            Assert.AreEqual(2, grid.Rows.Count());
            Assert.AreEqual(8, grid.Cols.Count());
            Assert.AreEqual(HMarker.VAL, grid.row(0).get("site"));
            Assert.AreEqual(HStr.make("Entities are separated by one or more dashes"), grid.row(1).get("summary"));
        }
    }
}