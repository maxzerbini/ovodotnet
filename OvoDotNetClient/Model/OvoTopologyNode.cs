using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoTopologyNode
    {
        public String Name { get; set; }
	    public List<Int32> HashRange { get; set; }
	    public String Host { get; set; }
	    public Int32 Port  { get; set; }
	    public String State  { get; set; }
        public List<String> Twins { get; set; }

        public OvoTopologyNode() 
        {
            HashRange = new List<Int32>();
            Twins = new List<String>();
        }
    }
}
