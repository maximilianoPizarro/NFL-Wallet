using ApiWalletLasVegasRaiders.Data;
using ApiWalletLasVegasRaiders.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=lasvegasraiders.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        // Customer 1 - Las Vegas Raiders wallet
        var b1 = new WalletBalance { CustomerId = 1, Currency = "USD", AvailableBalance = 3100.00m, PendingBalance = 100.00m, UpdatedAt = now };
        db.Balances.Add(b1);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Credit", Amount = 3200, Description = "Payroll deposit", Status = "Completed", CreatedAt = now.AddDays(-7) },
            new WalletTransaction { WalletBalanceId = b1.Id, Type = "Debit", Amount = 100, Description = "Bill payment", Status = "Pending", CreatedAt = now.AddHours(-1) });

        // Customer 2
        var b2 = new WalletBalance { CustomerId = 2, Currency = "USD", AvailableBalance = 12500.75m, PendingBalance = 0, UpdatedAt = now };
        db.Balances.Add(b2);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b2.Id, Type = "Credit", Amount = 8000, Description = "Bonus deposit", Status = "Completed", CreatedAt = now.AddDays(-20) },
            new WalletTransaction { WalletBalanceId = b2.Id, Type = "Credit", Amount = 5000, Description = "Investment return", Status = "Completed", CreatedAt = now.AddDays(-5) },
            new WalletTransaction { WalletBalanceId = b2.Id, Type = "Debit", Amount = 499.25m, Description = "Subscription", Status = "Completed", CreatedAt = now.AddDays(-1) });

        // Customer 3
        var b3 = new WalletBalance { CustomerId = 3, Currency = "USD", AvailableBalance = 450.00m, PendingBalance = 50.00m, UpdatedAt = now };
        db.Balances.Add(b3);
        db.SaveChanges();
        db.Transactions.AddRange(
            new WalletTransaction { WalletBalanceId = b3.Id, Type = "Credit", Amount = 500, Description = "Transfer in", Status = "Completed", CreatedAt = now.AddDays(-2) },
            new WalletTransaction { WalletBalanceId = b3.Id, Type = "Debit", Amount = 50, Description = "P2P transfer", Status = "Pending", CreatedAt = now });

        db.SaveChanges();
    }
}

app.UsePathBase("/api");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
