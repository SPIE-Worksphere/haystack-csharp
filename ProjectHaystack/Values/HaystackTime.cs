﻿using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack time of day value.
    /// </summary>
    public class HaystackTime : HaystackValue, IComparable
    {
        public HaystackTime(TimeSpan time)
        {
            Value = time;
        }

        public HaystackTime(int hours, int minutes, int seconds)
            : this(new TimeSpan(hours, minutes, seconds))
        {
        }

        public TimeSpan Value { get; private set; }

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackTime time && Value.Equals(time.Value);

        public int CompareTo(object obj)
        {
            if (obj == null || obj is not HaystackTime time)
            {
                return 1;
            }
            return Value.CompareTo(time.Value);
        }
    }
}