using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class HttpDeliveryService : IDeliveryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _rootUri;

    public HttpDeliveryService(IHttpClientFactory httpClientFactory, string rootUri)
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
        _rootUri = rootUri.Replace('\\', '/').TrimEnd('/');
    }

    public async Task CreateDeliveryOrderAsync(DeliveryOrder order)
    {
        var requestUri = $"{_rootUri}/orders";

        HttpResponseMessage reserveResponse;
        using (HttpClient client = _httpClientFactory.CreateClient(nameof(HttpDeliveryService)))
        {
            reserveResponse = await client.PostAsync(requestUri, new StringContent(order.ToJson()));
        }

        if (!reserveResponse.IsSuccessStatusCode)
        {
            var exception =
                new Exception($"Delivery order creation failed: {reserveResponse.ReasonPhrase}({(int)reserveResponse.StatusCode}).");

            exception.Data.Add("Content", await reserveResponse.Content.ReadAsStringAsync());

            throw exception;
        }
    }
}
