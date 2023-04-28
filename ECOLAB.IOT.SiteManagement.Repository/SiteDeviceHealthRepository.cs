namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Text;

    public interface ISiteDeviceHealthRepository
    {
        public NodeFileInfo GetDeviceStatus(string siteNo, string deviceNo);
        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo);
        public List<SiteDeviceMode>? GetDeviceListFromInternalDb(string siteNo,string gatewayNo);
        // public List<SiteDeviceMode>? GetDeviceListStatusFromExternalDb(string siteNo);

        public List<NodeFileInfo>? GetDeviceListStatusFromExternalDb(List<SiteDeviceMode> siteDeviceModes);
    }


    public class SiteDeviceHealthRepository : Repository, ISiteDeviceHealthRepository
    {
        private IConfiguration _config;
        public SiteDeviceHealthRepository(IConfiguration config) : base(config, "ConnectionStrings:SqlConnectionStringHealthPVM")
        {
            _config = config;
        }


        public NodeFileInfo? GetDeviceStatus(string siteNo, string deviceNo)
        {

            var exist = Execute(_config["ConnectionStrings:SqlConnectionString"], (conn) =>
            {
                string query = $@"SELECT DeviceNo
                                  FROM [dbo].[GatewayDevice] as a inner join [dbo].[SiteGateway] as b
                                  on a.GatewayId=b.Id inner join [dbo].[Site] as c on a.SiteId=c.Id
                                  where c.SiteNo='{siteNo}'  and a.DeviceNo='{deviceNo}'";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0)
                {
                    return false;
                }
                return true;
            });

            if (!exist)
            {
                throw new Exception($"siteNo:{siteNo} or deviceNo:{deviceNo} doesn't exist.");
            }

            string query = $@"SELECT top 1 a.[DeviceId],a.[CreatedOnUtc] as Last_seen,'' as Mode,[FileURL],case when a.DeviceId is null then 'offline' else 'online'end as Status
                              FROM [dbo].[nodeInfo] as a Left join [dbo].[fileInfo] as b
                              on a.DeviceId=b.DeviceId
                              where a.DeviceId='{deviceNo}' and a.[CreatedOnUtc]>DATEADD(DAY,-1,GETDATE())  
                              order by a.[CreatedOnUtc]";  //PVM-ECOLAB19

            return Execute((conn) =>
            {
                var list = conn.Query<NodeFileInfo>(query).ToList();
                return list.FirstOrDefault();
            });
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
                throw new Exception($"siteNo:{siteNo} doesn't exist.");
            }

            string query = $@"SELECT Model ,a.DeviceNo
                              FROM [dbo].[GatewayDevice] as a
                              Inner join  [dbo].[Site] as b
                              on a.SiteId=b.Id
                              Inner join  [dbo].[SiteRegistry] as c
                              on c.SiteId=b.Id
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
            string query = $@"SELECT Model ,d.DeviceNo
                         FROM [dbo].[Site] as a
                              Inner join  [dbo].[SiteRegistry] as c
                              on c.SiteId=a.Id
							  inner join [dbo].[SiteDevice] as d
							  on a.Id=d.SiteId
                              where a.SiteNo = '{siteNo}'
                              order by Model";
            if (!string.IsNullOrEmpty(gatewayNo))
            {
                query = $@"SELECT Model ,a.DeviceNo
                              FROM [dbo].[GatewayDevice] as a
                              Inner join  [dbo].[Site] as b
                              on a.SiteId=b.Id
                              Inner join  [dbo].[SiteRegistry] as c
                              on c.SiteId=b.Id
                              inner join [dbo].[SiteGateway] as d
							  on a.GatewayId=d.Id
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

            var sqltable = GenarateSql(siteDeviceModes);

            var devices = string.Join("','", siteDeviceModes.Select(n => n.DeviceNo));

            var temp=string.Join("','", devices);
            string query = $@"select m.DeviceId,m.Mode,n.Last_seen,n.FileURL,case when n.DeviceId is null then 'offline' else 'online'end as Status from  ({sqltable}) as m
                            left join 
                            (select * from (SELECT  a.[DeviceId],a.[CreatedOnUtc] as Last_seen,[FileURL], ROW_NUMBER() over (partition by a.[DeviceId] order by a.[CreatedOnUtc]) as rowNum
                              FROM [dbo].[nodeInfo] as a Left join [dbo].[fileInfo] as b
                              on a.DeviceId=b.DeviceId
                              where a.DeviceId in('{devices}') and a.[CreatedOnUtc]>DATEADD(DAY,-1,GETDATE())) temp
							  where temp.rowNum=1) as n
							  on m.DeviceId=n.DeviceId";  //PVM-ECOLAB19

            return Execute((conn) =>
            {
                var list = conn.Query<NodeFileInfo>(query).ToList();
                return list;
            });
        }

        private string GenarateSql(List<SiteDeviceMode> siteDeviceModes)
        {
            if (siteDeviceModes == null || siteDeviceModes.Count == 0)
                return null;

            var sb = new StringBuilder();

            var i = 0;
            foreach (var item in siteDeviceModes)
            {
                if (i == 0)
                {
                    sb.Append($"select '{item.DeviceNo}'as DeviceId,'{item.Model}' as Mode");
                }

                sb.Append($" union select '{item.DeviceNo}'as DeviceId,'{item.Model}' as Mode");
                i++;
            }

            var sql = sb.ToString();

            return sql;
        }
    }
}
