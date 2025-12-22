using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinLedger.Modules.Ledger.Api.Infrastructure;

public class TenantHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Tenant-Id",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema 
            { 
                Type = "string", 
                Default = new OpenApiString("public") 
            },
            Description = "Tenant identifier used to isolate data into separate database schemas."
        });
    }
}
