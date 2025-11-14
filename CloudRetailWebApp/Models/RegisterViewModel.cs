using System.ComponentModel.DataAnnotations;

namespace CloudRetailWebApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Full name")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account type")]
        [RegularExpression("^(Admin|Customer)$", ErrorMessage = "Role must be Admin or Customer.")]
        public string Role { get; set; } = "Customer";
    }
}

