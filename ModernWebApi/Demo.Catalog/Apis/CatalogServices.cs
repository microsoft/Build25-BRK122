using Microsoft.AspNetCore.Mvc;

using Catalog.Data;
using Catalog.API;

public class CatalogServices(
    // CatalogContext context,
    ILogger<CatalogServices> logger
)
{
    // [FromServices]
    // public CatalogContext Context { get; } = context;
    [FromServices]
    public ILogger<CatalogServices> Logger { get; } = logger;
};