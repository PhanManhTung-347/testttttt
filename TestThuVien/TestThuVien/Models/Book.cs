

using QuanLyThuVien.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestThuVien.Models;

public partial class Book
{
    public int BookId { get; set; }

    [RequiredBookInfo(ErrorMessage = "Không để trống mã sách.")]
    public string BookCode { get; set; } = null!;

    [RequiredBookInfo(ErrorMessage = "Không để trống tên sách.")]
    public string BookTitle { get; set; } = null!;

    public string? Author { get; set; }

    public int? CategoryId { get; set; }

    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    [QuantityGreaterThanZero]
    public int Quantity { get; set; }

    [AvailableQuantity]
    public int AvailableQuantity { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    [BookInBorrowSlip]
    public bool IsBorrowing { get; set; }

    public virtual ICollection<BorrowTicketDetail> BorrowTicketDetails { get; set; } = new List<BorrowTicketDetail>();

    public virtual BookCategory? Category { get; set; }
}

