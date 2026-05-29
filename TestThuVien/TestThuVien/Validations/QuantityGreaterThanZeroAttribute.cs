using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Số lượng sách >= 0
    public class QuantityGreaterThanZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int quantity)
            {
                if (quantity < 0)
                {
                    return new ValidationResult("Số lượng sách phải lớn hơn hoặc bằng 0.");
                }
            }

            return ValidationResult.Success;
        }
    }
}