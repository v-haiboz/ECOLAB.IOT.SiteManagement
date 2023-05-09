﻿namespace ECOLAB.IOT.SiteManagement.Service
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

        public static SiteDeviceTransformerDto GetSiteDeviceFromMeshJson(string json)
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
                var deviceToken = node["nodeName"];
                if (deviceToken == null)
                {
                    continue;
                }
                var deviceNo = deviceToken.ToString();
                if (string.IsNullOrEmpty(deviceNo))
                    continue;

                siteDeviceMesh.DeviceTransformerDtos.Add(new DeviceTransformerDto()
                {
                    DeviceNo = deviceNo,
                    JOjectInAllowList = node.ToString(),
                    Json = json
                });
            }

            return siteDeviceMesh;
        }

        public static SiteDeviceTransformerDto GetSiteDeviceFromLoraJson(string json)
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
                var deviceToken = node["SN"];
                if (deviceToken == null)
                {
                    continue;
                }
                var deviceNo = deviceToken.ToString();
                if (string.IsNullOrEmpty(deviceNo))
                    continue;

                siteDevice.DeviceTransformerDtos.Add(new DeviceTransformerDto()
                {
                    DeviceNo = deviceNo,
                    JOjectInAllowList = node.ToString(),
                    Json = json
                });
            }

            return siteDevice;
        }

        public static string GetAllowListJson(List<SiteDeviceDetailInfoDto> siteDeviceDetailInfoDtos,string siteNo)
        {
            var jobject = new JObject();
         
            jobject.Add("siteId", siteNo);
            var models = siteDeviceDetailInfoDtos?.Select(item => item.Model)?.ToList()?.Distinct();
            if (models != null)
            {
                foreach (var model in models)
                {
                    var deviceAllowList = new JArray();
                    var vccDevices = siteDeviceDetailInfoDtos?.Where(vcc => vcc.Model == model);
                    if (vccDevices != null)
                    {
                        foreach (var item in vccDevices)
                        {
                            deviceAllowList.Add(JToken.Parse(item.JObjectInAllowList));
                        }
                    }

                    if (model.ToLowerInvariant() == ModelEnum.VCC.ToString().ToLowerInvariant())
                    {
                        jobject.Add("nodes", deviceAllowList);
                    }
                    else
                    {
                        jobject.Add($"nodes_{model}", deviceAllowList);
                    }
                }
            }
            
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(jobject, settings);
        }
    }
}
