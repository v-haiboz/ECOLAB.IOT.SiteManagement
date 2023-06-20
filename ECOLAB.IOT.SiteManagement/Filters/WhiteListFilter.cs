namespace ECOLAB.IOT.SiteManagement.Filters
{
    using ECOLAB.IOT.SiteManagement.Common.Utilities;
    using ECOLAB.IOT.SiteManagement.Service;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;

    public class WhiteListFilter : ActionFilterAttribute
    {
        public WhiteListFilter() { }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            string tokenStr = authHeader.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var payload = handler.ReadJwtToken(tokenStr).Payload;
            var claims = payload.Claims;
            var currentEmail = "";
            foreach (var claim in claims)
            {
                if (claim.Type.Equals("unique_name"))
                {
                    currentEmail = claim.Value;
                    break;
                }
            }
            var isAllowAccessSys = true;
            if (!string.IsNullOrWhiteSpace(currentEmail))
            {
                using (IServiceScope scope = context.HttpContext.RequestServices.CreateScope())
                {
                    var cacheService = scope.ServiceProvider.GetService<IMemoryCacheService>();
                    if (cacheService.TryGetValue(currentEmail.AddPrefix(UserWhiteListService.UserWhiteListKeyPrefix), out string email))
                    {
                        isAllowAccessSys = true;
                    }
                    else
                    {
                        var userWhiteListService = scope.ServiceProvider.GetService<IUserWhiteListService>();
                        if (userWhiteListService != null)
                        {
                            var item = await userWhiteListService.GetUserWhiteListByEmail(currentEmail);
                            if (item != null && !string.IsNullOrEmpty(item.Email))
                            {
                                cacheService.SetValue(currentEmail.AddPrefix(UserWhiteListService.UserWhiteListKeyPrefix), currentEmail, true, DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds());
                                isAllowAccessSys = true;
                            }
                            else
                            {
                                isAllowAccessSys = false;
                            }
                        }
                        else
                        {
                            isAllowAccessSys = false;
                        }
                    }
                }
            }
            if (isAllowAccessSys)
            {
                await next();
            }
            else
            {
                context.Result = new ContentResult()
                {
                    ContentType = context.HttpContext.Response.ContentType,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Content = "unique_name is not in whitelist"
                };
                return;
            }
        }
    }
}
