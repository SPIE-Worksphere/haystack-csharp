using System;

namespace ProjectHaystack.Util
{
    public static class UriExtensions
    {
        /// <summary>
        /// Ensure a URI ending with a slash.
        /// </summary>
        public static Uri EndWithSlash(this Uri uri)
        {
            if (uri.AbsolutePath.EndsWith("/"))
            {
                return uri;
            }

            var builder = new UriBuilder(uri);
            builder.Path += "/";
            return builder.Uri;
        }
    }
}