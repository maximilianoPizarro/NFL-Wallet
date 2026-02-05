using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

var customersBaseUrl = builder.Configuration["Api:CustomersBaseUrl"] ?? "http://localhost:5001/api";
var buffaloBillsBaseUrl = builder.Configuration["Api:BuffaloBillsBaseUrl"] ?? "http://localhost:5002/api";
var lasVegasRaidersBaseUrl = builder.Configuration["Api:LasVegasRaidersBaseUrl"] ?? "http://localhost:5003/api";

builder.Services.AddHttpClient<CustomersApiService>(c => c.BaseAddress = new Uri(customersBaseUrl.TrimEnd('/') + "/"));
builder.Services.AddHttpClient<BuffaloBillsWalletService>(c => c.BaseAddress = new Uri(buffaloBillsBaseUrl.TrimEnd('/') + "/"));
builder.Services.AddHttpClient<LasVegasRaidersWalletService>(c => c.BaseAddress = new Uri(lasVegasRaidersBaseUrl.TrimEnd('/') + "/"));

builder.Services.AddControllersWithViews();

// Keycloak/OpenID: uncomment and configure when using authentication
// builder.Services.AddAuthentication(options => { ... })
//     .AddOpenIdConnect("Keycloak", options => { ... });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
