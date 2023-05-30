namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Repository;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    public interface ISiteDeviceHealthService
    {
        public Task<JObject?> GetDeviceStatus(string siteNo, string deviceNo);
        public bool GetDeviceListStatus(string siteNo);

        public Task<JObject> GetDeviceStatusListBySiteId(string siteNo);
    }

    public class SiteDeviceHealthService : ISiteDeviceHealthService
    {
        private readonly ISiteDeviceHealthRepository _siteDeviceHealthRepository;

        public SiteDeviceHealthService(ISiteDeviceHealthRepository siteDeviceHealthRepository)
        {
            _siteDeviceHealthRepository = siteDeviceHealthRepository;
        }

        public bool GetDeviceListStatus(string siteNo)
        {
            throw new NotImplementedException();
        }

        public async Task<JObject?> GetDeviceStatus(string siteNo, string deviceNo)
        {
            var deviceHealth = _siteDeviceHealthRepository.GetDeviceStatus(siteNo, deviceNo);

            return await Task.FromResult(deviceHealth);
        }

        public async Task<JObject> GetDeviceStatusListBySiteId(string siteNo)
        {
            var deviceModes = _siteDeviceHealthRepository.GetDeviceListFromInternalDb(siteNo);

            var healths = _siteDeviceHealthRepository.GetDeviceListStatusFromExternalDb(deviceModes);

            var jobject = new JObject();
            var modes = deviceModes.Select(item => item.Model).Distinct();
            jobject.Add("site_id", siteNo);
            if (modes == null || modes.Count() == 0)
            {
                return jobject;
            }

            foreach (var mode in modes)
            {
                var data = healths?.Where(item => item.Mode == mode).ToList();
                List<dynamic> list = new List<dynamic>();
                foreach (var item in data)
                {
                    list.Add(new
                    {
                        id = item.DeviceId,
                        connection_state = item.Status,
                        last_seen = item.Last_seen
                    });

                }
                var total = data.Count;
                var onlineCount = data.Where(item => item.Status == "online")?.Count();
                var jtoken = new JObject();
                jtoken.Add("total",total);
                jtoken.Add("online", onlineCount);
                jtoken.Add("data", JArray.Parse(JsonConvert.SerializeObject(list)));

               jobject.Add(mode, jtoken);
            }

            return await Task.FromResult(jobject);
        }
    }
}
