using System.Net.Http.Json;
using WebApp.Models;

namespace WebApp.Services;

public class WalletApiService
{
    private readonly HttpClient _http;
    public string WalletDisplayName { get; }

    public WalletApiService(HttpClient http, string walletDisplayName)
    {
        _http = http;
        WalletDisplayName = walletDisplayName;
    }

    public async Task<WalletBalanceViewModel?> GetBalanceAsync(int customerId, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<WalletBalanceViewModel>($"Wallet/balance/{customerId}", ct);
    }

    public async Task<IEnumerable<WalletTransactionViewModel>> GetTransactionsAsync(int customerId, CancellationToken ct = default)
    {
        var list = await _http.GetFromJsonAsync<WalletTransactionViewModel[]>($"Wallet/transactions/{customerId}", ct);
        return list ?? Array.Empty<WalletTransactionViewModel>();
    }
}
