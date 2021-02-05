using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class TrioWriterTest
    {
        [TestMethod]
        public void WriteEntity_SafeString_IsValid()
        {
            using (var writer = new StringWriter())
            {
                // Arrange.
                var trioWriter = new TrioWriter(writer);
                var entity = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["strTag"] = HStr.make("OK if unquoted if only safe chars"),
                });

                // Act.
                trioWriter.WriteEntity(entity);
                var trio = writer.ToString();

                // Assert.
                var target = @"dis:Site 1
site
area:3702ft²
geoAddr:""100 Main St, Richmond, VA""
geoCoord:C(37.5458,-77.4491)
strTag:OK if unquoted if only safe chars
";
                Assert.AreEqual(target.Replace("\r", ""), trio.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntity_UnsafeString_IsValid()
        {
            using (var writer = new StringWriter())
            {
                // Arrange.
                var trioWriter = new TrioWriter(writer);
                var entity = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["strTag"] = HStr.make("Not ok if unquoted (with unsafe chars)."),
                });

                // Act.
                trioWriter.WriteEntity(entity);
                var trio = writer.ToString();

                // Assert.
                var target = @"dis:Site 1
site
area:3702ft²
geoAddr:""100 Main St, Richmond, VA""
geoCoord:C(37.5458,-77.4491)
strTag:""Not ok if unquoted (with unsafe chars).""
";
                Assert.AreEqual(target.Replace("\r", ""), trio.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntity_MultiLineString_IsValid()
        {
            using (var writer = new StringWriter())
            {
                // Arrange.
                var trioWriter = new TrioWriter(writer);
                var entity = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["summary"] = HStr.make("This is a string value which spans multiple\nlines with two or more space characters"),
                });

                // Act.
                trioWriter.WriteEntity(entity);
                var trio = writer.ToString();

                // Assert.
                var target = @"dis:Site 1
site
area:3702ft²
geoAddr:""100 Main St, Richmond, VA""
geoCoord:C(37.5458,-77.4491)
summary:
  This is a string value which spans multiple
  lines with two or more space characters
";
                Assert.AreEqual(target.Replace("\r", ""), trio.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntities_TwoEntities_IsValid()
        {
            using (var writer = new StringWriter())
            {
                // Arrange.
                var trioWriter = new TrioWriter(writer);
                var entity1 = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["strTag"] = HStr.make("OK if unquoted if only safe chars"),
                    ["summary"] = HStr.make("This is a string value which spans multiple\nlines with two or more space characters"),
                });
                var entity2 = new HDict(new Dictionary<string, HVal>
                {
                    ["name"] = HStr.make("Site 2"),
                    ["site"] = HMarker.VAL,
                    ["summary"] = HStr.make("Entities are separated by one or more dashes"),
                });

                // Act.
                trioWriter.WriteEntity(entity1);
                trioWriter.WriteEntity(entity2);
                var trio = writer.ToString();

                // Assert.
                var target = @"dis:Site 1
site
area:3702ft²
geoAddr:""100 Main St, Richmond, VA""
geoCoord:C(37.5458,-77.4491)
strTag:OK if unquoted if only safe chars
summary:
  This is a string value which spans multiple
  lines with two or more space characters
---
name:Site 2
site
summary:Entities are separated by one or more dashes
";
                Assert.AreEqual(target.Replace("\r", ""), trio.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntities_NestedGrid_IsValid()
        {
            using (var writer = new StringWriter())
            {
                // Arrange.
                var trioWriter = new TrioWriter(writer);
                var entity1 = new HDict(new Dictionary<string, HVal>
                {
                    ["type"] = HStr.make("list"),
                    ["val"] = HList.make(HNum.make(1), HNum.make(2), HNum.make(3)),
                });
                var entity2 = new HDict(new Dictionary<string, HVal>
                {
                    ["type"] = HStr.make("dict"),
                    ["val"] = new HDict(new Dictionary<string, HVal>
                    {
                        ["dis"] = HStr.make("Dict!"),
                        ["foo"] = HMarker.VAL,
                    }),
                });
                var gridBuilder = new HGridBuilder();
                gridBuilder.addCol("b");
                gridBuilder.addCol("a");
                gridBuilder.addRow(HNum.make(20), HNum.make(10));
                var entity3 = new HDict(new Dictionary<string, HVal>
                {
                    ["type"] = HStr.make("grid"),
                    ["val"] = gridBuilder.toGrid(),
                });

                // Act.
                trioWriter.WriteComment("Trio");
                trioWriter.WriteEntity(entity1);
                trioWriter.WriteEntity(entity2);
                trioWriter.WriteEntity(entity3);
                var trio = writer.ToString();

                // Assert.
                var target = @"// Trio
type:list
val:[1,2,3]
---
type:dict
val:{dis:""Dict!"" foo}
---
type:grid
val:Zinc:
  ver:""3.0""
  b,a
  20,10
";
                Assert.AreEqual(target.Replace("\r", ""), trio.Replace("\r", ""));
            }
        }
    }
}