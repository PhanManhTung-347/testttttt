
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;   
namespace TestThuVien.Controllers;

public class HomeController : Controller
{
    private readonly QuanLyThuVienContext _context;

    public HomeController(QuanLyThuVienContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Gọi Stored Procedure sp_GetDashboardStats
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "EXEC sp_GetDashboardStats";
        _context.Database.OpenConnection();

        using var result = await command.ExecuteReaderAsync();
        var stats = new DashboardViewModel();

        if (await result.ReadAsync())
        {
            stats.TotalBooks = result.GetInt32(0);
            stats.AvailableBooks = result.IsDBNull(1) ? 0 : result.GetInt32(1);
            stats.BorrowedBooks = result.IsDBNull(2) ? 0 : result.GetInt32(2);
            stats.ActiveTickets = result.GetInt32(3);
            stats.ReturnedTickets = result.GetInt32(4);
            stats.OverdueTickets = result.GetInt32(5);
        }

        return View(stats);
    }
}