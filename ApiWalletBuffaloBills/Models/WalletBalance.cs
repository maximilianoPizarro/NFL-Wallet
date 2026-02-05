namespace ApiWalletBuffaloBills.Models;

public class WalletBalance
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public DateTime UpdatedAt { get; set; }
}
