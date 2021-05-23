using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Exception which is thrown by the authentication framework if an error occurs while trying
    /// to authenticat a user.
    /// </summary>
    public class HaystackAuthException : Exception
    {
        public HaystackAuthException(string s)
          : base(s) { }

        public HaystackAuthException(string s, Exception throwable)
          : base(s, throwable) { }
    }
}