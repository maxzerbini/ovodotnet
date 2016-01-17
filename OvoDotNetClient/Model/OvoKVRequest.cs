using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoKVRequest
    {
        public String Key { get; set; }
	    public byte[] Data { get; set; }  
	    public Int32 TTL { get; set; }
        public Int32 Hash { get; set; }  
    }
}
