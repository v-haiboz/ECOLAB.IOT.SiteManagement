namespace ECOLAB.IOT.SiteManagement
{
    using Azure;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Linq;
    public class SwaggerParameterAttributeFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
           
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //var attributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            //   .Union(context.MethodInfo.GetCustomAttributes(true))
            //   .OfType<SwaggerParameterAttribute>();

            //foreach (var attribute in attributes)
            //    operation.Parameters.Add(new OpenApiParameter
            //    {
            //        Name = attribute.Name,
            //        Description = attribute.Description,
            //        In = attribute.ParameterType,
            //        Required = attribute.Required,
            //        Type = attribute.DataType
            //    });
        }
    }
}
