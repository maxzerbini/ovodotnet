using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient.Util
{
    public static class EndPoints
    {
        public static string CreateTopologyEndpoint() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/cluster");
            return sb.ToString();
        }

        public static string CreateTopologyNodeEndpoint()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/cluster/me");
            return sb.ToString();
        }

        public static string CreateKeysEndpoint()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keys");
            return sb.ToString();
        }

        public static string CreateKeyStorageEndpoint()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keystorage");
            return sb.ToString();
        }

        public static string CreateGetKeyStorageEndpoint(string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keystorage/").Append(key);
            return sb.ToString();
        }

        public static string CreateGetAndRemoveEndpoint(string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keystorage/").Append(key).Append("/getandremove");
            return sb.ToString();
        }

        public static string CreateUpdateValueIfEqualEndpoint(string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keystorage/").Append(key).Append("/updatevalueifequal");
            return sb.ToString();
        }

        public static string CreateCountersEndpoint()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/counters");
            return sb.ToString();
        }

        public static string CreateCounterEndpoint( string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/counters/").Append(key);
            return sb.ToString();
        }

        public static string CreateDeleteValueIfEqualEndpoint(string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/ovo/keystorage/").Append(key).Append("/deletevalueifequal");
            return sb.ToString();
        }
    }
}
