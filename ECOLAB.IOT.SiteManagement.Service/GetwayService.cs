namespace ECOLAB.IOT.SiteManagement.Service
{
    using ECOLAB.IOT.SiteManagement.Data.Dto;
    using ECOLAB.IOT.SiteManagement.Repository;
    using System.Threading.Tasks;

    public interface IGetwayService
    {
        public Task<bool> Insert(string siteNo, string sn);

        public Task<bool> Update(string siteNo, string newSn, string oldSn);

        public Task<bool> Delete(string siteNo, string sn);

        public Task<GatewayHealthDto> GetGatewayListHealth(string siteNo);
    }

    public class GetwayService : IGetwayService
    {
        private readonly IGetwayRepository _getwayRepository;
        public GetwayService(IGetwayRepository getwayRepository)
        {
            _getwayRepository = getwayRepository;
        }

        public async Task<bool> Delete(string siteNo, string sn)
        {
            var bl = _getwayRepository.Delete(siteNo, sn);
            return await Task.FromResult(bl);
        }

        public async Task<GatewayHealthDto> GetGatewayListHealth(string siteNo)
        {
            var gateways = _getwayRepository.GetGatewayListHealth(siteNo);
            var gatewayHealthDto = new GatewayHealthDto()
            {
                SiteNo = siteNo,
                Gateways = new List<GatewayItemDto>()
            };

            if (gateways == null || gateways.Count <= 0)
                return gatewayHealthDto;

            gatewayHealthDto.Total = gateways.Count;

            foreach (var item in gateways)
            {
                gatewayHealthDto.Gateways.Add(new GatewayItemDto()
                {
                    SiteNo = siteNo,
                    GatewayNo = item.DeviceId,
                    ConnectionState = item.ConnectionState,
                    Lastseen = item.CreatedOnUtc
                });
            }

            return await Task.FromResult(gatewayHealthDto);

        }

        public async Task<bool> Insert(string siteNo, string sn)
        {
            var bl = _getwayRepository.Insert(siteNo, sn);
            return await Task.FromResult(bl);
        }

        public async Task<bool> Update(string siteNo, string newSn, string oldSn)
        {
            var bl = _getwayRepository.Update(siteNo, newSn, oldSn);
            return await Task.FromResult(bl);
        }
    }
}
