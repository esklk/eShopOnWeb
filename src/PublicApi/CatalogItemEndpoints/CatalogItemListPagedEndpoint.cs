using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.Extensions.Logging;
using MinimalApi.Endpoint;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

/// <summary>
/// List Catalog Items (paged)
/// </summary>
public class CatalogItemListPagedEndpoint : IEndpoint<IResult, ListPagedCatalogItemRequest>
{
    private IRepository<CatalogItem> _itemRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IMapper _mapper;
    private readonly ILogger<CatalogItemListPagedEndpoint> _logger;

    public CatalogItemListPagedEndpoint(IUriComposer uriComposer, IMapper mapper, ILoggerFactory loggerFactory)
    {
        _uriComposer = uriComposer;
        _mapper = mapper;
        _logger = loggerFactory.CreateLogger<CatalogItemListPagedEndpoint>();
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("api/catalog-items",
            async (int? pageSize, int? pageIndex, int? catalogBrandId, int? catalogTypeId, IRepository<CatalogItem> itemRepository) =>
            {
                _itemRepository = itemRepository;
                return await HandleAsync(new ListPagedCatalogItemRequest(pageSize, pageIndex, catalogBrandId, catalogTypeId));
            })            
            .Produces<ListPagedCatalogItemResponse>()
            .WithTags("CatalogItemEndpoints");
    }

    public async Task<IResult> HandleAsync(ListPagedCatalogItemRequest request)
    {
        // Break the ListPaged by adding throw new Exception("Cannot move further"); to HandleAsync method. Redeploy the Public API into app service. Check the logs in Application Insights
        throw new Exception("Cannot move further");
        // It is impossible to complete the following actions item
        // 9. Evaluate the error through Failures Tab in Application Insights to get the stacktrace
        // since exception is handled by ExceptionMiddleware, and stack trace swallowed

        var response = new ListPagedCatalogItemResponse(request.CorrelationId());

        var filterSpec = new CatalogFilterSpecification(request.CatalogBrandId, request.CatalogTypeId);
        int totalItems = await _itemRepository.CountAsync(filterSpec);

        var pagedSpec = new CatalogFilterPaginatedSpecification(
            skip: request.PageIndex.Value * request.PageSize.Value,
            take: request.PageSize.Value,
            brandId: request.CatalogBrandId,
            typeId: request.CatalogTypeId);

        var items = await _itemRepository.ListAsync(pagedSpec);

        // 3. Inject logger in ListPaged controller and add logging of the number items that were returned from the database.
        _logger.LogInformation($"{items.Count} catalog items were returned from database.");


        response.CatalogItems.AddRange(items.Select(_mapper.Map<CatalogItemDto>));
        foreach (CatalogItemDto item in response.CatalogItems)
        {
            item.PictureUri = _uriComposer.ComposePicUri(item.PictureUri);
        }

        if (request.PageSize > 0)
        {
            response.PageCount = int.Parse(Math.Ceiling((decimal)totalItems / request.PageSize.Value).ToString());
        }
        else
        {
            response.PageCount = totalItems > 0 ? 1 : 0;
        }

        return Results.Ok(response);
    }
}
