using System.Net.Http.Json;
using WebApp.Models;

namespace WebApp.Services;

public class CustomersApiService
{
    private readonly HttpClient _http;

    public CustomersApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<CustomerViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _http.GetFromJsonAsync<CustomerViewModel[]>("Customers", ct);
        return list ?? Array.Empty<CustomerViewModel>();
    }

    public async Task<CustomerViewModel?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<CustomerViewModel>($"Customers/{id}", ct);
    }
}
