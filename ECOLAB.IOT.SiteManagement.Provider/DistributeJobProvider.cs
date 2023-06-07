
namespace ECOLAB.IOT.SiteManagement.Provider
{
    using Azure.Storage.Blobs;
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
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

                    var connectionString = _config["BlobOfRegistry:ConnectionString"];
                    Uri uri = new Uri(gatewayAllowListTask.AllowListUrl);
                    BlobClient blobClient = new BlobClient(uri);
                    var blobContainerName = blobClient.BlobContainerName;
                    var blobName = blobClient.Name;
                    var md5 = await _storageProvider.GetBlobMD5(connectionString, blobContainerName, blobName);

                    var result = await url.WithHeader("Authorization", $"Bearer {token}").PostJsonAsync(new
                    {
                        filetype = "AllowList.json",
                        otaHead="no",
                        checksum= md5,
                        configFileUrl = sasUrl,
                        version = Utility.GenerateAllowListVersion()
                    }).ReceiveJson<DistributeTaskResponseDto>();


                    if (result.status == 200 || result.status == 409)
                    {
                        _logger.LogInformation($"Task distribution sucessfule=>_distributeUrl:{_distributeUrl},sasUrl:{sasUrl},result:{result}");
                        return KeyValuePair.Create(true, sasUrl);
                    }

                    _logger.LogWarning($"Task distribution failed=>_distributeUrl:{_distributeUrl},sasUrl:{sasUrl},result:{result}");

                    return KeyValuePair.Create(false, sasUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Task distribution failed=>{ex.Message}");
                    return KeyValuePair.Create(false, "");
                }
            }
        }
    }
}