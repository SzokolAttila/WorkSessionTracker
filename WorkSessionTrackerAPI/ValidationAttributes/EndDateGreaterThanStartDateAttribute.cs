using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSessionTrackerAPI.ValidationAttributes
{
    public class EndDateGreaterThanStartDateAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;

        public EndDateGreaterThanStartDateAttribute(string startDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Let [Required] handle nulls
            }

            var endDate = (DateTime)value;
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            var startDate = (DateTime?)startDateProperty?.GetValue(validationContext.ObjectInstance);

            if (startDate.HasValue && endDate <= startDate.Value)
            {
                return new ValidationResult(
                    ErrorMessage ?? "End date must be later than start date.",
                    validationContext.MemberName != null
                        ? new[] { validationContext.MemberName }
                        : null); // Or new string[0] if you prefer an empty array
            }

            return ValidationResult.Success;
        }
    }
}
