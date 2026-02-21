using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiWalletBuffaloBills;

/// <summary>
/// Adds server URLs to Swagger so "Try it out" can target /api-bills (default) or /api.
/// </summary>
public class SwaggerServersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        openApiDocument.Servers = new List<OpenApiServer>
        {
            new() { Url = "/api-bills", Description = "api-bills (default)" },
            new() { Url = "/api", Description = "api" }
        };
    }
}
