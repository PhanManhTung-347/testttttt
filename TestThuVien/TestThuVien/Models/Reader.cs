using System;
using System.Collections.Generic;

namespace TestThuVien.Models;

public partial class Reader
{
    public int ReaderId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? ClassName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<BorrowTicket> BorrowTickets { get; set; } = new List<BorrowTicket>();
}
