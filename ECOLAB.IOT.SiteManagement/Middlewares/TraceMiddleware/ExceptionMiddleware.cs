namespace ECOLAB.IOT.SiteManagement.Middlewares.TraceMiddleware
{
    using ECOLAB.IOT.SiteManagement.Filters;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Customer Exception Middleware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ExceptionMiddleware(
            RequestDelegate next
            , ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context">context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            #region logging request
            var logInfo = new RequestResponseLog();
            HttpRequest request = context.Request;
            logInfo.Url = request.Path.ToString();
            var authHeader = request.Headers["Authorization"].ToString();
            logInfo.TokenInfo = GetTokenInfo(authHeader);
            logInfo.Method = request.Method;
            logInfo.ExcuteStartTime = DateTime.Now;
            logInfo.QueryString = request.QueryString.Value;
            var method = request.Method.ToLower();
            if (method.Equals("post") || method.Equals("patch") || method.Equals("put"))
            {
                if (request.ContentLength.HasValue && request.ContentLength.Value > 0)
                {
                    request.EnableBuffering();
                    Stream stream = request.Body;
                    byte[] buffer = new byte[request.ContentLength.Value];
                    stream.Read(buffer, 0, buffer.Length);
                    logInfo.RequestBody = Encoding.UTF8.GetString(buffer);
                    request.Body.Position = 0;
                }
            }
            var originalBodyStream = context.Response.Body;
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            #endregion

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var result = new UniformResponse<dynamic>();
                result.Failure(ex.Message);
                _logger.LogError(ex.Message);
                //Write the log to the database as required by UPS
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;
                var setting = new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };
                var responseResult = JsonConvert.SerializeObject(result, setting);
                await context.Response.WriteAsync(responseResult);
            }

            #region logging response
            logInfo.ResponseBody = await FormatResponse(context.Response);
            logInfo.ExcuteEndTime = DateTime.Now;
            _logger.LogInformation($"AccessLog: {logInfo.ToString()}");
            await responseBody.CopyToAsync(originalBodyStream);
            responseBody.Dispose();
            #endregion
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return text;
        }

        private string GetTokenInfo(string authHeader)
        {
            try
            {
                var result = new JObject();
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    string tokenStr = authHeader.Replace("Bearer ", "");
                    var handler = new JwtSecurityTokenHandler();
                    var payload = handler.ReadJwtToken(tokenStr).Payload;
                    var claims = payload.Claims;
                    foreach (var claim in claims)
                    {
                        if (!result.ContainsKey(claim.Type))
                        {
                            result.Add(claim.Type, claim.Value);
                        }
                        else
                        {
                            var jToken = result[claim.Type];
                            try
                            {
                                var jArray = (JArray)jToken;
                                jArray.Add(claim.Value);
                                result[claim.Type] = jArray;
                            }
                            catch
                            {
                                var jArray = new JArray();
                                jArray.Add(jToken);
                                jArray.Add(claim.Value);
                                result[claim.Type] = jArray;
                            }

                        }
                    }
                }
                return JsonConvert.SerializeObject(result);
            }
            catch
            {
                return null;
            }

        }
    }
}
