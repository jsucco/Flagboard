using System;
using System.Web;

namespace FlagBoard_mvc.Helpers
{
    public class CtxCache
    {
        private readonly string[] MasterCacheKeyArray = { "APR-Flagboard" };

        protected object getCacheitem(string rawKey)
        {
            return HttpContext.Current.Cache[GetCacheKey(rawKey)];
        }

        private string GetCacheKey(string cacheKey)
        {
            return string.Concat(MasterCacheKeyArray[0], "-", cacheKey);
        }

        const double CacheDuration = 1; //days 
        protected void AddCacheItem(string rawKey, object value)
        {
            System.Web.Caching.Cache DataCache = HttpContext.Current.Cache;
            // Make sure cache dependency key is in the cache - if not , add it 
            if (DataCache[MasterCacheKeyArray[0]] == null)
                DataCache[MasterCacheKeyArray[0]] = DateTime.Now;

            // Add Dependency 
            System.Web.Caching.CacheDependency dependency =
                new System.Web.Caching.CacheDependency(null, MasterCacheKeyArray);

            DataCache.Insert(GetCacheKey(rawKey), value, dependency,
                DateTime.Now.AddDays(CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        protected void InvalidateCache()
        {
            HttpContext.Current.Cache.Remove(MasterCacheKeyArray[0]);
        }

        protected void InvalidateCacheRawKey(string RawKey)
        {
            HttpContext.Current.Cache.Remove(GetCacheKey(RawKey));
        }
    }
}