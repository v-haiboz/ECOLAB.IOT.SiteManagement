namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using ECOLAB.IOT.SiteManagement.Provider;
    using ECOLAB.IOT.SiteManagement.Repository;
    using System;

    public interface IDistributeJobService
    {
        public Task<bool> Travel();
    }

    public class DistributeJobService : IDistributeJobService
    {
        private readonly IGatewayAllowListTaskRepository _gatewayAllowListTaskRepository;
        private readonly ITokenProvider _tokenProvider;
        private readonly IDistributeJobProvider _distributeJobProvider;
        public DistributeJobService(IGatewayAllowListTaskRepository gatewayAllowListTaskRepository, ITokenProvider tokenProvider, IStorageProvider storageProvider, IDistributeJobProvider distributeJobProvider)
        {
            _gatewayAllowListTaskRepository = gatewayAllowListTaskRepository;
            _tokenProvider = tokenProvider;
            _distributeJobProvider = distributeJobProvider;
        }

        public async Task<bool> Travel()
        {
            var time = DateTime.UtcNow;

            try
            {
                var tasks = _gatewayAllowListTaskRepository.GetTaskAll();
                await Dispatch(tasks);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }

        }

        private async Task Dispatch(List<GatewayAllowListTask> gatewayAllowListTasks)
        {
            foreach (var task in gatewayAllowListTasks)
            {
                var tokenInfoDto = await _tokenProvider.GetToken();
                var result = await _distributeJobProvider.DistributeTask(task, "localNetwork", tokenInfoDto.Access_Token);
                if (result.Key)
                {
                    _gatewayAllowListTaskRepository.InsertHistory(new GatewayAllowListTaskHistory()
                    {
                        SiteId = task.SiteId,
                        SiteNo = task.SiteNo,
                        GatewayId = task.GatewayId,
                        GatewayNo = task.GatewayNo,
                        AllowListSASUrl = result.Value,
                        Status = 1
                    });
                }
            }

            await Task.CompletedTask;
        }
    }
}
