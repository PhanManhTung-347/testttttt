using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Validation
{
    // Ngày trả >= ngày mượn
    public class ReturnDateGreaterThanBorrowDateAttribute : ValidationAttribute
    {
        private readonly string _borrowDateProperty;

        public ReturnDateGreaterThanBorrowDateAttribute(string borrowDateProperty)
        {
            _borrowDateProperty = borrowDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var borrowDateProperty = validationContext.ObjectType.GetProperty(_borrowDateProperty);

            if (borrowDateProperty == null)
            {
                return new ValidationResult("Không tìm thấy ngày mượn.");
            }

            var borrowDate = (DateTime)borrowDateProperty.GetValue(validationContext.ObjectInstance)!;
            var returnDate = (DateTime)value!;

            if (returnDate < borrowDate)
            {
                return new ValidationResult("Ngày dự kiến trả không được nhỏ hơn ngày mượn.");
            }

            return ValidationResult.Success;
        }
    }
}