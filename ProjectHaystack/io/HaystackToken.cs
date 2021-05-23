namespace ProjectHaystack.io
{
    public class HaystackToken
    {
        public string Symbol { get; private set; }
        public bool Literal { get; private set; }

        public static HaystackToken eof = new HaystackToken("eof");

        public static HaystackToken id = new HaystackToken("identifier");
        public static HaystackToken num = new HaystackToken("Number", true);
        public static HaystackToken str = new HaystackToken("Str", true);
        public static HaystackToken @ref = new HaystackToken("Ref", true);
        public static HaystackToken uri = new HaystackToken("Uri", true);
        public static HaystackToken date = new HaystackToken("Date", true);
        public static HaystackToken time = new HaystackToken("Time", true);
        public static HaystackToken dateTime = new HaystackToken("DateTime", true);

        public static HaystackToken colon = new HaystackToken(":");
        public static HaystackToken comma = new HaystackToken(",");
        public static HaystackToken semicolon = new HaystackToken(";");
        public static HaystackToken minus = new HaystackToken("-");
        public static HaystackToken eq = new HaystackToken("==");
        public static HaystackToken notEq = new HaystackToken("!=");
        public static HaystackToken lt = new HaystackToken("<");
        public static HaystackToken lt2 = new HaystackToken("<<");
        public static HaystackToken ltEq = new HaystackToken("<=");
        public static HaystackToken gt = new HaystackToken(">");
        public static HaystackToken gt2 = new HaystackToken(">>");
        public static HaystackToken gtEq = new HaystackToken(">=");
        public static HaystackToken lbracket = new HaystackToken("[");
        public static HaystackToken rbracket = new HaystackToken("]");
        public static HaystackToken lbrace = new HaystackToken("{");
        public static HaystackToken rbrace = new HaystackToken("}");
        public static HaystackToken lparen = new HaystackToken("(");
        public static HaystackToken rparen = new HaystackToken(")");
        public static HaystackToken arrow = new HaystackToken("->");
        public static HaystackToken slash = new HaystackToken("/");
        public static HaystackToken assign = new HaystackToken("=");
        public static HaystackToken bang = new HaystackToken("!");
        public static HaystackToken nl = new HaystackToken("newline");

        public HaystackToken(string symbol)
        {
            Symbol = symbol;
            Literal = false;
        }

        public HaystackToken(string symbol, bool literal)
        {
            Symbol = symbol;
            Literal = literal;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true; // reference check
            if (o == null || (!(o is HaystackToken))) return false; // null and type check

            HaystackToken that = (HaystackToken)o;
            // Value compare
            if (Literal != that.Literal) return false;
            return (Symbol.CompareTo(that.Symbol) == 0);
        }

        public override int GetHashCode()
        {
            int result = Symbol.GetHashCode();
            result = 31 * result + (Literal ? 1 : 0);
            return result;
        }

        public override string ToString()
        {
            return Symbol;
        }
    }
}