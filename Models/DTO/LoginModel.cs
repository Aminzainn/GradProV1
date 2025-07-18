using System.ComponentModel.DataAnnotations;

namespace GP.Models.DTOs
{
    public class LoginModel
    {
        [Required]
        public string UserNameOrEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
