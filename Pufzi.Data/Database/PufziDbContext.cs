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

    public DbSet<BusinessWorkingHour> BusinessWorkingHours =>
        Set<BusinessWorkingHour>();

    public DbSet<EmployeeWorkingHour> EmployeeWorkingHours =>
        Set<EmployeeWorkingHour>();

    public DbSet<BusinessScheduleException> BusinessScheduleExceptions =>
        Set<BusinessScheduleException>();

    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances =>
        Set<EmployeeLeaveBalance>();

    public DbSet<LeaveRequest> LeaveRequests =>
        Set<LeaveRequest>();

    public DbSet<BusinessInvitation> BusinessInvitations =>
        Set<BusinessInvitation>();

    public DbSet<BusinessImage> BusinessImages =>
        Set<BusinessImage>();

    public DbSet<AnimalSpecies> AnimalSpecies => Set<AnimalSpecies>();

    public DbSet<ServicePackage> ServicePackages =>
        Set<ServicePackage>();

    public DbSet<ServicePackageAnimalSpecies> ServicePackageAnimalSpecies =>
        Set<ServicePackageAnimalSpecies>();

    public DbSet<ServicePackageEmployee> ServicePackageEmployees =>
        Set<ServicePackageEmployee>();

    public DbSet<ServicePackageItem> ServicePackageItems =>
        Set<ServicePackageItem>();

    public DbSet<ServicePackageItemOption> ServicePackageItemOptions =>
        Set<ServicePackageItemOption>();

    public DbSet<ServicePackageItemVariant> ServicePackageItemVariants =>
        Set<ServicePackageItemVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PufziDbContext).Assembly);
    }
}