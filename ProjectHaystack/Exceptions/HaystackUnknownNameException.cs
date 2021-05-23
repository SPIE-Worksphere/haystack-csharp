using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Exception thrown when a checked lookup for a name in a dict or grid finds no such tag.
    /// </summary>
    public class HaystackUnknownNameException : Exception
    {
        public HaystackUnknownNameException(string message) : base(message)
        {
        }
    }
}