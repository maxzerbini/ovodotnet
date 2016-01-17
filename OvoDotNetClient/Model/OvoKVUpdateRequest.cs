using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoKVUpdateRequest : OvoKVRequest
    {
        public String NewKey { get; set; }
        public byte[] NewData { get; set; }
        public Int32 NewHash { get; set; }
    }
}
