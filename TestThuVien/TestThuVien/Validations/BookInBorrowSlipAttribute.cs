using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Không cho xóa sách đang được mượn
    public class BookInBorrowSlipAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is bool isBorrowing)
            {
                if (isBorrowing)
                {
                    return new ValidationResult("Không thể xóa sách đang nằm trong phiếu mượn chưa trả.");
                }
            }

            return ValidationResult.Success;
        }
    }
}