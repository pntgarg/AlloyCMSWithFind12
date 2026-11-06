using AlloyCMS12withfind.Models.Pages;
using AlloyCMS12withfind.Models.ViewModels;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Find.UnifiedSearch;
using Microsoft.AspNetCore.Mvc;

namespace AlloyCMS12withfind.Controllers;

public class SearchPageController : PageControllerBase<SearchPage>
{
    private const int MaxResults = 40;
    private readonly IClient _searchClient;

    public SearchPageController(IClient searchClient)
    {
        _searchClient = searchClient;
    }

    public async Task<ViewResult> Index(SearchPage currentPage, string q)
    {
        var model = new SearchContentModel(currentPage)
        {
            SearchedQuery = q,
            SearchServiceDisabled = false
        };

        if (string.IsNullOrWhiteSpace(q))
        {
            return View(model);
        }

        var results = await _searchClient.UnifiedSearchFor(q.Trim())
            .UsingSynonyms()
            .Take(MaxResults)
            .GetResultAsync(new HitSpecification
            {
                HighlightTitle = false,
                HighlightExcerpt = false,
                EncodeTitle = false,
                EncodeExcerpt = false
            }, filterForPublicSearch: true);

        model.Hits = results.Select(hit => new SearchContentModel.SearchHit
        {
            Title = hit.Title,
            Url = hit.Url,
            Excerpt = hit.Excerpt
        }).ToList();
        model.NumberOfHits = results.TotalMatching;

        return View(model);
    }
}
