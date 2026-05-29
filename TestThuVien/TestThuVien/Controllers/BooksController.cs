// Controllers/BookController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;

namespace TestThuVien.Controllers;


public class BooksController : Controller
{
    private readonly QuanLyThuVienContext _context;
    public BooksController(QuanLyThuVienContext context) => _context = context;

    public async Task<IActionResult> Index(string keyword, int? categoryId, string status)
    {
        // 1. Nạp danh sách bộ lọc danh mục ra View như bình thường
        ViewBag.Categories = new SelectList(await _context.BookCategories.ToListAsync(), "CategoryId", "CategoryName");

        // 2. Gọi Stored Procedure để lấy danh sách Book thuần túy (Đã sửa SP nên chạy mượt mà, không lỗi)
        var books = await _context.Books
            .FromSqlInterpolated($"EXEC sp_SearchBooks {keyword}, {categoryId}, {status}")
            .ToListAsync();

        // 3. Để hiển thị được tên danh mục (CategoryName) ngoài giao diện mà không bị lỗi non-composable:
        // Lấy danh sách toàn bộ Category đang có gán vào bộ nhớ tạm
        var categoriesMap = await _context.BookCategories.ToDictionaryAsync(c => c.CategoryId);

        // Gán thủ công thực thể Category cho từng cuốn sách dựa trên Id trả về
        foreach (var book in books)
        {
            if (book.CategoryId.HasValue && categoriesMap.TryGetValue(book.CategoryId.Value, out var category))
            {
                book.Category = category;
            }
        }

        // 4. Trả kết quả sạch về cho View hiển thị
        return View(books);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CategoryId = new SelectList(await _context.BookCategories.Where(c => c.IsActive == true).ToListAsync(), "CategoryId", "CategoryName");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        if (_context.Books.Any(b => b.BookCode == book.BookCode))
        {
            ModelState.AddModelError("BookCode", "Mã sách này đã tồn tại trên hệ thống!");
        }

        if (ModelState.IsValid)
        {
            book.AvailableQuantity = book.Quantity;
            book.Status = book.Quantity > 0 ? "Available" : "Unavailable";
            _context.Add(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm sách mới thành công!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(await _context.BookCategories.ToListAsync(), "CategoryId", "CategoryName");
        return View(book);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        ViewBag.CategoryId = new SelectList(await _context.BookCategories.ToListAsync(), "CategoryId", "CategoryName", book.CategoryId);
        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Book book)
    {
        if (ModelState.IsValid)
        {
            var oldBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == book.BookId);
            int borrowedCount = oldBook.Quantity - oldBook.AvailableQuantity;

            book.AvailableQuantity = book.Quantity - borrowedCount;
            book.Status = book.AvailableQuantity > 0 ? "Available" : "Unavailable";

            _context.Update(book);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.CategoryId = new SelectList(await _context.BookCategories.ToListAsync(), "CategoryId", "CategoryName", book.CategoryId);
        return View(book);
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                book.IsDeleted = true; // Kích hoạt cơ chế soft-delete chặn bởi Trigger database
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa mềm sách thành công!";
            }
        }
        catch (Exception ex)
        {
            // Bắt lỗi từ RAISERROR trg_Book_DeleteSoft trong Trigger SQL Server gửi về
            TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}