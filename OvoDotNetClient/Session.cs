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
    /// <summary>
    /// Http REST client.
    /// </summary>
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

        internal void Delete(string key)
        {
            OvoResponse<string> response = CallDirectMethod<OvoResponse<string>>(Method.DELETE, EndPoints.CreateGetKeyStorageEndpoint(key), null);
            if (response != null)
            {
                return;
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal long Count()
        {
            OvoResponse<long> response = CallDirectMethod<OvoResponse<long>>(Method.GET, EndPoints.CreateKeyStorageEndpoint(), null);
            if (response != null)
            {
                return response.Data;
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal List<string> Keys()
        {
            OvoResponse<OvoKVKeys> response = CallDirectMethod<OvoResponse<OvoKVKeys>>(Method.GET, EndPoints.CreateKeysEndpoint(), null);
            if (response != null)
            {
                return response.Data.Keys;
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal byte[] GetAndRemove(string key)
        {
            OvoResponse<OvoKVResponse> response = CallDirectMethod<OvoResponse<OvoKVResponse>>(Method.GET, EndPoints.CreateGetAndRemoveEndpoint(key), null);
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

        internal bool UpdateValueIfEqual(OvoKVUpdateRequest req)
        {
            OvoResponse<string> response = CallDirectMethod<OvoResponse<string>>(Method.POST, EndPoints.CreateUpdateValueIfEqualEndpoint(req.Key), req);
            if (response != null)
            {
                if (response.Status == "done")
                    return true;
                else
                    return false;
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal long Increment(OvoCounter req)
        {
            OvoCounterResponse response = CallDirectMethod<OvoCounterResponse>(Method.PUT, EndPoints.CreateCountersEndpoint(), req);
            if (response != null)
            {
                if (response.Status == "done")
                {
                    return response.Data.Value;
                }
                else
                {
                    return 0;
                }
            }
            else 
            {
                throw new Exception("Node not found.");
            }
        }

        internal long SetCounter(OvoCounter req)
        {
            OvoCounterResponse response = CallDirectMethod<OvoCounterResponse>(Method.POST, EndPoints.CreateCountersEndpoint(), req);
            if (response != null)
            {
                if (response.Status == "done")
                {
                    return response.Data.Value;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal long GetCounter(string key)
        {
            OvoCounterResponse response = CallDirectMethod<OvoCounterResponse>(Method.GET, EndPoints.CreateCounterEndpoint(key), null);
            if (response != null)
            {
                if (response.Status == "done")
                {
                    return response.Data.Value;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                throw new Exception("Node not found.");
            }
        }

        internal void DeleteCounter(string key)
        {
            OvoCounterResponse response = CallDirectMethod<OvoCounterResponse>(Method.DELETE, EndPoints.CreateCounterEndpoint(key), null);
            if (response != null)
            {
                return;
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
