

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TestThuVien.Models;

public partial class BorrowTicketDetail
{
    public int TicketDetailId { get; set; }

    [Required(ErrorMessage = "Không được để trống phiếu mượn.")]
    public int? TicketId { get; set; }

    [Required(ErrorMessage = "Không được để trống sách.")]
    public int? BookId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng mượn phải lớn hơn 0.")]
    public int Quantity { get; set; }

    public string? ConditionBefore { get; set; }

    public string? ConditionAfter { get; set; }

    public virtual Book? Book { get; set; }

    public virtual BorrowTicket? Ticket { get; set; }
}
