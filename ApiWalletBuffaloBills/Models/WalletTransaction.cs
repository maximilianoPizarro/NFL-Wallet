namespace ApiWalletBuffaloBills.Models;

public class WalletTransaction
{
    public int Id { get; set; }
    public int WalletBalanceId { get; set; }
    public string Type { get; set; } = string.Empty; // Credit, Debit, Transfer
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Completed, Pending, Failed
    public DateTime CreatedAt { get; set; }
}
