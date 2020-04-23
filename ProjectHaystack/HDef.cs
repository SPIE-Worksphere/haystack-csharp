using System;

namespace ProjectHaystack
{
    public class HDef : HVal
    {
        private string m_val;

        private HDef(string val)
        {
            if (val == null || !val.StartsWith("^") || !HDict.isTagName(val))
                throw new ArgumentException("Invalid def val: \"" + val + "\"");
            m_val = val;
        }

        public static HDef make(string val)
        {
            return new HDef(val);
        }

        public override bool hequals(object obj)
        {
            if (!(obj is HDef)) return false;
            return (m_val.CompareTo(((HDef)obj).ToString()) == 0);
        }

        public override string toJson()
        {
            return m_val;
        }

        public override string toZinc()
        {
            return m_val;
        }
    }
}