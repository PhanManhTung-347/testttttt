

using QuanLyThuVien.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TestThuVien.Models;

public partial class BookCategory
{
    public int CategoryId { get; set; }

    [RequiredCategoryName]
    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}