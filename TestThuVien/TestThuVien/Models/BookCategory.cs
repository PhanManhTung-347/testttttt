using System;
using System.Collections.Generic;

namespace TestThuVien.Models;

public partial class BookCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
