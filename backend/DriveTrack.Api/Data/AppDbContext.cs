using DriveTrack.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace DriveTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FuelType> FuelTypes => Set<FuelType>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleFuelType> VehicleFuelTypes => Set<VehicleFuelType>();
    public DbSet<FuelEntry> FuelEntries => Set<FuelEntry>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VehicleFuelType>()
            .HasIndex(vf => new { vf.VehicleId, vf.FuelTypeId })
            .IsUnique();

        modelBuilder.Entity<FuelEntry>(e =>
        {
            e.HasIndex(x => new { x.VehicleId, x.Date });
            e.HasIndex(x => new { x.VehicleId, x.OdometerKm });
            e.Property(x => x.Volume).HasPrecision(10, 3);
            e.Property(x => x.PricePerUnit).HasPrecision(10, 3);
            e.Property(x => x.TotalCost).HasPrecision(12, 2);
        });

         modelBuilder.Entity<Category>(c =>
        {
            c.HasIndex(x => x.OwnerUserId);
            c.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.HasIndex(x => new { x.VehicleId, x.Date });
            e.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<Reminder>(r =>
        {
            r.HasIndex(x => new { x.VehicleId, x.DueDate });
            r.HasIndex(x => x.IsCompleted);
        });

        // AppUser
        modelBuilder.Entity<AppUser>(u =>
        {
            u.HasIndex(x => x.Email).IsUnique();
        });

        // Vehicle
        modelBuilder.Entity<Vehicle>(v =>
        {
            v.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
        });

        // Category
        modelBuilder.Entity<Category>(c =>
        {
            c.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);
        });

        // UserVehicle
        modelBuilder.Entity<UserVehicle>(uv =>
        {
            uv.HasKey(x => new { x.UserId, x.VehicleId });
            uv.HasOne(x => x.User)
            .WithMany(u => u.Vehicles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            uv.HasOne(x => x.Vehicle)
            .WithMany(v => v.Users)
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

            uv.HasIndex(x => new { x.VehicleId, x.UserId }).IsUnique();
        });

    }
}
