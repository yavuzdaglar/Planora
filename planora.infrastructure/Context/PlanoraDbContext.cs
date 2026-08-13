using Microsoft.EntityFrameworkCore;
using Planora.Entities;

namespace Planora.Infrastructure.Context;

public class PlanoraDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // SQL Lokasyonu / Bağlantı Metni
        optionsBuilder.UseSqlServer("Server=(localdb)\\planora;Database=Planora;Trusted_Connection=True;");
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Block> Blocks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<Block>().HasKey(b => b.Id);

        // Blok ilişkileri
        modelBuilder.Entity<Block>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId);

        // Tarih filtreleme sorgusu için index
        modelBuilder.Entity<Block>()
            .HasIndex(b => new { b.UserId, b.Date });
    }
}