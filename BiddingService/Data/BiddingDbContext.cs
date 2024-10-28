using System;
using System.Collections.Generic;
using AuctionService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public partial class BiddingDbContext : DbContext
{
    public BiddingDbContext()
    {
    }

    public BiddingDbContext(DbContextOptions<BiddingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BidLog> BidLogs { get; set; }

    public virtual DbSet<SoldLot> SoldLots { get; set; }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];

        return strConn!;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BidLog>(entity =>
        {
            entity.HasKey(e => e.BidLogId).HasName("PK__BidLog__A459EE9EEEAEDB8A");

            entity.ToTable("BidLog");

            entity.Property(e => e.BidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SoldLot>(entity =>
        {
            entity.HasKey(e => e.SoldLotId).HasName("PK__SoldLot__A006956D4D1D1274");

            entity.ToTable("SoldLot");

            entity.Property(e => e.SoldLotId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
