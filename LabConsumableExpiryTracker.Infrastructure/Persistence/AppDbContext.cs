using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Consumption> Consumptions => Set<Consumption>();
    public DbSet<Disposal> Disposals => Set<Disposal>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureItem(builder);
        ConfigureLot(builder);
        ConfigureJob(builder);
        ConfigureConsumption(builder);
        ConfigureDisposal(builder);
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RefreshLotRowVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        RefreshLotRowVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RefreshLotRowVersions()
    {
        var changedLots = ChangeTracker.Entries<Lot>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in changedLots)
        {
            // PostgreSQL tidak menyediakan SQL Server rowversion/byte[] secara native.
            // Token baru dibuat aplikasi pada setiap INSERT atau UPDATE Lot.
            entry.Property(lot => lot.RowVersion).CurrentValue = Guid.NewGuid().ToByteArray();
        }
    }

    private static void ConfigureItem(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Item>();

        builder.ToTable("items", table =>
        {
            table.HasCheckConstraint("ck_items_minimum_stock", "\"minimum_stock\" >= 0");
            table.HasCheckConstraint("ck_items_expiring_soon_days", "\"expiring_soon_days\" >= 0");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(item => item.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(item => item.BaseUnit)
            .HasColumnName("base_unit")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(item => item.MinimumStock)
            .HasColumnName("minimum_stock")
            .HasPrecision(18, 3)
            .IsRequired();
        builder.Property(item => item.ExpiringSoonDays)
            .HasColumnName("expiring_soon_days")
            .IsRequired();

        builder.HasIndex(item => item.Code).IsUnique().HasDatabaseName("ux_items_code");

        builder.HasMany(item => item.Lots)
            .WithOne()
            .HasForeignKey(lot => lot.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(item => item.Lots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureLot(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Lot>();

        builder.ToTable("lots", table =>
        {
            table.HasCheckConstraint("ck_lots_initial_quantity", "\"initial_quantity\" >= 0");
            table.HasCheckConstraint("ck_lots_remaining_quantity", "\"remaining_quantity\" >= 0");
            table.HasCheckConstraint(
                "ck_lots_remaining_not_above_initial",
                "\"remaining_quantity\" <= \"initial_quantity\"");
        });

        builder.HasKey(lot => lot.Id);
        builder.Property(lot => lot.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(lot => lot.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(lot => lot.LotNumber).HasColumnName("lot_number").HasMaxLength(100).IsRequired();
        builder.Property(lot => lot.SupplierLotNumber).HasColumnName("supplier_lot_number").HasMaxLength(100);
        builder.Property(lot => lot.ReceivedAt).HasColumnName("received_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(lot => lot.SupplierName).HasColumnName("supplier_name").HasMaxLength(200);
        builder.Property(lot => lot.InitialQuantity).HasColumnName("initial_quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(lot => lot.RemainingQuantity).HasColumnName("remaining_quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(lot => lot.ExpiryDate).HasColumnName("expiry_date").HasColumnType("date").IsRequired();
        builder.Property(lot => lot.StorageLocation).HasColumnName("storage_location").HasMaxLength(200).IsRequired();
        builder.Property(lot => lot.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(lot => lot.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("bytea")
            .IsConcurrencyToken()
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(lot => new { lot.ItemId, lot.LotNumber })
            .IsUnique()
            .HasDatabaseName("ux_lots_item_id_lot_number");
        builder.HasIndex(lot => new { lot.ItemId, lot.Status, lot.ExpiryDate, lot.ReceivedAt })
            .HasDatabaseName("ix_lots_fefo");

        // SubLotId pada model lama diperlakukan sebagai FK menuju Lot.Id.
        builder.HasMany<Consumption>()
            .WithOne()
            .HasForeignKey(consumption => consumption.SubLotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<Disposal>()
            .WithOne()
            .HasForeignKey(disposal => disposal.SubLotId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Job>();

        builder.ToTable("jobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(job => job.JobNumber).HasColumnName("job_number").HasMaxLength(100).IsRequired();
        builder.Property(job => job.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(job => job.StartedAt).HasColumnName("started_at").HasColumnType("timestamp with time zone");
        builder.Property(job => job.CompletedAt).HasColumnName("completed_at").HasColumnType("timestamp with time zone");

        builder.HasIndex(job => job.JobNumber).IsUnique().HasDatabaseName("ux_jobs_job_number");

        builder.HasMany(job => job.Consumptions)
            .WithOne()
            .HasForeignKey(consumption => consumption.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(job => job.Consumptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureConsumption(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Consumption>();

        builder.ToTable("consumptions", table =>
            table.HasCheckConstraint("ck_consumptions_quantity", "\"quantity\" > 0"));

        builder.HasKey(consumption => consumption.Id);
        builder.Property(consumption => consumption.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(consumption => consumption.JobId).HasColumnName("job_id").IsRequired();
        builder.Property(consumption => consumption.SubLotId).HasColumnName("lot_id").IsRequired();
        builder.Property(consumption => consumption.Quantity).HasColumnName("quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(consumption => consumption.ConsumedAt).HasColumnName("consumed_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(consumption => consumption.ConsumedBy).HasColumnName("consumed_by").IsRequired();

        builder.HasIndex(consumption => consumption.JobId).HasDatabaseName("ix_consumptions_job_id");
        builder.HasIndex(consumption => consumption.SubLotId).HasDatabaseName("ix_consumptions_lot_id");
    }

    private static void ConfigureDisposal(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Disposal>();

        builder.ToTable("disposals", table =>
            table.HasCheckConstraint("ck_disposals_quantity", "\"quantity\" > 0"));

        builder.HasKey(disposal => disposal.Id);
        builder.Property(disposal => disposal.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(disposal => disposal.SubLotId).HasColumnName("lot_id").IsRequired();
        builder.Property(disposal => disposal.Quantity).HasColumnName("quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(disposal => disposal.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
        builder.Property(disposal => disposal.DisposedAt).HasColumnName("disposed_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(disposal => disposal.DisposedBy).HasColumnName("disposed_by").IsRequired();

        builder.HasIndex(disposal => disposal.SubLotId).HasDatabaseName("ix_disposals_lot_id");
    }
}