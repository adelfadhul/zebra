using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zebra.Core.Senders;

namespace Zebra.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly WireSender _wireSender;

        public IndexModel(ILogger<IndexModel> logger, WireSender wireSender)
        {
            _logger = logger;
            _wireSender = wireSender;
        }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string CPR { get; set; } = string.Empty;

        public void OnGet([FromQuery] string? name, [FromQuery] string? cpr)
        {
            // Populate properties from query parameters if they exist
            Name = name ?? string.Empty;
            CPR = cpr ?? string.Empty;
        }

        public IActionResult OnPost()
        {
            // Example: Send ZPL command to the printer
            bool success = _wireSender.SendCardToPrinter(Name, CPR);

            if (success)
            {
                TempData["Message"] = "ZPL command sent successfully!";
            }
            else
            {
                TempData["Message"] = "Failed to send ZPL command.";
            }

            return Page();
        }
    }
}
