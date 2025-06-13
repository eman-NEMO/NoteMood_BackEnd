using Microsoft.AspNetCore.Identity;
using NoteMoodUOW.Core.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteMoodUOW.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Name is required"),MaxLength(100)]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Country is required"), MaxLength(100)]

        public required string Country  { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender  Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]

        public DateOnly DateOfBirth { get; set; }

        // image

        public List<RefreshToken>? RefreshTokens { get; set; }
        public ICollection<Entry> Entries { get; set; }
        public ICollection<DailySentiment>? DailySentiments { get; set; }

    }
}
