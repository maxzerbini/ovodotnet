using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoKVKeys
    {
        public List<String> Keys { get; set; }

        public OvoKVKeys() 
        {
            Keys = new List<String>();
        }
    }
}
