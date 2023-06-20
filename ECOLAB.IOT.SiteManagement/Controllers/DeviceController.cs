using ECOLAB.IOT.SiteManagement.Data.Dto;
using ECOLAB.IOT.SiteManagement.Filters;
using ECOLAB.IOT.SiteManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Authorize]
    [Route("api/sites")]
    [ApiController]
    public class DeviceController : ControllerBase
    {

        private readonly IGatewayDeviceService _gatewayDeviceService;

        public DeviceController(IGatewayDeviceService gatewayDeviceService)
        {
            _gatewayDeviceService = gatewayDeviceService;
        }

        /// <summary>
        /// 删除dateway的设备.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="DeviceNo"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpDelete]
        [Route("{siteId}/devicemapping/{deviceId}")]
        public async Task<dynamic> DeleteDeviceFromSite([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, [Required]  string deviceId)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(deviceId))
                {
                    var bl = await _gatewayDeviceService.Delete(siteId, deviceId);
                    if (!bl)
                    {
                        result.Failure("Delete failed.");
                    }
                }
                else {
                    result.Failure("siteId or deviceid can't empty.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 查询site下有多少设备.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpGet("{siteId}/devicemapping")]
        public async Task<dynamic> GetDevicesBySiteNoOrMode([FromHeader(Name = "API-Certificate")][Required] string certificationToken, string siteId, [FromQuery] string? gatewayId = "")
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    var list= await _gatewayDeviceService.QueryDeviceListBySiteNo(siteId, gatewayId);
                    result.Data = list;
                    return result.ToConvertJObj().ToString();
                }
                else {
                    result.Failure("siteId or deviceid can't empty.");
                    return result.ToJsonResult();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result.ToJsonResult();
            }
        }

        /// <summary>
        /// 把设备添加到gateway下.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayId"></param>
        /// <param name="deviceToDGWRequestDto"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpPost("{siteId}/devicemapping/{gatewayId}")]
        public async Task<dynamic> ConfigureDeviceToDGW([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId,string gatewayId, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId) && deviceToDGWRequestDto != null && deviceToDGWRequestDto.Validate())
                {
                    var url = await _gatewayDeviceService.ConfigureDeviceToDGW(siteId, gatewayId, deviceToDGWRequestDto);
                    if (string.IsNullOrEmpty(url))
                    {
                        result.Failure("Insert into failed.");
                    }
                }
                else {
                    result.Failure("siteId and gatewayId can't empty, or deviceToDGWRequestDto invalidate.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 添加一个设备到gateway.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayId"></param>
        /// <param name="deviceToDGWOneByOneRequestDto"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpPatch("{siteId}/devicemapping/{gatewayId}")]
        public async Task<dynamic> AddDeviceToDGWOneByOne([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, string gatewayId, DeviceToDGWOneByOneRequestDto deviceToDGWOneByOneRequestDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId) && deviceToDGWOneByOneRequestDto != null && !string.IsNullOrEmpty(deviceToDGWOneByOneRequestDto.DeviceId))
                {
                    var bl = await _gatewayDeviceService.AddDeviceToDGWOneByOne(siteId, gatewayId, deviceToDGWOneByOneRequestDto);
                    if (!bl)
                    {
                        result.Failure("add Device to DGW failed.");
                    }
                }
                else
                {
                    result.Failure("siteId and gatewayId and deviceId can't empty.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }
    }
}
