using ECOLAB.IOT.SiteManagement.Data.Dto;
using ECOLAB.IOT.SiteManagement.Filters;
using ECOLAB.IOT.SiteManagement.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Route("api/sites")]
    [ApiController]
    public class HealthController : ControllerBase
    {

        private readonly ISiteDeviceHealthService _siteDeviceHealthService;

        public HealthController(ISiteDeviceHealthService siteDeviceHealthService)
        {
            _siteDeviceHealthService = siteDeviceHealthService;
        }

        /// <summary>
        /// 查询单台设备状态
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Tags("Health")]
        [HttpGet("{siteId}/{deviceId}")]
        public async Task<dynamic?> GetDeviceStatus([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, string deviceId)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(deviceId))
                {
                    result.Data= await _siteDeviceHealthService.GetDeviceStatus(siteId, deviceId);
                    return result.ToConvertJObj().ToString();
                }
                else
                {
                    result.Failure("siteId or deviceid can't empty.");
                    return result.ToJsonResult();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 查询Site下设备状态
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Tags("Health")]
        [HttpGet("{siteId}/health")]
        public async Task<dynamic?> GetDeviceStatusListBySiteId([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    result.Data = await _siteDeviceHealthService.GetDeviceStatusListBySiteId(siteId);
                    return result.ToConvertJObj().ToString();
                }
                else {
                    result.Failure("siteId can't empty.");
                    return result.ToJsonResult();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result.ToJsonResult();
            }
        }
    }
}
