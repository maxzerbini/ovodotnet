using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    public class OvoTopology
    {
        public List<OvoTopologyNode> Nodes { get; set; }

        public OvoTopology() 
        {
            Nodes = new List<OvoTopologyNode>();
        }

        public List<OvoTopologyNode> GetTwins(List<String> names) 
        {
            return Nodes.Where(n => names.Contains(n.Name)).ToList<OvoTopologyNode>();
        }
    }
}
