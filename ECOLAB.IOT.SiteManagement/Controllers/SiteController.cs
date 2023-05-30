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
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data= await _siteService.GetRegistryBySiteNo(siteId);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }
           
            return result.ToJsonResult();
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
            var result = new UniformResponse<dynamic>();
            try
            {
                if (siteRequestDto.Validate())
                {
                    result.Data = await _siteService.Insert(siteRequestDto);
                }
                else
                {
                    result.Failure("SiteRequestDto invalidate.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();

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
            var result = new UniformResponse<dynamic>();
            try
            {
                if (siteRequestDto.Validate(false))
                {
                    result.Data = await _siteService.Update(siteId, siteRequestDto);
                }
                else
                {
                    result.Failure("SiteRequestDto invalidate.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
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
            var result = new UniformResponse<dynamic>();
            try
            {
                if (gatewayRequestDto.Validate())
                {
                    var bl = await _getwayService.Insert(siteId, gatewayRequestDto.SN);
                    if (!bl)
                    {
                        result.Failure("Insert into failed.");
                    }
                }
                else
                {
                    result.Failure("GatewayRequestDto invalidate.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
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
            var result = new UniformResponse<dynamic>();
            try
            {
                if (gatewayRequestUpdateDto.Validate())
                {
                    var bl = await _getwayService.Update(siteId, gatewayRequestUpdateDto.New, gatewayRequestUpdateDto.Old);

                    if (!bl)
                    {
                        result.Failure("Updated failed.");
                    }
                }
                else {
                    result.Failure("GatewayRequestUpdateDto invalidate.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayId"></param>
        /// <returns></returns>
        [Tags("Gateway")]
        [HttpDelete("{siteId}/gateway/{gatewayId}")]
        public async Task<dynamic> DeleteGateway([Required] string siteId, string gatewayId)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId))
                {
                    var bl = await _getwayService.Delete(siteId, gatewayId);
                    if (!bl)
                    {
                        result.Failure("Delete failed.");
                    }
                }
                else
                {
                    result.Failure("siteId or gatewayId can't empty.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
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
            var result = new UniformResponse<dynamic>();
            try
            {
                if (!string.IsNullOrEmpty(siteId))
                {
                    result.Data = await _getwayService.GetGatewayListHealth(siteId);
                }
                else
                {
                    result.Failure("siteId can't empty.");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
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
        public async Task<dynamic> DeleteDeviceFromSite([Required] string siteId, [Required]  string deviceId)
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
        public async Task<dynamic> GetDevicesBySiteNoOrMode(string siteId, [FromQuery] string? gatewayId = "")
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
        public async Task<dynamic> ConfigureDeviceToDGW([Required] string siteId,string gatewayId, DeviceToDGWRequestDto deviceToDGWRequestDto)
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

        ///// <summary>
        ///// 更新gateway下的设备.
        ///// </summary>
        ///// <param name="siteId"></param>
        ///// <param name="gatewayId"></param>
        ///// <param name="deviceToDGWRequestDto"></param>
        ///// <returns></returns>
        //[Tags("Device")]
        //[HttpPut("{siteId}/devicemapping/{gatewayId}")]
        //public async Task<dynamic> UpdateDeviceToDGW([Required] string siteId, string gatewayId, DeviceToDGWRequestDto deviceToDGWRequestDto)
        //{
        //    var result = new UniformResponse<dynamic>();
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(siteId) && !string.IsNullOrEmpty(gatewayId) && deviceToDGWRequestDto != null && deviceToDGWRequestDto.Validate())
        //        {
        //            var url = await _gatewayDeviceService.UpdateDeviceToDGW(siteId, gatewayId, deviceToDGWRequestDto);
        //            if (string.IsNullOrEmpty(url))
        //            {
        //                result.Failure("Update DeviceToDGW failed.");
        //            }
        //        }
        //        else
        //        {
        //            result.Failure("siteId and gatewayId can't empty, or deviceToDGWRequestDto invalidate.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Errors.Add(ex.Message);
        //    }

        //    return result.ToJsonResult();
        //}

        /// <summary>
        /// 添加一个设备到gateway.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayId"></param>
        /// <param name="deviceToDGWOneByOneRequestDto"></param>
        /// <returns></returns>
        [Tags("Device")]
        [HttpPatch("{siteId}/devicemapping/{gatewayId}")]
        public async Task<dynamic> AddDeviceToDGWOneByOne([Required] string siteId, string gatewayId, DeviceToDGWOneByOneRequestDto deviceToDGWOneByOneRequestDto)
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
        public async Task<dynamic?> GetDeviceStatusListBySiteId([Required] string siteId)
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
