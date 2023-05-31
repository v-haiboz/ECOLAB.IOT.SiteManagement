namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Data.Exceptions;
    using Microsoft.Extensions.Configuration;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public interface ISiteRepository
    {
        public bool Insert(Site site);

        public bool Update(string siteNo, Site site);

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo);

        public List<SiteDevice> GeAllowListOfSiteDeviceBySiteNoAndDeviceNos(string siteNo, List<string> deviceNos);

        public List<SiteDeviceDetailInfo> GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(string siteNo, string gatewayNo, List<string> deviceNos);
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
                return Id == null ? -1 : (Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist.");
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
                throw new Exception($"Some device numbers do not exist.");
            }

            return Execute((conn) =>
            {
                var devicesList = string.Join("','", deviceNos);
                string query = $@"SELECT * FROM [dbo].[SiteDevice] where SiteId={siteId} and DeviceNo in('{devicesList}')";
                var rows = conn.Query<SiteDevice>(query);

                return rows.Distinct().ToList();
            });
        }


        public class GateWayDevice
        { 
           public string GatewayNo { get; set; }
            public string DeviceNo { get; set; }
        }

        public List<SiteDeviceDetailInfo> GeAllowListOfSiteDeviceBySiteNoAndGatewayNoAndDeviceNos(string siteNo, string gatewayNo, List<string> deviceNos)
        {
            var gateWayDevice = Execute((conn) =>
            {
                string query = $@"select b.GatewayNo,a.DeviceNo from [dbo].[GatewayDevice] as a
                                inner join [dbo].[SiteGateway] as b
                                on a.GatewayId=b.Id where a.DeviceNo in('{string.Join("','", deviceNos)}') and b.GatewayNo!='{gatewayNo}'";
                var site = conn.Query<GateWayDevice>(query);
                return site;
            });

            if (gateWayDevice != null && gateWayDevice.Count()>0)
            {
                var gateways = gateWayDevice.Select(item => item.GatewayNo)?.ToList()?.Distinct();
                var sb = new StringBuilder("Some Devices have been configured:");
                if (gateways != null)
                {
                   
                    foreach (var item in gateways)
                    {
                        var devices = gateWayDevice.Where(s => s.GatewayNo == $"{item}")?.ToList();
                        sb.Append($"gateway({item})->'{string.Join("','", devices?.Select(device=>device.DeviceNo))}';") ;
                    }
                }
                

                throw new Exception(sb.ToString());
            }

            return Execute((conn) =>
            {
                var devicesList = string.Join("','", deviceNos);
                string query = $@"select a.SiteId,c.SiteNo,a.SiteRegistryId,b.Model,d.Id as GatewayId,d.GatewayNo,a.DeviceNo,a.JObjectInAllowList,a.UpdatedAt,a.CreatedAt from [dbo].[SiteDevice] as a
                                    inner join [dbo].[SiteRegistry] as b
                                    on a.SiteId=b.SiteId and a.SiteRegistryId=b.Id
                                    inner join [dbo].[Site] as c
                                    on a.SiteId=c.Id
                                    inner join [dbo].[SiteGateway] as d
                                    on a.SiteId=d.SiteId
                                    where d.GatewayNo='{gatewayNo}' and c.SiteNo='{siteNo}' and a.DeviceNo in('{devicesList}')";
                var rows = conn.Query<SiteDeviceDetailInfo>(query);

                return rows.Distinct().ToList();
            });
        }

        public List<Site> GetSiteRegistiesBySiteNo(string siteNo)
        {
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
                    throw new BizException($"SiteId:{siteNo} doesn't exist.");
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
            var exist = Execute((conn) =>
            {
                string query = $@"SELECT Id
                                  FROM [dbo].[Site]
                                  where SiteNo='{site.SiteNo}'";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0)
                {
                    return false;
                }
                return true;
            });

            if (exist)
            {
                throw new BizException($"SiteId:{site.SiteNo} already exists.");
            }

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
                throw new Exception("SiteId doesn't exist.");
            }

            if (site.SiteRegistries != null)
            {
                foreach (var registry in site.SiteRegistries)
                {
                    if (registry.SiteDevices != null)
                    {
                        var devices = registry.SiteDevices.Select(item => item.DeviceNo);
                        var exist = Execute((conn) =>
                        {
                            var deviceNos = string.Join("','", devices);
                            string query = $@"SELECT distinct b.SiteNo,a.DeviceNo FROM [dbo].[SiteDevice] as a
                                             inner join [dbo].[Site] as b
	                                         on a.SiteId=b.Id where a.DeviceNo in ('{deviceNos}') and b.siteNo!='{siteNo}'";
                            var siteNos = conn.Query<SiteNoAndSiteDevice>(query);
                            return siteNos.ToList();

                        });

                        if (exist != null && exist.Count > 0)
                        {
                            var deviceNos=exist?.Select(item => item.DeviceNo);
                            if (deviceNos == null)
                            {
                                deviceNos = new List<string>();
                            }

                            var siteNos = exist?.Select(item => item.SiteNo)?.ToList()?.Distinct();
                            if (siteNos == null)
                            {
                                siteNos = new List<string>();
                            }
                            throw new Exception($"The devices:'{string.Join("','", deviceNos)}' of registry Mode {registry.Model} also exist in other SiteId:'{string.Join("','", siteNos)}'.");
                        }
                    }
                }
            }

            var gatewayIds = Execute<List<float>?>((conn) =>
            {
                string query = $@"select Id from [dbo].[SiteGateway] where SiteId='{siteId}'";
                var Ids = conn.Query<float>(query);
                return Ids?.ToList();
            });

            return Execute((conn, transaction) =>
            {
                try
                {
                    var deviceNos = new List<string>();
                    if (site.SiteRegistries != null)
                    {
                        var devices = site.SiteRegistries.SelectMany(item => item.SiteDevices)?.ToList();
                        deviceNos = devices?.Select(item => item.DeviceNo)?.Distinct()?.ToList();
                        var models= site.SiteRegistries.Select(item => item.Model)?.ToList()?.Distinct();
                       var deleteSql = $"delete from SiteRegistry where SiteId={siteId} and Model not in('{string.Join("','",models)}'); delete from SiteDevice where SiteId={siteId} and DeviceNo not in('{string.Join("','", deviceNos)}')";
                       conn.Execute(deleteSql, transaction: transaction);
                    }


                    if (site.SiteRegistries != null)
                    {
                        foreach (var registry in site.SiteRegistries)
                        {

                            var siteRegistry = Execute((conn) =>
                            {
                                string query = $@"select * from [dbo].[SiteRegistry] where SiteId='{siteId}' and Model='{registry.Model}'";
                                var siteRegistries = conn.Query<SiteRegistry>(query);
                                return siteRegistries.FirstOrDefault();
                            });

                            var siteRegistrySb = new StringBuilder();
                           
                            if (siteRegistry == null)
                            {
                                siteRegistrySb.Append(@$"insert into SiteRegistry(SiteId,Model,SourceUrl,TargetUrl,Checksum,JObject,Version,CreatedAt) values('{siteId}','{registry.Model}','{registry.SourceUrl}','{registry.TargetUrl}','{registry.Checksum}','{registry.JObject}','{registry.Version}','{registry.CreatedAt}');");
                                siteRegistrySb.Append(" SELECT CAST(SCOPE_IDENTITY() as int) ");
                            }
                            else if (siteRegistry != null && siteRegistry.Checksum != registry.Checksum)
                            {
                                var deleteSql = $"delete from SiteDevice where SiteId={siteId} and SiteRegistryId='{siteRegistry.Id}'";
                                conn.Execute(deleteSql, transaction: transaction);
                                siteRegistrySb.Append(@$"Update SiteRegistry set SourceUrl='{registry.SourceUrl}',TargetUrl='{registry.TargetUrl}',Checksum='{registry.Checksum}',JObject='{registry.JObject}',Version='{registry.Version}',UpdatedAt='{DateTime.UtcNow}' where SiteId='{siteId}' and Model='{registry.Model}'");
                                siteRegistrySb.Append($" SELECT Id from SiteRegistry  where SiteId='{siteId}' and Model='{registry.Model}' ");
                            }
                            else
                            {
                                registry.Version = siteRegistry.Version;
                                continue;
                            }

                            var siteRegistryId = conn.ExecuteScalar(siteRegistrySb.ToString(), registry, transaction: transaction);

                            if (registry.SiteDevices != null && registry.SiteDevices.Count>0)
                            {
                                var siteDeviceSb = new StringBuilder();
                                siteDeviceSb.Append("INSERT INTO [SiteDevice] (SiteId, SiteRegistryId,DeviceNo,JObjectInAllowList,CreatedAt) VALUES ");
                                foreach (var device in registry.SiteDevices)
                                {
                                    siteDeviceSb.AppendFormat("('{0}','{1}','{2}','{3}','{4}'),", siteId, siteRegistryId, device.DeviceNo, device.JObjectInAllowList, device.CreatedAt);
                                    //gatewayDevices.Add(device.DeviceNo);
                                }

                                var inserSql = siteDeviceSb.ToString();
                                var sql = inserSql.Substring(0, inserSql.LastIndexOf(','));
                                conn.Execute(sql.ToString(), transaction: transaction);
                            }
                        }
                    }


                    if (gatewayIds != null && gatewayIds.Count > 0)
                    {
                        var deleteGatewayDeviceSql = $"delete FROM [dbo].[GatewayDevice] where GatewayId in ('{string.Join("','", gatewayIds)}')";
                        if (deviceNos.Count > 0)
                        {
                            deleteGatewayDeviceSql = $"delete FROM [dbo].[GatewayDevice] where GatewayId in ('{string.Join("','", gatewayIds)}') and DeviceNo not in('{string.Join("','", deviceNos)}')";
                        }

                        conn.Execute(deleteGatewayDeviceSql, transaction: transaction);
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

        public class SiteNoAndSiteDevice
        {
            public string SiteNo { get; set; } = string.Empty;
            public string DeviceNo { get; set; } = string.Empty;
        }
    }
}
