using System;

namespace AStar.DataProvider
{
    internal interface ICacheService
    {
        T GetOrSet<T>(string cacheKey, Func<T> getItemCallback) where T : class;
    }
}