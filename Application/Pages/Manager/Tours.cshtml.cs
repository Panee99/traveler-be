﻿using Application.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

[IgnoreAntiforgeryToken]
public class ToursModel : PageModel
{
    private const int PageSize = 5;
    
    // DI
    private readonly ITourService _tourService;

    // Get
    [BindProperty(SupportsGet = true)] public string SearchValue { get; set; } = "";
    [BindProperty(SupportsGet = true)] public int PageNo { get; set; } = 1;

    public PaginationModel<TourViewModel> Data = new();
    public UserSessionModel? CurrentUser { get; set; }

    // Post
    public IFormFile ImportFile { get; set; }

    public string? ErrorMessage { get; set; }

    public ToursModel(ITourService tourService)
    {
        _tourService = tourService;
    }

    public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        CurrentUser = RazorPageHelper.GetUserFromSession(HttpContext.Session);
        base.OnPageHandlerSelected(context);
    }

    /// <summary>
    /// Import Tour
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        // Check if file exist
        if (ReferenceEquals(ImportFile, null))
        {
            // ErrorMessage = "Choose a file";
        }
        // Import data
        else
        {
            var result = await _tourService.ImportTour(
                CurrentUser.Id, ImportFile.OpenReadStream());

            // Error message
            ErrorMessage = result.IsSuccess ? "" : result.Error.ErrorDetails.FirstOrDefault();
        }

        await _loadPageData();
        return Page();
    }

    /// <summary>
    /// Get page
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        // Load page data
        await _loadPageData();
        return Page();
    }

    private async Task _loadPageData()
    {
        var filterModel = new TourFilterModel()
        {
            Page = PageNo,
            Size = PageSize,
            Title = SearchValue,
        };

        var result = await _tourService.Filter(filterModel);
        if (result.IsSuccess) Data = result.Value;
    }
}