namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Repository;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    public interface ISiteDeviceHealthService
    {
        public Task<DeviceHealthDto?> GetDeviceStatus(string siteNo, string deviceNo);
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

        public async Task<DeviceHealthDto?> GetDeviceStatus(string siteNo, string deviceNo)
        {
            var device = _siteDeviceHealthRepository.GetDeviceStatus(siteNo, deviceNo);

            var deviceHealth = new DeviceHealthDto()
            {
                Id = deviceNo,
                SiteId = siteNo,
            };

            if (device != null)
            {
                deviceHealth.Connection_state = "online";
                deviceHealth.Last_seen = device.Last_seen;
                deviceHealth.Last_event = new LastEvent()
                {
                    Image = device.FileURL,
                    Captured_at = device.Captured_at
                };
            }
            else {
                deviceHealth.Connection_state = "offline";
                deviceHealth.Last_seen = null;
                deviceHealth.Last_event = null;
            }

            return await Task.FromResult(deviceHealth);
        }

        public async Task<JObject> GetDeviceStatusListBySiteId(string siteNo)
        {
            var deviceModes = _siteDeviceHealthRepository.GetDeviceListFromInternalDb(siteNo);

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
                var onlineCount = data.Where(item => item.Status == "online").Count();
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
