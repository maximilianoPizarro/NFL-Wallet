using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiCustomers;

/// <summary>
/// Adds server URLs to Swagger so "Try it out" can target /api-customers (default) or /api.
/// </summary>
public class SwaggerServersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        openApiDocument.Servers = new List<OpenApiServer>
        {
            new() { Url = "/api-customers", Description = "api-customers (default)" },
            new() { Url = "/api", Description = "api" }
        };
    }
}
