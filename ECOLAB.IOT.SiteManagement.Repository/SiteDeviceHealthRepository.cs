namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Data.Entity.ExternalDB;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System.Text;
    using static System.Net.Mime.MediaTypeNames;

    public interface ISiteDeviceHealthRepository
    {
        public JObject GetDeviceStatus(string siteNo, string deviceNo);
        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo);
        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo,string gatewayNo);
       
        public List<NodeFileInfo>? GetDeviceListStatusFromExternalDb(List<SiteDeviceMode> siteDeviceModes);
    }

    public class SiteDeviceHealthRepository : Repository, ISiteDeviceHealthRepository
    {
        private IConfiguration _config;
        private int _onlineDays = -1;
        public SiteDeviceHealthRepository(IConfiguration config) : base(config, "ConnectionStrings:SqlConnectionStringHealthPVM")
        {
            _config = config;
            if (int.TryParse(_config["Health:OnlineDays"], out var onlineDays))
            {
                _onlineDays = -1*onlineDays;
            }
           
        }


        public JObject? GetDeviceStatus(string siteNo, string deviceNo)
        {

            var row = Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {

                string query = $@"SELECT Model ,c.DeviceNo,case when g.DeviceNo is null then 0 else 1 end as IsConfig
							  FROM [dbo].[SiteDevice] c
							  inner join [dbo].[Site] as b
							  on b.Id=c.SiteId
                              Inner join  [dbo].[SiteRegistry] as d
                              on d.SiteId=c.SiteId and d.Id=c.SiteRegistryId
                              left join [dbo].[GatewayDevice] as g
							  on d.SiteId=g.SiteId and c.DeviceNo=g.DeviceNo
                              where b.SiteNo='{siteNo}' and c.DeviceNo='{deviceNo}'
                              order by Model";
                var row = conn.Query<SiteDeviceMode>(query).FirstOrDefault();
                return row;
            });

            if (row == null)
            {
                throw new Exception($"siteId:{siteNo} or deviceId:{deviceNo} doesn't exist.");
            }

            var sqltable = GenarateSql(new List<SiteDeviceMode>() { row });
            //var deviceHealth = new DeviceHealthDto()
            //{
            //    Id = deviceNo,
            //    SiteId = siteNo,
            //};

            var jobject = new JObject();
            jobject.Add("id", deviceNo);
            jobject.Add("site_id", siteNo);
            
            if (row != null && row?.Model == "vcc")
            {
                string query = $@"select m.DeviceId as DeviceId,m.Mode,case when m.IsConfig=0 then null else n.Last_seen end as Last_seen,n.FileURL,case when m.IsConfig=0 then 'unassigned' else case when n.DeviceId is null then 'offline' else 'online' end end as Status 
							  from  ({sqltable}) as m
                              left join
							  (SELECT top 1 a.[DeviceSN] as DeviceId,a.[CreatedOnUtc] as Last_seen,'' as Mode,[FileURL],'online' as Status
                              FROM [dbo].[nodeInfo] as a Left join [dbo].[fileInfo] as b
                              on a.DeviceId=b.DeviceId
                              where a.DeviceSN='{deviceNo}' and a.[CreatedOnUtc]>DATEADD(DAY,{_onlineDays},GETDATE())  
                              order by a.[CreatedOnUtc] desc,b.[CreatedOnUtc] desc) as n
							  on m.DeviceId =n.DeviceId";  //PVM-ECOLAB19

                return Execute((conn) =>
                {
                    var list = conn.Query<NodeFileInfo>(query).ToList();
                    var device = list.FirstOrDefault();
                    if (device != null)
                    {
                        jobject.Add("connection_state", device?.Status);
                        jobject.Add("last_seen", device?.Last_seen);
                        jobject.Add("last_event", JToken.FromObject(new {
                            image=device?.FileURL,
                            captured_at = device?.Captured_at
                        }));
                    }

                    return jobject;
                });
            }
            else if(row != null && row?.Model == "vrc")
            {
                string query = $@"select m.DeviceId,m.Mode,case when m.IsConfig=0 then null else n.Last_seen end as Last_seen,n.Low_alarm,case when m.IsConfig=0 then 'null' else case when n.DeviceId is null then 'offline' else 'online'end end as Status from  ({sqltable}) as m
                            left join 
                            ( SELECT  top 1  a.[DeviceId],a.[CreatedOnUtc] as Last_seen,'' as Mode,Low_alarm , 'online' as Status,b.Captured_at
                              FROM [dbo].[BaitStation] as a Left join [dbo].[BatteryAlarm] as b
                              on a.DeviceId=b.DeviceId
                              where a.DeviceId ='{deviceNo}' and a.[CreatedOnUtc]>DATEADD(DAY,{_onlineDays},GETDATE())
							  order by a.[CreatedOnUtc]) as n
							  on m.DeviceId=n.DeviceId";  //PVM-ECOLAB19

                return Execute(_config["ConnectionStrings:SqlConnectionStringHealthPEC"], (conn) =>
                {
                    var list = conn.Query<NodeAlarmInfo>(query).ToList();
                    var device = list.FirstOrDefault();
                    if (device != null)
                    {
                        jobject.Add("connection_state", device?.Status);
                        jobject.Add("last_seen", device?.Last_seen);
                        jobject.Add("last_event", JToken.FromObject(new
                        {
                            low_alarm = device?.Low_alarm,
                            captured_at = device?.GetDateTime()
                        }));
                    }

                    return jobject;
                });
            }

            throw new Exception($"siteNo:{siteNo} or deviceNo:{deviceNo},this model of query is currently not supported:{row?.Model}.");
        }

        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo)
        {
            var exist = Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {
                string query = $@"SELECT Id
                                  FROM [dbo].[Site]
                                  where SiteNo='{siteNo}'";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0)
                {
                    return false;
                }
                return true;
            });

            if (!exist)
            {
                throw new Exception($"siteId:{siteNo} doesn't exist.");
            }

            string query = $@"SELECT Model ,c.DeviceNo,case when g.DeviceNo is null then 0 else 1 end as IsConfig
							  FROM [dbo].[SiteDevice] c
							  inner join [dbo].[Site] as b
							  on b.Id=c.SiteId
                              Inner join  [dbo].[SiteRegistry] as d
                              on d.SiteId=c.SiteId and d.Id=c.SiteRegistryId
                              left join [dbo].[GatewayDevice] as g
							  on d.SiteId=g.SiteId and c.DeviceNo=g.DeviceNo
                              where b.SiteNo='{siteNo}'
                              order by Model"; 

            return Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {
                var list = conn.Query<SiteDeviceMode>(query).ToList();
                return list;
            });
        }

        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo,string gatewayNo)
        {
            var exist = Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {
                string query = $@"SELECT Id
                                  FROM [dbo].[Site]
                                  where SiteNo='{siteNo}'";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0)
                {
                    return false;
                }
                return true;
            });

            if (!exist)
            {
                throw new Exception($"siteId:{siteNo} doesn't exist.");
            }

            string query = $@"SELECT Model ,d.DeviceNo,case when g.DeviceNo is null then 0 else 1 end as IsConfig
                         FROM [dbo].[Site] as a
                              Inner join  [dbo].[SiteRegistry] as c
                              on c.SiteId=a.Id
							  inner join [dbo].[SiteDevice] as d
							  on a.Id=d.SiteId and c.Id=d.SiteRegistryId
                              left join [dbo].[GatewayDevice] as g
							  on d.SiteId=g.SiteId and d.DeviceNo=g.DeviceNo
                              where a.SiteNo = '{siteNo}'
                              order by Model";
            if (!string.IsNullOrEmpty(gatewayNo))
            {
                query = $@"SELECT Model ,a.DeviceNo,1 as IsConfig
                                              FROM [dbo].[GatewayDevice] as a
                              Inner join  [dbo].[Site] as b
                              on a.SiteId=b.Id
                              Inner join  [dbo].[SiteRegistry] as c
                              on c.SiteId=b.Id 
                              inner join [dbo].[SiteGateway] as d
							  on a.GatewayId=d.Id 
							  inner join [dbo].[SiteDevice] as e
							  on e.DeviceNo=a.DeviceNo and e.SiteRegistryId=c.Id and e.SiteId=b.Id
                             where b.SiteNo = '{siteNo}'  and d.GatewayNo='{gatewayNo}'
                              order by Model";
            }

            return Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {
                var list = conn.Query<SiteDeviceMode>(query).ToList();
                return list;
            });
        }



        public List<NodeFileInfo>? GetDeviceListStatusFromExternalDb(List<SiteDeviceMode> siteDeviceModes)
        {

            if (siteDeviceModes == null || siteDeviceModes.Count == 0)
                return null;
            var list = new List<NodeFileInfo>();
            var listOfPVC = GetDeviceListStatusFromPVC(siteDeviceModes?.Where(item=>item.Model?.ToLower()=="vcc"));
            if (listOfPVC != null)
            {
                list.AddRange(listOfPVC);
            }
            
            var listOfPEC = GetDeviceListStatusFromPEC(siteDeviceModes?.Where(item => item.Model?.ToLower() == "vrc"));
            if (listOfPEC != null)
            {
                list.AddRange(listOfPEC);
            }
            return list;
        }

        private string GenarateSql(IEnumerable<SiteDeviceMode> siteDeviceModes)
        {
            if (siteDeviceModes == null || siteDeviceModes.Count() == 0)
                return null;

            var sb = new StringBuilder();

            var i = 0;
            foreach (var item in siteDeviceModes)
            {
                var isConfig = item.IsConfig ? 1 : 0;
                if (i == 0)
                {
                    sb.Append($"select '{item.DeviceNo}'as DeviceId,'{item.Model}' as Mode,{isConfig} as IsConfig");
                }
                else
                {
                    sb.Append($" union select '{item.DeviceNo}'as DeviceId,'{item.Model}' as Mode,{isConfig} as IsConfig");
                }
                i++;
            }

            var sql = sb.ToString();

            return sql;
        }

        private List<NodeFileInfo>? GetDeviceListStatusFromPVC(IEnumerable<SiteDeviceMode> siteDeviceModes)
        {
            if (siteDeviceModes.Count() <= 0)
                return null;
            var sqltable = GenarateSql(siteDeviceModes);
            if (string.IsNullOrEmpty(sqltable))
                return null;
            var devices = string.Join("','", siteDeviceModes.Select(n => n.DeviceNo));

            var temp = string.Join("','", devices);
   

            string query = $@"select m.DeviceId as DeviceId,m.Mode,case when m.IsConfig=0 then null else n.Last_seen end as Last_seen,n.FileURL,case when m.IsConfig=0 then 'unassigned' else case when n.DeviceId is null then 'offline' else 'online' end end as Status from  ({sqltable}) as m
                            left join 
                            (select * from (select  [DeviceSN] as DeviceId,[CreatedOnUtc] as Last_seen,null as FileURL, ROW_NUMBER() over (partition by [DeviceSN] order by [CreatedOnUtc] desc) as rowNum
							  from [dbo].[nodeInfo] where DeviceSN in('{devices}') 
							  and [CreatedOnUtc]>DATEADD(DAY,{_onlineDays},GETDATE())) as temp where rowNum=1) as n
							  on m.DeviceId=n.DeviceId";  //PVM-ECOLAB19


            return Execute((conn) =>
            {
                var list = conn.Query<NodeFileInfo>(query).ToList();
                return list;
            });
        }

        private List<NodeFileInfo>? GetDeviceListStatusFromPEC(IEnumerable<SiteDeviceMode> siteDeviceModes)
        {
            if (siteDeviceModes.Count() <= 0)
                return null;

            var sqltable = GenarateSql(siteDeviceModes);
            if (string.IsNullOrEmpty(sqltable))
                return null;
            var devices = string.Join("','", siteDeviceModes.Select(n => n.DeviceNo));

            var temp = string.Join("','", devices);
    
            string query = $@"select m.DeviceId,m.Mode,case when m.IsConfig=0 then null else n.Last_seen end as Last_seen,n.FileURL,case when m.IsConfig=0 then 'unassigned' else case when n.DeviceId is null then 'offline' else 'online'end end as Status from  ({sqltable}) as m
                            left join 
                            (select * from (select  [DeviceId],[CreatedOnUtc] as Last_seen,null as FileURL, ROW_NUMBER() over (partition by DeviceId order by [CreatedOnUtc] desc) as rowNum
                              FROM [dbo].[BaitStation] 
                              where DeviceId in('{devices}') and [CreatedOnUtc]>DATEADD(DAY,{_onlineDays},GETDATE())) as temp where rowNum=1) as n
							  on m.DeviceId=n.DeviceId";  //PVM-ECOLAB19

            return Execute(_config["ConnectionStrings:SqlConnectionStringHealthPEC"], (conn) =>
            {
                var list = conn.Query<NodeFileInfo>(query).ToList();
                return list;
            });
        }
    }
}
