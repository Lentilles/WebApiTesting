using System.ComponentModel.DataAnnotations;

namespace WebApiTesting.Shared.CustomValidation
{
    public class DateTimeRangeAttribute : ValidationAttribute
    {
        public DateTime Min { get; set; }
        public DateTime Max { get; set; }


        public DateTimeRangeAttribute(DateTime min, DateTime max)
        {
            Min = min;
            Max = max;
        }

        public DateTimeRangeAttribute(DateTime min)
        {
            Min = min;
            Max = DateTime.UtcNow;
        }

        public DateTimeRangeAttribute()
        {
            Min = new DateTime(2000, 01, 01);
            Max = DateTime.UtcNow;
        }

        public string GetErrorMessage() => $"Incorrect Date. Input date between {Min.ToShortDateString()} and {Max.ToShortDateString()}";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var Date = (DateTime)value;

                var toLate = DateTime.Compare(Date, Max) > 0;
                var toEarly = DateTime.Compare(Date, Min) < 0;

                if (toEarly || toLate)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            else
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }
    }
}
