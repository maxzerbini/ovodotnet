using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Model
{
    /// <summary>
    /// The client configuration.
    /// </summary>
    public class Configuration
    {
        public List<Node> ClusterNodes {get; set;}

        public Configuration() 
        {
            ClusterNodes = new List<Node>();
        }

        public static Configuration LoadConfiguration(string path) 
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Configuration conf = JsonConvert.DeserializeObject<Configuration>(json);
                return conf;
            }
        }
    }

    public class Node 
    {
        public String Host { get; set; }
        public Int32 Port { get; set; }
    }
}
