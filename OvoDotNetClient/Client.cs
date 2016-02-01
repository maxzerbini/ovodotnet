using Newtonsoft.Json;
using OvoDotNetClient.Model;
using OvoDotNetClient.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OvoDotNetClient
{
    /// <summary>
    /// The OVO .Net client.
    /// </summary>
    public class Client : IDisposable, OvoDotNetClient.IOvoClient
    {
        public const String DEFAULT_CONFIG_FILE = "./config.json";
        public const Int32 MAXSERVERNODES = 128;
        public const long DUETIME = 10000;
        public const long PERIOD = 10000;
        private Configuration _config;
        private OvoTopology _topology;
        private ConcurrentDictionary<String, Session> _clients;
        private ConcurrentDictionary<Int32, Session> _clientsHash;
        private Timer _timer;
        private TextWriter _log;

        #region Constructor
        /// <summary>
        /// Create the client reading the default configuration file.
        /// </summary>
        public Client() 
        {
            _clients = new ConcurrentDictionary<string, Session>();
            _clientsHash = new ConcurrentDictionary<Int32, Session>();
            _timer = new Timer(this.Check, this, DUETIME, PERIOD);
            LoadConfiguration(DEFAULT_CONFIG_FILE);
            Init();
        }
        /// <summary>
        /// Create the client usint the input configuration.
        /// </summary>
        /// <param name="config">the client's configuration</param>
        public Client(Configuration config) 
        {
            _clients = new ConcurrentDictionary<string, Session>();
            _clientsHash = new ConcurrentDictionary<Int32, Session>();
            _timer = new Timer(this.Check, this, DUETIME, PERIOD);
            this._config = config;
            Init();
        }
        /// <summary>
        /// Create the client reading the configuration file from the input path.
        /// </summary>
        /// <param name="path">the input file path</param>
        public Client(String path)
        {
            _clients = new ConcurrentDictionary<string, Session>();
            _clientsHash = new ConcurrentDictionary<Int32, Session>();
            _timer = new Timer(this.Check, this, DUETIME, PERIOD);
            LoadConfiguration(path);
            Init();
        }
        #endregion
        
        #region Public Members

        /// <summary>
        /// Set the logger.
        /// </summary>
        /// <param name="output"></param>
        public void SetLog(TextWriter output)
        {
            _log = output;
        }
        /// <summary>
        /// Dispose all connections.
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            foreach (var s in _clients.Values) 
            {
                s.Dispose();
            }
        }
        /// <summary>
        /// Put data in raw format into the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="data">the raw data</param>
        /// <param name="ttl">the time to live in seconds</param>
        public void PutRawData(String key, Byte[] data, Int32 ttl = 0) 
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoKVRequest req = new OvoKVRequest() 
            { 
                Data = data,
                Hash = hash,
                Key = key,
                TTL = ttl,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    s.PutRawData(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins())) 
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            st.PutRawData(req);
                            done = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Put data in raw format into the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="data">the data is serialized in JSON and encoded in UTF-8</param>
        /// <param name="ttl">the time to live in seconds</param>
        public void Put(String key, Object data, Int32 ttl = 0)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoKVRequest req = new OvoKVRequest()
            {
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
                Hash = hash,
                Key = key,
                TTL = ttl,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    s.PutRawData(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            st.PutRawData(req);
                            done = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Get a raw format rapresentation of the object stored in the OVO cluster.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the raw data or null if not found</returns>
        public Byte[] GetRawData(String key) 
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    Byte[] data = s.GetRawData(key);
                    return data;
                }
                catch (Exception ex)
                {
                    var done = false;
                    Byte[] data = null;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            data = s.GetRawData(key);
                            done = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return data;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Retrieve an object previously serialized in JSON.
        /// </summary>
        /// <typeparam name="T">the type of the object</typeparam>
        /// <param name="key">the key</param>
        /// <returns>the deserialized object or null if not found</returns>
        public T Get<T>(String key)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    Byte[] data = s.GetRawData(key);
                    if (data == null) return default(T);
                    T obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
                    return obj;
                }
                catch (Exception ex)
                {
                    var done = false;
                    Byte[] data = null;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            data = s.GetRawData(key);
                            done = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(string.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        if (data == null) return default(T);
                        T obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
                        return obj;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Delete an entry from the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        public void Delete(String key)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    s.Delete(key);
                }
                catch (Exception ex)
                {
                    var done = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            s.Delete(key);
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(string.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Give the number of object store in every node (also replicated object are counted).
        /// </summary>
        /// <returns>the number of object (also replicated ones are counted)</returns>
        public long Count()
        {
            long count = 0L;
            foreach (var node in _clients.Values) 
            {
                try
                {
                    count += node.Count();
                }
                catch (Exception ex)
                {
                    Log(string.Format("Fail connecting node {0} and its twins due to {1}", node.GetNodeName(), ex.Message));
                }
            }
            return count;
        }
        /// <summary>
        /// Get the list of all the keys.
        /// </summary>
        /// <returns>the list of keys.</returns>
        public List<string> Keys()
        {
            Dictionary<string, bool> keys = new Dictionary<string,bool>();
            foreach (var node in _clients.Values)
            {
                try
                {
                    List<string> list = node.Keys();
                    foreach (var k in list) 
                    {
                        keys[k] = true;
                    }

                }
                catch (Exception ex)
                {
                    Log(string.Format("Fail connecting node {0} and its twins due to {1}", node.GetNodeName(), ex.Message));
                }
            }
            return keys.Keys.ToList();
        }
        /// <summary>
        /// Retrieve an object previously serialized in JSON and remove it from the storage.
        /// </summary>
        /// <typeparam name="T">the type of the object</typeparam>
        /// <param name="key">the key</param>
        /// <returns>the deserialized object or null if not found</returns>
        public T GetAndRemove<T>(String key)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    Byte[] data = s.GetAndRemove(key);
                    if (data == null) return default(T);
                    T obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
                    return obj;
                }
                catch (Exception ex)
                {
                    var done = false;
                    Byte[] data = null;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            data = s.GetAndRemove(key);
                            done = true;
                            break;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(string.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        if (data == null) return default(T);
                        T obj = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
                        return obj;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Update an object with the newData if the oldData is equal to the stored data.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="oldData">the old data value</param>
        /// <param name="newData">the new data value</param>
        /// <returns>true if the operation is correctly done, else false</returns>
        public bool UpdateValueIfEqual(String key, Object oldData, Object newData)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoKVUpdateRequest req = new OvoKVUpdateRequest()
            {
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(oldData)),
                Hash = hash,
                Key = key,
                NewData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newData)),
                NewKey = key,
                NewHash = hash,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    return s.UpdateValueIfEqual(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    bool result = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            result = result || st.UpdateValueIfEqual(req);
                            done = true;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Increment (or decrement) the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the increment value (can be negative)</param>
        /// <param name="ttl">the time to live of the counter</param>
        /// <returns>the update value of the counter (0 if the counter was not found)</returns>
        public long Increment(String key, long value, Int32 ttl = 0)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoCounter req = new OvoCounter()
            {
                Value = value,
                Hash = hash,
                Key = key,
                TTL = ttl,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    return s.Increment(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    long result = 0;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            result = st.Increment(req);
                            done = true;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Set the value of the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the counter value</param>
        /// <param name="ttl">the time to live</param>
        /// <returns>the setted counter value</returns>
        public long SetCounter(String key, long value, Int32 ttl = 0)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoCounter req = new OvoCounter()
            {
                Value = value,
                Hash = hash,
                Key = key,
                TTL = ttl,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    return s.SetCounter(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    long result = 0;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            result = st.SetCounter(req);
                            done = true;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Get the value of the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the value of the counter</returns>
        public long GetCounter(String key)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    return s.GetCounter(key);
                }
                catch (Exception ex)
                {
                    var done = false;
                    long result = 0;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            result = st.GetCounter(key);
                            return result;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Delete a counter.
        /// </summary>
        /// <param name="key">the key</param>
        public void DeleteCounter(String key)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    s.DeleteCounter(key);
                }
                catch (Exception ex)
                {
                    var done = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            st.DeleteCounter(key);
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }
        /// <summary>
        /// Delete an object if its value is not changed.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="oldData">the value</param>
        /// <returns>true if the object has been deleted else false</returns>
        public bool DeleteValueIfEqual(String key, Object oldData)
        {
            if (key == null) throw new Exception("Null key not allowed.");
            Int32 hash = HashCodeHelper.GetPositiveHashCode(key, MAXSERVERNODES);
            OvoKVRequest req = new OvoKVRequest()
            {
                Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(oldData)),
                Hash = hash,
                Key = key,
            };
            var s = _clientsHash[hash];
            if (s != null)
            {
                try
                {
                    return s.DeleteValueIfEqual(req);
                }
                catch (Exception ex)
                {
                    var done = false;
                    bool result = false;
                    foreach (var twin in _topology.GetTwins(s.GetNodeTwins()))
                    {
                        try
                        {
                            var st = _clients[twin.Name];
                            result = result || st.DeleteValueIfEqual(req);
                            done = true;
                        }
                        catch (Exception)
                        {
                            Log(String.Format("Fail connecting node {0}", twin.Name));
                        }
                    }
                    if (!done)
                    {
                        Log(String.Format("Fail connecting node {0} and its twins", s.GetNodeName()));
                        throw ex;
                    }
                    else
                    {
                        CheckCluster();
                        return result;
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Server Node not found for hashcode {0}.", hash));
            }
        }

        #endregion


        #region Private Members
        /// <summary>
        /// Load the configuration file.
        /// </summary>
        /// <param name="path"></param>
        private void LoadConfiguration(string path) 
        {
            try
            {
                this._config = Configuration.LoadConfiguration(path);
            }
            catch (Exception ex)
            {
                // Log if it's enabled
                Log(ex);
                throw ex;
            }
        }

        private void Init() 
        {
            InitTopology();
            RebuildClients();
        }

        private void InitTopology() 
        {
            foreach (var node in _config.ClusterNodes) 
            {
                try
                {
                    Session s = new Session(node.Host, node.Port);
                    OvoTopology t = s.GetTopology(node.Host, node.Port);
                    if (t != null)
                    {
                        _topology = t;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // Log if it's enabled
                    Log(ex);
                }
            }
            if (_topology == null) throw new Exception("The connections to all nodes are failed.");
        }

        private void RebuildClients() 
        {
            foreach (var node in _topology.Nodes) 
            {
                Session s = new Session(node);
                foreach (var hash in node.HashRange) 
                {
                    _clientsHash[hash] = s;
                }
                _clients[node.Name] = s;
            }
        }

        private void CheckTopology()
        {
            OvoTopology t = null;
            foreach (var node in _topology.Nodes)
            {
                Session s = new Session(node.Host, node.Port);
                t = s.GetTopology(node.Host, node.Port);
                if (t != null)
                {
                    break;
                }
            }
            if (t != null)
            {
                _topology = t;
            }
        }

        private void CheckCluster() 
        {
            CheckTopology();
            RebuildClients();
        }

        private void Check(object state)
        {
            try 
	        {
                CheckCluster();
	        }
	        catch (Exception ex)
	        {
                // Log if it's enabled
                Log(ex);
	        };
        }

        protected void Log(String message) 
        {
            if (_log != null) _log.WriteLine(String.Format("{0} {1}", DateTime.Now, message));
        }
        protected void Log(Exception ex)
        {
            if (_log != null) _log.WriteLine(string.Format("{0} {1}", DateTime.Now, ex.Message));
        }
        #endregion
    }
}
