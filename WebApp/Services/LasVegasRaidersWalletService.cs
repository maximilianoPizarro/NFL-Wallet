namespace WebApp.Services;

public class LasVegasRaidersWalletService : WalletApiService
{
    public const string DisplayName = "Las Vegas Raiders";

    public LasVegasRaidersWalletService(HttpClient http) : base(http, DisplayName) { }
}
