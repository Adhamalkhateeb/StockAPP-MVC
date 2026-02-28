using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Data.Config;

public class BuyOrderConfiguration : IEntityTypeConfiguration<BuyOrder>
{
    public void Configure(EntityTypeBuilder<BuyOrder> builder)
    {

        builder.HasKey(bo => bo.Id);
        builder.Property(bo => bo.Id).ValueGeneratedNever().HasColumnType("uniqueidentifier");

        builder.Property(bo => bo.StockSymbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bo => bo.StockName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bo => bo.DateAndTimeOfOrder)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(bo => bo.Quantity)
            .IsRequired();

        builder.Property(bo => bo.Price)
            .HasPrecision(15, 2)
            .IsRequired();

        builder.ToTable("BuyOrders", table =>
        {
            table.HasCheckConstraint(
                "CHK_BuyOrders_Quantity",
                "[Quantity] > 0 AND [Quantity] <= 100000"
                );

            table.HasCheckConstraint(
                "CHK_BuyOrders_Price",
                "[Price] > 0 AND [Price] <= 100000"
                );

            table.HasCheckConstraint(
                "CHK__BuyOrders_OrderDate_Min",
                "[DateAndTimeOfOrder] >= '2000-01-01'"
            );
        });


    }
}
