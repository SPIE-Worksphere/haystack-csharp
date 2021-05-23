using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProjectHaystack.Validation
{
    public static class HaystackValidator
    {
        private static bool[] _idCharMatches = GetCharMatchArray("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_:-.~");
        private static bool[] _tagCharMatches = GetCharMatchArray("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_^");
        private static bool[] _tagStartCharMatches = GetCharMatchArray("abcdefghijklmnopqrstuvwxyz^");
        private static bool[] _typeStartCharMatches = GetCharMatchArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ^");
        private static bool[] _unitCharMatches = GetCharMatchArray("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_$%/");

        /// <summary>
        /// Validate latitude value.
        /// </summary>
        public static bool IsLatitude(decimal latitude)
        {
            return -90 <= latitude && latitude <= 90;
        }

        /// <summary>
        /// Validate longitude value.
        /// </summary>
        public static bool IsLongitude(decimal longitude)
        {
            return -180 <= longitude && longitude <= 180;
        }

        /// <summary>
        /// Validate tag name.
        /// The first char must be an ASCII lower case letter.
        /// The rest must be ASCII letters, digits or underscore.
        /// </summary>
        public static bool IsTagName(string tag)
        {
            return !string.IsNullOrEmpty(tag) && IsCharMatch(_tagCharMatches, tag) && IsCharMatch(_tagStartCharMatches, tag[0]);
        }

        /// <summary>
        /// Validate a reference ID.
        /// </summary>
        public static bool IsReferenceIdChar(char chr) => IsCharMatch(_idCharMatches, chr);

        /// <summary>
        /// Validate a reference ID.
        /// </summary>
        public static bool IsReferenceId(string id) => !string.IsNullOrEmpty(id) && IsCharMatch(_idCharMatches, id);

        /// <summary>
        /// Validate a mime type.
        /// </summary>
        public static bool IsMimeType(string mime) => !string.IsNullOrEmpty(mime) && Regex.IsMatch(mime, @"^[\w-]+\/[-.\w]+(?:\+[-.\w]+)?$");

        /// <summary>
        /// Validate a unit name.
        /// </summary>
        public static bool IsUnitName(string unit) => unit == null || (unit.Length > 0 && IsCharMatch(_unitCharMatches, unit, chr => chr > 128));

        /// <summary>
        /// Validate a type name.
        /// </summary>
        public static bool IsTypeName(string type)
        {
            return !string.IsNullOrEmpty(type) && IsCharMatch(_tagCharMatches, type) && IsCharMatch(_typeStartCharMatches, type[0]);
        }

        #region Quick string validation

        /// <summary>
        /// Get an array to quickly look up character validation.
        /// To be used in conjunction with <see cref="IsCharMatch"/>.
        /// </summary>
        /// <param name="chars">List of valid characters.</param>
        /// <returns>Array of booleans.</returns>
        private static bool[] GetCharMatchArray(string chars) => Enumerable.Range(0, chars.Max() + 1).Select(i => chars.Contains((char)i)).ToArray();

        /// <summary>
        /// Quick validation of a character against a list of valid characters.
        /// To be used in conjuction with <see cref="GetCharMatchArray"/>.
        /// </summary>
        /// <param name="charMatches">Array with char validations.</param>
        /// <param name="value">Value to check.</param>
        /// <returns>Whether the character matches.</returns>
        private static bool IsCharMatch(bool[] charMatches, char value)
        {
            return value >= 0 && value < charMatches.Length && charMatches[value];
        }

        /// <summary>
        /// Quick validation of a string against a list of valid characters.
        /// To be used in conjuction with <see cref="GetCharMatchArray"/>.
        /// </summary>
        /// <param name="charMatches">Array with char validations.</param>
        /// <param name="value">Value to check.</param>
        /// <param name="forceValid">Validator that is fired first to skip other validation.</param>
        /// <returns>Whether all characters match.</returns>
        private static bool IsCharMatch(bool[] charMatches, string value, Func<char, bool> forceValid = null)
        {
            if (forceValid == null)
            {
                forceValid = _ => false;
            }

            foreach (var chr in value)
            {
                if (!forceValid(chr) && (chr >= charMatches.Length || !charMatches[chr]))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion Quick string validation
    }
}