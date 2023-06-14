namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Text;

    public interface IUserWhiteListRepository
    {
        public List<UserWhiteList>? GetUserWhiteList(string email, int pageIndex, int pageSize, out int total);

        public UserWhiteList InsertUserWhiteList(UserWhiteList userWhiteList);

        public bool DeleteUserWhiteList(string email);
    }

    public class UserWhiteListRepository : Repository, IUserWhiteListRepository
    {
        public UserWhiteListRepository(IConfiguration config) : base(config)
        {
        }

        public bool DeleteUserWhiteList(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new Exception("Email can't be null.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var deleteSql = $"delete from [UserWhiteList] where Email='{email}'";
                    conn.Execute(deleteSql);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Email delete failed.");
                }
                finally
                {
                    conn.Close();
                }
            });
        }

        public List<UserWhiteList>? GetUserWhiteList(string email, int pageIndex, int pageSize, out int total)
        {
            StringBuilder sb = new StringBuilder();

            var wheresql= $@"where 1=1";
            if(!string.IsNullOrEmpty(email))
                wheresql = $@"where a.Email='{email}'";

            sb.AppendFormat("SELECT COUNT(1) FROM [dbo].[UserWhiteList] {0};", wheresql);
            sb.AppendFormat(@"SELECT * FROM [dbo].[UserWhiteList] {0} ORDER BY [CreatedAt] OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY",
                    wheresql, (pageIndex-1)*pageSize, pageSize);
            total = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                try
                {
                    using (var reader = conn.QueryMultiple(sb.ToString()))
                    {
                        total = reader.ReadFirst<int>();
                        return reader.Read<UserWhiteList>()?.ToList();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Query failed.");
                }
            }
        }

        public UserWhiteList InsertUserWhiteList(UserWhiteList userWhiteList)
        {
            if (userWhiteList == null || string.IsNullOrEmpty(userWhiteList.Email))
            {
                throw new Exception("Email can't be null.");
            }

            var entity = Execute((conn) =>
            {
                string query = $@"SELECT *  FROM [dbo].[UserWhiteList] as a where a.Email='{userWhiteList.Email}'";
                var entity = conn.Query(query);

                return entity.FirstOrDefault();
            });

            if (entity!=null)
            {
                throw new Exception($"Email:{userWhiteList.Email} exists.");
            }

            return Execute((conn) =>
            {
                try
                {
                    var insertSql = $"INSERT INTO [UserWhiteList] (Email,CreatedAt) values ('{userWhiteList.Email}','{DateTime.UtcNow}')";
                    conn.Execute(insertSql);
                    return userWhiteList;
                }
                catch (Exception ex)
                {
                    throw new Exception("Email Insert failed."); 
                }
                finally
                {
                    conn.Close();
                }
            });
        }
    }
}
