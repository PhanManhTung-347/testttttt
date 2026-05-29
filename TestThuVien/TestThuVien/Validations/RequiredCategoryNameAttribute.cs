using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không để trống tên danh mục sách
    public class RequiredCategoryNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult("Tên danh mục sách không được để trống.");
            }

            return ValidationResult.Success;
        }
    }
}