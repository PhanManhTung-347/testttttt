// Controllers/ReaderController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;

namespace TestThuVien.Controllers;

public class ReadersController : Controller
{
    private readonly QuanLyThuVienContext _context;
    public ReadersController(QuanLyThuVienContext context) => _context = context;

    public async Task<IActionResult> Index(string keyword)
    {
        var query = _context.Readers.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(r => r.StudentCode.Contains(keyword) ||
                                     r.FullName.Contains(keyword) ||
                                     r.PhoneNumber.Contains(keyword));
        }
        return View(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(Reader reader)
    {
        if (reader.ReaderId == 0)
        {
            if (_context.Readers.Any(r => r.StudentCode == reader.StudentCode))
            {
                TempData["Error"] = "Mã sinh viên đã tồn tại!";
                return RedirectToAction(nameof(Index));
            }
            _context.Add(reader);
            TempData["Success"] = "Thêm độc giả thành công!";
        }
        else
        {
            _context.Update(reader);
            TempData["Success"] = "Cập nhật thông tin thành công!";
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}