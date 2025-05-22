using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Catalog.Models;

namespace Catalog.Data;

public static class CatalogSeeder
{
    public static async Task Seed(WebApplication app)
    {
        // Create and seed the database
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            try
            {
                var context = services.GetRequiredService<CatalogContext>();
                context.Database.EnsureCreated();

                await SeedAsync(context, app.Environment);
                logger.LogInformation("Catalog seeding complete.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }

    }

    public static async Task SeedAsync(CatalogContext context, IWebHostEnvironment env)
    {
        var contentRootPath = env.ContentRootPath;
        var picturePath = env.WebRootPath;

        if (!context.CatalogItems.Any())
        {
            var sourcePath = Path.Combine(contentRootPath, "Data", "catalog.json");
            var sourceJson = File.ReadAllText(sourcePath);
            var sourceItems = JsonSerializer.Deserialize<CatalogSourceEntry[]>(sourceJson) ?? [];

            var catalogItems = sourceItems.Select(source => new CatalogItem
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                Price = source.Price,
                Sku = source.Sku,
                CatalogBrand = source.Brand,
                CatalogType = source.Type,
                AvailableStock = 100,
                MaxStockThreshold = 200,
                RestockThreshold = 10,
                RestockAmount = 50,
                PictureFileName = $"{source.Id}.webp",
            }).ToArray();

            await context.CatalogItems.AddRangeAsync(catalogItems);
            await context.SaveChangesAsync();
        }
    }

    private class CatalogSourceEntry
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Brand { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Sku { get; set; }
    }
}
