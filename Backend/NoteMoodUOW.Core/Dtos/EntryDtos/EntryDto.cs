using NoteMoodUOW.Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace NoteMoodUOW.Core.Dtos.EntryDtos
{
   // [DateNotFutureForEntry(ErrorMessage = "The date and time must not be in the future.")]
    public class EntryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot be more than 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [MaxLength(10000, ErrorMessage = "Content cannot be more than 10000 characters.")]
        public string Content { get; set; }
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Time is required.")]
        public TimeOnly Time { get; set; }
        public string? OverallSentiment { get; set; }
    }
}