using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NoteMoodUOW.Core.Validation
{
    public class DateNotFutureForEntry : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Use reflection to get properties by name
            var dateProperty = validationContext.ObjectType.GetProperty("Date");
            var timeProperty = validationContext.ObjectType.GetProperty("Time");

            if (dateProperty == null || timeProperty == null)
            {
                return new ValidationResult("Date and Time properties are required.");
            }

            // Get property values
            var date = (DateOnly)dateProperty.GetValue(validationContext.ObjectInstance);
            var time = (TimeOnly)timeProperty.GetValue(validationContext.ObjectInstance);

            // Combine DateOnly and TimeOnly into a DateTime
            var dateTime = date.ToDateTime(time);

            if (dateTime > DateTime.Now)
            {
                return new ValidationResult("The date and time must not be in the future.");
            }

            return ValidationResult.Success;
        }
    }
}