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
    public class GatewayController : ControllerBase
    {

        private readonly IGetwayService _getwayService;

        public GatewayController(IGetwayService getwayService)
        {
            _getwayService = getwayService;
        }

        /// <summary>
        /// 把gateway 挂到site下(for example:通过扫描二维码,来调用这个接口).
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="gatewayRequestDto"></param>
        /// <returns></returns>
        [Tags("Gateway")]
        [HttpPost("{siteId}/gateway")]
        public async Task<dynamic> Post([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, GatewayRequestDto gatewayRequestDto)
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
        public async Task<dynamic> Put([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, GatewayRequestUpdateDto gatewayRequestUpdateDto)
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
        public async Task<dynamic> DeleteGateway([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, string gatewayId)
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
        public async Task<dynamic> GetGatewayBySiteId([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId)
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

    }
}
