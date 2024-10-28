using System;
using System.Collections.Generic;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

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

    public virtual DbSet<AuctionLotJob> AuctionLotJobs { get; set; }

    public virtual DbSet<AuctionLotStatus> AuctionLotStatuses { get; set; }

    public virtual DbSet<AuctionMethod> AuctionMethods { get; set; }

    public virtual DbSet<AuctionStatus> AuctionStatuses { get; set; }

    public virtual DbSet<KoiFish> KoiFishes { get; set; }

    public virtual DbSet<KoiMedia> KoiMedia { get; set; }

    public virtual DbSet<Lot> Lots { get; set; }

    public virtual DbSet<LotStatus> LotStatuses { get; set; }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //         => optionsBuilder.UseSqlServer("Server=tcp:fpt-koi.database.windows.net,1433;Initial Catalog=KoiAuctionDB;Persist Security Info=False;User ID=fptdevteam;Password=12345Fpt;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.AuctionId).HasName("PK__Auction__51004A4CAA2054EE");

            entity.ToTable("Auction");

            entity.HasIndex(e => e.AuctionName, "UQ_Auction_AuctionName").IsUnique();

            entity.Property(e => e.AuctionName).HasMaxLength(100);
            entity.Property(e => e.AuctionStatusId).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AuctionStatus).WithMany(p => p.Auctions)
                .HasForeignKey(d => d.AuctionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_AuctionStatus");
        });

        modelBuilder.Entity<AuctionLot>(entity =>
        {
            entity.HasKey(e => e.AuctionLotId).HasName("PK__AuctionL__8A0269CF4469A727");

            entity.ToTable("AuctionLot");

            entity.Property(e => e.AuctionLotId).ValueGeneratedNever();
            entity.Property(e => e.AuctionLotStatusId).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StepPercent).HasDefaultValue(0);
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

            entity.HasOne(d => d.AuctionLotStatus).WithMany(p => p.AuctionLots)
                .HasForeignKey(d => d.AuctionLotStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuctionLot_AuctionLotStatus");
        });

        modelBuilder.Entity<AuctionLotJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuctionL__3214EC076F2A7208");

            entity.ToTable("AuctionLotJob");

            entity.Property(e => e.HangfireJobId).HasMaxLength(100);
        });

        modelBuilder.Entity<AuctionLotStatus>(entity =>
        {
            entity.HasKey(e => e.AuctionLotStatusId).HasName("PK__AuctionL__07A5E3C39FE294C8");

            entity.ToTable("AuctionLotStatus");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<AuctionMethod>(entity =>
        {
            entity.HasKey(e => e.AuctionMethodId).HasName("PK__AuctionM__FCD1A18B371CBB3F");

            entity.ToTable("AuctionMethod");

            entity.Property(e => e.AuctionMethodName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<AuctionStatus>(entity =>
        {
            entity.HasKey(e => e.AuctionStatusId).HasName("PK__AuctionS__B2535E95B679012E");

            entity.ToTable("AuctionStatus");

            entity.Property(e => e.AuctionStatusName).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KoiFish>(entity =>
        {
            entity.HasKey(e => e.KoiFishId).HasName("PK__KoiFish__44D35C25776325B3");

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

        modelBuilder.Entity<KoiMedia>(entity =>
        {
            entity.HasKey(e => e.KoiMediaId).HasName("PK__KoiMedia__4CC7808360C9C026");

            entity.HasIndex(e => e.FilePath, "UQ_KoiMedia_FilePath").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(500);

            entity.HasOne(d => d.KoiFish).WithMany(p => p.KoiMedia)
                .HasForeignKey(d => d.KoiFishId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KoiMedia_KoiFish");
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasKey(e => e.LotId).HasName("PK__Lot__4160EFAD2877410E");

            entity.ToTable("Lot");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LotStatusId).HasDefaultValue(1);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasDefaultValue("TEMP-SKU")
                .HasColumnName("SKU");
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
            entity.HasKey(e => e.LotStatusId).HasName("PK__LotStatu__B5B83877A3248B3F");

            entity.ToTable("LotStatus");

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
