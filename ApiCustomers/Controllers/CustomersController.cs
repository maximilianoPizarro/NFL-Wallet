using ApiCustomers.Data;
using ApiCustomers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCustomers.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomersDbContext _db;

    public CustomersController(CustomersDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll(CancellationToken ct)
    {
        return await _db.Customers.OrderBy(c => c.LastName).ToListAsync(ct);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id, CancellationToken ct)
    {
        var customer = await _db.Customers.FindAsync([id], ct);
        if (customer == null) return NotFound();
        return customer;
    }
}
