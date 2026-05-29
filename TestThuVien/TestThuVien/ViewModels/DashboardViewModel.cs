// Models/DashboardViewModel.cs
namespace TestThuVien.Models;

public class DashboardViewModel
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int BorrowedBooks { get; set; }
    public int ActiveTickets { get; set; }
    public int ReturnedTickets { get; set; }
    public int OverdueTickets { get; set; }
}