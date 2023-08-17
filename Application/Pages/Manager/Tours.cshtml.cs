using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

public class ToursModel : PageModel
{
    // DI
    private readonly ITourService _tourService;

    // Get
    [BindProperty(SupportsGet = true)] public string? SearchValue { get; set; }

    public List<TourViewModel> Tours = new();
    public UserSessionModel CurrentUser { get; set; }

    // Post
    public IFormFile ImportFile { get; set; }

    public string? ErrorMessage { get; set; }

    public ToursModel(ITourService tourService)
    {
        _tourService = tourService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ReferenceEquals(ImportFile, null))
        {
            ErrorMessage = "Choose a file";
        }
        else
        {
            var result = await _tourService.ImportTour(
                Guid.Parse("69f7719f-be55-42ca-843e-bc46cd1b450d"),
                ImportFile.OpenReadStream());

            ErrorMessage = result.IsSuccess ? "" : result.Error.ErrorDetails.FirstOrDefault();
        }

        await _loadPageData();
        return Page();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Authenticate User
        var userSessionData = HttpContext.Session.GetString("User");
        var userData = userSessionData is null
            ? null
            : JsonConvert.DeserializeObject<UserSessionModel>(userSessionData);
        if (userData is null) return RedirectToPage("Login");
        CurrentUser = userData;
        
        // Load page data
        await _loadPageData();
        return Page();
    }

    private async Task _loadPageData()
    {
        var filterModel = new TourFilterModel()
        {
            Page = 1,
            Size = 5,
            Title = SearchValue,
        };

        var result = await _tourService.Filter(filterModel);
        if (result.IsSuccess) Tours = result.Value.Values;
    }
}