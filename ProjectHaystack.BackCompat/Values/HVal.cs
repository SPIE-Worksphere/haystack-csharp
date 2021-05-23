using System;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackValue")]
    public abstract class HVal : IComparable
    {
        public HVal() { }
        public override string ToString() { return toZinc(); }
        public abstract string toZinc();
        public abstract string toJson();
        public virtual bool hequals(object that) => Equals(that);
        public virtual int hashCode() => GetHashCode();
        public virtual int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }

        public static bool operator ==(HVal left, HVal right) => Equals(left, null) ? Equals(right, null) : left.Equals(right);
        public static bool operator !=(HVal left, HVal right) => !(left == right);
    }
}