using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

public class ToursModel : PageModel
{
    // DI
    private readonly ITourService _tourService;

    // View
    public List<TourViewModel> Tours = new();

    // Post
    public IFormFile ImportFile { get; set; }

    public bool? ImportSuccess { get; set; }

    public ToursModel(ITourService tourService)
    {
        _tourService = tourService;
    }

    public async Task<IActionResult> OnPost()
    {
        var result = await _tourService.ImportTour(
            Guid.Parse("69f7719f-be55-42ca-843e-bc46cd1b450d"),
            ImportFile.OpenReadStream());

        if (!result.IsSuccess)
        {
            return RedirectToPage("Tours", new { ImportSuccess = 0 });
        }

        return RedirectToPage("Tours", new { ImportSuccess = 1 });
    }

    public async Task OnGetAsync()
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