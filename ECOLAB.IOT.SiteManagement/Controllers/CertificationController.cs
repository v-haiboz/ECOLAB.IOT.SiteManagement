using ECOLAB.IOT.SiteManagement.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Authorize]
    [Route("api/sites")]
    public class CertificationController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Tags("Certification")]
        [HttpGet("certification")]
        public async Task<dynamic> Get(string? email = null, int pageIndex = 1, int pageSize = 50)
        {
            var result = new UniformPageResponse<dynamic>();
            //try
            //{
            //    result.Data = await _userWhiteListService.GetUserWhiteList(email, pageIndex, pageSize, out var total);
            //    result.PageSize = pageSize;
            //    result.PageIndex = pageIndex;
            //    result.Total = total;
            //}
            //catch (Exception ex)
            //{
            //    result.Errors.Add(ex.Message);
            //}

            return result.ToJsonResult();
        }
    }
}
