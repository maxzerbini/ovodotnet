using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoCounter
    {
        public String Key { get; set; }
        public Int64 Value { get; set; }
        public Int32 TTL { get; set; }
        public Int32 Hash { get; set; }
    }
}
