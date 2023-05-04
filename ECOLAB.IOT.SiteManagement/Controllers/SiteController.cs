using ECOLAB.IOT.SiteManagement.Data.Dto;
using ECOLAB.IOT.SiteManagement.Service;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Route("api/sites")]
    [ApiController]
    public class SiteController : ControllerBase
    {

        private readonly ISiteService _siteService;
        private readonly IGetwayService _getwayService;
        private readonly IGatewayDeviceService _gatewayDeviceService;
        private readonly ISiteDeviceHealthService _siteDeviceHealthService;

        public SiteController(ISiteService siteService, IGetwayService getwayService, IGatewayDeviceService gatewayDeviceService, ISiteDeviceHealthService siteDeviceHealthService)
        {
            _siteService = siteService;
            _getwayService = getwayService;
            _gatewayDeviceService = gatewayDeviceService;
            _siteDeviceHealthService = siteDeviceHealthService;
        }


        /// <summary>
        /// 查询site 用siteId
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Tags("Site")]
        [HttpGet("{siteId}")]
        public async Task<dynamic> Get([Required] string siteId)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    return await _siteService.GetRegistryBySiteNo(siteId);
                }

                var result = new JsonResult("SiteId can't empty.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 将初始化现场.(创建site,生成site下包含哪些设备(根据传进来的mesh.json,解析出来device id))
        /// </summary>
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("Site")]
        [HttpPost]
        public async Task<dynamic> Post(SiteRequestDto siteRequestDto)
        {
            try
            {
                if (siteRequestDto.Validate())
                {
                    return await _siteService.Insert(siteRequestDto);
                }

                var result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }

        }

        /// <summary>
        /// 更新现场.(根据传进来的site id 和对应的mesh.json,更新site下有哪些设备)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("Site")]
        [HttpPut("{siteId}")]
        public async Task<dynamic> Put([Required] string siteId, SiteRequestDto siteRequestDto)
        {
            try
            {
                if (siteRequestDto.Validate(false))
                {
                    return await _siteService.Update(siteId, siteRequestDto);
                }

                var result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 把gateway 挂到site下(for example:通过扫描二维码,来调用这个接口).
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayRequestDto"></param>
        /// <returns></returns>
        [Tags("Gateway")]
        [HttpPost("{siteId}/gateway")]
        public async Task<dynamic> Post([Required] string siteId, GatewayRequestDto gatewayRequestDto)
        {
            try
            {
                var result = new JsonResult("Insert into successful.");
                result.StatusCode = (int)HttpStatusCode.OK;
                if (gatewayRequestDto.Validate())
                {
                    var bl= await _getwayService.Insert(siteId, gatewayRequestDto.SN);
                    if (!bl)
                    {
                        result = new JsonResult("Insert into failed.");
                        result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    return result;
                }

                result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }

        }

        /// <summary>
        /// 更新gateway挂到site下(for example:通过扫描二维码,来调用这个接口).
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayRequestUpdateDto"></param>
        /// <returns></returns>
        [Tags("Gateway")]
        [HttpPut("{siteId}/gateway")]
        public async Task<dynamic> Put([Required] string siteId, GatewayRequestUpdateDto gatewayRequestUpdateDto)
        {
            try
            {
                var result = new JsonResult("Updated successful.");
                result.StatusCode= (int)HttpStatusCode.OK;
                if (gatewayRequestUpdateDto.Validate())
                {
                    var bl= await _getwayService.Update(siteId, gatewayRequestUpdateDto.New, gatewayRequestUpdateDto.Old);

                    if (!bl)
                    {
                        result = new JsonResult("Updated failed.");
                        result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    return result;
                }

                result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 查询site下边有那些gateway
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayRequestUpdateDto"></param>
        /// <returns></returns>
        [Tags("Gateway")]
        [HttpGet("{siteId}/gateway")]
        public async Task<dynamic> GetGatewayBySiteId([Required] string siteId)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    return await _getwayService.GetGatewayListHealth(siteId);
                }

                var result = new JsonResult("SiteId can't empty.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 删除dateway的设备.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="DeviceNo"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpDelete]
        [Route("{siteId}/gateway/{deviceNo}")]
        public async Task<dynamic> DeleteDeviceFromSite([Required] string siteId, [Required]  string deviceNo)
        {
            try
            {
                var result = new JsonResult("Delete successful.");
                result.StatusCode = (int)HttpStatusCode.OK;
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(deviceNo))
                {
                    var bl= await _gatewayDeviceService.Delete(siteId, deviceNo);
                    if (!bl)
                    {
                        result = new JsonResult("Delete failed.");
                        result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    return result;
                }

                result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 查询site下有多少设备.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpGet("{siteId}/deviceList")]
        public async Task<dynamic> GetDevicesBySiteNoOrMode(string siteId)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    var list = await _gatewayDeviceService.QueryDeviceListBySiteNo(siteId,"");
                    return list.ToString();
                }

                var result = new JsonResult("SiteId can't empty.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
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
        [HttpPost("{siteId}/gateway/{gatewayId}")]
        public async Task<dynamic> ConfigureDeviceToDGW([Required] string siteId,string gatewayId, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {
            try
            {
                var result = new JsonResult("Insert into successful.");
                result.StatusCode = (int)HttpStatusCode.OK;
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId) && deviceToDGWRequestDto!=null && deviceToDGWRequestDto.Validate())
                {
                    var url = await _gatewayDeviceService.ConfigureDeviceToDGW(siteId, gatewayId, deviceToDGWRequestDto);
                    if (string.IsNullOrEmpty(url))
                    {
                        result = new JsonResult("Insert into failed.");
                        result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    return result;
                }

                result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }

        }

        /// <summary>
        /// 更新gateway下的设备.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayId"></param>
        /// <param name="deviceToDGWRequestDto"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpPut("{siteId}/gateway/{gatewayId}")]
        public async Task<dynamic> UpdateDeviceToDGW([Required] string siteId, string gatewayId, DeviceToDGWRequestDto deviceToDGWRequestDto)
        {
            try
            {
                var result = new JsonResult("Update DeviceToDGW successful.");
                result.StatusCode = (int)HttpStatusCode.OK;
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId) && deviceToDGWRequestDto != null && deviceToDGWRequestDto.Validate())
                {
                    var url = await _gatewayDeviceService.UpdateDeviceToDGW(siteId, gatewayId, deviceToDGWRequestDto);
                    if (string.IsNullOrEmpty(url))
                    {
                        result = new JsonResult("Update DeviceToDGW failed.");
                        result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }

                    return url;
                }

                result = new JsonResult("Request paramter Invalidate.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 查询单台设备状态
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Tags("Health")]
        [HttpGet("{siteId}/{deviceId}")]
        public async Task<dynamic?> GetDeviceStatus([Required] string siteId, string deviceId)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(deviceId))
                {
                    return await _siteDeviceHealthService.GetDeviceStatus(siteId, deviceId);
                }

                var result = new JsonResult("siteId or deviceid can't empty.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }

        /// <summary>
        /// 查询Site下设备状态
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        [Tags("Health")]
        [HttpGet("{siteId}/health")]
        public async Task<dynamic?> GetDeviceStatusListBySiteId([Required] string siteId)
        {
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    var list = await _siteDeviceHealthService.GetDeviceStatusListBySiteId(siteId);
                    return list.ToString();
                }

                var result = new JsonResult("siteId or deviceid can't empty.");
                result.StatusCode = (int)HttpStatusCode.InternalServerError;
                return result;
            }
            catch (Exception ex)
            {
                var result = new JsonResult(ex.Message);
                result.StatusCode = (int)HttpStatusCode.InternalServerError;

                return result;
            }
        }
    }
}
