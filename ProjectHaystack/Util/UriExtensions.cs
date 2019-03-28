using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHaystack.Util
{
    public static class UriExtensions
    {
        public static Uri EndWithSlash(this Uri uri)
        {
            if (!uri.AbsolutePath.EndsWith("/"))
            {
                var builder = new UriBuilder(uri);
                builder.Path += "/";
                uri = builder.Uri;
            }

            return uri;
        }
    }
}
