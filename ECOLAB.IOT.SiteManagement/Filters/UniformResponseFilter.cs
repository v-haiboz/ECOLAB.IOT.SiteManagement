using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ECOLAB.IOT.SiteManagement.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class UniformResponseFilter : IAsyncResultFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public UniformResponseFilter(ILogger<UniformResponseFilter> logger)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is FileResult)
            {
                await next();
                return;
            }

            if (context.Result is BadRequestObjectResult badResult)
            {
                UniformResponse<object> targetResult = new UniformResponse<object>();
                string error = "异常";
                if (badResult.Value is ValidationProblemDetails details)
                {
                    error = $@"{details.Title}:{string.Join(";", details.Errors.Select(keyValue => $"[{keyValue.Key}:{string.Join(",", keyValue.Value)}]"))}";
                }
                int status = badResult.StatusCode.HasValue ? badResult.StatusCode.Value : 400;

                targetResult.Failure(error);
                context.Result = new ObjectResult(targetResult);
                context.HttpContext.Response.StatusCode = status;
            }
            else if (context.Result is EmptyResult)
            {
                //targetResult = UniformResponse<object>.Succeed(null);
            }
            else if (context.Result is JsonResult jr)
            {
                if (jr.StatusCode.HasValue)
                {
                    context.HttpContext.Response.StatusCode = jr.StatusCode.Value;
                }
            }
            else
            {
                var objResult = (ObjectResult)context.Result;
                if (objResult.StatusCode == 200 || objResult.StatusCode is null)
                {
                    var value = objResult.Value;
                    object? result = context.Result == null ? true : value;
                    context.Result = new ObjectResult(result);

                    try
                    {
                        if (result != null)
                        {
                            var obj = JsonConvert.DeserializeObject<UniformResponse<dynamic>>(result.ToString());
                            if (obj != null && obj.Status.HasValue)
                            {
                                context.HttpContext.Response.StatusCode = obj.Status.Value;
                            }
                        }
                    }
                    catch
                    {
                    }

                }
                else
                {
                    if (objResult.Value is ProblemDetails details)
                    {
                        int status = details.Status.HasValue ? details.Status.Value : 400;
                        // targetResult = UniformResponse<object>.Failure(new List<string>() { details.Title }, status);
                        context.HttpContext.Response.StatusCode = status;
                    }
                    else
                    {
                        UniformResponse<object> targetResult = new UniformResponse<object>();
                        targetResult.Failure("请求异常");
                        context.Result = new ObjectResult(targetResult);
                        context.HttpContext.Response.StatusCode = 400;
                    }
                }
            }

            await next();
        }
    }
}
