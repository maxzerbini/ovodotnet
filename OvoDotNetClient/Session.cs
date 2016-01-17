using OvoDotNetClient.Model;
using OvoDotNetClient.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OvoDotNetClient
{
    public class Session : IDisposable
    {
        private RestSharp.RestClient _client;
        private OvoTopologyNode _node;
        
        /// <summary>
        /// Create a session.
        /// </summary>
        public Session(String host, int port) 
        {
            _client = new RestClient(String.Format("http://{0}:{1}",host, port));
        }

        /// <summary>
        /// Create a session for a node.
        /// </summary>
        /// <param name="node"></param>
        public Session(OvoTopologyNode node) 
        {
            this._node = node;
            this._client = new RestClient(String.Format("http://{0}:{1}", node.Host, node.Port));
        }

        #region Public Members 

        /// <summary>
        /// Set the node for the current session.
        /// </summary>
        /// <param name="node"></param>
        public void SetNode(OvoTopologyNode node) 
        {
            this._node = node;
        }

        public String GetNodeName() 
        {
            if (_node != null) return _node.Name;
            else return null;
        }

        public List<String> GetNodeTwins()
        {
            if (_node != null) return _node.Twins;
            else return new List<string>();
        }

        internal OvoTopology GetTopology(string host, int port)
        {
            var response = CallDirectMethod <OvoResponse<OvoTopology>>(Method.GET, EndPoints.CreateTopologyEndpoint(), null);
            if (response.Status == "done")
            {
                return response.Data;
            }
            else
            {
                throw new Exception("Node topology not found.");
            }
        }

        internal void PutRawData(OvoKVRequest req)
        {
            CallDirectMethod<OvoResponse<String>>(Method.POST, EndPoints.CreateKeyStorageEndpoint(), req);
        }

        internal byte[] GetRawData(string key)
        {
            OvoResponse<OvoKVResponse> response = CallDirectMethod<OvoResponse<OvoKVResponse>>(Method.GET, EndPoints.CreateGetKeyStorageEndpoint(key), null);
            if (response != null)
            {
                if (response.Status == "done")
                    return response.Data.Data;
                else
                    return null;
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        public void Dispose()
        {
            _client = null;
            _node = null;
        }
        
        #endregion

        #region Private Members

        /// <summary>
        /// Call a web method with the given arguments
        /// </summary>
        /// <typeparam name="T">expected returning type</typeparam>
        /// <param name="verb">HTTP verb to use</param>
        /// <param name="url">relative endpoint of the REST resource, complete with eventual querystring parameters</param>
        /// <param name="requestData">body of the request</param>
        /// <returns></returns>
        /// <exception cref="MailUp.Sdk.Base.MailUpException">if any error occurred</exception>
        private T CallDirectMethod<T>(Method verb, string url, object requestData)
           where T : class
        {
            var request = new RestRequest(url, verb);
            request.RequestFormat = DataFormat.Json;
            if (requestData != null) request.AddBody(requestData);

            IRestResponse response = null;
            try
            {
                response = _client.Execute(request);
            }
            catch (Exception e)
            {
                throw e;
            }

            string result = response.Content;
            if (string.IsNullOrEmpty(result))
                return default(T);

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
    
}
