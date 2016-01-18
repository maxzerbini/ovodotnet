using System;
namespace OvoDotNetClient
{
    /// <summary>
    /// OVO client interface.
    /// </summary>
    public interface IOvoClient
    {
        /// <summary>
        /// Give the number of object store in every node (also replicated object are counted).
        /// </summary>
        /// <returns>the number of object (also replicated ones are counted)</returns>
        long Count();
        /// <summary>
        /// Delete an entry from the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        void Delete(string key);
        /// <summary>
        /// Delete a counter.
        /// </summary>
        /// <param name="key">the key</param>
        void DeleteCounter(string key);
        /// <summary>
        /// Retrieve an object previously serialized in JSON.
        /// </summary>
        /// <typeparam name="T">the type of the object</typeparam>
        /// <param name="key">the key</param>
        /// <returns>the deserialized object or null if not found</returns>
        T Get<T>(string key);
        /// <summary>
        /// Retrieve an object previously serialized in JSON and remove it from the storage.
        /// </summary>
        /// <typeparam name="T">the type of the object</typeparam>
        /// <param name="key">the key</param>
        /// <returns>the deserialized object or null if not found</returns>
        T GetAndRemove<T>(string key);
        /// <summary>
        /// Get the value of the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the value of the counter</returns>
        long GetCounter(string key);
        /// <summary>
        /// Get a raw format rapresentation of the object stored in the OVO cluster.
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the raw data or null if not found</returns>
        byte[] GetRawData(string key);
        /// <summary>
        /// Increment (or decrement) the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the increment value (can be negative)</param>
        /// <param name="ttl">the time to live of the counter</param>
        /// <returns>the update value of the counter (0 if the counter was not found)</returns>
        long Increment(string key, long value, int ttl);
        /// <summary>
        /// Get the list of all the keys.
        /// </summary>
        /// <returns>the list of keys.</returns>
        System.Collections.Generic.List<string> Keys();
        /// <summary>
        /// Put data in raw format into the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="data">the data is serialized in JSON and encoded in UTF-8</param>
        /// <param name="ttl">the time to live in seconds</param>
        void Put(string key, object data, int ttl);
        /// <summary>
        /// Put data in raw format into the OVO storage.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="data">the raw data</param>
        /// <param name="ttl">the time to live in seconds</param>
        void PutRawData(string key, byte[] data, int ttl);
        /// <summary>
        /// Set the value of the counter.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="value">the counter value</param>
        /// <param name="ttl">the time to live</param>
        /// <returns>the setted counter value</returns>
        long SetCounter(string key, long value, int ttl);
        /// <summary>
        /// Update an object with the newData if the oldData is equal to the stored data.
        /// </summary>
        /// <param name="key">the key</param>
        /// <param name="oldData">the old data value</param>
        /// <param name="newData">the new data value</param>
        /// <returns>true if the operation is correctly done, else false</returns>
        bool UpdateValueIfEqual(string key, object oldData, object newData);
    }
}
