using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SKFProductAssistant.Services
{
    public class CacheService
    {
        private readonly IDatabase _cache;

        public CacheService(string redisConnectionString)
        {
            var connection = ConnectionMultiplexer.Connect(redisConnectionString);
            _cache = connection.GetDatabase();
        }

        public async Task<string> GetCachedResponseAsync(string query)
        {
            return await _cache.StringGetAsync(query);
        }

        public async Task CacheResponseAsync(string query, string response)
        {
            await _cache.StringSetAsync(query, response, TimeSpan.FromMinutes(10));
        }
    }
}
