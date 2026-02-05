using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly CustomersApiService _customersApi;
    private readonly BuffaloBillsWalletService _buffaloBillsWallet;
    private readonly LasVegasRaidersWalletService _lasVegasRaidersWallet;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        CustomersApiService customersApi,
        BuffaloBillsWalletService buffaloBillsWallet,
        LasVegasRaidersWalletService lasVegasRaidersWallet,
        ILogger<HomeController> logger)
    {
        _customersApi = customersApi;
        _buffaloBillsWallet = buffaloBillsWallet;
        _lasVegasRaidersWallet = lasVegasRaidersWallet;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var customers = await _customersApi.GetAllAsync(ct);
        return View(customers.ToList());
    }

    public async Task<IActionResult> CustomerWallets(int id, CancellationToken ct)
    {
        var customer = await _customersApi.GetByIdAsync(id, ct);
        if (customer == null) return NotFound();

        ViewData["Customer"] = customer;

        var buffaloBillsBalance = await _buffaloBillsWallet.GetBalanceAsync(id, ct);
        var buffaloBillsTransactions = buffaloBillsBalance != null ? await _buffaloBillsWallet.GetTransactionsAsync(id, ct) : Enumerable.Empty<WalletTransactionViewModel>();

        var lasVegasRaidersBalance = await _lasVegasRaidersWallet.GetBalanceAsync(id, ct);
        var lasVegasRaidersTransactions = lasVegasRaidersBalance != null ? await _lasVegasRaidersWallet.GetTransactionsAsync(id, ct) : Enumerable.Empty<WalletTransactionViewModel>();

        var model = new CustomerWalletsViewModel
        {
            Customer = customer,
            BuffaloBills = new WalletSummaryViewModel
            {
                DisplayName = BuffaloBillsWalletService.DisplayName,
                Balance = buffaloBillsBalance,
                Transactions = buffaloBillsTransactions.ToList()
            },
            LasVegasRaiders = new WalletSummaryViewModel
            {
                DisplayName = LasVegasRaidersWalletService.DisplayName,
                Balance = lasVegasRaidersBalance,
                Transactions = lasVegasRaidersTransactions.ToList()
            }
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
