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

        public Task<JObject> QueryDeviceListBySiteNo(string siteNo, string gatewayId = "", int pageIndex = 1, int pageSize = 50);

        public Task<string> ConfigureDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto);

        public Task<string> UpdateDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto);

        public Task<bool> AddDeviceToDGWOneByOne(string siteNo, string gatewayNo, DeviceToDGWOneByOneRequestDto  deviceToDGWOneByOneRequestDto);

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

            var siteDeviceDetailInfoList= _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds);
            if (siteDeviceDetailInfoList == null || siteDeviceDetailInfoList.Count <= 0 || deviceToDGWRequestDto.DeviceIds.Count!= siteDeviceDetailInfoList.Count)
            {
                throw new Exception("Some device numbers do not exist");
            }

            var connectionString = _config["BlobOfAllowList:ConnectionString"]; 
            var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
            var allowListUrl = _storageProvider.UploadJsonToBlob(connectionString, blobContainerName,$"gwconfigfile/deviceAllowList/{siteNo}/{gatewayNo}/AllowList.Json",Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));


            var bl=_gatewayDeviceRepository.ConfigureDeviceToDGW(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds, allowListUrl);

            if (!bl)
            {
                throw new Exception("Configure Device To DGW failed ");
            }

            return await Task.FromResult(allowListUrl);
        }

        public async Task<string> UpdateDeviceToDGW(string siteNo, string gatewayNo, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {

            var siteDeviceDetailInfoList = _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds);
            if (siteDeviceDetailInfoList == null || siteDeviceDetailInfoList.Count <= 0)
            {
                throw new Exception("Some device numbers do not exist");
            }

            var connectionString = _config["BlobOfAllowList:ConnectionString"];
            var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
            var allowListUrl = _storageProvider.UploadJsonToBlob(connectionString, blobContainerName, $"gwconfigfile/deviceAllowList/{siteNo}/{gatewayNo}/AllowList.Json", Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));


            var bl = _gatewayDeviceRepository.UpdateDeviceToDGW(siteNo, gatewayNo, deviceToDGWRequestDto.DeviceIds, allowListUrl);

            if (!bl)
            {
                throw new Exception("Update Device To DGW failed ");
            }

            return await Task.FromResult(allowListUrl);
        }

        public async Task<bool> Delete(string siteNo, string deviceNo)
        {
            var siteGateway = _getwayRepository.GetGatewayByDeviceNoAndSiteNo(deviceNo, siteNo);
            var bl = _gatewayDeviceRepository.Delete(siteNo, deviceNo);
            if (bl)
            {
                 GenarateJob(siteGateway, siteNo);
            }
            return await Task.FromResult(bl);
        }

        private string GenarateJob(SiteGateway? siteGateway,string siteNo)
        {
            if (siteGateway != null)
            {
                var deviceOfgateway = _gatewayDeviceRepository.GetGatewayDevicesByGatewayId(siteGateway.Id);
                var list = deviceOfgateway?.Select(item => item.DeviceNo)?.ToList();
                if (list == null)
                {
                    list = new List<string>();
                }

                var siteDeviceDetailInfoList = _siteService.GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(siteNo, siteGateway.GatewayNo, list);
                if (siteDeviceDetailInfoList == null)
                {
                    siteDeviceDetailInfoList = new List<SiteDeviceDetailInfoDto>();
                }

                var connectionString = _config["BlobOfAllowList:ConnectionString"];
                var blobContainerName = _config["BlobOfAllowList:BlobContainerName"];
                var allowListUrl = _storageProvider.UploadJsonToBlob(connectionString, blobContainerName, $"gwconfigfile/deviceAllowList/{siteNo}/{siteGateway.GatewayNo}/AllowList.Json", Utilities.GetAllowListJson(siteDeviceDetailInfoList, siteNo));
                _gatewayDeviceRepository.GenerateJob(siteNo, siteGateway.GatewayNo, allowListUrl);
                return allowListUrl;
            }
            return "";
        }

        private JObject? ConvertToJObject(List<GatewayDeviceMode>? gatewayDeviceMode,string gatewayNo="")
        {
            if (gatewayDeviceMode == null || gatewayDeviceMode.Count==0)
            {
                return null;
            }

            var jobj = new JObject();
            if (!string.IsNullOrEmpty(gatewayNo))
            {
                jobj.Add("id", gatewayNo);
            }

            if (gatewayDeviceMode != null)
            { 
                var models = gatewayDeviceMode?.Where(g=>!string.IsNullOrEmpty(g.Model))?.Select(item => item.Model)?.Distinct();
                foreach (var model in models)
                {
                    var data = gatewayDeviceMode?.Where(i => i.Model == model);
                    if (data != null)
                    {
                        List<dynamic> subList = new List<dynamic>();
                        foreach (var item in data)
                        {
                            subList.Add(new { id= item.DeviceNo });
                        }
                        jobj.Add(model, JArray.Parse(JsonConvert.SerializeObject(subList)));
                    }
                }
            }

            return jobj;
        }

        public async Task<JObject> QueryDeviceListBySiteNo(string siteNo, string gatewayId = "", int pageIndex = 1, int pageSize = 1000)
        {
            var deviceModes = _gatewayDeviceRepository.GetGatewayDeviceListFromInternalDb(siteNo, gatewayId);
            var jobject = new JObject();
            jobject.Add("site_id", siteNo);
            var noOwnerDevices= deviceModes?.Where(item => string.IsNullOrEmpty(item.GatewayNo))?.ToList();
            var noOwnerDevicejobjs = ConvertToJObject(noOwnerDevices);
            if(noOwnerDevicejobjs!=null)
                jobject.Add("no_owner", noOwnerDevicejobjs);
            var ownerDevices = deviceModes?.Where(item => !string.IsNullOrEmpty(item.GatewayNo))?.ToList();
            var ownerGateways = ownerDevices?.Select(item => item.GatewayNo)?.Distinct()?.ToList();

            if (ownerGateways != null)
            {
                List<dynamic> gateways = new List<dynamic>();
                foreach (var gateway in ownerGateways)
                {
                    var gatewayDevices = ownerDevices?.Where(device => device.GatewayNo == gateway)?.ToList();
                    var subJobj= ConvertToJObject(gatewayDevices, gateway);
                    if (subJobj == null)
                    {
                        subJobj = new JObject();
                    }

                    gateways.Add(subJobj);
                }

                jobject.Add("gateway", JArray.Parse(JsonConvert.SerializeObject(gateways)));
            }
         
            return await Task.FromResult(jobject);
        }

        public async Task<bool> AddDeviceToDGWOneByOne(string siteNo, string gatewayNo, DeviceToDGWOneByOneRequestDto deviceToDGWOneByOneRequestDto)
        {
            var bl = _gatewayDeviceRepository.AddDeviceToDGWOneByOne(siteNo, gatewayNo, deviceToDGWOneByOneRequestDto.DeviceId , GenarateAllowList);

            if (!bl)
            {
                throw new Exception("Add Device To DGW failed ");
            }

            return await Task.FromResult(bl);
        }

        private string GenarateAllowList(string siteNo, string gatewayNo)
        {
            var siteGateway = _getwayRepository.GetGatewayByGatewayNoAndSiteNo(gatewayNo, siteNo);
            if (siteGateway!=null)
            {
              var allowListUrl=  GenarateJob(siteGateway, siteNo);
              return allowListUrl;
            }

            return "";
        }
    }
}
