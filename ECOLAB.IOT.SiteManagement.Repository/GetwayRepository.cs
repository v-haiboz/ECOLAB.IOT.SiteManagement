namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Linq;

    public interface IGetwayRepository
    {
        public bool Insert(string siteNo,string sn);

        public bool Update(string siteNo, string newSn, string oldSn);

        public bool Delete(string siteNo, string sn);

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo);

        public List<GatewayInfo> GetGatewayListHealth(string siteNo);
    }

    public class GetwayRepository : Repository, IGetwayRepository
    {
        
        public GetwayRepository(IConfiguration config) : base(config)
        {
        }

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo) {
            return Execute((conn) =>
            {
                string query = $@"SELECT *
                                          FROM [dbo].[Site] as a
                                          LEFT JOIN [dbo].[SiteRegistry] as b
                                          on a.Id=b.SiteId
                                          where a.SiteNo='{siteNo}'";
                Site lookup = null;
                var list = conn.Query<Site, SiteRegistry, Site>(query,
                    (site, siteRegistry) =>
                    {
                        if (lookup == null || lookup.Id != site.Id)
                            lookup = site;
                        if (siteRegistry != null)
                            lookup.SiteRegistries.Add(siteRegistry);
                        return lookup;
                    }).Distinct().ToList();

                return list;
            });
        }

        public bool Delete(string siteNo, string sn)
        {
            var siteId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }
                return (Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist, pls double check.");
            }

            return Execute((conn) =>
            {
                var datetime = DateTime.UtcNow;
                string query = $"delete from  SiteGateway where SiteId='{siteId}'and GatewayNo='{sn}'";
                conn.Execute(query);
                return true;
            });
        }

        public bool Insert(string siteNo, string sn)
        {
            var siteId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }
                return (Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist, pls double check.");
            }

            var siteGatewayId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[SiteGateway] as a where a.SiteId='{siteId}' and a.GatewayNo='{sn}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }
                return (Int64)Id;
            });

            if (siteGatewayId > 0)
            {
                throw new Exception("SN exist, pls double check.");
            }

            return Execute((conn) =>
            {
                var datetime = DateTime.UtcNow;
                string query = $"insert into SiteGateway(SiteId,GatewayNo,CreatedAt) values('{siteId}','{sn}','{datetime}')";
                conn.Execute(query);
                return true;
            });
        }

        public bool Update(string siteNo, string newSn, string oldSn)
        {

            var siteId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }
                return (Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist, pls double check.");
            }

            var siteGatewayId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id FROM [dbo].[SiteGateway] as a where a.GatewayNo='{oldSn}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }
                return (Int64)Id;
            });

            if (siteGatewayId < 0)
            {
                throw new Exception("SN doesn't exist, pls double check.");
            }


            return Execute((conn, transaction) =>
            {
                try
                {

                    var deleteSql = $"delete from GatewayDevice where SiteId={siteId} and GatewayId={siteGatewayId}";
                    conn.Execute(deleteSql, transaction: transaction);

                    var deleteTaskSql = $"delete from GatewayAllowListTask where SiteId={siteId} and GatewayId={siteGatewayId}";
                    conn.Execute(deleteTaskSql, transaction: transaction);

                    var updatedNow = DateTime.UtcNow;

                    var updateSql = $"Update SiteGateway set GatewayNo='{newSn}',UpdatedAt='{updatedNow}' where SiteId={siteId} and GatewayNo='{oldSn}'";
                    conn.Execute(updateSql, transaction: transaction);
                    
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public List<GatewayInfo> GetGatewayListHealth(string siteNo)
        {
            var deviceList = Execute<List<string>>((conn) =>
            {
                string query = $@"SELECT GatewayNo
                                  FROM [dbo].[SiteGateway] a 
                                  inner join [dbo].[Site] b
                                  on a.SiteId=b.Id 
                                  where b.SiteNo='{siteNo}'";
                var devices = conn.Query<string>(query);
                if (devices == null || devices.Count() <= 0)
                {
                    return null;
                }

                return devices.ToList();
            });

            if (deviceList==null || deviceList.Count<=0)
            {
                return new List<GatewayInfo>();
            }

            var deviceNo = string.Join("','", deviceList);

            string query = $@"select * from
                            (select Id, substring(DeviceId,5,LEN(DeviceId)-4) as DeviceId,'online' as ConnectionState,CreatedOnUtc,
                            ROW_NUMBER() over( partition by DeviceId order by CreatedOnUtc desc) as new_index
                            from GatewayInfo where DeviceId in('DGW-{deviceNo}') and [CreatedOnUtc]>DATEADD(DAY,-1,GETDATE())) b where b.new_index = 1";  //PVM-ECOLAB19

            return Execute(_config["ConnectionStrings:SqlConnectionStringHealthDGW"],(conn) =>
            {
                var list = conn.Query<GatewayInfo>(query).ToList();
                var existDeviceNo = list.Select(item=>item.DeviceId);
                var exceptDeviceNo = deviceList.Except(existDeviceNo);
                foreach (var item in exceptDeviceNo)
                {
                    list.Add(new GatewayInfo() {  DeviceId= item , ConnectionState="offline"});
                }

                return list;
            });
        }
    }
}
