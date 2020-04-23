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

        public override int GetHashCode() => m_val.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is HDef && m_val.Equals(((HDef)obj).m_val);
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