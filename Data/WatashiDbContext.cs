using Microsoft.EntityFrameworkCore;
using Watashi.Models;
using Watashi.Models.ClientInfo;
using Watashi.Models.UserInfo;

namespace Watashi.Data;

public class WatashiDbContext(DbContextOptions<WatashiDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<RedirectUri> RedirectUris { get; set; }
    public DbSet<GrantType> GrantTypes { get; set; }
    public DbSet<ResponseType> ResponseTypes { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<RecoveryCode> RecoveryCodes { get; set; }

    public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<RedirectUri>()
            .HasOne(r => r.Client)
            .WithMany(c => c.RedirectUris)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<GrantType>()
            .HasOne(g => g.Client)
            .WithMany(c => c.GrantTypes)
            .HasForeignKey(g => g.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ResponseType>()
            .HasOne(r => r.Client)
            .WithMany(c => c.ResponseTypes)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>().HasIndex(c => c.ClientId).IsUnique();

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder
            .Entity<RecoveryCode>()
            .HasOne(rc => rc.User)
            .WithMany(u => u.RecoveryCodes)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
