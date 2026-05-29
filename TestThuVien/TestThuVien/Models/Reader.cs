

using QuanLyThuVien.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TestThuVien.Models;

public partial class Reader
{
    public int ReaderId { get; set; }

    [Required(ErrorMessage = "Không để trống mã sinh viên.")]
    public string StudentCode { get; set; } = null!;

    [Required(ErrorMessage = "Không để trống họ tên độc giả.")]
    public string FullName { get; set; } = null!;

    public string? ClassName { get; set; }

    [RequiredPhone]
    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
}
