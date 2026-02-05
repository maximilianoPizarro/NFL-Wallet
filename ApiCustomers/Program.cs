using ApiCustomers.Data;
using ApiCustomers.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=customers.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NFL Wallet - Customers API", Version = "v1", Description = "Customer data for NFL Stadium Wallet." });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "API Key to authorize requests. Set in Swagger to test."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }, Array.Empty<string>() }
    });
});

var corsOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5160,http://localhost:5173";
var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (origins.Length == 1 && origins[0] == "*")
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(origins);
        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

// Keycloak/OpenID: uncomment and configure when using authentication
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
    db.Database.EnsureCreated();
    if (!db.Customers.Any())
    {
        var now = DateTime.UtcNow;
        db.Customers.AddRange(
            new Customer { DocumentNumber = "20123456", FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", CreatedAt = now.AddMonths(-6) },
            new Customer { DocumentNumber = "20876543", FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", CreatedAt = now.AddMonths(-4) },
            new Customer { DocumentNumber = "30111222", FirstName = "Robert", LastName = "Johnson", Email = "robert.johnson@example.com", CreatedAt = now.AddMonths(-3) },
            new Customer { DocumentNumber = "30333444", FirstName = "Maria", LastName = "Garcia", Email = "maria.garcia@example.com", CreatedAt = now.AddMonths(-2) },
            new Customer { DocumentNumber = "40555666", FirstName = "James", LastName = "Wilson", Email = "james.wilson@example.com", CreatedAt = now.AddMonths(-1) },
            new Customer { DocumentNumber = "40777888", FirstName = "Emily", LastName = "Brown", Email = "emily.brown@example.com", CreatedAt = now });
        db.SaveChanges();
    }
}

app.UsePathBase("/api");
app.UseRouting();
var apiKey = builder.Configuration["ApiKey"];
if (!string.IsNullOrWhiteSpace(apiKey))
{
    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Headers.TryGetValue("X-API-Key", out var key) && key == apiKey)
        {
            await next(ctx);
            return;
        }
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsync("Missing or invalid X-API-Key.");
    });
}
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "Customers API v1"); });
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
