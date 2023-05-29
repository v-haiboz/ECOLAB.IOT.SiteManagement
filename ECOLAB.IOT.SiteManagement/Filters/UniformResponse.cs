using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Runtime.CompilerServices;
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
        /// status
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
        /// data of result
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();
        /// <summary>
        /// response timestamp.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// sucessful.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void Succeed(T? data)
        {
            Data = data;
        }

        /// <summary>
        /// filed.
        /// </summary>
        /// <param name="errors">错误信息</param>
        /// <param name="statusCode">自定义的错误响应码</param>
        /// <returns></returns>
        public void Failure(string errors)
        {
            Errors.Add(errors);
        }

        /// <summary>
        /// To Conver JObj.
        /// </summary>
        /// <returns></returns>
        public JObject ToConvertJObj()
        {
            var jobj = JObject.FromObject(this, new Newtonsoft.Json.JsonSerializer() { Formatting = Newtonsoft.Json.Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return jobj;
        }

        /// <summary>
        /// To Json Result.
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
