-- 1. TẠO CƠ SỞ DỮ LIỆU
CREATE DATABASE QuanLyThuVien;
GO
USE QuanLyThuVien;
GO

-- ====================================================================
-- II. TẠO CẤU TRÚC CÁC BẢNG (TABLES)
-- ====================================================================

-- 1. Bảng BookCategory - Danh mục sách
CREATE TABLE BookCategory (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1
);
GO

-- 2. Bảng Book - Sách
CREATE TABLE Book (
    BookId INT IDENTITY(1,1) PRIMARY KEY,
    BookCode VARCHAR(20) NOT NULL UNIQUE,
    BookTitle NVARCHAR(200) NOT NULL,
    Author NVARCHAR(150),
    CategoryId INT,
    Publisher NVARCHAR(150),
    PublishYear INT,
    Quantity INT NOT NULL DEFAULT 0,
    AvailableQuantity INT NOT NULL DEFAULT 0,
    Status NVARCHAR(30) DEFAULT 'Available', -- Available, Unavailable
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_Book_Category FOREIGN KEY (CategoryId) REFERENCES BookCategory(CategoryId),
    CONSTRAINT CHK_Quantity CHECK (Quantity >= 0),
    CONSTRAINT CHK_AvailableQuantity CHECK (AvailableQuantity <= Quantity AND AvailableQuantity >= 0)
);
GO

-- 3. Bảng Reader - Độc giả/Sinh viên
CREATE TABLE Reader (
    ReaderId INT IDENTITY(1,1) PRIMARY KEY,
    StudentCode VARCHAR(20) NOT NULL UNIQUE,
    FullName NVARCHAR(150) NOT NULL,
    ClassName NVARCHAR(50),
    PhoneNumber VARCHAR(15),
    Email NVARCHAR(150),
    IsActive BIT DEFAULT 1
);
GO

-- 4. Bảng BorrowTicket - Phiếu mượn
CREATE TABLE BorrowTicket (
    TicketId INT IDENTITY(1,1) PRIMARY KEY,
    TicketCode VARCHAR(30) NOT NULL UNIQUE,
    ReaderId INT,
    BorrowDate DATETIME NOT NULL DEFAULT GETDATE(),
    ExpectedReturnDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status NVARCHAR(30) DEFAULT 'Borrowing', -- Borrowing, Returned, Cancelled, Overdue
    Note NVARCHAR(255),
    CONSTRAINT FK_BorrowTicket_Reader FOREIGN KEY (ReaderId) REFERENCES Reader(ReaderId),
    CONSTRAINT CHK_ReturnDate CHECK (ReturnDate >= BorrowDate OR ReturnDate IS NULL)
);
GO

-- 5. Bảng BorrowTicketDetail - Chi tiết phiếu mượn
CREATE TABLE BorrowTicketDetail (
    TicketDetailId INT IDENTITY(1,1) PRIMARY KEY,
    TicketId INT,
    BookId INT,
    Quantity INT NOT NULL DEFAULT 1,
    ConditionBefore NVARCHAR(255),
    ConditionAfter NVARCHAR(255),
    CONSTRAINT FK_Detail_Ticket FOREIGN KEY (TicketId) REFERENCES BorrowTicket(TicketId),
    CONSTRAINT FK_Detail_Book FOREIGN KEY (BookId) REFERENCES Book(BookId),
    CONSTRAINT CHK_Detail_Quantity CHECK (Quantity > 0)
);
GO


-- ====================================================================
-- III. CÀI ĐẶT RÀNG BUỘC VÀ TỰ ĐỘNG HÓA (TRIGGERS)
-- ====================================================================

-- Trigger 1: Tự động cập nhật số lượng sách khả dụng (AvailableQuantity) khi có chi tiết phiếu mượn mới
-- Đồng thời kiểm tra không cho mượn nếu vượt quá số lượng sách hiện có.
CREATE TRIGGER trg_BorrowTicketDetail_Insert
ON BorrowTicketDetail
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra điều kiện đủ sách mượn
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        JOIN Book b ON i.BookId = b.BookId
        WHERE b.AvailableQuantity < i.Quantity OR b.Status = 'Unavailable' OR b.IsDeleted = 1
    )
    BEGIN
        RAISERROR (N'Lỗi: Sách đã hết số lượng khả dụng hoặc không sẵn sàng để mượn!', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- Cập nhật giảm số lượng khả dụng của sách
    UPDATE Book
    SET AvailableQuantity = Book.AvailableQuantity - i.Quantity,
        Status = CASE WHEN (Book.AvailableQuantity - i.Quantity) = 0 THEN 'Unavailable' ELSE 'Available' END
    FROM Book
    JOIN inserted i ON Book.BookId = i.BookId;
END;
GO

-- Trigger 2: Ngăn chặn xóa sách đang trong quá trình được mượn (Chưa trả)
CREATE TRIGGER trg_Book_DeleteSoft
ON Book
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Nếu cập nhật chuyển trạng thái IsDeleted từ 0 sang 1 (Xóa mềm)
    IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.BookId = d.BookId WHERE i.IsDeleted = 1 AND d.IsDeleted = 0)
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM inserted i
            JOIN BorrowTicketDetail btd ON i.BookId = btd.BookId
            JOIN BorrowTicket bt ON btd.TicketId = bt.TicketId
            WHERE bt.Status = 'Borrowing' OR bt.Status = 'Overdue'
        )
        BEGIN
            RAISERROR (N'Lỗi: Không thể xóa sách đang nằm trong phiếu mượn chưa hoàn trả!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END;
GO


-- ====================================================================
-- IV. CÁC THỦ TỤC LƯU TRỮ (STORED PROCEDURES)
-- ====================================================================

-- Procedure 1: Dashboard Thống kê
CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Cập nhật trạng thái Quá hạn tự động trước khi thống kê dựa theo thời gian hiện tại
    UPDATE BorrowTicket 
    SET Status = 'Overdue' 
    WHERE Status = 'Borrowing' AND ExpectedReturnDate < GETDATE();

    SELECT 
        (SELECT COUNT(*) FROM Book WHERE IsDeleted = 0) AS TotalBooks,
        (SELECT SUM(AvailableQuantity) FROM Book WHERE IsDeleted = 0) AS AvailableBooks,
        (SELECT SUM(Quantity - AvailableQuantity) FROM Book WHERE IsDeleted = 0) AS BorrowedBooks,
        (SELECT COUNT(*) FROM BorrowTicket WHERE Status = 'Borrowing') AS ActiveTickets,
        (SELECT COUNT(*) FROM BorrowTicket WHERE Status = 'Returned') AS ReturnedTickets,
        (SELECT COUNT(*) FROM BorrowTicket WHERE Status = 'Overdue') AS OverdueTickets;
END;
GO

-- Procedure 2: Xác nhận trả sách
CREATE PROCEDURE sp_ConfirmReturnBook
    @TicketId INT,
    @ConditionAfter NVARCHAR(255) = N'Bình thường',
    @Note NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Kiểm tra trạng thái phiếu mượn
    DECLARE @CurrentStatus NVARCHAR(30);
    SELECT @CurrentStatus = Status FROM BorrowTicket WHERE TicketId = @TicketId;
    
    IF @CurrentStatus = 'Returned'
    BEGIN
        RAISERROR (N'Lỗi: Phiếu mượn này đã được hoàn trả trước đó rồi!', 16, 1);
        RETURN;
    END
    IF @CurrentStatus = 'Cancelled'
    BEGIN
        RAISERROR (N'Lỗi: Phiếu mượn này đã bị hủy bỏ!', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 2. Cập nhật ngày trả thực tế và trạng thái phiếu mượn
        UPDATE BorrowTicket
        SET ReturnDate = GETDATE(),
            Status = 'Returned',
            Note = COALESCE(@Note, Note)
        WHERE TicketId = @TicketId;

        -- 3. Cập nhật tình trạng sách sau khi trả trong bảng chi tiết
        UPDATE BorrowTicketDetail
        SET ConditionAfter = @ConditionAfter
        WHERE TicketId = @TicketId;

        -- 4. Hoàn trả lại số lượng khả dụng cho sách
        UPDATE Book
        SET Book.AvailableQuantity = Book.AvailableQuantity + btd.Quantity,
            Book.Status = 'Available'
        FROM Book
        JOIN BorrowTicketDetail btd ON Book.BookId = btd.BookId
        WHERE btd.TicketId = @TicketId;

        COMMIT TRANSACTION;
        PRINT N'Xác nhận trả sách thành công!';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END;
GO

-- Procedure 3: Tìm kiếm và lọc Sách
CREATE PROCEDURE sp_SearchBooks
    @Keyword NVARCHAR(100) = NULL,
    @CategoryId INT = NULL,
    @Status NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT b.BookId, b.BookCode, b.BookTitle, b.Author, bc.CategoryName, b.Publisher, b.PublishYear, b.Quantity, b.AvailableQuantity, b.Status
    FROM Book b
    LEFT JOIN BookCategory bc ON b.CategoryId = bc.CategoryId
    WHERE b.IsDeleted = 0
      AND (@Keyword IS NULL OR b.BookTitle LIKE '%' + @Keyword + '%' OR b.BookCode LIKE '%' + @Keyword + '%' OR b.Author LIKE '%' + @Keyword + '%')
      AND (@CategoryId IS NULL OR b.CategoryId = @CategoryId)
      AND (@Status IS NULL OR b.Status = @Status);
END;
GO


-- ====================================================================
-- V. CHÈN DỮ LIỆU MẪU (INSERT SAMPLE DATA) - Mỗi bảng 5 bản ghi
-- ====================================================================

-- 1. Insert BookCategory
INSERT INTO BookCategory (CategoryName, Description, IsActive) VALUES
(N'Giáo trình', N'Sách giáo trình giảng dạy chính quy tại trường', 1),
(N'Tham khảo', N'Sách tham khảo mở rộng kiến thức chuyên sâu', 1),
(N'Ngoại ngữ', N'Sách học tiếng Anh, Nhật, Trung, Hàn...', 1),
(N'Lập trình', N'Sách hướng dẫn phát triển phần mềm và công nghệ', 1),
(N'Kỹ năng sống', N'Sách phát triển kỹ năng mềm và tư duy', 1);

-- 2. Insert Book (Ban đầu Quantity = AvailableQuantity)
INSERT INTO Book (BookCode, BookTitle, Author, CategoryId, Publisher, PublishYear, Quantity, AvailableQuantity, Status, IsDeleted) VALUES
('BOOK001', N'Giáo trình Cơ sở dữ liệu', N'Nguyễn Văn A', 1, N'NXB Giáo dục', 2022, 10, 10, 'Available', 0),
('BOOK002', N'Lập trình C# nâng cao', N'Trần Văn B', 4, N'NXB Khoa học kỹ thuật', 2023, 5, 5, 'Available', 0),
('BOOK003', N'English Grammar in Use', N'Raymond Murphy', 3, N'Cambridge', 2021, 15, 15, 'Available', 0),
('BOOK004', N'Đắc Nhân Tâm', N'Dale Carnegie', 5, N'NXB Trẻ', 2020, 8, 8, 'Available', 0),
('BOOK005', N'Cấu trúc dữ liệu và Giải thuật', N'Phạm Thế C', 4, N'NXB Thông tin truyền thông', 2024, 7, 7, 'Available', 0);

-- 3. Insert Reader
INSERT INTO Reader (StudentCode, FullName, ClassName, PhoneNumber, Email, IsActive) VALUES
('SV001', N'Lê Minh Hoàng', 'IT01', '0912345678', 'hoanglm@school.edu.vn', 1),
('SV002', N'Nguyễn Thị Mai', 'BA02', '0987654321', 'maint@school.edu.vn', 1),
('SV003', N'Trần Bảo Long', 'IT02', '0933334444', 'longtb@school.edu.vn', 1),
('SV004', N'Phạm Thu Hà', 'ENG01', '0944445555', 'hapt@school.edu.vn', 1),
('SV005', N'Vũ Hoàng Nam', 'MKT01', '0955556666', 'namvh@school.edu.vn', 1);

-- 4. Insert BorrowTicket
INSERT INTO BorrowTicket (TicketCode, ReaderId, BorrowDate, ExpectedReturnDate, ReturnDate, Status, Note) VALUES
('TK001', 1, '2026-05-20 08:00:00', '2026-06-03 17:00:00', NULL, 'Borrowing', N'Sinh viên mượn học kỳ mới'),
('TK002', 2, '2026-05-15 09:30:00', '2026-05-25 17:00:00', '2026-05-24 15:00:00', 'Returned', N'Trả đúng hạn'),
('TK003', 3, '2026-05-01 10:00:00', '2026-05-15 17:00:00', NULL, 'Overdue', N'Quá hạn chưa thấy liên lạc'),
('TK004', 4, '2026-05-28 14:00:00', '2026-06-11 17:00:00', NULL, 'Borrowing', N'Mượn tài liệu ôn thi'),
('TK005', 5, '2026-05-29 16:20:00', '2026-06-12 17:00:00', NULL, 'Borrowing', N'Mượn sách đọc thêm');

-- 5. Insert BorrowTicketDetail
-- Lưu ý: Khi insert vào đây bằng tay, chúng ta cần chủ động UPDATE bớt AvailableQuantity của bảng Book 
-- để mô phỏng tính đúng đắn trước khi Trigger hoạt động ở các phiên nhập mới tiếp theo.
INSERT INTO BorrowTicketDetail (TicketId, BookId, Quantity, ConditionBefore, ConditionAfter) VALUES
(1, 1, 1, N'Sách mới, sạch đẹp', NULL),
(1, 2, 1, N'Sách hơi cũ, có vết xước nhẹ', NULL),
(2, 3, 2, N'Sách mới', N'Sách bình thường không rách nát'),
(3, 5, 1, N'Sách mới', NULL),
(4, 4, 1, N'Sách mới góc trang hơi nhăn', NULL);

-- Đồng bộ cập nhật lại số lượng sách sẵn có cho đúng với 5 dòng dữ liệu mẫu đã mượn ở trên:
UPDATE Book SET AvailableQuantity = 9 WHERE BookId = 1;
UPDATE Book SET AvailableQuantity = 4 WHERE BookId = 2;
-- BookId = 3 đã được trả ở phiếu TK002 nên giữ nguyên số lượng gốc.
UPDATE Book SET AvailableQuantity = 6 WHERE BookId = 5;
UPDATE Book SET AvailableQuantity = 7 WHERE BookId = 4;
GO

-- ====================================================================
-- VI. HƯỚNG DẪN KIỂM TRA CHỨC NĂNG (TESTING)
-- ====================================================================

-- 1. Xem Thống kê Dashboard
-- EXEC sp_GetDashboardStats;

-- 2. Tìm kiếm sách có từ khóa 'Cơ sở dữ liệu'
-- EXEC sp_SearchBooks @Keyword = N'Cơ sở dữ liệu';

-- 3. Gọi thủ tục Trả Sách cho phiếu mượn ID = 1 (TK001)
-- EXEC sp_ConfirmReturnBook @TicketId = 1, @ConditionAfter = N'Sách nguyên vẹn', @Note = N'Sinh viên trả đầy đủ';

-- 4. Thử kiểm tra Trigger chặn mượn quá số lượng khả dụng (Sách ID = 2 hiện tại còn 4 cuốn, thử mượn 10 cuốn sẽ báo lỗi)
-- INSERT INTO BorrowTicketDetail (TicketId, BookId, Quantity, ConditionBefore) VALUES (5, 2, 10, N'Mới');