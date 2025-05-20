using Microsoft.EntityFrameworkCore;
using Catalog.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Catalog.Data;

class CatalogItemEntityTypeConfiguration
    : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        // builder.ToTable("Catalog");

        builder.Property(ci => ci.Name)
            .HasMaxLength(50);

        builder.Property(ci => ci.CatalogBrand)
            .HasMaxLength(100);

        builder.Property(ci => ci.CatalogType)
            .HasMaxLength(100);

        builder.HasIndex(ci => ci.Name);
    }
}