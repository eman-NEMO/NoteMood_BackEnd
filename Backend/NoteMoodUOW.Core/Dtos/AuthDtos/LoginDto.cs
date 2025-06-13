using System.ComponentModel.DataAnnotations;

namespace NoteMoodUOW.Core.Dtos.AuthDtos
{
    public class LoginDto
    {
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
         ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}