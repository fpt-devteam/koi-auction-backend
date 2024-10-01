using System;
using System.Collections.Generic;
using AuctionManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionManagementService.Data;

public partial class AuctionManagementDbContext : DbContext
{
    public AuctionManagementDbContext()
    {
    }

    public AuctionManagementDbContext(DbContextOptions<AuctionManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auction> Auctions { get; set; }

    public virtual DbSet<AuctionLot> AuctionLots { get; set; }

    public virtual DbSet<AuctionMethod> AuctionMethods { get; set; }

    public virtual DbSet<KoiFish> KoiFishes { get; set; }

    public virtual DbSet<Lot> Lots { get; set; }

    public virtual DbSet<LotStatus> LotStatuses { get; set; }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];

        return strConn;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.AuctionId).HasName("PK__Auction__51004A4C59DF4F8C");

            entity.ToTable("Auction", tb => tb.HasTrigger("trg_Auction_Update"));

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<AuctionLot>(entity =>
        {
            entity.HasKey(e => e.AuctionLotId).HasName("PK__AuctionL__8A0269CF99A7491A");

            entity.ToTable("AuctionLot", tb => tb.HasTrigger("trg_AuctionLot_Update"));

            entity.Property(e => e.AuctionLotId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionLots)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuctionLot_Auction");

            entity.HasOne(d => d.AuctionLotNavigation).WithOne(p => p.AuctionLot)
                .HasForeignKey<AuctionLot>(d => d.AuctionLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuctionLot_Lot");
        });

        modelBuilder.Entity<AuctionMethod>(entity =>
        {
            entity.HasKey(e => e.AuctionMethodId).HasName("PK__AuctionM__FCD1A18B9425F901");

            entity.ToTable("AuctionMethod", tb => tb.HasTrigger("trg_Update_AuctionMethod"));

            entity.Property(e => e.AuctionMethodName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KoiFish>(entity =>
        {
            entity.HasKey(e => e.KoiFishId).HasName("PK__KoiFish__44D35C258EF63F2F");

            entity.ToTable("KoiFish");

            entity.Property(e => e.KoiFishId).ValueGeneratedNever();
            entity.Property(e => e.SizeCm)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SizeCM");
            entity.Property(e => e.Variety).HasMaxLength(500);
            entity.Property(e => e.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("WeightKG");

            entity.HasOne(d => d.KoiFishNavigation).WithOne(p => p.KoiFish)
                .HasForeignKey<KoiFish>(d => d.KoiFishId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KoiFish_Lot");
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasKey(e => e.LotId).HasName("PK__Lot__4160EFAD09632CC5");

            entity.ToTable("Lot");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StartingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AuctionMethod).WithMany(p => p.Lots)
                .HasForeignKey(d => d.AuctionMethodId)
                .HasConstraintName("FK_Lot_AuctionMethod");

            entity.HasOne(d => d.LotStatus).WithMany(p => p.Lots)
                .HasForeignKey(d => d.LotStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lot_LotStatus");
        });

        modelBuilder.Entity<LotStatus>(entity =>
        {
            entity.HasKey(e => e.LotStatusId).HasName("PK__LotStatu__B5B838777D8DB099");

            entity.ToTable("LotStatus", tb => tb.HasTrigger("trg_Update_LotStatus"));

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LotStatusName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
