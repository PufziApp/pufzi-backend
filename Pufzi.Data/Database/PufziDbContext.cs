using Microsoft.EntityFrameworkCore;
using Pufzi.Data.Database.Entities;

namespace Pufzi.Data.Database;

public class PufziDbContext : DbContext
{
    public PufziDbContext(DbContextOptions<PufziDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<BusinessMembership> BusinessMemberships =>
        Set<BusinessMembership>();

    public DbSet<BusinessInvitation> BusinessInvitations =>
        Set<BusinessInvitation>();

    public DbSet<BusinessImage> BusinessImages =>
        Set<BusinessImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PufziDbContext).Assembly);
    }
}