using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celer.Utilities
{
    public class XML
    {
        public static string ExtractXmlValue(string xml, string key)
        {
            int idx = xml.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return "Unknown";

            var snippet = xml.Substring(idx, 200);
            var value = snippet.Split('>').Skip(1).FirstOrDefault()?.Split('<').FirstOrDefault();
            return value?.Trim() ?? "Unknown";
        }
    }
}
