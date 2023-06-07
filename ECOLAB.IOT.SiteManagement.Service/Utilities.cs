namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Common;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    public static class Utilities
    {
        public static List<string> GetDevicesFromDGWGetway(string json, string propertyKey)
        {

            var nodes = JsonConvert.DeserializeObject<JToken>(json);
            var list = new List<string>();
            FindNodes(nodes, propertyKey, "", list);
            return list;
        }

        public static void FindNodes(JToken json, string name, string value, List<string> nodes)
        {
            if (json.Type == JTokenType.Object)
            {
                foreach (JProperty child in json.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        nodes.Add(child.Value.ToString());
                    }
                    FindNodes(child.Value, name, value, nodes);
                }
            }
            else if (json.Type == JTokenType.Array)
            {
                foreach (JToken child in json.Children())
                {
                    FindNodes(child, name, value, nodes);
                }
            }
        }

        public static SiteDeviceTransformerDto GetSiteDeviceFromMeshJson(string json, string mode = "vcc")
        {
            JObject jObject = JObject.Parse(json);
            var siteDeviceMesh = new SiteDeviceTransformerDto();

            if (jObject.SelectToken("version") != null)
            {

                siteDeviceMesh.Version = jObject.SelectToken("version").ToString();
            }

            var nodes = (JArray)jObject["nodes"];

            if (nodes == null)
                return siteDeviceMesh;

            foreach (JToken node in nodes)
            {
                var nodeName = node["nodeName"];
                if (nodeName == null)
                {
                    continue;
                }
                var deviceNo = nodeName.ToString();
                if (string.IsNullOrEmpty(deviceNo))
                    continue;


                var jOjectInAllowList = new JObject();
                jOjectInAllowList.Add("SN", nodeName);
                jOjectInAllowList.Add("mode", mode);

                var properties = new JObject();
                
                if (nodeName != null) {
                    properties.Add("nodeName", nodeName);
                }

                var deviceType = node["deviceType"];
                if (deviceType != null)
                {
                    properties.Add("deviceType", deviceType);
                }

                var macAddress = node["macAddress"];
                if (macAddress != null)
                {
                    properties.Add("macAddress", macAddress);
                }

                var unicastAddress = node["unicastAddress"];
                if (unicastAddress != null)
                {
                    properties.Add("unicastAddress", unicastAddress);
                }

                jOjectInAllowList.Add("property", properties);

                siteDeviceMesh.DeviceTransformerDtos.Add(new DeviceTransformerDto()
                {
                    DeviceNo = deviceNo,
                    JOjectInAllowList = jOjectInAllowList.ToString(),
                    Json = json
                });
            }

            return siteDeviceMesh;
        }

        public static SiteDeviceTransformerDto GetSiteDeviceFromLoraJson(string json, string mode = "vrc")
        { 
            JObject jObject = JObject.Parse(json);
            var siteDevice = new SiteDeviceTransformerDto();
            if (jObject.SelectToken("version") != null)
            {
                siteDevice.Version = jObject.SelectToken("version").ToString();
            }

            var nodes = (JArray)jObject["nodes"];

            if (nodes == null)
                return siteDevice;

            foreach (JToken node in nodes)
            {
                var sn = node["SN"];
                if (sn == null)
                {
                    continue;
                }
                var deviceNo = sn.ToString();
                if (string.IsNullOrEmpty(deviceNo))
                    continue;

                var jOjectInAllowList = new JObject();
                jOjectInAllowList.Add("SN", sn);
                jOjectInAllowList.Add("mode", mode);

                var properties = new JObject();
                var code = node["Code"];
                if (code != null)
                {
                    properties.Add("Code", code);
                }

                jOjectInAllowList.Add("property", properties);

                siteDevice.DeviceTransformerDtos.Add(new DeviceTransformerDto()
                {
                    DeviceNo = deviceNo,
                    JOjectInAllowList = jOjectInAllowList.ToString(),
                    Json = json
                });
            }

            return siteDevice;
        }


        public static string GetAllowListJson(List<SiteDeviceDetailInfoDto> siteDeviceDetailInfoDtos,string siteNo)
        {
            var jobject = new JObject();
         
            jobject.Add("siteId", siteNo);
            var deviceAllowList = new JArray();
            if (siteDeviceDetailInfoDtos != null)
            {
                foreach (var item in siteDeviceDetailInfoDtos)
                {
                    deviceAllowList.Add(JToken.Parse(item.JObjectInAllowList));
                }
            }

            jobject.Add("nodes", deviceAllowList);
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(jobject, settings);
        }
    }
}
