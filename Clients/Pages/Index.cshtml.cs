using Clients.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clients.Pages;

public class FirstClientModel(ChatHttpClient chatHttpClient) : PageModel
{
    [BindProperty]
    public string Message { get; set; }
    
    [BindProperty]
    public int SerialNumber { get; set; }

    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPost()
    {
        var response = "";
        if (!string.IsNullOrEmpty(Message))
        {
            response = await chatHttpClient.SendMessage(Message, SerialNumber);
            TempData["AlertMessage"] = response;
        }
        else
        {
            TempData["AlertMessage"] = response;
        }

        return Page();
    }
}