using System.ComponentModel.DataAnnotations;

namespace GP.Models.DTOs
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[!@#\$%\^&\*])(?=.*\d).{8,}$",
        ErrorMessage = "Password must be at least 8 characters, contain a capital letter, number, and special character.")]

        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool Gender { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }
    }
}
