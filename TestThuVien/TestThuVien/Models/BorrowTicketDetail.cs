using System;
using System.Collections.Generic;

namespace TestThuVien.Models;

public partial class BorrowTicketDetail
{
    public int TicketDetailId { get; set; }

    public int? TicketId { get; set; }

    public int? BookId { get; set; }

    public int Quantity { get; set; }

    public string? ConditionBefore { get; set; }

    public string? ConditionAfter { get; set; }

    public virtual Book? Book { get; set; }

    public virtual BorrowTicket? Ticket { get; set; }
}
