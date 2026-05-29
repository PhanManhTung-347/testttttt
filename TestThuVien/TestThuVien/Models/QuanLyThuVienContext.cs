using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestThuVien.Models;

public partial class QuanLyThuVienContext : DbContext
{
    public QuanLyThuVienContext()
    {
    }

    public QuanLyThuVienContext(DbContextOptions<QuanLyThuVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCategory> BookCategories { get; set; }

    public virtual DbSet<BorrowTicket> BorrowTickets { get; set; }

    public virtual DbSet<BorrowTicketDetail> BorrowTicketDetails { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=QuanLyThuVien;User Id=sa;Password=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C20712BD6694");

            entity.ToTable("Book", tb => tb.HasTrigger("trg_Book_DeleteSoft"));

            entity.HasIndex(e => e.BookCode, "UQ__Book__0A5FFCC700F2C1B7").IsUnique();

            entity.Property(e => e.Author).HasMaxLength(150);
            entity.Property(e => e.BookCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.BookTitle).HasMaxLength(200);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Publisher).HasMaxLength(150);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Book_Category");
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__BookCate__19093A0B383B32A1");

            entity.ToTable("BookCategory");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<BorrowTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__BorrowTi__712CC6077B58563B");

            entity.ToTable("BorrowTicket");

            entity.HasIndex(e => e.TicketCode, "UQ__BorrowTi__598CF7A3FF68ED85").IsUnique();

            entity.Property(e => e.BorrowDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpectedReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.ReturnDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Borrowing");
            entity.Property(e => e.TicketCode)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Reader).WithMany(p => p.BorrowTickets)
                .HasForeignKey(d => d.ReaderId)
                .HasConstraintName("FK_BorrowTicket_Reader");
        });

        modelBuilder.Entity<BorrowTicketDetail>(entity =>
        {
            entity.HasKey(e => e.TicketDetailId).HasName("PK__BorrowTi__39BFBDE699D045A6");

            entity.ToTable("BorrowTicketDetail", tb => tb.HasTrigger("trg_BorrowTicketDetail_Insert"));

            entity.Property(e => e.ConditionAfter).HasMaxLength(255);
            entity.Property(e => e.ConditionBefore).HasMaxLength(255);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Book).WithMany(p => p.BorrowTicketDetails)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_Detail_Book");

            entity.HasOne(d => d.Ticket).WithMany(p => p.BorrowTicketDetails)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_Detail_Ticket");
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("PK__Reader__8E67A5E18594DEBF");

            entity.ToTable("Reader");

            entity.HasIndex(e => e.StudentCode, "UQ__Reader__1FC886045ED4B60D").IsUnique();

            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.StudentCode)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
