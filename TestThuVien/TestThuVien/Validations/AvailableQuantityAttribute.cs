using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không cho mượn khi số lượng còn 0
    public class AvailableQuantityAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int quantity)
            {
                if (quantity <= 0)
                {
                    return new ValidationResult("Sách đã hết, không thể mượn.");
                }
            }

            return ValidationResult.Success;
        }
    }
}