namespace ECOLAB.IOT.SiteManagement.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// 
    /// </summary>
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
      //  private readonly ILogger<GlobalExceptionFilterAttribute> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public GlobalExceptionFilterAttribute()
        {
           // _logger = logger;
        }
        public override void OnException(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            UniformResponse<object> targetResult = new UniformResponse<object>(); ;
            //非业务异常记录errorLog,返回500状态码，前端通过捕获500状态码进行友好提示
            if (context.Exception is Data.Exceptions.BizException biz)
            {
                //targetResult = UniformResponse<object>.Failure(new List<string>() { biz.Message }, biz.status);
                context.HttpContext.Response.StatusCode = biz.status;
            }
            else
            {
                //targetResult = UniformResponse<object>.Failure(new List<string>() { context.Exception.ToString() });
                context.HttpContext.Response.StatusCode = 500;
            }

            context.Result = new ObjectResult(targetResult);
            base.OnException(context);
        }
    }
}
