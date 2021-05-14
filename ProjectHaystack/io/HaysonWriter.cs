using System;
using System.IO;
using Newtonsoft.Json;

namespace ProjectHaystack.io
{
    public class HaysonWriter : IDisposable
    {
        private JsonWriter _haysonWriter;

        public HaysonWriter(JsonWriter writer)
        {
            _haysonWriter = writer;
        }

        public HaysonWriter(TextWriter writer)
            : this(new JsonTextWriter(writer))
        {
        }

        public HaysonWriter(Stream haysonStream)
            : this(new StreamWriter(haysonStream, null, 1024, true))
        {
        }

        public static string ToHayson(HaystackValue val)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            using (var writer = new HaysonWriter(streamWriter))
            {
                writer.WriteValue(val);
                streamWriter.Flush();
                stream.Position = 0;
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public void WriteGrid(HaystackGrid grid)
        {
            _haysonWriter.WriteStartObject();
            _haysonWriter.WritePropertyName("_kind");
            _haysonWriter.WriteValue("Grid");

            if (grid.Meta != null && !grid.Meta.IsEmpty())
            {
                _haysonWriter.WritePropertyName("meta");
                WriteValue(grid.Meta);
            }

            _haysonWriter.WritePropertyName("cols");
            _haysonWriter.WriteStartArray();
            foreach (var col in grid.Columns)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("name");
                _haysonWriter.WriteValue(col.Name);
                if (col.Meta != null && !col.Meta.IsEmpty())
                {
                    _haysonWriter.WritePropertyName("meta");
                    WriteValue(col.Meta);
                }
                _haysonWriter.WriteEndObject();
            }
            _haysonWriter.WriteEndArray();

            _haysonWriter.WritePropertyName("rows");
            _haysonWriter.WriteStartArray();
            foreach (var row in grid.Rows)
            {
                WriteEntity(row);
            }
            _haysonWriter.WriteEndArray();

            _haysonWriter.WriteEndObject();
        }

        public void WriteEntities(params HaystackDictionary[] entities)
        {
            _haysonWriter.WriteStartArray();
            foreach (var entity in entities)
            {
                WriteEntity(entity);
            }
            _haysonWriter.WriteEndArray();
        }

        public void WriteEntity(HaystackDictionary entity)
        {
            _haysonWriter.WriteStartObject();
            foreach (var kv in entity)
            {
                if (kv.Value == null)
                    continue;

                _haysonWriter.WritePropertyName(kv.Key);
                WriteValue(kv.Value);
            }
            _haysonWriter.WriteEndObject();
        }

        public void WriteValue(HaystackValue value)
        {
            if (value == null)
            {
                _haysonWriter.WriteNull();
                return;
            }
            if (value is HaystackString strValue)
            {
                _haysonWriter.WriteValue(strValue.Value);
                return;
            }
            if (value is HaystackNumber numValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("number");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(numValue.Value);
                _haysonWriter.WritePropertyName("unit");
                _haysonWriter.WriteValue(numValue.Unit);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackBoolean boolValue)
            {
                _haysonWriter.WriteValue(boolValue.Value);
                return;
            }
            if (value is HaystackList listValue)
            {
                _haysonWriter.WriteStartArray();
                foreach (var val in listValue)
                {
                    WriteValue(val);
                }
                _haysonWriter.WriteEndArray();
                return;
            }
            if (value is HaystackDictionary dictValue)
            {
                WriteEntity(dictValue);
                return;
            }
            if (value is HaystackGrid gridValue)
            {
                WriteGrid(gridValue);
                return;
            }
            if (value is HaystackMarker)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Marker");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackRemove)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Remove");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackNotAvailable)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("NA");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackReference refValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(refValue.Value);
                _haysonWriter.WritePropertyName("dis");
                _haysonWriter.WriteValue(refValue.Display);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackDate dateValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Date");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue($"{dateValue.Value.Year:d4}-{dateValue.Value.Month:d2}-{dateValue.Value.Day:d2}");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackTime timeValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue($"{timeValue.Value.Hours:d2}-{timeValue.Value.Minutes:d2}-{timeValue.Value.Seconds:d2}");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackDateTime dateTimeValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(dateTimeValue.Value.ToString("o"));
                _haysonWriter.WritePropertyName("tz");
                _haysonWriter.WriteValue(dateTimeValue.TimeZone.ToString());
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackUri uriValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Uri");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(uriValue.Value);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackCoordinate coordValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Coord");
                _haysonWriter.WritePropertyName("lat");
                _haysonWriter.WriteValue(coordValue.Latitude);
                _haysonWriter.WritePropertyName("lng");
                _haysonWriter.WriteValue(coordValue.Longitude);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HaystackXString xStrValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("XStr");
                _haysonWriter.WritePropertyName("type");
                _haysonWriter.WriteValue(xStrValue.Type);
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(xStrValue.Value);
                _haysonWriter.WriteEndObject();
                return;
            }
        }
        public void Dispose()
        {
            if (_haysonWriter != null)
            {
                var writer = _haysonWriter;
                _haysonWriter = null;
                ((IDisposable)writer).Dispose();
            }
        }
    }
}