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
}
