using ApiWalletBuffaloBills.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiWalletBuffaloBills.Data;

public class WalletDbContext : DbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<WalletBalance> Balances => Set<WalletBalance>();
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WalletBalance>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Currency).HasMaxLength(3);
        });
        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
        });
    }
}
