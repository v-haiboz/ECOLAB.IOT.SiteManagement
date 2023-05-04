namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Linq;
    using System.Text;

    public interface ISiteRepository
    {
        public bool Insert(Site site);

        public bool Update(string siteNo, Site site);

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo);

        public List<SiteDevice> GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(string siteNo, List<string> deviceNos);
    }

    public class SiteRepository : Repository, ISiteRepository
    {
        
        public SiteRepository(IConfiguration config) : base(config)
        {
        }

        public List<SiteDevice> GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(string siteNo, List<string> deviceNos)
        {
            var siteId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var Id = conn.ExecuteScalar(query);
                return Id==null?-1:(Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist, pls double check.");
            }

            var validate = Execute((conn) =>
            {
                var devicesList = string.Join("','", deviceNos);
                string query = $@"SELECT Id FROM [dbo].[SiteDevice] where SiteId={siteId} and DeviceNo in('{devicesList}')";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0 || (rows.Count() != deviceNos.Count))
                {
                    return false;
                }
                return true;
            });

            if (!validate)
            {
                throw new Exception($"Some device numbers do not exist, pls double check.");
            }

            return Execute((conn) =>
            {
                var devicesList = string.Join("','", deviceNos);
                string query = $@"SELECT * FROM [dbo].[SiteDevice] where SiteId={siteId} and DeviceNo in('{devicesList}')";
                var rows = conn.Query<SiteDevice>(query);

                return rows.Distinct().ToList();
            });
        }

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo) {
            return Execute((conn) =>
            {
                var exist = Execute((conn) =>
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

        public bool Insert(Site site)
        {
            return Execute((conn, transaction) =>
            {
                try
                {
                    var siteSb = new StringBuilder();
                    siteSb.Append("insert into Site(SiteNo,CreatedAt) values(@SiteNo,@CreatedAt);");
                    siteSb.Append(" SELECT CAST(SCOPE_IDENTITY() as int) ");

                    var siteId = conn.ExecuteScalar(siteSb.ToString(), site, transaction: transaction);
                    if (site.SiteRegistries != null)
                    {
                        foreach (var registry in site.SiteRegistries)
                        {
                            var siteRegistrySb = new StringBuilder();
                            siteRegistrySb.Append(@$"insert into SiteRegistry(SiteId,Model,SourceUrl,TargetUrl,Checksum,JObject,Version,CreatedAt) values('{siteId}','{registry.Model}','{registry.SourceUrl}','{registry.TargetUrl}','{registry.Checksum}','{registry.JObject}','{registry.Version}','{registry.CreatedAt}');");
                            siteRegistrySb.Append(" SELECT CAST(SCOPE_IDENTITY() as int) ");

                            var siteRegistryId = conn.ExecuteScalar(siteRegistrySb.ToString(), registry, transaction: transaction);

                            if (registry.SiteDevices != null)
                            {
                                var siteDeviceSb = new StringBuilder();
                                siteDeviceSb.Append("INSERT INTO [SiteDevice] (SiteId, SiteRegistryId,DeviceNo,JObjectInAllowList,CreatedAt) VALUES ");
                                foreach (var device in registry.SiteDevices)
                                {
                                    siteDeviceSb.AppendFormat("('{0}','{1}','{2}','{3}','{4}'),", siteId, siteRegistryId, device.DeviceNo, device.JObjectInAllowList, device.CreatedAt);
                                }

                                var inserSql = siteDeviceSb.ToString();
                                var sql = inserSql.Substring(0, inserSql.LastIndexOf(','));
                                conn.Execute(sql.ToString(), transaction: transaction);
                            }
                        }
                    }
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

        public bool Update(string siteNo, Site site)
        {  

            var siteId= Execute<float>((conn) =>
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

            if (site.SiteRegistries != null)
            {
                foreach (var registry in site.SiteRegistries)
                {
                    if (registry.SiteDevices != null)
                    {
                        var devices = registry.SiteDevices.Select(item => item.DeviceNo);
                        var exist = Execute<List<string>>((conn) =>
                        {
                            var deviceNos = string.Join("','", devices);
                            string query = $@"SELECT distinct b.SiteNo  FROM [dbo].[SiteDevice] as a
                                             inner join [dbo].[Site] as b
	                                         on a.SiteId=b.Id where a.DeviceNo in ('{deviceNos}') and b.siteNo!='{siteNo}'";
                            var siteNos = conn.Query<string>(query);
                            return siteNos.ToList();

                        });

                        if (exist != null && exist.Count > 0)
                        {
                            throw new Exception($"The devices of registry Mode {registry.Model} also exist in other gateways:{string.Join("','", exist)}.");
                        }
                    }
                }
            }
        

            return Execute((conn, transaction) =>
            {
                try
                {

                    var deleteSql = $"delete from SiteRegistry where SiteId={siteId};delete from SiteDevice where SiteId={siteId}";
                    conn.Execute(deleteSql,transaction: transaction);

                    if (site.SiteRegistries != null)
                    {
                        foreach (var registry in site.SiteRegistries)
                        {
                            var siteRegistrySb = new StringBuilder();
                            siteRegistrySb.Append(@$"insert into SiteRegistry(SiteId,Model,SourceUrl,TargetUrl,Checksum,JObject,Version,CreatedAt) values('{siteId}','{registry.Model}','{registry.SourceUrl}','{registry.TargetUrl}','{registry.Checksum}','{registry.JObject}','{registry.Version}','{registry.CreatedAt}');");
                            siteRegistrySb.Append(" SELECT CAST(SCOPE_IDENTITY() as int) ");

                            var siteRegistryId = conn.ExecuteScalar(siteRegistrySb.ToString(), registry, transaction: transaction);

                            if (registry.SiteDevices != null)
                            {
                                var siteDeviceSb = new StringBuilder();
                                siteDeviceSb.Append("INSERT INTO [SiteDevice] (SiteId, SiteRegistryId,DeviceNo,JObjectInAllowList,CreatedAt) VALUES ");
                                foreach (var device in registry.SiteDevices)
                                {
                                    siteDeviceSb.AppendFormat("('{0}','{1}','{2}','{3}','{4}'),", siteId, siteRegistryId, device.DeviceNo, device.JObjectInAllowList, device.CreatedAt);
                                }

                                var inserSql = siteDeviceSb.ToString();
                                var sql = inserSql.Substring(0, inserSql.LastIndexOf(','));
                                conn.Execute(sql.ToString(), transaction: transaction);
                            }
                        }
                    }
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
     }
}
