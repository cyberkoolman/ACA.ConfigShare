using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedConfigApp.Models;
using SharedConfigApp.Services;

namespace SharedConfigApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ConfigService _configService;
        private readonly IConfiguration _configuration;

        public IndexModel(ConfigService configService, IConfiguration configuration)
        {
            _configService = configService;
            _configuration = configuration;
        }

        public ConfigModel Config { get; set; } = new();
        public string ConfigPath { get; set; } = string.Empty;

        public void OnGet()
        {
            Config = _configService.GetCurrentConfig();
            ConfigPath = _configuration["SharedConfigPath"] ?? "SharedData/config.xml";
        }
    }
}