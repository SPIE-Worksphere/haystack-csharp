using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHaystack.io
{
    public class TrioReader
    {
        public HGrid ToGrid(Stream trioStream)
        {
//entities are separated by lines beginning with "-", the lines can have as many dashes as you want
//each entity is defined by one or more tags
//one line is used per tag formatted as "name:val"
//if no value is specified, the value is assumed to be Marker
//the value is encoded using the same grammar as Zinc
//string values may be left unquoted if they begin with a non-ASCII Unicode character or contain only the "safe" chars: A-Z, a-z, underbar, dash, or space
//if a newline follows the colon, then the value is an indented multi-line string terminated by the first non-indented line
//nested grids are encoded as a multi-line string prefixed with the string value "Zinc:" on the tag line
//can use // as line comment
            throw new NotImplementedException();
        }
    }
}