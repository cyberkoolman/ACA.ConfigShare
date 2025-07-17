using System.ComponentModel.DataAnnotations;

namespace SharedConfigApp.Models
{
    public class ConfigModel
    {
        [Required]
        [Display(Name = "Application Name")]
        public string AppName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Database Connection")]
        public string DatabaseConnection { get; set; } = string.Empty;

        [Required]
        [Display(Name = "API Timeout (seconds)")]
        [Range(1, 300)]
        public int ApiTimeout { get; set; }

        [Display(Name = "Enable Logging")]
        public bool EnableLogging { get; set; }

        [Required]
        [Display(Name = "Maximum Users")]
        [Range(1, 10000)]
        public int MaxUsers { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}