// Controllers/BookCategoryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;

namespace TestThuVien.Controllers;

public class BookCategoriesController : Controller
{
    private readonly QuanLyThuVienContext _context;

    public BookCategoriesController(QuanLyThuVienContext context) => _context = context;
    public async Task<IActionResult> Index(string keyword)
    {
        var query = _context.BookCategories.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(c => c.CategoryName.Contains(keyword));
        return View(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
        {
            TempData["Error"] = "Tên danh mục không được để trống!";
            return RedirectToAction(nameof(Index));
        }
        _context.Add(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm danh mục thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BookCategory category)
    {
        _context.Update(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật danh mục thành công!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ToggleStatus(int id)
    {
        var category = await _context.BookCategories.FindAsync(id);
        if (category != null)
        {
            category.IsActive = !(category.IsActive ?? true);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}