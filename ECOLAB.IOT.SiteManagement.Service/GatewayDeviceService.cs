namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Provider;
    using ECOLAB.IOT.SiteManagement.Repository;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public interface IGatewayDeviceService
    {
        public Task<bool> Delete(string siteNo, string deviceNo);

        public Task<JObject> QueryDeviceListBySiteNo(string siteNo, string gatewayNo = null, int pageIndex = 1, int pageSize = 50);

        public Task<string> ConfigureDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto);

        public Task<string> UpdateDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto);

    }

    public class GatewayDeviceService : IGatewayDeviceService
    {
        private readonly IGatewayDeviceRepository _gatewayDeviceRepository;
        private readonly IGetwayRepository _getwayRepository;
        private readonly ISiteService _siteService;
        private readonly IStorageProvider _storageProvider;
        private readonly IConfiguration _config;
        private readonly ISiteDeviceHealthRepository _siteDeviceHealthRepository;

        public GatewayDeviceService(IGatewayDeviceRepository gatewayDeviceRepository, ISiteService siteService, IStorageProvider storageProvider,IConfiguration config, ISiteDeviceHealthRepository siteDeviceHealthRepository, IGetwayRepository getwayRepository)
        {
            _gatewayDeviceRepository = gatewayDeviceRepository;
            _siteDeviceHealthRepository = siteDeviceHealthRepository;
            _siteService = siteService;
            _storageProvider = storageProvider;
            _config = config;
            _getwayRepository = getwayRepository;
        }

        public async Task<string> ConfigureDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {

            var siteDeviceDetailInfoList= await _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds);
            if (siteDeviceDetailInfoList == null || siteDeviceDetailInfoList.Count <= 0 || deviceToDGWRequestDto.DeviceIds.Count!= siteDeviceDetailInfoList.Count)
            {
                throw new Exception("Some device numbers do not exist");
            }

            var connectionString = _config["BlobOfAllowList:ConnectionString"]; 
            var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
            var allowListUrl = await _storageProvider.UploadJsonToBlob(connectionString, blobContainerName,$"gwconfigfile/deviceAllowList/{siteNo}/{gatewayNo}/AllowList.Json",Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));


            var bl=_gatewayDeviceRepository.ConfigureDeviceToDGW(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds, allowListUrl);

            if (!bl)
            {
                throw new Exception("Configure Device To DGW failed ");
            }

            return allowListUrl;
        }

        public async Task<string> UpdateDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {

            var siteDeviceDetailInfoList = await _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds);
            if (siteDeviceDetailInfoList == null || siteDeviceDetailInfoList.Count <= 0)
            {
                throw new Exception("Some device numbers do not exist");
            }

            var connectionString = _config["BlobOfAllowList:ConnectionString"];
            var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
            var allowListUrl = await _storageProvider.UploadJsonToBlob(connectionString, blobContainerName, $"gwconfigfile/deviceAllowList/{siteNo}/{gatewayNo}/AllowList.Json", Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));


            var bl = _gatewayDeviceRepository.UpdateDeviceToDGW(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds, allowListUrl);

            if (!bl)
            {
                throw new Exception("Update Device To DGW failed ");
            }

            return allowListUrl;
        }

        public async Task<bool> Delete(string siteNo, string deviceNo)
        {
            var siteGateway = _getwayRepository.GetGatewayByDeviceNoAndSiteNo(siteNo, deviceNo);
            var bl = _gatewayDeviceRepository.Delete(siteNo, deviceNo);
            if (bl)
            {
                await GenarateJob(siteGateway, siteNo);
            }
            return await Task.FromResult(bl);
        }

        private async Task GenarateJob(SiteGateway? siteGateway,string siteNo)
        {
            if (siteGateway != null)
            {
                var deviceOfgateway = _gatewayDeviceRepository.GetGatewayDevicesByGatewayId(siteGateway.Id);


                var list = deviceOfgateway?.Select(item => item.DeviceNo)?.ToList();
                if (list == null)
                {
                    list = new List<string>();
                }

                var siteDeviceDetailInfoList = await _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, siteGateway.GatewayNo, list);
                if (siteDeviceDetailInfoList == null)
                {
                    siteDeviceDetailInfoList = new List<SiteDeviceDetailInfoDto>();
                }

                var connectionString = _config["BlobOfAllowList:ConnectionString"];
                var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
                var allowListUrl = await _storageProvider.UploadJsonToBlob(connectionString, blobContainerName, $"gwconfigfile/deviceAllowList/{siteNo}/{siteGateway.GatewayNo}/AllowList.Json", Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));
                _gatewayDeviceRepository.GenerateJob(siteNo, siteGateway.GatewayNo, allowListUrl);
            }
        }

        public async Task<JObject> QueryDeviceListBySiteNo(string siteNo, string gatewayNo= null, int pageIndex = 1, int pageSize = 1000)
        {
            var deviceModes = _siteDeviceHealthRepository.GetDeviceListFromInternalDb(siteNo, gatewayNo);

            var healths = _siteDeviceHealthRepository.GetDeviceListStatusFromExternalDb(deviceModes);
            var jobject = new JObject();
            var modes = deviceModes.Select(item => item.Model).Distinct();
            jobject.Add("id", siteNo);
            if (modes == null || modes.Count() == 0)
            {
                return jobject;
            }


            foreach (var mode in modes)
            {
                var data = healths?.Where(item => item.Mode == mode).ToList();
                List<dynamic> subList = new List<dynamic>();
                foreach (var item in data)
                {
                    subList.Add(new
                    {
                        id = item.DeviceId,
                        connection_state = item.Status,
                        last_seen = item.Last_seen
                    });
                }
                var total = data.Count;
                var onlineCount = data.Where(item => item.Status == "online").Count();
                var jtoken = new JObject();
                jtoken.Add("total", total);
                jtoken.Add("online", onlineCount);
                jtoken.Add("data", JArray.Parse(JsonConvert.SerializeObject(subList)));

                jobject.Add(mode, jtoken);
            }

            return await Task.FromResult(jobject);
        }
    }
}
