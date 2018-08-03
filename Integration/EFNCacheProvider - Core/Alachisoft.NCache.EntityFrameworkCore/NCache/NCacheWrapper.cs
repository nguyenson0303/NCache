using Microsoft.Extensions.Caching.Memory;
using System;
using Alachisoft.NCache.Web.Caching;
using Alachisoft.NCache.Runtime.Caching;
using System.Collections;
using Alachisoft.NCache.EntityFrameworkCore.NCache;
using System.Collections.Generic;

namespace Alachisoft.NCache.EntityFrameworkCore
{
    internal class NCacheWrapper /*: Cache*///, IMemoryCache
    {
        Alachisoft.NCache.Web.Caching.Cache _nCache;
        private DefaultKeyGenerator _defaultKeyGen;

        internal Alachisoft.NCache.Web.Caching.Cache NCacheInstance => _nCache;

        public DefaultKeyGenerator DefaultKeyGen => _defaultKeyGen;

        public NCacheWrapper(MemoryCacheOptions options)
        {
            if (!NCacheConfiguration.IsConfigured)
                throw new Exception("NCache initialization configuration is not provided. Please use Alachisoft.NCache.EntityFrameworkCore.NCacheConfiguration.Configure() method to configure NCache before using it.");
            if (NCacheConfiguration.InitParams != null)
                _nCache = Web.Caching.NCache.InitializeCache(NCacheConfiguration.CacheId, NCacheConfiguration.InitParams);
            else
                _nCache = Web.Caching.NCache.InitializeCache(NCacheConfiguration.CacheId);

            _defaultKeyGen = new DefaultKeyGenerator();
        }

        internal CacheEntry CreateEntry(object key)
        {
            Logger.Log(
                "Creating cache entry against key '" + key + "'.",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            var cacheEntry = new CacheEntry(key, _nCache);
            //_cache.Add(key.ToString(), cacheEntry);
            return cacheEntry;
        }

        public void Insert(object key, CacheItem value)
        {
            Logger.Log(
                "Inserting item '" + value + "' against key '" + key + "'.",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            _nCache.Insert(key.ToString(), value);
        }

        internal void InsertBulk(object[] keys, CacheItem[] values)
        {
            Logger.Log(
                "Inserting items in bulk against respective keys.",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            if (keys.Length > 0)
            {
                string[] strKeys = new string[keys.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    strKeys[i] = keys[i].ToString();
                }
                IDictionary issues = _nCache.InsertBulk(strKeys, values);
                if (issues.Count > 0)
                {
                    _nCache.RemoveBulk(strKeys);

                    IDictionaryEnumerator enumerator = issues.GetEnumerator();
                    enumerator.MoveNext();
                    throw (Exception)enumerator.Entry.Value;
                }
            }
        }

        public new void Remove(object key)
        {
            Logger.Log(
                "Removing item against key '" + key + "'",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            _nCache.Remove(key.ToString());
        }

        public bool TryGetValue(object key, out object value)
        {
            Logger.Log(
                "Trying to get item against key '" + key + "'",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            value = _nCache.Get(key.ToString());
            return value != null;
        }

        public bool GetByTags(Tag tag, out Hashtable value)
        {
            Logger.Log(
                "Trying to get item against tag '" + tag + "'",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            Hashtable resultSet = _nCache.GetByTag(tag);
            value = resultSet;
            return resultSet.Count > 0;
        }
        public bool GetByKey(string key, out IDictionary value)
        {
            Logger.Log(
                "Trying to get item against Key '" + key + "'",
                Microsoft.Extensions.Logging.LogLevel.Trace
            );
            IDictionary resultSet = new Hashtable();
            object listKeys =_nCache.Get(key);
            try
            {
                if (listKeys != null)
                {
                    var keys = listKeys as string[];
                    if(keys==null)
                    {
                        CacheEntry entry = listKeys as CacheEntry;
                        //to do:: Get Values from Cache against keys
                        resultSet.Add(key, listKeys);
                    }
                    else
                    {
                        resultSet = _nCache.GetBulk(keys);
                    }

                    /*                int index = 0;
                                    foreach(var itm in items)
                                    {
                                        resultSet.Add(keys[index], itm);
                                        index++;
                                    }
                    */
                }
            }
            catch(Exception e)
            {
                //casting exception
            }
            value = resultSet;
            return resultSet.Count > 0;

        }

        public void Dispose()
        {
            _nCache = null;
        }
    }
}
