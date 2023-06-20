namespace ECOLAB.IOT.SiteManagement.Service
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public interface IMemoryCacheService
    {
        /// <summary>
        /// Get Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetValue<T>(string key);

        /// <summary>
        /// Set Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isAbsoluteExpiration"></param>
        /// <param name="expirationSeconds"></param>
        void SetValue(string key, object value, bool isAbsoluteExpiration, long? expirationSeconds);

        /// <summary>
        /// Remove Value
        /// </summary>
        /// <param name="key"></param>
        public void RemoveValue(string key);
        /// <summary>
        /// Gets the item associated with this key if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(string key, out T value);

    }
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private string keyPrefix;
        private double slidingExpirationSeconds;
        private double absoluteExpirationSeconds;

        public MemoryCacheService(IMemoryCache memoryCache
            , ILogger<MemoryCacheService> logger, IConfiguration config)
        {
            _config = config;
            _memoryCache = memoryCache;
            keyPrefix = _config["Cache:KeyPrefix"];
            slidingExpirationSeconds = double.Parse(_config["Cache:SlidingExpirationSeconds"]);
            absoluteExpirationSeconds = double.Parse(_config["Cache:AbsoluteExpirationSeconds"]);
            _logger = logger;
        }
        public T GetValue<T>(string key)
        {
            key = keyPrefix + key;
            try
            {
                var value = _memoryCache.Get(key);
                if (value != null)
                {
                    return (T)value;
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return default(T);
            }
        }

        public void SetValue(string key, object value, bool isAbsoluteExpiration, long? expirationSeconds)
        {
            key = keyPrefix + key;
            try
            {
                var options = new MemoryCacheEntryOptions();
                if (isAbsoluteExpiration)
                {
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds.HasValue ? expirationSeconds.Value : absoluteExpirationSeconds);
                }
                else
                {
                    options.SlidingExpiration = TimeSpan.FromSeconds(expirationSeconds.HasValue ? expirationSeconds.Value : slidingExpirationSeconds);
                }
                _memoryCache.Set(key, value, options);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        public void RemoveValue(string key)
        {
            key = keyPrefix + key;
            try
            {
                _memoryCache.Remove(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public bool TryGetValue<T>(string key, out T defValue) 
        {
            bool flag = false;
            key = keyPrefix + key;
            try
            {
                flag = _memoryCache.TryGetValue(key, out var value);
                if (!flag)
                {
                    defValue = default(T);
                }
                else
                {
                    defValue = (T)value;
                }
            }
            catch (Exception e)
            {
                defValue = default(T);
                _logger.LogError(e.Message);
            }


            return flag;
        }
    }
}
