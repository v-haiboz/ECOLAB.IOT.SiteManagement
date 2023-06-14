using ECOLAB.IOT.SiteManagement.Data.Dto;
using ECOLAB.IOT.SiteManagement.Filters;
using ECOLAB.IOT.SiteManagement.Service;
using Microsoft.AspNetCore.Mvc;

namespace ECOLAB.IOT.SiteManagement.Controllers
{
    [Route("api/sites")]
    public class UserWhiteListController : ControllerBase
    {

        private readonly IUserWhiteListService _userWhiteListService;

        public UserWhiteListController(IUserWhiteListService userWhiteListService)
        {
            _userWhiteListService = userWhiteListService;
        }


        /// <summary>
        /// 查询UserWhiteList.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpGet("UserWhiteList")]
        public async Task<dynamic> Get(string? email = null, int pageIndex = 1, int pageSize = 50)
        {
            var result = new UniformPageResponse<dynamic>();
            try
            {
                result.Data = await _userWhiteListService.GetUserWhiteList(email, pageIndex, pageSize,out var total);
                result.PageSize = pageSize;
                result.PageIndex = pageIndex;
                result.Total = total;
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
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpDelete("UserWhiteList")]

        public async Task<dynamic> Delete(string email)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _userWhiteListService.DeleteUserWhiteList(email);
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
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpPost("UserWhiteList")]

        public async Task<dynamic> Post(InsertUserWhiteListDto insertUserWhiteListDto)
        {
            var result = new UniformResponse<dynamic>();
            try
            {
                result.Data = await _userWhiteListService.InsertUserWhiteList(insertUserWhiteListDto);
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
            }

            return result.ToJsonResult();
        }

    }
}
