using System.ComponentModel.DataAnnotations;

namespace Template.Web.Models.Settings;

public sealed class SupportRequestViewModel
{
    [Required, StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = "Normal";
}
