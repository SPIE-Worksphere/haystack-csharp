//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Text;

namespace ProjectHaystack.Util
{

    /// <summary>
    /// Base64 handles various methods of encoding and decoding
    /// base 64 format.
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// Return a Base64 codec that uses standard Base64 format.
        /// </summary>
        public static Base64 STANDARD = new Base64("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), "=");

        /// <summary>
        /// Return a Base64 codec that uses a custom, Uri-friendly Base64 format.
        /// <para>
        /// This codec <i>mostly</i> follows the RFC 3548 standard for Base64.
        /// It uses '-' and '_' instead of '+' and '/' (as per RFC 3548),
        /// This coded uses no padding character.
        /// </para>
        /// <para>
        /// This approach allows us to Encode and Decode HRef instances.
        /// HRef has five special chars available for us to Use: ':', '.', '-', '_', '~'.
        /// We are using three of them here, leaving two still available: ':' and '.'
        /// </para>
        /// </summary>
        public static Base64 URI = new Base64("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray());

        ////////////////////////////////////////////////////////////////
        // constructor
        ////////////////////////////////////////////////////////////////

        private Base64(char[] alphabet)
          : this(alphabet, null)
        {
        }
        private Base64(char[] alphabet, string pad)
        {
            this.base64chars = alphabet;
            this.pad = pad;
            for (int i = 0; i < base64inv.Length; ++i)
            {
                base64inv[i] = -1;
            }
            for (int i = 0; i < base64chars.Length; ++i)
            {
                base64inv[base64chars[i]] = i;
            }
        }

        ////////////////////////////////////////////////////////////////
        // Util
        ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sniff the string to determine how it was encoded and then Decode it to sbyte[] </summary>
        public static byte[] DecodeUtf8(string str)
        {
            if (str.EndsWith("=", StringComparison.Ordinal) || str.IndexOf('+') >= 0 || str.IndexOf('/') >= 0)
            {
                return Base64.STANDARD.DecodeBytes(str);
            }
            else
            {
                return Base64.URI.DecodeBytes(str);
            }
        }

        ////////////////////////////////////////////////////////////////
        // API
        ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Encode the string to base 64, using the platform's default charset.
        /// </summary>
        public virtual string Encode(string str)
        {
            return EncodeBytes(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Encode the string to base 64, using the UTF8 charset.
        /// </summary>
        public virtual string EncodeUtf8(string str)
        {
            return EncodeBytes(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Decode the string from base 64, using the platform's default charset.
        /// </summary>
        public virtual string Decode(string str)
        {
            return Encoding.UTF8.GetString(DecodeBytes(str));
        }

        /// <summary>
        /// Decode the string from base 64, using the UTF8 charset.
        /// </summary>
        public virtual string decodeUTF8(string str)
        {
            return Encoding.UTF8.GetString(DecodeBytes(str));
        }

        /// <summary>
        /// Encode the byte array to base 64.
        /// </summary>
        public virtual string EncodeBytes(byte[] buf)
        {
            char[] table = this.base64chars;
            int size = buf.Length;
            StringBuilder s = new StringBuilder(size * 2);
            int i = 0;

            // append full 24-bit chunks
            int end = size - 2;
            for (; i < end; i += 3)
            {
                int n = ((buf[i] & 0xff) << 16) + ((buf[i + 1] & 0xff) << 8) + (buf[i + 2] & 0xff);
                s.Append(table[((int)((uint)n >> 18)) & 0x3f]);
                s.Append(table[((int)((uint)n >> 12)) & 0x3f]);
                s.Append(table[((int)((uint)n >> 6)) & 0x3f]);
                s.Append(table[n & 0x3f]);
            }

            // pad and Encode remaining bits
            int rem = size - i;
            if (rem > 0)
            {
                int n = ((buf[i] & 0xff) << 10) | (rem == 2 ? ((buf[size - 1] & 0xff) << 2) : 0);
                s.Append(table[((int)((uint)n >> 12)) & 0x3f]);
                s.Append(table[((int)((uint)n >> 6)) & 0x3f]);

                if (rem == 2)
                {
                    s.Append(table[n & 0x3f]);
                }
                else if (HasPad())
                {
                    s.Append(PadChar());
                }

                if (HasPad())
                {
                    s.Append(PadChar());
                }
            }

            return s.ToString();
        }

        /// <summary>
        /// Decode the byte array from base 64.
        /// </summary>
        public virtual byte[] DecodeBytes(string s)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((s.Length % 4) == 0)
                {
                    break;
                }
                s = s.PadRight(s.Length + 1, '=');
                if ((s.Length % 4) == 0)
                {
                    break;
                }
            }
            byte[] test = System.Convert.FromBase64String(s);
            sbyte[] signedTest = Array.ConvertAll(test, b => unchecked((sbyte)b));
            return test;
        }

        private bool HasPad()
        {
            return !string.ReferenceEquals(pad, null);
        }
        private char PadChar()
        {
            return this.pad[0];
        }

        ////////////////////////////////////////////////////////////////
        // Attributes
        ////////////////////////////////////////////////////////////////

        private readonly char[] base64chars;
        private readonly int[] base64inv = new int[128];
        private readonly string pad;
    }
}