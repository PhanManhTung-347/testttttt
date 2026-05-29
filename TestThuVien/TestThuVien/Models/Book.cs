using System;
using System.Collections.Generic;

namespace TestThuVien.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string BookCode { get; set; } = null!;

    public string BookTitle { get; set; } = null!;

    public string? Author { get; set; }

    public int? CategoryId { get; set; }

    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    public int Quantity { get; set; }

    public int AvailableQuantity { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<BorrowTicketDetail> BorrowTicketDetails { get; set; } = new List<BorrowTicketDetail>();

    public virtual BookCategory? Category { get; set; }
}
