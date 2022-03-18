using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollector
{
    public static class Extensions
    {
        public static string ToISOFormat(this DateTime t)
        {
            return t.ToString("yyyyMMDDTHHmmss.fff");
        }
    }
}
