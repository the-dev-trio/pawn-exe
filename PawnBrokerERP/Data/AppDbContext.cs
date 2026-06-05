using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PawnBrokerERP.Helpers;
using PawnBrokerERP.Models;
using System.IO;
using System.Reflection;

namespace PawnBrokerERP.Data;

public class AppDbContext : DbContext
{
    private const string DbPassword = "PawnERP@SecureKey#2024";
    private const string DevPhone = "7845550512";
    private const string DevPassword = "7845550512";

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Pledge> Pledges => Set<Pledge>();
    public DbSet<AppLicense> AppLicense => Set<AppLicense>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                     ?? AppDomain.CurrentDomain.BaseDirectory;
        var dbPath = Path.Combine(exeDir, "PawnERP.db");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Password = DbPassword
        }.ToString();

        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppLicense>().HasKey(a => a.Id);
        modelBuilder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();
        modelBuilder.Entity<Pledge>().HasIndex(p => p.TicketNumber).IsUnique();
        modelBuilder.Entity<Pledge>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Pledges)
            .HasForeignKey(p => p.CustomerId);

        modelBuilder.Entity<Pledge>()
            .Property(p => p.Status)
            .HasConversion<string>();
    }

    public void InitializeDatabase()
    {
        Database.EnsureCreated();

        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");

        SeedDevUser();
    }

    private void SeedDevUser()
    {
        if (!Users.Any(u => u.PhoneNumber == DevPhone))
        {
            Users.Add(new User
            {
                PhoneNumber = DevPhone,
                PasswordHash = PasswordHelper.Hash(DevPassword)
            });
            SaveChanges();
        }
    }

    public string GenerateTicketNumber()
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var count = Pledges.Count(p => p.PledgeDate.Year == year && p.PledgeDate.Month == month) + 1;
        return $"PB-{year}{month:D2}-{count:D4}";
    }
}
