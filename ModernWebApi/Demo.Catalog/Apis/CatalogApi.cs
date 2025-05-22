using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Catalog.Data;
using Catalog.Models;
using System.ComponentModel.DataAnnotations;

namespace Catalog.API;

public static class CatalogApi
{
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder app)
    {
        // RouteGroupBuilder for catalog endpoints
        var api = app.MapGroup("catalog");

        // Routes for querying catalog items.
        api.MapGet("/items", GetAllItems);

        api.MapGet("/items/by", GetItemsByIds);

        api.MapGet("/items/{id:int}", GetItemById);

        api.MapGet("/items/{id:int}/pic", GetItemPictureById);

        // Disable patch endpoint due to known issue with Validation and JsonPatchDocument
        // api.MapPatch("/items/{id:int}", UpdateItem);

        api.MapPost("/items", CreateItem);

        api.MapDelete("/items/{id:int}", DeleteItemById);

        return app;
    }

    /// <summary>
    /// List catalog items.
    /// </summary>
    /// <remarks>
    /// Get a paginated list of items in the catalog.
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="name">The name of the item to return</param>
    /// <param name="type">The type of items to return</param>
    /// <param name="brand">The brand of items to return</param>
    /// <param name="paginationRequest"></param>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<PaginatedItems<CatalogItem>>> GetAllItems(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        string? name,
        string? type,
        string? brand,
        [AsParameters] PaginationRequest paginationRequest
    )
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var pageSize = paginationRequest.PageSize;
        var pageIndex = paginationRequest.PageIndex;

        var root = (IQueryable<CatalogItem>)Context.CatalogItems;

        if (name is not null)
        {
            root = root.Where(c => c.Name!.StartsWith(name));
        }
        if (type is not null)
        {
            root = root.Where(c => c.CatalogType == type);
        }
        if (brand is not null)
        {
            root = root.Where(c => c.CatalogBrand == brand);
        }

        var totalItems = await root
            .LongCountAsync();

        var itemsOnPage = await root
            .OrderBy(c => c.Name)
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage));
    }

    /// <summary>
    /// Batch get catalog items
    /// </summary>
    /// <remarks>
    /// Get multiple items from the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="ids">The ids of the items to return</param>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Ok<List<CatalogItem>>> GetItemsByIds(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        [MinLength(1)] int[] ids)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var items = await Context.CatalogItems.Where(item => ids.Contains(item.Id)).ToListAsync();
        return TypedResults.Ok(items);
    }

    /// <summary>
    /// Get a catalog item by id
    /// </summary>
    /// <remarks>
    /// Get a single item from the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="id">The id of the item to return</param>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Results<Ok<CatalogItem>, NotFound, BadRequest<ProblemDetails>>> GetItemById(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest<ProblemDetails>(new (){
                Detail = "Id is not valid"
            });
        }

        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(item);
    }

    /// <summary>
    /// Get a catalog item picture by id
    /// </summary>
    /// <remarks>
    /// Get a single item picture from the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="environment">The web host environment</param>
    /// <param name="id">The id of the item to return</param>
    [ProducesResponseType<byte[]>(StatusCodes.Status200OK, "application/octet-stream",
        [ "image/png", "image/gif", "image/jpeg", "image/bmp", "image/tiff",
          "image/wmf", "image/jp2", "image/svg+xml", "image/webp" ])]
    public static async Task<Results<PhysicalFileHttpResult,NotFound>> GetItemPictureById(
        HttpRequest httpRequest,
        IWebHostEnvironment environment,
        int id)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.FindAsync(id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        var path = GetFullPath(environment.ContentRootPath, item.PictureFileName!);

        string imageFileExtension = Path.GetExtension(item.PictureFileName!);
        string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);
        DateTime lastModified = File.GetLastWriteTimeUtc(path);

        return TypedResults.PhysicalFile(path, mimetype, lastModified: lastModified);
    }

    /// <summary>
    /// Update a catalog item
    /// </summary>
    /// <remarks>
    /// Update a single item in the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="id">The id of the item to update</param>
    /// <param name="patchDoc">The patch document to apply</param>
    public static async Task<Results<Ok<CatalogItem>, ValidationProblem, NotFound<ProblemDetails>>> UpdateItem(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int id,
        JsonPatchDocument<CatalogItem> patchDoc
    )
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var catalogItem = await Context.CatalogItems.SingleOrDefaultAsync(i => i.Id == id);

        if (catalogItem == null)
        {
            return TypedResults.NotFound<ProblemDetails>(new()
            {
                Detail = $"Item with id {id} not found."
            });
        }

        if (patchDoc != null)
        {
            Dictionary<string, string[]>? errors = null;
            patchDoc.ApplyTo(catalogItem, jsonPatchError =>
                {
                    errors ??= new();
                    var key = jsonPatchError.AffectedObject.GetType().Name;
                    if (!errors.ContainsKey(key))
                    {
                        errors.Add(key, new string[] { });
                    }
                    errors[key] = errors[key].Append(jsonPatchError.ErrorMessage).ToArray();
                });
            if (errors != null)
            {
                return TypedResults.ValidationProblem(errors);
            }
            await Context.SaveChangesAsync();
        }

        return TypedResults.Ok(catalogItem);
    }

    /// <summary>
    /// Create a catalog item
    /// </summary>
    /// <remarks>
    /// Create a new item in the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="product">The item to create</param>
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public static async Task<Created> CreateItem(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        CatalogItem product)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = new CatalogItem
        {
            Id = product.Id,
            CatalogBrand = product.CatalogBrand,
            CatalogType = product.CatalogType,
            Description = product.Description,
            Name = product.Name,
            PictureFileName = product.PictureFileName,
            Price = product.Price,
            AvailableStock = product.AvailableStock,
            RestockThreshold = product.RestockThreshold,
            MaxStockThreshold = product.MaxStockThreshold
        };

        Context.CatalogItems.Add(item);
        await Context.SaveChangesAsync();

        return TypedResults.Created($"/api/catalog/items/{item.Id}");
    }

    /// <summary>
    /// Delete a catalog item
    /// </summary>
    /// <remarks>
    /// Delete a single item from the catalog
    /// </remarks>
    /// <param name="httpRequest"></param>
    /// <param name="services">The catalog services</param>
    /// <param name="id">The id of the item to delete</param>
    public static async Task<Results<NoContent, NotFound>> DeleteItemById(
        HttpRequest httpRequest,
        [AsParameters] CatalogServices services,
        int id)
    {
        // workaround for https://github.com/dotnet/aspnetcore/issues/61770
        var Context = httpRequest.HttpContext.RequestServices.GetRequiredService<CatalogContext>();

        var item = await Context.CatalogItems.SingleOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return TypedResults.NotFound();
        }

        Context.CatalogItems.Remove(item);
        await Context.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static string GetImageMimeTypeFromImageFileExtension(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };

    public static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Pics", pictureFileName);
}
