using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không để trống mã sách và tên sách
    public class RequiredBookInfoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}