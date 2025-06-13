using NoteMoodUOW.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Dtos
{
    [DateNotFuture(ErrorMessage = "The provided date must not be today or in the future.")]
    public class ProfileDto
    {
        [Required(ErrorMessage = "Name is required"), MaxLength(100)]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Country is required"), MaxLength(100)]
        public required string Country { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("Male|Female", ErrorMessage = "It is should be Male or Female")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Day is required")]
        public int Day { get; set; }

        [Required(ErrorMessage = "Month is required")]
        public string Month { get; set; }

        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }

}
