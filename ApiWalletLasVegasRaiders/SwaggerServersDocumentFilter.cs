using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiWalletLasVegasRaiders;

/// <summary>
/// Adds server URLs to Swagger so "Try it out" can target /api-raiders (default) or /api.
/// </summary>
public class SwaggerServersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        openApiDocument.Servers = new List<OpenApiServer>
        {
            new() { Url = "/api-raiders", Description = "api-raiders (default)" },
            new() { Url = "/api", Description = "api" }
        };
    }
}
