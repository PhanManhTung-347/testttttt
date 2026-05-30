// Controllers/BorrowTicketController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;

namespace TestThuVien.Controllers;

public class BorrowTicketsController : Controller
{
    private readonly QuanLyThuVienContext _context;
    public BorrowTicketsController(QuanLyThuVienContext context) => _context = context;

    public async Task<IActionResult> Index(string keyword, string status)
    {
        var query = _context.BorrowTickets.Include(t => t.Reader).AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(t => t.TicketCode.Contains(keyword) || t.Reader.FullName.Contains(keyword));
        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        return View(await query.OrderByDescending(t => t.BorrowDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ReaderId = new SelectList(await _context.Readers.Where(r => r.IsActive == true).ToListAsync(), "ReaderId", "FullName");
        ViewBag.Books = await _context.Books.Where(b => b.AvailableQuantity > 0 && b.IsDeleted == false).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(BorrowTicket ticket, int[] selectedBooks)
    {
        try
        {
            ticket.TicketCode = "TK-" + DateTime.Now.Ticks.ToString().Substring(10);
            ticket.BorrowDate = DateTime.Now;
            ticket.Status = "Borrowing";

            foreach (var bookId in selectedBooks)
            {
                ticket.BorrowTicketDetails.Add(new BorrowTicketDetail
                {
                    BookId = bookId,
                    Quantity = 1,
                    ConditionBefore = "Bình thường"
                });
            }

            _context.Add(ticket);
            await _context.SaveChangesAsync(); // Sẽ kích hoạt trg_BorrowTicketDetail_Insert tự động trừ sách
            TempData["Success"] = "Tạo phiếu mượn sách thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi nghiệp vụ: " + (ex.InnerException?.Message ?? ex.Message);
            return RedirectToAction(nameof(Create));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _context.BorrowTickets
            .Include(t => t.Reader)
            .Include(t => t.BorrowTicketDetails).ThenInclude(d => d.Book)
            .FirstOrDefaultAsync(t => t.TicketId == id);
        return View(ticket);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmReturn(int ticketId, string conditionAfter, string note)
    {
        try
        {
            // Thực thi Procedure xác nhận trả sách an toàn hệ thống
            await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC sp_ConfirmReturnBook {ticketId}, {conditionAfter}, {note}");
            TempData["Success"] = "Đã xác nhận trả sách thành công, kho số lượng đã hoàn tăng!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = ticketId });
    }
}