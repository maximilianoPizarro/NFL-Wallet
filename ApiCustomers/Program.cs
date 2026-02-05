using ApiCustomers.Data;
using ApiCustomers.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CustomersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=customers.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
