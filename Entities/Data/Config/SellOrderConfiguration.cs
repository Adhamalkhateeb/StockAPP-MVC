using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Data.Config;

public class SellOrderConfiguration : IEntityTypeConfiguration<SellOrder>
{
    public void Configure(EntityTypeBuilder<SellOrder> builder)
    {

        builder.HasKey(so => so.Id);
        builder.Property(so => so.Id).ValueGeneratedNever().HasColumnType("uniqueidentifier");

        builder.Property(so => so.StockSymbol)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(so => so.StockName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(so => so.DateAndTimeOfOrder)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(so => so.Quantity)
            .IsRequired();

        builder.Property(so => so.Price)
            .HasPrecision(15, 2)
            .IsRequired();

        builder.ToTable("SellOrders", table =>
        {
            table.HasCheckConstraint(
            "CHK_SellOrders_Quantity",
            "[Quantity] > 0 AND [Quantity] <= 100000"
            );

            table.HasCheckConstraint(
                "CHK_SellOrders_Price",
                "[Price] > 0 AND [Price] <= 100000"
                );

            table.HasCheckConstraint(
                "CHK__SellOrders_OrderDate_Min",
                "[DateAndTimeOfOrder] >= '2000-01-01'"
            );
        });


    }
}
