namespace WebApp.Services;

public class BuffaloBillsWalletService : WalletApiService
{
    public const string DisplayName = "Buffalo Bills";

    public BuffaloBillsWalletService(HttpClient http) : base(http, DisplayName) { }
}
