using System.IO;
using Newtonsoft.Json;

namespace ProjectHaystack.io
{
    public class HaysonWriter
    {
        private readonly JsonWriter _haysonWriter;

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

        public void WriteGrid(HGrid grid)
        {
            _haysonWriter.WriteStartObject();
            _haysonWriter.WritePropertyName("_kind");
            _haysonWriter.WriteValue("Grid");

            if (grid.meta != null && !grid.meta.isEmpty())
            {
                _haysonWriter.WritePropertyName("meta");
                WriteValue(grid.meta);
            }

            _haysonWriter.WritePropertyName("cols");
            _haysonWriter.WriteStartArray();
            foreach (var col in grid.Cols)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("name");
                _haysonWriter.WriteValue(col.Name);
                if (col.meta != null && !col.meta.isEmpty())
                {
                    _haysonWriter.WritePropertyName("meta");
                    WriteValue(col.meta);
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

        public void WriteEntities(params HDict[] entities)
        {
            _haysonWriter.WriteStartArray();
            foreach (var entity in entities)
            {
                WriteEntity(entity);
            }
            _haysonWriter.WriteEndArray();
        }

        public void WriteEntity(HDict entity)
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

        public void WriteValue(HVal value)
        {
            if (value == null)
            {
                _haysonWriter.WriteNull();
                return;
            }
            if (value is HStr strValue)
            {
                _haysonWriter.WriteValue(strValue.Value);
                return;
            }
            if (value is HNum numValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Num");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(numValue.doubleval);
                _haysonWriter.WritePropertyName("unit");
                _haysonWriter.WriteValue(numValue.unit);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HBool boolValue)
            {
                _haysonWriter.WriteValue(boolValue.val);
                return;
            }
            if (value is HList listValue)
            {
                _haysonWriter.WriteStartArray();
                foreach (var val in listValue)
                {
                    WriteValue(val);
                }
                _haysonWriter.WriteEndArray();
                return;
            }
            if (value is HDict dictValue)
            {
                WriteEntity(dictValue);
                return;
            }
            if (value is HGrid gridValue)
            {
                WriteGrid(gridValue);
                return;
            }
            if (value is HMarker)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Marker");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HRemove)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Remove");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HNA)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("NA");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HRef refValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(refValue.val);
                _haysonWriter.WritePropertyName("dis");
                _haysonWriter.WriteValue(refValue.display());
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HDate dateValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Date");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue($"{dateValue.Year:d4}-{dateValue.Month:d2}-{dateValue.Day:d2}");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HTime timeValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue($"{timeValue.Hour:d2}-{timeValue.Minute:d2}-{timeValue.Second:d2}");
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HDateTime dateTimeValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Ref");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(dateTimeValue.CopyOfDTO.ToString("o"));
                _haysonWriter.WritePropertyName("tz");
                _haysonWriter.WriteValue(dateTimeValue.TimeZone.ToString());
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HUri uriValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Uri");
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(uriValue.UriVal);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HCoord coordValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("Coord");
                _haysonWriter.WritePropertyName("lat");
                _haysonWriter.WriteValue(coordValue.lat);
                _haysonWriter.WritePropertyName("lng");
                _haysonWriter.WriteValue(coordValue.lng);
                _haysonWriter.WriteEndObject();
                return;
            }
            if (value is HXStr xStrValue)
            {
                _haysonWriter.WriteStartObject();
                _haysonWriter.WritePropertyName("_kind");
                _haysonWriter.WriteValue("XStr");
                _haysonWriter.WritePropertyName("type");
                _haysonWriter.WriteValue(xStrValue.Type);
                _haysonWriter.WritePropertyName("val");
                _haysonWriter.WriteValue(xStrValue.Val);
                _haysonWriter.WriteEndObject();
                return;
            }
        }
    }
}