using NoteMoodUOW.Core.Constants;
using NoteMoodUOW.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos.AuthDtos
{
    [DateNotFuture(ErrorMessage = "The provided date must not be today or in the future.")]
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required"), MaxLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Country is required"), MaxLength(100)]
        public string Country { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("Male|Female", ErrorMessage ="It is should be Male or Female")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Day is required")]
        public int Day { get; set; }

        [Required(ErrorMessage = "Month is required")]
        public string  Month { get; set; } 

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one number and one special character")]
        [Required(ErrorMessage = "Password is required"), MaxLength(100)]
        public string Password { get; set; }
    }
}