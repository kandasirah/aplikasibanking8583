using Microsoft.EntityFrameworkCore;
using AplikasiBanking8583.Models;

namespace AplikasiBanking8583.Models
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("accounts");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(a => a.AccountHolderName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(a => a.AccountType).IsRequired().HasMaxLength(50);
                entity.Property(a => a.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(a => a.AccountNumber).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.AccountId).IsRequired();
                entity.Property(t => t.TransactionType).IsRequired().HasMaxLength(20);
                entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Description).HasMaxLength(200);
                entity.Property(t => t.Reference).HasMaxLength(50);
                entity.Property(t => t.TransactionDate).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(t => t.Stan).HasMaxLength(6);
                entity.Property(t => t.Rrn).HasMaxLength(12);
                entity.Property(t => t.ResponseCode).HasMaxLength(4).HasDefaultValue("00");
                entity.Property(t => t.MTI).HasMaxLength(20);
                entity.Property(t => t.Iso8583MessageRaw).HasMaxLength(2000);
                entity.Property(t => t.TransactionCategory).HasMaxLength(20).HasDefaultValue("FINANCIAL");
                entity.Property(t => t.OperationType).HasMaxLength(50);
                entity.Property(t => t.ResponseMessage).HasMaxLength(200);
                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
