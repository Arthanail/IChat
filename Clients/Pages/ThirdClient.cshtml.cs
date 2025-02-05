using Clients.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clients.Pages;

public class ThirdClient(ChatHttpClient chatHttpClient) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        TempData["Messages"] = await chatHttpClient.GetMessages();
        return Page();
    }
}