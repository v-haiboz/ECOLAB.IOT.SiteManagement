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

        public SiteController(ISiteService siteService)
        {
            _siteService = siteService;
        }


        /// <summary>
        /// 查询site 用siteId
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Tags("Site")]
        [HttpGet("{siteId}")]
        public async Task<dynamic> Get([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId)
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
        
        public async Task<dynamic> Post([FromHeader(Name = "API-Certificate")][Required] string certificationToken,SiteRequestDto siteRequestDto)
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
        public async Task<dynamic> Put([FromHeader(Name = "API-Certificate")][Required] string certificationToken, [Required] string siteId, SiteRequestDto siteRequestDto)
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

    }
}
