using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.AuthDtos
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
        [Required(ErrorMessage = "Current password is required")]
        public required string CurrentPassword { get; set; }
        [Required(ErrorMessage = "New password is required")]
        public required string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "The new password and the confirmation password don't match.")]
        public required string ConfirmPassword { get; set; }

    }
}
