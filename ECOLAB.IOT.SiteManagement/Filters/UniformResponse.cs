using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;

namespace ECOLAB.IOT.SiteManagement.Filters
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniformResponse<T>
    {
        /// <summary>
        /// 自定义响应码
        /// </summary>
        [JsonPropertyName("status")]
        public int? Status
        {
            get
            {
                return Errors.Count > 0 ? 400 : 200;
            }
        }
        /// <summary>
        /// 请求结果
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
       [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();
        /// <summary>
        /// 响应的时间
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="data">响应数据</param>
        /// <returns></returns>
        public void Succeed(T? data)
        {
            Data=data;
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="errors">错误信息</param>
        /// <param name="statusCode">自定义的错误响应码</param>
        /// <returns></returns>
        public  void Failure(string errors)
            {
              Errors.Add(errors);
            }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JObject ToConvertJObj()
        {
            var jobj = JObject.FromObject(this,new Newtonsoft.Json.JsonSerializer() {  Formatting=Newtonsoft.Json.Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return jobj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult ToJsonResult()
        {
            var jsonResult = new JsonResult(this);
            jsonResult.StatusCode = Status;
            return jsonResult;
        }
    }
}
