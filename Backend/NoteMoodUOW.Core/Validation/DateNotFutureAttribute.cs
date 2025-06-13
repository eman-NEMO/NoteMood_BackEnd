using NoteMoodUOW.Core.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NoteMoodUOW.Core.Validation
{
    public class DateNotFutureAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Use reflection to get properties by name
            var dayProperty = validationContext.ObjectType.GetProperty("Day");
            var monthProperty = validationContext.ObjectType.GetProperty("Month");
            var yearProperty = validationContext.ObjectType.GetProperty("Year");

            if (dayProperty == null || monthProperty == null || yearProperty == null)
            {
                return new ValidationResult("Day, Month, and Year properties are required.");
            }

            // Get property values
            int day = (int)dayProperty.GetValue(validationContext.ObjectInstance);
            string monthString = (string)monthProperty.GetValue(validationContext.ObjectInstance);
            int year = (int)yearProperty.GetValue(validationContext.ObjectInstance);

            if (!Enum.TryParse(typeof(Month), monthString, true, out var monthValue))
            {
                return new ValidationResult("Invalid month.");
            }

            var monthInt = (int)monthValue;
            try
            {
                var date = new DateTime(year, monthInt, day);
                if (date > DateTime.Today)
                {
                    return new ValidationResult("The date must not be in the future.");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult("Invalid date.");
            }

            return ValidationResult.Success;
        }
    }
}