using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không để trống số điện thoại
    public class RequiredPhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult("Số điện thoại độc giả không được để trống.");
            }

            return ValidationResult.Success;
        }
    }
}