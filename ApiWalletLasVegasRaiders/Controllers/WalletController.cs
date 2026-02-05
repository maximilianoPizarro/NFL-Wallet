using ApiWalletLasVegasRaiders.Data;
using ApiWalletLasVegasRaiders.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiWalletLasVegasRaiders.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    public const string WalletDisplayName = "Las Vegas Raiders";

    private readonly WalletDbContext _db;

    public WalletController(WalletDbContext db)
    {
        _db = db;
    }

    [HttpGet("balance/{customerId:int}")]
    public async Task<ActionResult<WalletBalance>> GetBalance(int customerId, CancellationToken ct)
    {
        var balance = await _db.Balances
            .FirstOrDefaultAsync(b => b.CustomerId == customerId, ct);
        if (balance == null) return NotFound();
        return balance;
    }

    [HttpGet("transactions/{customerId:int}")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetTransactions(int customerId, CancellationToken ct)
    {
        var balance = await _db.Balances.FirstOrDefaultAsync(b => b.CustomerId == customerId, ct);
        if (balance == null) return NotFound();
        var list = await _db.Transactions
            .Where(t => t.WalletBalanceId == balance.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
        return list;
    }

    [HttpPost("load/{customerId:int}")]
    public async Task<ActionResult<WalletBalance>> Load(int customerId, [FromBody] LoadPayRequest request, CancellationToken ct)
    {
        if (request?.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        var balance = await _db.Balances.FirstOrDefaultAsync(b => b.CustomerId == customerId, ct);
        if (balance == null) return NotFound();
        balance.AvailableBalance += request.Amount;
        balance.UpdatedAt = DateTime.UtcNow;
        var tx = new WalletTransaction
        {
            WalletBalanceId = balance.Id,
            Type = "Credit",
            Amount = request.Amount,
            Description = "Credit card top-up (mock)",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        return balance;
    }

    [HttpPost("pay/{customerId:int}")]
    public async Task<ActionResult<WalletBalance>> Pay(int customerId, [FromBody] LoadPayRequest request, CancellationToken ct)
    {
        if (request?.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        var balance = await _db.Balances.FirstOrDefaultAsync(b => b.CustomerId == customerId, ct);
        if (balance == null) return NotFound();
        if (balance.AvailableBalance < request.Amount) return BadRequest("Insufficient balance.");
        balance.AvailableBalance -= request.Amount;
        balance.UpdatedAt = DateTime.UtcNow;
        var tx = new WalletTransaction
        {
            WalletBalanceId = balance.Id,
            Type = "Debit",
            Amount = request.Amount,
            Description = "Payment (QR)",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync(ct);
        return balance;
    }
}

public class LoadPayRequest
{
    public decimal Amount { get; set; }
}
