using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<Warehouse> Warehouses { get; set; }
    public required DbSet<WarehouseZone> WarehouseZones { get; set; }
    public required DbSet<ZoneSlot> ZoneSlots { get; set; }
    public required DbSet<Pallet> Pallets { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Warehouse>().HasIndex(w => w.Code).IsUnique();
        builder.Entity<Warehouse>().Property(w => w.Code).HasMaxLength(20).IsRequired();
        builder.Entity<Warehouse>().Property(w => w.Name).HasMaxLength(100).IsRequired();
        builder.Entity<Warehouse>().Property(w => w.Address).HasMaxLength(200);

        builder.Entity<WarehouseZone>().HasIndex(z => new { z.WarehouseId, z.Code }).IsUnique();
        builder.Entity<WarehouseZone>().Property(z => z.Code).HasMaxLength(20).IsRequired();
        builder.Entity<WarehouseZone>().Property(z => z.Name).HasMaxLength(100).IsRequired();
        builder.Entity<WarehouseZone>().Property(z => z.Description).HasMaxLength(200);
        builder.Entity<WarehouseZone>().Property(z => z.ZoneType).HasConversion<string>();

        builder.Entity<ZoneSlot>().HasIndex(s => new { s.ZoneId, s.Code }).IsUnique();
        builder.Entity<ZoneSlot>().Property(s => s.Code).IsRequired();

        builder.Entity<Pallet>().HasIndex(p => p.Code).IsUnique();
        builder.Entity<Pallet>().Property(p => p.Code).HasMaxLength(30).IsRequired();
        builder.Entity<Pallet>().Property(p => p.Description).HasMaxLength(200);

        builder.Entity<Pallet>()
            .HasOne(p => p.ZoneSlot)
            .WithOne(s => s.Pallet)
            .HasForeignKey<Pallet>(p => p.ZoneSlotId)
            .OnDelete(DeleteBehavior.SetNull);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        foreach (var entityType in builder.Model.GetEntityTypes())
        foreach (var property in entityType.GetProperties())
            if (property.ClrType == typeof(DateTime))
                property.SetValueConverter(dateTimeConverter);
    }
}