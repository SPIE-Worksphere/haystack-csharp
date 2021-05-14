using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    public static class HaystackValueMapper
    {
        public static HaystackValue Map(HVal value)
        {
            if (value == null)
                return null;
            if (value is HBin bin)
                return bin.Source;
            if (value is HBool @bool)
                return @bool.Source;
            if (value is HCoord coord)
                return coord.Source;
            if (value is HDate date)
                return date.Source;
            if (value is HDateTime dateTime)
                return dateTime.Source;
            if (value is HDef def)
                return def.Source;
            if (value is HDict dict)
                return dict.Source;
            if (value is HGrid grid)
                return grid.Source;
            if (value is HHisItem hisItem)
                return hisItem.Source;
            if (value is HList list)
                return list.Source;
            if (value is HMarker marker)
                return marker.Source;
            if (value is HNA na)
                return na.Source;
            if (value is HNum num)
                return num.Source;
            if (value is HRef @ref)
                return @ref.Source;
            if (value is HRemove remove)
                return remove.Source;
            if (value is HRow row)
                return row.Source;
            if (value is HStr str)
                return str.Source;
            if (value is HTime time)
                return time.Source;
            if (value is HUri uri)
                return uri.Source;
            if (value is HXStr xstr)
                return xstr.Source;
            throw new InvalidOperationException($"Cannot map value of type {value.GetType().Name}");
        }

        public static HaystackDictionary Map(HDict value)
        {
            return value.Source;
        }

        public static HaystackGrid Map(HGrid value)
        {
            return value.Source;
        }

        public static HaystackReference Map(HRef value)
        {
            return value.Source;
        }

        public static HaystackReference[] Map(HRef[] value)
        {
            return value.Select(v => v.Source).ToArray();
        }

        public static HaystackNumber Map(HNum value)
        {
            return value.Source;
        }

        public static HaystackDateTimeRange Map(HDateTimeRange value)
        {
            return value.Source;
        }

        public static HaystackHistoryItem[] Map(HHisItem[] value)
        {
            return value.Select(v => v.Source).ToArray();
        }

        public static HaystackColumn Map(HCol value)
        {
            return value.Source;
        }

        public static HaystackTimeZone Map(HTimeZone value)
        {
            return value.Source;
        }

        public static HaystackTime Map(HTime value)
        {
            return value.Source;
        }

        public static HaystackDate Map(HDate value)
        {
            return value.Source;
        }

        public static HaystackDateTime Map(HDateTime value)
        {
            return value.Source;
        }

        public static HVal Map(HaystackValue value)
        {
            if (value == null)
                return null;
            if (value is HaystackBinary bin)
                return new HBin(bin);
            if (value is HaystackBoolean @bool)
                return new HBool(@bool);
            if (value is HaystackCoordinate coord)
                return new HCoord(coord);
            if (value is HaystackDate date)
                return new HDate(date);
            if (value is HaystackDateTime dateTime)
                return new HDateTime(dateTime);
            if (value is HaystackDefinition def)
                return new HDef(def);
            if (value is HaystackDictionary dict)
                return new HDict(dict);
            if (value is HaystackGrid grid)
                return new HGrid(grid);
            if (value is HaystackHistoryItem hisItem)
                return new HHisItem(hisItem);
            if (value is HaystackList list)
                return new HList(list);
            if (value is HaystackMarker marker)
                return new HMarker(marker);
            if (value is HaystackNotAvailable na)
                return new HNA(na);
            if (value is HaystackNumber num)
                return new HNum(num);
            if (value is HaystackReference @ref)
                return new HRef(@ref);
            if (value is HaystackRemove remove)
                return new HRemove(remove);
            if (value is HaystackRow row)
                return new HRow(row);
            if (value is HaystackString str)
                return new HStr(str);
            if (value is HaystackTime time)
                return new HTime(time);
            if (value is HaystackUri uri)
                return new HUri(uri);
            if (value is HaystackXString xstr)
                return new HXStr(xstr);
            throw new InvalidOperationException($"Cannot map value of type {value.GetType().Name}");
        }

        public static HStr Map(HaystackString value)
        {
            return new HStr(value);
        }

        public static HGrid Map(HaystackGrid value)
        {
            return new HGrid(value);
        }

        public static HGrid[] Map(HaystackGrid[] value)
        {
            return value.Select(grid => new HGrid(grid)).ToArray();
        }

        public static HDict Map(HaystackDictionary value)
        {
            return new HDict(value);
        }

        public static HRow Map(HaystackRow value)
        {
            return new HRow(value);
        }

        public static HRef Map(HaystackReference value)
        {
            return new HRef(value);
        }

        public static HDef Map(HaystackDefinition value)
        {
            return new HDef(value);
        }

        public static HCol Map(HaystackColumn value)
        {
            return new HCol(value);
        }

        public static HBin Map(HaystackBinary value)
        {
            return new HBin(value);
        }

        public static HCoord Map(HaystackCoordinate value)
        {
            return new HCoord(value);
        }

        public static HDate Map(HaystackDate value)
        {
            return new HDate(value);
        }

        public static HDateTime Map(HaystackDateTime value)
        {
            return new HDateTime(value);
        }

        public static HTimeZone Map(HaystackTimeZone value)
        {
            return new HTimeZone(value);
        }

        public static HDateTimeRange Map(HaystackDateTimeRange value)
        {
            return new HDateTimeRange(value);
        }

        public static HHisItem Map(HaystackHistoryItem value)
        {
            return new HHisItem(value);
        }

        public static HUri Map(HaystackUri value)
        {
            return new HUri(value);
        }

        public static HTime Map(HaystackTime value)
        {
            return new HTime(value);
        }

        public static HNum Map(HaystackNumber value)
        {
            return new HNum(value);
        }

        public static HRemove Map(HaystackRemove value)
        {
            return new HRemove(value);
        }

        public static HNA Map(HaystackNotAvailable value)
        {
            return new HNA(value);
        }

        public static HMarker Map(HaystackMarker value)
        {
            return new HMarker(value);
        }

        public static HList Map(HaystackList value)
        {
            return new HList(value);
        }

        public static HXStr Map(HaystackXString value)
        {
            return new HXStr(value);
        }

        public static T Checked<T>(Func<T> op, bool chkd)
            where T : class
        {
            if (chkd)
            {
                return op();
            }
            try
            {
                return op();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<T> Checked<T>(Func<Task<T>> op, bool chkd)
            where T : class
        {
            if (chkd)
            {
                return await op();
            }
            try
            {
                return await op();
            }
            catch
            {
                return null;
            }
        }
    }
}