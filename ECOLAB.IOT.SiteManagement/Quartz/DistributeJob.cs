
namespace ECOLAB.IOT.SiteManagement.Quartz
{
    using ECOLAB.IOT.SiteManagement.Service;
    using global::Quartz;
    public class DistributeJob : IJob
    {

        private readonly IServiceProvider _provider;
        public DistributeJob(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 创建一个新的作用域
            using (var scope = _provider.CreateScope())
            {
                // 解析你的作用域服务
                var service = scope.ServiceProvider.GetService<IDistributeJobService>();
                if (service!=null)
                {
                    _ = await service.Travle();
                }
            }

           await Task.CompletedTask;
        }
    }
}
