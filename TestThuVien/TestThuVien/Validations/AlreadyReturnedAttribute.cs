using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không cho trả phiếu đã trả
    public class AlreadyReturnedAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is bool isReturned)
            {
                if (isReturned)
                {
                    return new ValidationResult("Phiếu mượn này đã được trả.");
                }
            }

            return ValidationResult.Success;
        }
    }
}