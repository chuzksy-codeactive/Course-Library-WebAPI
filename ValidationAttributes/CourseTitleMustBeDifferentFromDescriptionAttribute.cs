using System.ComponentModel.DataAnnotations;

using Library.API.Models;

namespace Library.API.ValidationAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid (object value,
            ValidationContext validationContext)
        {
            var course = (CourseForCreationDto) validationContext.ObjectInstance;

            if (course.Title == course.Description)
            {
                return new ValidationResult (
                    ErrorMessage,
                    new [] { "CourseForCreationDto" }
                );
            }

            return ValidationResult.Success;
        }
    }
}
