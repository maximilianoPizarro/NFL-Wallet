using ApiWalletBuffaloBills.Data;
using ApiWalletBuffaloBills.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=buffalobills.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NFL Wallet - Buffalo Bills API", Version = "v1", Description = "Buffalo Bills wallet balance and transactions for NFL Stadium Wallet." });
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
    c.DocumentFilter<ApiWalletBuffaloBills.SwaggerServersDocumentFilter>();
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
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    db.Database.EnsureCreated();
    if (!db.Balances.Any())
    {
        var now = DateTime.UtcNow;
        // Customer 1 - Buffalo Bills wallet
        var b1 = new WalletBalance { CustomerId = 1, Currency = "USD", AvailableBalance = 2847.25m, PendingBalance = 0, UpdatedAt = now };
        db.Balances.Add(b1);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Credit", Amount = 1000, Description = "Initial deposit", Status = "Completed", CreatedAt = now.AddDays(-30) },
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Credit", Amount = 2500, Description = "Payroll deposit", Status = "Completed", CreatedAt = now.AddDays(-14) },
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Debit", Amount = 150.75m, Description = "Utility payment", Status = "Completed", CreatedAt = now.AddDays(-7) },
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Debit", Amount = 502, Description = "Online purchase", Status = "Completed", CreatedAt = now.AddDays(-2) },
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Credit", Amount = 1000.50m, Description = "Transfer in", Status = "Completed", CreatedAt = now.AddDays(-1) });

        // Customer 2
        var b2 = new WalletBalance { CustomerId = 2, Currency = "USD", AvailableBalance = 5200.00m, PendingBalance = 200.00m, UpdatedAt = now };
        db.Balances.Add(b2);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b2.Id, Type = "Credit", Amount = 5400, Description = "Salary transfer", Status = "Completed", CreatedAt = now.AddDays(-5) },
            new WalletTransaction { WalletBalanceId = b2.Id, Type = "Debit", Amount = 200, Description = "Transfer out", Status = "Pending", CreatedAt = now.AddHours(-2) });

        // Customer 3
        var b3 = new WalletBalance { CustomerId = 3, Currency = "USD", AvailableBalance = 890.50m, PendingBalance = 0, UpdatedAt = now };
        db.Balances.Add(b3);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b3.Id, Type = "Credit", Amount = 500, Description = "Cash load", Status = "Completed", CreatedAt = now.AddDays(-10) },
            new WalletTransaction { WalletBalanceId = b3.Id, Type = "Credit", Amount = 400.50m, Description = "Refund", Status = "Completed", CreatedAt = now.AddDays(-3) });

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
app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "Buffalo Bills API v1"); });
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
