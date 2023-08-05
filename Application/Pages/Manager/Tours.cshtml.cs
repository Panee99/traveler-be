using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

public class ToursModel : PageModel
{
    private readonly ITourService _tourService;

    public List<TourViewModel> Tours = new();

    public ToursModel(ITourService tourService)
    {
        _tourService = tourService;
    }

    public async Task OnGet()
    {
        var filterModel = new TourFilterModel()
        {
            Page = 1,
            Size = 5
        };

        var result = await _tourService.Filter(filterModel);
        if (result.IsSuccess) Tours = result.Value.Values;
    }
}