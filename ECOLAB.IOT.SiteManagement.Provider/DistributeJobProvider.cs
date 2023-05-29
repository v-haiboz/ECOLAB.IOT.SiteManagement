
namespace ECOLAB.IOT.SiteManagement.Provider
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public interface IDistributeJobProvider
    {
        public Task<KeyValuePair<bool, string>> DistributeTask(GatewayAllowListTask gatewayAllowListTask, string groupName, string token);


        public class DistributeJobProvider : IDistributeJobProvider
        {
            private readonly IStorageProvider _storageProvider;
            IConfiguration _config;
            private string _distributeUrl;
            private readonly ILogger<DistributeJobProvider> _logger;
            public DistributeJobProvider(IStorageProvider storageProvider, IConfiguration config, ILogger<DistributeJobProvider> logger)
            {
                _storageProvider = storageProvider;
                _config = config;
                _distributeUrl = _config["Distribute:Url"];
                _logger = logger;
            }

            public async Task<KeyValuePair<bool, string>> DistributeTask(GatewayAllowListTask gatewayAllowListTask, string groupName, string token)
            {
                try
                {
                    var url = _distributeUrl.Replace("{dgwSnNO}", gatewayAllowListTask.GatewayNo).Replace("{groupName}", groupName);

                    var sasUrl = await _storageProvider.GetAllowListSASUrl(gatewayAllowListTask.AllowListUrl);
                    if (string.IsNullOrEmpty(sasUrl))
                        return KeyValuePair.Create(false, "");

                    var result = await url.WithHeader("Authorization", $"Bearer {token}").PostJsonAsync(new
                    {
                        netWork = "BLEMesh",
                        configFileUrl = sasUrl,
                        appId = "PVM",
                        fileName = "AllowList.json",
                        version = "1.0.1"
                    }).ReceiveJson<DistributeTaskResponseDto>();


                    if (result.status == 200 || result.status == 409)
                    {
                        _logger.LogInformation($"Task successfully distributed=>_distributeUrl:{_distributeUrl},sasUrl:{sasUrl}");
                        return KeyValuePair.Create(true, sasUrl);
                    }

                    _logger.LogWarning($"Task distribution failed=>_distributeUrl:{_distributeUrl},sasUrl:{sasUrl}");
                    return KeyValuePair.Create(false,"");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Task distribution failed=>{ex.Message}");
                    return KeyValuePair.Create(false, "");
                }
            }
        }
    }
}