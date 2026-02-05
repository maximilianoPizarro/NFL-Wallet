namespace WebApp.Models;

public class CustomerWalletsViewModel
{
    public CustomerViewModel Customer { get; set; } = null!;
    public WalletSummaryViewModel BuffaloBills { get; set; } = null!;
    public WalletSummaryViewModel LasVegasRaiders { get; set; } = null!;
}

public class WalletSummaryViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public WalletBalanceViewModel? Balance { get; set; }
    public List<WalletTransactionViewModel> Transactions { get; set; } = new();
}
