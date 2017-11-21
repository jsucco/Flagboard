using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlagBoard_mvc.Helpers
{
    public class AprMangCache
    {
        private readonly string[] MasterCacheKeyArray = { "FlagBoard-AprManager" }; 

        protected object getCacheitem(string rawkey)
        {
            return HttpContext.Current.Cache[GetCacheKey(rawkey)]; 
        }

        private string GetCacheKey(string cacheKey)
        {
            return string.Concat(MasterCacheKeyArray[0], "-", cacheKey); 
        }

        const double CacheDuration = 2; //days 

        public void CacheAdminUsernames(string[] us)
        {
            try
            {
                if (us.Length == 0)
                    return;

                AddCacheItem("AdminUsernames", us); 
            } catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            }
        }

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