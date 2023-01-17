using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class HttpReserveService : IReserveService
{
    private const string Path = "/reserve";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _requestUri;

    public HttpReserveService(IHttpClientFactory httpClientFactory, string rootUri)
    {
        var uri = new Uri(rootUri);
        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException("Value must be an absolute uri.", nameof(rootUri));
        }

        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            throw new ArgumentException("Value must not contain query parameters.", nameof(rootUri));
        }

        _httpClientFactory = httpClientFactory;
        _requestUri = rootUri.Replace('\\', '/').TrimEnd('/') + Path;
    }

    public async Task ReserveItemsAsync(IEnumerable<OrderItem> orderItems)
    {
        HttpResponseMessage reserveResponse;
        using (HttpClient client = _httpClientFactory.CreateClient(nameof(HttpReserveService)))
        {
            reserveResponse = await client.PostAsync(_requestUri, new StringContent(orderItems.ToJson()));
        }

        if (!reserveResponse.IsSuccessStatusCode)
        {
            var exception =
                new Exception($"Reservation failed: {reserveResponse.ReasonPhrase}({reserveResponse.StatusCode}).");

            exception.Data.Add("Content", await reserveResponse.Content.ReadAsStringAsync());

            throw exception;
        }
    }
}
