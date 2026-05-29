using System;
using System.Collections.Generic;

namespace TestThuVien.Models;

public partial class BorrowTicket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public int? ReaderId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime ExpectedReturnDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<BorrowTicketDetail> BorrowTicketDetails { get; set; } = new List<BorrowTicketDetail>();

    public virtual Reader? Reader { get; set; }
}
