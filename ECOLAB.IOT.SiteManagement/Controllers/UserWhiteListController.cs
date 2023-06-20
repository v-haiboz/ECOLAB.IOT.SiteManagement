using ECOLAB.IOT.SiteManagement.Data.Dto;
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
    public class UserWhiteListController : ControllerBase
    {

        private readonly IUserWhiteListService _userWhiteListService;

        public UserWhiteListController(IUserWhiteListService userWhiteListService)
        {
            _userWhiteListService = userWhiteListService;
        }


        /// <summary>
        /// 查询 UserWhiteList.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpGet("userwhitelist")]
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
        /// 删除 UserWhiteList.
        /// </summary>
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpDelete()]
        [Route("userwhitelist/{email}")]

        public async Task<dynamic> Delete([Required] string email)
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
        /// 添加 UserWhiteList.
        /// </summary>
        /// <param name="siteRequestDto"></param>
        /// <returns></returns>
        [Tags("UserWhiteList")]
        [HttpPost("userwhitelist")]

        public async Task<dynamic> Post([FromBody]InsertUserWhiteListDto insertUserWhiteListDto)
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
