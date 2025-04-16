using System.ComponentModel.DataAnnotations;

namespace LuxenHotel.Models.ViewModels;

public class LoginViewModel
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    public bool RememberMe { get; set; }
}