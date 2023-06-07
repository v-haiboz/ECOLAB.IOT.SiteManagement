
namespace ECOLAB.IOT.SiteManagement.Provider
{
    using Azure.Storage.Blobs;
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Flurl.Http;
    using Microsoft.Extensions.Configuration;

    public interface IDistributeJobProvider
    {
        public Task<KeyValuePair<bool, string>> DistributeTask(GatewayAllowListTask gatewayAllowListTask, string groupName, string token);


        public class DistributeJobProvider : IDistributeJobProvider
        {
            private readonly IStorageProvider _storageProvider;
            IConfiguration _config;
            private string _distributeUrl;
            public DistributeJobProvider(IStorageProvider storageProvider, IConfiguration config)
            {
                _storageProvider = storageProvider;
                _config = config;
                _distributeUrl = _config["Distribute:Url"];
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
                        return KeyValuePair.Create(true, sasUrl);
                    }

                    return KeyValuePair.Create(false, sasUrl);
                }
                catch (Exception ex)
                {
                    return KeyValuePair.Create(false, "");
                }
            }
        }
    }
}