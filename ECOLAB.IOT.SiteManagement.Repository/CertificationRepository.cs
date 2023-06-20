namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;
    using ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos;
    using Microsoft.Extensions.Configuration;
    using System.Data.SqlClient;
    using System.Text;

    public interface ICertificationRepository
    {
        public CertificationInfo? Insert(CertificationInfo insertCertificationInfo);

        public List<CertificationInfo>? GetCertificationInfos(out int total, string? certificationName = null, string? certificationToken = null, int pageIndex = 1, int pageSize = 50);
        public CertificationInfo? Refresh(CertificationInfo insertCertificationInfo);

        public bool Delete(string certificationName);

        public CertificationInfo? Update(CertificationInfo insertCertificationInfo);

        public CertificationInfo? GetCertificationInfoByCertificationName(string certificationName);
    }

    public class CertificationRepository : Repository, ICertificationRepository
    {
        public CertificationRepository(IConfiguration config) : base(config)
        {
        }

        public bool Delete(string certificationName)
        {

            if (string.IsNullOrEmpty(certificationName))
            {
                throw new Exception("CertificationName can't be null");
            }

            var item = Execute((conn) =>
            {
                var items = conn.Query($"select * from CertificationInfo where CertificationName='{certificationName}'");

                return items.FirstOrDefault();
            });

            if (item == null)
            {
                throw new Exception("CertificationName doesn't exist.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var insertSql = $"delete from [CertificationInfo] where CertificationName='{certificationName}'";
                    conn.Execute(insertSql);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("CertificationInfo Delete failed.");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public CertificationInfo? GetCertificationInfoByCertificationName(string certificationName)
        {
            var item = Execute((conn) =>
            {
                var querySql = @$"SELECT * FROM [dbo].[CertificationInfo] where CertificationName='{certificationName}'";
                var items = conn.Query<CertificationInfo>(querySql);
                return items.FirstOrDefault();
            });

            return item;
        }

        public List<CertificationInfo>? GetCertificationInfos(out int total, string? certificationName = null, string? certificationToken = null, int pageIndex = 1, int pageSize = 50)
        {
            StringBuilder sb = new StringBuilder();

            var wheresql = $@"where 1=1";
            if (!string.IsNullOrEmpty(certificationName))
                wheresql += $@" and CertificationName='{certificationName}'";

            if (!string.IsNullOrEmpty(certificationToken))
                wheresql += $@" and CertificationToken='{certificationToken}'";

            sb.AppendFormat("SELECT COUNT(1) FROM [dbo].[CertificationInfo] {0};", wheresql);
            sb.AppendFormat(@"SELECT * FROM [dbo].[CertificationInfo] {0} ORDER BY [CreatedAt] OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                    wheresql, (pageIndex - 1) * pageSize, pageSize);
            total = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    using (var reader = conn.QueryMultiple(sb.ToString()))
                    {
                        total = reader.ReadFirst<int>();
                        return reader.Read<CertificationInfo>()?.ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Query failed.");
                }
            }
        }

        public CertificationInfo? Insert(CertificationInfo insertCertificationInfo)
        {
            if (insertCertificationInfo == null || string.IsNullOrEmpty(insertCertificationInfo.CertificationName))
            {
                throw new Exception("CertificationName can't be null");
            }

            var item = Execute((conn) =>
            {
                var items = conn.Query($"select * from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");

                return items.FirstOrDefault();
            });

            if (item!=null && item?.CertificationName== insertCertificationInfo.CertificationName)
            {
                throw new Exception("CertificationName exists.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var insertSql = $"INSERT INTO [CertificationInfo](CertificationName,CertificationDesc,CertificationToken,CertificationTokenExpirationUtcTime,CreatedAt)" +
                    $" values ('{insertCertificationInfo.CertificationName}','{insertCertificationInfo.CertificationDesc}','{insertCertificationInfo.CertificationToken}','{insertCertificationInfo.CertificationTokenExpirationUtcTime}','{DateTime.UtcNow}')";
                    conn.Execute(insertSql);

                    var items=conn.Query<CertificationInfo>($"select CertificationName,CertificationDesc,CertificationToken,CertificationTokenExpirationUtcTime,UpdatedAt,CreatedAt from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");
                    return items.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw new Exception("CertificationInfo Insert failed.");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public CertificationInfo? Refresh(CertificationInfo insertCertificationInfo)
        {
            if (insertCertificationInfo == null || string.IsNullOrEmpty(insertCertificationInfo.CertificationName))
            {
                throw new Exception("CertificationName can't be null");
            }

            var item = Execute((conn) =>
            {
                var items = conn.Query($"select * from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");

                return items.FirstOrDefault();
            });

            if (item == null)
            {
                throw new Exception("CertificationName doesn't exist.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var insertSql = $"Update [CertificationInfo] Set CertificationToken='{insertCertificationInfo.CertificationToken}',CertificationTokenExpirationUtcTime='{insertCertificationInfo.CertificationTokenExpirationUtcTime}',UpdatedAt='{insertCertificationInfo.UpdatedAt}' where CertificationName='{insertCertificationInfo.CertificationName}' ";
                    conn.Execute(insertSql);

                    var items = conn.Query<CertificationInfo>($"select CertificationName,CertificationDesc,CertificationToken,CertificationTokenExpirationUtcTime,UpdatedAt,CreatedAt from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");
                    return items.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw new Exception("CertificationInfo Refresh failed.");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public CertificationInfo? Update(CertificationInfo insertCertificationInfo)
        {
            if (insertCertificationInfo == null || string.IsNullOrEmpty(insertCertificationInfo.CertificationName))
            {
                throw new Exception("CertificationName can't be null");
            }

            var item = Execute((conn) =>
            {
                var items = conn.Query($"select * from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");

                return items.FirstOrDefault();
            });

            if (item == null)
            {
                throw new Exception("CertificationName doesn't exist.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var insertSql = $"Update [CertificationInfo] Set CertificationDesc='{insertCertificationInfo.CertificationDesc}',CertificationTokenExpirationUtcTime='{insertCertificationInfo.CertificationTokenExpirationUtcTime}',UpdatedAt='{insertCertificationInfo.UpdatedAt}' where CertificationName='{insertCertificationInfo.CertificationName}' ";
                    conn.Execute(insertSql);

                    var items = conn.Query<CertificationInfo>($"select CertificationName,CertificationDesc,CertificationToken,CertificationTokenExpirationUtcTime,UpdatedAt,CreatedAt from CertificationInfo where CertificationName='{insertCertificationInfo.CertificationName}'");
                    return items.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw new Exception("CertificationInfo Update failed.");
                }
                finally
                {
                    conn.Close();
                }
            });
        }
    }
}
