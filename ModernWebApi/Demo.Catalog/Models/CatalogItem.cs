namespace Catalog.Models;

public class CatalogItem
{
    // Unique identifier for the catalog item
    public int Id { get; set; }

    // Name of the product
    public string? Name { get; set; }

    // Detailed description of the product
    public string? Description { get; set; }

    // Current price of the product
    public decimal Price { get; set; }

    // Filename of the product image
    public string? PictureFileName { get; set; }

    // Category or type of the product
    public string? CatalogType { get; set; }

    // Brand of the product
    public string? CatalogBrand { get; set; }

    // Current quantity available in stock
    public int AvailableStock { get; set; }

    // Available stock at which we should reorder
    public int RestockThreshold { get; set; }

    // Maximum number of units that can be in-stock at any time (due to physical/logistical constraints in warehouses)
    public int MaxStockThreshold { get; set; }

    // Indicates whether the item is currently in the process of being restocked
    public bool OnReorder { get; set; }

    public CatalogItem() { }
}