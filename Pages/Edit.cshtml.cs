using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedConfigApp.Models;
using SharedConfigApp.Services;

namespace SharedConfigApp.Pages
{
    public class EditModel : PageModel
    {
        private readonly ConfigService _configService;
        private readonly IConfiguration _configuration;

        public EditModel(ConfigService configService, IConfiguration configuration)
        {
            _configService = configService;
            _configuration = configuration;
        }

        [BindProperty]
        public ConfigModel Config { get; set; } = new();
        
        public string ConfigPath { get; set; } = string.Empty;

        public void OnGet()
        {
            Config = _configService.GetCurrentConfig();
            ConfigPath = _configuration["SharedConfigPath"] ?? "SharedData/config.xml";
        }

        public IActionResult OnPost()
        {
            ConfigPath = _configuration["SharedConfigPath"] ?? "SharedData/config.xml";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var success = _configService.UpdateConfig(Config);

            if (success)
            {
                TempData["Success"] = "Configuration updated successfully!";
                return RedirectToPage("/Index");
            }
            else
            {
                TempData["Error"] = "Failed to update configuration. Please try again.";
                return Page();
            }
        }
    }
}