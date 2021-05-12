using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProjectHaystack;
using ProjectHaystack.io;

namespace ProjectHaystackTest.io
{
    [TestClass]
    public class HaysonWriterTest
    {
        [TestMethod]
        public void WriteEntity_IsValid()
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                // Arrange.
                var haysonWriter = new HaysonWriter(jsonWriter);
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
                haysonWriter.WriteEntity(entity);
                var hayson = writer.ToString();

                // Assert.
                var target = @"{
  ""dis"": ""Site 1"",
  ""site"": {
    ""_kind"": ""Marker""
  },
  ""area"": {
    ""_kind"": ""number"",
    ""val"": 3702.0,
    ""unit"": ""ft²""
  },
  ""geoAddr"": ""100 Main St, Richmond, VA"",
  ""geoCoord"": {
    ""_kind"": ""Coord"",
    ""lat"": 37.5458,
    ""lng"": -77.4491
  },
  ""strTag"": ""OK if unquoted if only safe chars""
}";
                Assert.AreEqual(target.Replace("\r", ""), hayson.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntity_QuotedString_IsValid()
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                // Arrange.
                var haysonWriter = new HaysonWriter(jsonWriter);
                var entity = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["strTag"] = HStr.make("Line with \"inline\" quotes."),
                });

                // Act.
                haysonWriter.WriteEntity(entity);
                var hayson = writer.ToString();

                // Assert.
                var target = @"{
  ""dis"": ""Site 1"",
  ""site"": {
    ""_kind"": ""Marker""
  },
  ""area"": {
    ""_kind"": ""number"",
    ""val"": 3702.0,
    ""unit"": ""ft²""
  },
  ""geoAddr"": ""100 Main St, Richmond, VA"",
  ""geoCoord"": {
    ""_kind"": ""Coord"",
    ""lat"": 37.5458,
    ""lng"": -77.4491
  },
  ""strTag"": ""Line with \""inline\"" quotes.""
}";
                Assert.AreEqual(target.Replace("\r", ""), hayson.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntities_TwoEntities_IsValid()
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                // Arrange.
                var haysonWriter = new HaysonWriter(jsonWriter);
                var entity1 = new HDict(new Dictionary<string, HVal>
                {
                    ["dis"] = HStr.make("Site 1"),
                    ["site"] = HMarker.VAL,
                    ["area"] = HNum.make(3702, "ft²"),
                    ["geoAddr"] = HStr.make("100 Main St, Richmond, VA"),
                    ["geoCoord"] = HCoord.make(37.5458, -77.4491),
                    ["strTag"] = HStr.make("Line with \"inline\" quotes."),
                    ["summary"] = HStr.make("This is a string value which spans multiple\nlines with two or more space characters"),
                });
                var entity2 = new HDict(new Dictionary<string, HVal>
                {
                    ["name"] = HStr.make("Site 2"),
                    ["site"] = HMarker.VAL,
                    ["summary"] = HStr.make("Entities are separated by one or more dashes"),
                });

                // Act.
                haysonWriter.WriteEntities(entity1, entity2);
                var hayson = writer.ToString();

                // Assert.
                var target = @"[
  {
    ""dis"": ""Site 1"",
    ""site"": {
      ""_kind"": ""Marker""
    },
    ""area"": {
      ""_kind"": ""number"",
      ""val"": 3702.0,
      ""unit"": ""ft²""
    },
    ""geoAddr"": ""100 Main St, Richmond, VA"",
    ""geoCoord"": {
      ""_kind"": ""Coord"",
      ""lat"": 37.5458,
      ""lng"": -77.4491
    },
    ""strTag"": ""Line with \""inline\"" quotes."",
    ""summary"": ""This is a string value which spans multiple\nlines with two or more space characters""
  },
  {
    ""name"": ""Site 2"",
    ""site"": {
      ""_kind"": ""Marker""
    },
    ""summary"": ""Entities are separated by one or more dashes""
  }
]";
                Assert.AreEqual(target.Replace("\r", ""), hayson.Replace("\r", ""));
            }
        }

        [TestMethod]
        public void WriteEntity_NestedGrid_IsValid()
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                // Arrange.
                var haysonWriter = new HaysonWriter(jsonWriter);
                var gridBuilder = new HGridBuilder();
                gridBuilder.addCol("b").add("dis", "Column B");
                gridBuilder.addCol("a");
                gridBuilder.addRow(HNum.make(20), HNum.make(10));
                var entity = new HDict(new Dictionary<string, HVal>
                {
                    ["type"] = HStr.make("grid"),
                    ["val"] = gridBuilder.toGrid(),
                });

                // Act.
                haysonWriter.WriteEntity(entity);
                var hayson = writer.ToString();

                // Assert.
                var target = @"{
  ""type"": ""grid"",
  ""val"": {
    ""_kind"": ""Grid"",
    ""cols"": [
      {
        ""name"": ""b"",
        ""meta"": {
          ""dis"": ""Column B""
        }
      },
      {
        ""name"": ""a""
      }
    ],
    ""rows"": [
      {
        ""b"": {
          ""_kind"": ""number"",
          ""val"": 20.0,
          ""unit"": null
        },
        ""a"": {
          ""_kind"": ""number"",
          ""val"": 10.0,
          ""unit"": null
        }
      }
    ]
  }
}";
                Assert.AreEqual(target.Replace("\r", ""), hayson.Replace("\r", ""));
            }
        }
    }
}