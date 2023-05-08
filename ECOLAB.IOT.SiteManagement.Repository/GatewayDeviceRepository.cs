namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Text;

    public interface IGatewayDeviceRepository
    {
        public bool Delete(string siteNo, string deviceNo);

        public List<SiteMappingDevice> QueryDeviceListBySiteNo(string siteNo, string mode = null, int pageIndex = 1, int pageSize = 50);

        public bool ConfigureDeviceToDGW(string siteNo, string gatewayNo, List<string> deviceNos,string allowListUrl);

        public bool UpdateDeviceToDGW(string siteNo, string gatewayNo, List<string> deviceNos, string allowListUrl);

        public List<GatewayDevice>? GetGatewayDevicesByGatewayId(int gatewayId);

        public bool GenerateJob(string siteNo, string gatewayNo, string allowListUrl);

    }

    public class GatewayDeviceRepository : Repository, IGatewayDeviceRepository
    {
        public GatewayDeviceRepository(IConfiguration config) : base(config)
        {
        }

        public bool ConfigureDeviceToDGW(string siteNo, string gatewayNo, List<string> deviceNos, string allowListUrl)
        {
            if (string.IsNullOrEmpty(allowListUrl))
            {
                throw new Exception("allowListUrl may fail to generate.");
            }

            var site = Execute((conn) =>
            {
                string query = $@"SELECT *  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var site = conn.Query<Site>(query);

                return site.FirstOrDefault();
            });

            if (site == null)
            {
                throw new Exception("SiteId doesn't exist.");
            }

            var siteGateway = Execute((conn) =>
            {
                string query = $@"SELECT * FROM [dbo].[SiteGateway] as a where a.SiteId='{site.Id}' and a.GatewayNo='{gatewayNo}'";
                var rows = conn.Query<SiteGateway>(query);

                return rows.FirstOrDefault();
            });

            if (siteGateway == null)
            {
                throw new Exception("GatewayId doesn't exist.");
            }

            var exist = Execute((conn) =>
            {
                var devicesList = string.Join("','", deviceNos);
                string query = $@"SELECT Id FROM [dbo].[SiteDevice] where SiteId={site.Id} and DeviceNo in('{devicesList}')";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0 || (rows.Count() != deviceNos.Count))
                {
                    return false;
                }

                return true;
            });

            if (!exist)
            {
                throw new Exception($"Some device numbers doesn't exist.");
            }

            exist = Execute((conn) =>
            {
                string query = $@"SELECT Id FROM [dbo].[GatewayDevice] as a where a.GatewayId='{siteGateway.Id}'";
                var rows = conn.Query(query);
                if (rows == null || rows.Count() <= 0)
                {
                    return false;
                }
                return true;
            });

            if (exist)
            {
                throw new Exception($"GatewayNo:{gatewayNo}  already has a matching device list.");
            }


            return Execute((conn, transaction) =>
            {
                try
                {

                    var datetime = DateTime.UtcNow;
                    var insertSqlSb = new StringBuilder();
                    insertSqlSb.Append("INSERT INTO [GatewayDevice] (SiteId, GatewayId,DeviceNo,CreatedAt) VALUES ");
                    foreach (var deviceNo in deviceNos)
                    {
                        insertSqlSb.AppendFormat("('{0}','{1}','{2}','{3}'),", site.Id, siteGateway.Id, deviceNo, datetime);
                    }

                    if (deviceNos != null && deviceNos.Count > 0)
                    {
                        var insertSql = insertSqlSb.ToString();
                        var sql = insertSql.Substring(0, insertSql.LastIndexOf(','));
                        conn.Execute(sql.ToString(), transaction: transaction);
                    }

                    var insertTaskSql = $"INSERT INTO [GatewayAllowListTask] (SiteId,SiteNo ,GatewayId,GatewayNo,AllowListUrl,Status,CreatedAt) " +
                    $"values ('{site.Id}','{siteNo}','{siteGateway.Id}','{gatewayNo}','{allowListUrl}','0','{datetime}')";
                    conn.Execute(insertTaskSql, transaction: transaction);
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


        public bool UpdateDeviceToDGW(string siteNo, string gatewayNo, List<string> deviceNos, string allowListUrl)
        {
            if (string.IsNullOrEmpty(allowListUrl))
            {
                throw new Exception("allowListUrl may fail to generate.");
            }

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

            var siteGatewayId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[SiteGateway] as a where a.SiteId='{siteId}' and a.GatewayNo='{gatewayNo}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }

                return (Int64)Id;
            });

            if (siteGatewayId < 0)
            {
                throw new Exception("GatewayId doesn't exist.");
            }

            var exist = Execute((conn) =>
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

            if (!exist)
            {
                throw new Exception($"Some device numbers do not exist.");
            }

            return Execute((conn, transaction) =>
            {
                try
                {

                    string deleteGatewayDevice = $@"delete FROM [dbo].[GatewayDevice] where GatewayId='{siteGatewayId}' and SiteId='{siteId}'";
                    conn.Execute(deleteGatewayDevice, transaction: transaction);

                    var datetime = DateTime.UtcNow;
                    var insertSqlSb = new StringBuilder();
                    insertSqlSb.Append("INSERT INTO [GatewayDevice] (SiteId, GatewayId,DeviceNo,CreatedAt) VALUES ");
                    foreach (var deviceNo in deviceNos)
                    {
                        insertSqlSb.AppendFormat("('{0}','{1}','{2}','{3}'),", siteId, siteGatewayId, deviceNo, datetime);
                    }

                    if (deviceNos!=null && deviceNos.Count > 0)
                    {
                        var insertSql = insertSqlSb.ToString();
                        var sql = insertSql.Substring(0, insertSql.LastIndexOf(','));
                        conn.Execute(sql.ToString(), transaction: transaction);
                    }

                    string deleteGatewayAllowListTask = $@"delete FROM [dbo].[GatewayAllowListTask] where GatewayId='{siteGatewayId}' and SiteId='{siteId}'";
                    conn.Execute(deleteGatewayAllowListTask, transaction: transaction);

                    var insertTaskSql = $"INSERT INTO [GatewayAllowListTask] (SiteId,SiteNo ,GatewayId,GatewayNo,AllowListUrl,Status,CreatedAt) " +
                    $"values ('{siteId}','{siteNo}','{siteGatewayId}','{gatewayNo}','{allowListUrl}','0','{datetime}')";
                    conn.Execute(insertTaskSql, transaction: transaction);
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

        public bool Delete(string siteNo, string deviceNo)
        {
            var siteId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[Site] as a where a.SiteNo='{siteNo}'";
                var Id = conn.ExecuteScalar(query);
                return (Int64)Id;
            });

            if (siteId < 0)
            {
                throw new Exception("SiteId doesn't exist.");
            }

            return Execute((conn) =>
            {
                var datetime = DateTime.UtcNow;
                string query = $"delete from GatewayDevice where SiteId='{siteId}'and DeviceNo='{deviceNo}'";
                conn.Execute(query);
                return true;
            });
        }


        public List<SiteMappingDevice> QueryDeviceListBySiteNo(string siteNo, string gatewayid, int pageIndex, int pageSize)
        {
            string query = $@"SELECT a.SiteNo,a.CreatedAt as SiteCreatedAt,c.DeviceNo,c.JObject,c.CreatedAt as DeviceCreatedSAt,b.Model 
                             FROM [dbo].[Site] as a
                             LEFT JOIN [dbo].[SiteRegistry] as b
		                     on a.Id=b.SiteId
                             LEFT JOIN [dbo].[SiteDevice] as c
                             on a.Id=b.SiteId and b.Id=c.SiteRegistryId
                             where a.SiteNo='{siteNo}'";
            if (!string.IsNullOrEmpty(gatewayid))
            {
                query = query + $" and b.Model='{gatewayid}'";
            }

            return Execute((conn) =>
            {
                var list =conn.Query<SiteMappingDevice>(query).ToList();
                list = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                return list;
            });
        }

        public bool GenerateJob(string siteNo, string gatewayNo, string allowListUrl)
        {
            if (string.IsNullOrEmpty(allowListUrl))
            {
                throw new Exception("allowListUrl may fail to generate.");
            }

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

            var siteGatewayId = Execute<float>((conn) =>
            {
                string query = $@"SELECT Id  FROM [dbo].[SiteGateway] as a where a.SiteId='{siteId}' and a.GatewayNo='{gatewayNo}'";
                var Id = conn.ExecuteScalar(query);
                if (Id == null)
                {
                    return -1;
                }

                return (Int64)Id;
            });

            if (siteGatewayId < 0)
            {
                throw new Exception("GatewayId doesn't exist.");
            }

            return Execute((conn, transaction) =>
            {
                try
                {
                    string deleteGatewayAllowListTask = $@"delete FROM [dbo].[GatewayAllowListTask] where GatewayId='{siteGatewayId}' and SiteId='{siteId}'";
                    conn.Execute(deleteGatewayAllowListTask, transaction: transaction);
                    var insertTaskSql = $"INSERT INTO [GatewayAllowListTask] (SiteId,SiteNo ,GatewayId,GatewayNo,AllowListUrl,Status,CreatedAt) " +
                    $"values ('{siteId}','{siteNo}','{siteGatewayId}','{gatewayNo}','{allowListUrl}','0','{DateTime.UtcNow}')";
                    conn.Execute(insertTaskSql, transaction: transaction);
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

        public List<GatewayDevice>? GetGatewayDevicesByGatewayId(int gatewayId)
        {
            return Execute((conn) =>
            {
                string query = $@"SELECT * from[dbo].[GatewayDevice]where GatewayId='{gatewayId}'";
                var list = conn.Query<GatewayDevice>(query);

                return list?.ToList();
            });
        }
    }
}
