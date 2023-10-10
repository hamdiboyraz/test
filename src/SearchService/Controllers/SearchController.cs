using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.ReqestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item, Item>();

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        // order by
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x => x.Ascending(a => a.Make)), // sort by make
            "new" => query.Sort(x => x.Descending(a => a.CreatedAt)), // newest first
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)) // default sort by auction end date
        };

        // filter by

        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow), // finished auctions
            "endingSoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow
                && x.AuctionEnd < DateTime.UtcNow.AddHours(6)), // auctions ending soon
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow) // default: active auctions
        };

        // seller
        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        // winner
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }


        query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });

    }
}