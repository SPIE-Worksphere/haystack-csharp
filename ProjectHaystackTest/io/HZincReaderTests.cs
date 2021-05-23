using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class HZincReaderTests
    {
        [TestMethod]
        public void readGrid_withTagDef()
        {
            var reader = new ZincReader(
@"ver:""3.0""
id,def,doc,mod
@p:struktonLibrary:r:25b81501-75003ad2 ""struktonActivePointOnly"",^struktonActivePointOnly,""Import only active points"",2020-01-20T07:36:33.162Z");
            var grid = reader.ReadValue<HaystackGrid>();
            Assert.IsTrue(grid.Row(0).Get("def") is HaystackDefinition);
        }
    }
}