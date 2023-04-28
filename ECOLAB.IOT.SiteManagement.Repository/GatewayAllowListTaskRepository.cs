namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;

    public interface IGatewayAllowListTaskRepository
    {
        public List<GatewayAllowListTask> GetTaskAll();

        public bool InsertHistory(GatewayAllowListTaskHistory gatewayAllowListTaskHistory);
    }

    public class GatewayAllowListTaskRepository : Repository, IGatewayAllowListTaskRepository
    {
        public GatewayAllowListTaskRepository(IConfiguration config) : base(config)
        {
        }

        public List<GatewayAllowListTask> GetTaskAll()
        {
            string query = $@"SELECT * FROM [dbo].[GatewayAllowListTask]";

            return Execute((conn) =>
            {
                var list = conn.Query<GatewayAllowListTask>(query).ToList();
                return list;
            });
        }

        public bool InsertHistory(GatewayAllowListTaskHistory gatewayAllowListTaskHistory)
        {
            return Execute((conn,trans) =>
            {
                try
                {
                    var deleteTaskSql = $"delete from [GatewayAllowListTask] where SiteNo='{gatewayAllowListTaskHistory.SiteNo}' and GatewayNo='{gatewayAllowListTaskHistory.GatewayNo}'";
                    conn.Execute(deleteTaskSql,transaction: trans);

                    var datetime = DateTime.UtcNow;
                    var insertTaskSql = $"INSERT INTO [GatewayAllowListTaskHistory] (SiteId,SiteNo ,GatewayId,GatewayNo,AllowListSASUrl,Status,CreatedAt) " +
                    $"values ('{gatewayAllowListTaskHistory.SiteId}','{gatewayAllowListTaskHistory.SiteNo}','{gatewayAllowListTaskHistory.GatewayId}','{gatewayAllowListTaskHistory.GatewayNo}','{gatewayAllowListTaskHistory.AllowListSASUrl}','1','{datetime}')";
                    conn.Execute(insertTaskSql,transaction: trans);
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return false;
                }
            });
        }

    }
}
