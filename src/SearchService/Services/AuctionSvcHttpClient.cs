using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        // Retrieves the last updated date of an item.
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        // Retrieves all items from the AuctionService that have been updated since the last updated date.
        return await _httpClient.GetFromJsonAsync<List<Item>>(
            $"{_configuration["AuctionServiceUrl"]}/api/auctions?date={lastUpdated}");

    }

}
