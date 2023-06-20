using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;
using ECOLAB.IOT.SiteManagement.Filters;
using ECOLAB.IOT.SiteManagement.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Authorize]
    [WhiteListFilter]
    [Route("api/sites")]
    public class CertificationController : ControllerBase
    {
        private readonly ICertificationService _certificationService;
        public CertificationController(ICertificationService certificationService)
        {
            _certificationService = certificationService;
        }

        /// <summary>
        /// 创建CertificationInfo
        /// </summary>
        /// <param name="insertCertificationInfoDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("certifications")]
        public async Task<dynamic> CreateCertificationInfo([FromBody]InsertCertificationInfoDto insertCertificationInfoDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _certificationService.Insert(insertCertificationInfoDto);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 获取 CertificationInfo
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Tags("Certification")]
        [HttpGet("certification")]
        public async Task<dynamic> Get(string? certificationName = null, string? certificationToken = null, int pageIndex = 1, int pageSize = 50)
        {
            var result = new UniformPageResponse<dynamic>();
            try
            {
                result.Data = _certificationService.GetCertificationInfos(certificationName, certificationToken, pageIndex, pageSize, out int total);
                result.PageSize = pageSize;
                result.PageIndex = pageIndex;
                result.Total = total;
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return await Task.FromResult(result.ToJsonResult());
        }

        /// <summary>
        /// 刷新 CertificationInfo
        /// </summary>
        /// <param name="insertCertificationInfoDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("certifications/{certificationName}")]
        public async Task<dynamic> Refresh([Required] string certificationName, [FromBody] RefreshCertificationInfoDto refreshCertificationInfoDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _certificationService.Refresh(certificationName,refreshCertificationInfoDto);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 更新CertificationInfo
        /// </summary>
        /// <param name="insertCertificationInfoDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("certifications/{certificationName}")]
        public async Task<dynamic> Update([Required] string certificationName, [FromBody] UpdateCertificationInfoDto updateCertificationInfoDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _certificationService.Update(certificationName,updateCertificationInfoDto);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

        /// <summary>
        /// 删除CertificationInfo
        /// </summary>
        /// <param name="insertCertificationInfoDto"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("certifications/{certificationName}")]
        public async Task<dynamic> Delete([Required] string certificationName)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _certificationService.Delete(certificationName);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }
    }
}
