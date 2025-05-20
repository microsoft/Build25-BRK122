using Microsoft.EntityFrameworkCore;
using Catalog.Models;
using System.Diagnostics.CodeAnalysis;

namespace Catalog.Data;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options, IConfiguration configuration) : base(options)
    {
    }

    public DbSet<CatalogItem> CatalogItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Add index on Name property
        builder.Entity<CatalogItem>()
            .HasIndex(ci => ci.Name);
    }
}