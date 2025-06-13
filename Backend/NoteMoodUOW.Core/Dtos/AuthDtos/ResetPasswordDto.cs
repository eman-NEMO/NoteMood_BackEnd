using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.AuthDtos
{
    public class ResetPasswordDto
    {
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one number and one special character")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and the confirmation password don't match.")]
        public string? confirmPassword { get; set; }
        public string? Token { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
