

using QuanLyThuVien.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TestThuVien.Models;

public partial class BorrowTicket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    [Required(ErrorMessage = "Không để trống độc giả.")]
    public int? ReaderId { get; set; }

    public DateTime BorrowDate { get; set; }

    [ReturnDateGreaterThanBorrowDate("BorrowDate")]
    public DateTime ExpectedReturnDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    [AlreadyReturned]
    public bool IsReturned { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<BorrowTicketDetail> BorrowTicketDetails { get; set; } = new List<BorrowTicketDetail>();

    public virtual Reader? Reader { get; set; }
}

