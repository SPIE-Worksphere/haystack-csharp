using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class TrioReaderTests
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
            Assert.AreEqual(new HaystackMarker(), entities[0]["site"]);
            Assert.AreEqual(new HaystackString("Entities are separated by one or more dashes"), entities[1]["summary"]);
        }

        [TestMethod]
        public void ReadEntities_NestedZincGrid_IsValid()
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
            Assert.AreEqual(new HaystackString("list"), entities[0]["type"]);
            Assert.IsTrue(entities[1]["val"] is HaystackDictionary);
            Assert.IsTrue(entities[2]["val"] is HaystackGrid);
            Assert.AreEqual(10, (((HaystackGrid)entities[2]["val"]).Row(0).Get("a") as HaystackNumber).Value);
        }

        [TestMethod]
        public void ReadEntities_NestedTrioGrid_IsValid()
        {
            // Arrange.
            var trio = @"// Trio
type:list
val:[1,2,3]
---
type:dict
val:{ dis:""Dict!"" foo}
---
type:list
val: Trio:
  b: 20
  a: 10";
            var reader = new TrioReader(trio);

            // Act.
            var entities = reader.ReadEntities().ToArray();

            // Assert.
            Assert.AreEqual(3, entities.Length);
            Assert.AreEqual(new HaystackString("list"), entities[0]["type"]);
            Assert.IsTrue(entities[1]["val"] is HaystackDictionary);
            Assert.IsTrue(entities[2]["val"] is HaystackDictionary);
            Assert.AreEqual(10, ((HaystackNumber)((HaystackDictionary)entities[2]["val"])["a"]).Value);
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
            Assert.AreEqual(8, grid.Columns.Count());
            Assert.AreEqual(new HaystackMarker(), grid.Row(0).Get("site"));
            Assert.AreEqual(new HaystackString("Entities are separated by one or more dashes"), grid.Row(1).Get("summary"));
        }
    }
}