namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Repository;
    using Flurl.Http;
    using System;

    public interface IDistributeJobService
    {
        public Task<bool> Travle();
    }

    public class DistributeJobService : IDistributeJobService
    {
        private readonly IGatewayAllowListTaskRepository _gatewayAllowListTaskRepository;
        public DistributeJobService(IGatewayAllowListTaskRepository gatewayAllowListTaskRepository)
        {
            _gatewayAllowListTaskRepository = gatewayAllowListTaskRepository;
        }

        public async Task<bool> Travle()
        {
            try
            {
                var tasks = _gatewayAllowListTaskRepository.GetTaskAll();
                await Dispatch(tasks);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }
         
        }

        private async Task Dispatch(List<GatewayAllowListTask> gatewayAllowListTasks)
        {
            var tokenInfoDto = await GetToken();
            foreach (var task in gatewayAllowListTasks)
            {
                var result=await DistributeTask(task, "localNetwork", tokenInfoDto.Access_Token);
                if (result.Key)
                {
                    _gatewayAllowListTaskRepository.InsertHistory(new GatewayAllowListTaskHistory()
                    {
                        SiteId = task.SiteId,
                        SiteNo = task.SiteNo,
                        GatewayId = task.GatewayId,
                        GatewayNo = task.GatewayNo,
                        AllowListSASUrl = result.Value,
                        Status = 1
                    });
                }
            }

            await Task.CompletedTask;
        }

        private string _tokenUrl = "https://login.partner.microsoftonline.cn/2c3c280f-2394-453f-944d-6df0dd9c338e/oauth2/v2.0/token";
        private async Task<TokenInfoDto> GetToken()
        {
            var result =await _tokenUrl.PostUrlEncodedAsync(new
            {
                client_id = "dd609f2a-99e0-4c1d-bd9f-58bdd4573384",
                scope = "dd609f2a-99e0-4c1d-bd9f-58bdd4573384/.default",
                client_secret = "i~5x_Gy2UrC1~R-k8h.KbDjM8bA.Z4DbhS",
                grant_type = "client_credentials"
            }).ReceiveJson<TokenInfoDto>();

            return result;
        }

        private string distributeUrl = "https://cn-ins-edmiot-appservice-002-d.chinacloudsites.cn/api/devices/DGW-{dgwSnNO}/settings/{groupName}?async=false&ttl=20";

        private async Task<KeyValuePair<bool,string>> DistributeTask(GatewayAllowListTask gatewayAllowListTask,string groupName,string token)
        {
            try
            {
                var url = distributeUrl.Replace("{dgwSnNO}", gatewayAllowListTask.GatewayNo).Replace("{groupName}", groupName);

                var sasUrl = await GetAllowListSASUrl(gatewayAllowListTask.AllowListUrl);
                if (string.IsNullOrEmpty(sasUrl))
                    return KeyValuePair.Create(false, "");

                var result = await url.WithHeader("Authorization", $"Bearer {token}").PostJsonAsync(new
                {
                    netWork= "BLEMesh",
                    configFileUrl = sasUrl,
                    appId="PVM",
                    fileName="AllowList.json",
                    version="1.0.1"
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

        private string getSASUrl = "https://cn-ins-edmiot-appservice-storageapi-d.chinacloudsites.cn/api/v1/file-sas-url/friendly";
        private async Task<string> GetAllowListSASUrl(string url)
        {
            try
            {
                var list = new List<SASUrl>();
                list.Add(new SASUrl() { noneKeyUrl = url });

                var result = await getSASUrl.WithHeader("Storage-API-Certificate", "f4bf4c87-d82b-4ab0-a224-3f6fe7bc77b5").PostJsonAsync(list).ReceiveJson<Root>();

                if (result.status == 200 && result.data != null && result.data.Count > 0)
                {
                    return result.data[0].fileSASUrl;
                }

                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
            
        }
    }

    public class DataItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string fileSASUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string fileFullPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string containerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string storageAccountName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int uploadUtcTime { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float timestamp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DataItem> data { get; set; }
    }


    public class SASUrl
    { 
        public string noneKeyUrl { get; set; }
    }
    public class TokenInfoDto
    {
        public string? TokenType { get; set; }
        public int Expires_In { get; set; }

        public int Ext_Expires_In { get; set; }

        public string? Access_Token { get; set; }

    }
}
