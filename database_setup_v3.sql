-- ============================================================
--  HutechStore - Database Setup Script v3
--  SQL Server Management Studio 2022
--  v3 changes: Added PaymentPaidAt, SepayTransactionId to Orders
--              (for Sepay bank transfer webhook integration)
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'HutechStore')
    DROP DATABASE HutechStore;
GO

CREATE DATABASE HutechStore COLLATE Vietnamese_CI_AS;
GO
USE HutechStore;
GO

-- ============================================================
--  TABLES
-- ============================================================

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'user',
    GoogleId NVARCHAR(100) NULL,
    Avatar NVARCHAR(255) NULL,
    AuthProvider NVARCHAR(20) NOT NULL DEFAULT 'local',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_GoogleId ON Users(GoogleId);

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    Status INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    Image NVARCHAR(500) NULL,
    CategoryId INT NOT NULL,
    Brand NVARCHAR(50) NOT NULL DEFAULT '',
    Slug NVARCHAR(250) NULL UNIQUE,
    Status INT NOT NULL DEFAULT 1,
    Featured INT NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
CREATE INDEX IX_Products_Brand ON Products(Brand);

CREATE TABLE PhoneSpecs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL UNIQUE,
    Chipset NVARCHAR(100) NULL,
    RamGb INT NOT NULL DEFAULT 0,
    StorageGb INT NOT NULL DEFAULT 0,
    DisplaySize NVARCHAR(50) NULL,
    DisplayType NVARCHAR(100) NULL,
    RefreshRate INT NOT NULL DEFAULT 60,
    MainCamera NVARCHAR(150) NULL,
    FrontCamera NVARCHAR(100) NULL,
    BatteryMah INT NOT NULL DEFAULT 0,
    FastCharge BIT NOT NULL DEFAULT 0,
    WirelessCharge BIT NOT NULL DEFAULT 0,
    Has5G BIT NOT NULL DEFAULT 1,
    HasNfc BIT NOT NULL DEFAULT 1,
    Os NVARCHAR(50) NULL,
    Colors NVARCHAR(500) NULL,
    Dimensions NVARCHAR(100) NULL,
    WeightGrams INT NOT NULL DEFAULT 0,
    ReleaseYear INT NOT NULL DEFAULT 2024,
    CONSTRAINT FK_PhoneSpecs_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE ProductVariants (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Color NVARCHAR(50) NULL,
    StorageGb INT NOT NULL DEFAULT 0,
    RamGb INT NOT NULL DEFAULT 0,
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Variants_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE UseCaseTags (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    Tag NVARCHAR(50) NOT NULL,
    Score INT NOT NULL DEFAULT 8,
    CONSTRAINT FK_Tags_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Tag_Product UNIQUE (ProductId, Tag)
);

CREATE TABLE ProductImages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsMain BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Images_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

CREATE TABLE Reviews (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Review_UserProduct UNIQUE (ProductId, UserId)
);

CREATE TABLE Carts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    SessionId NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

CREATE TABLE CartItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    VariantId INT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    Price DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id)
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(18,2) NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    ShippingPhone NVARCHAR(20) NOT NULL,
    ShippingName NVARCHAR(100) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'cod',
    Note NVARCHAR(500) NULL,
    CouponCode NVARCHAR(50) NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    -- pending | confirmed | shipping | delivered | cancelled
    Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
    -- Bank transfer tracking (Sepay webhook)
    PaymentPaidAt DATETIME2 NULL,
    SepayTransactionId NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    VariantId INT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_OrderItems_Variants FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id)
);

CREATE TABLE Wishlists (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Wishlists_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlists_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Wishlist UNIQUE (UserId, ProductId)
);

CREATE TABLE ChatHistories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SessionId NVARCHAR(100) NOT NULL,
    UserId INT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'user',
    Message NVARCHAR(MAX) NOT NULL,
    ProductsMentioned NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ChatHistories_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
CREATE INDEX IX_ChatHistories_SessionId ON ChatHistories(SessionId);

CREATE TABLE Coupons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Type NVARCHAR(20) NOT NULL DEFAULT 'percent',
    Value DECIMAL(18,2) NOT NULL,
    MinOrder DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxDiscount DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxUsage INT NOT NULL DEFAULT 0,
    UsedCount INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 1,
    ExpiresAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE PasswordOtps (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL,
    Otp NVARCHAR(6) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============================================================
--  SEED DATA
-- ============================================================

INSERT INTO Users (Name, Email, Password, Role)
VALUES (N'Admin', 'admin@hutechstore.com', 'Admin@123', 'admin');

INSERT INTO Categories (Name, Description) VALUES
    (N'iPhone',  N'Điện thoại Apple iPhone chính hãng'),
    (N'Samsung', N'Điện thoại Samsung Galaxy chính hãng');

-- ============================================================
--  IPHONE
-- ============================================================

-- 1. iPhone 16e
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 16e',N'Chip A16 Bionic, camera 48MP, pin lâu, giá tốt nhất dòng iPhone 16.',16990000,'/images/products/iphone-16e.jpg',1,'Apple','iphone-16e',1,0,50);
DECLARE @p1 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p1,'Apple A16 Bionic',8,128,'6.1 inch','Super Retina XDR OLED',60,'48MP f/1.78 + 12MP ultrawide','12MP TrueDepth',3279,1,1,1,1,'iOS 18',N'Đen,Trắng,Hồng','138.8 x 67.2 x 7.8 mm',167,2025);
INSERT INTO ProductVariants VALUES(@p1,N'Đen',128,8,16990000,30,1),(@p1,N'Trắng',128,8,16990000,10,1),(@p1,N'Hồng',128,8,16990000,10,1),(@p1,N'Đen',256,8,19490000,20,1),(@p1,N'Trắng',256,8,19490000,10,1);
INSERT INTO UseCaseTags VALUES(@p1,'student',9),(@p1,'value',9),(@p1,'camera',7),(@p1,'business',7);

-- 2. iPhone 15
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 15',N'Dynamic Island, chip A16, camera 48MP, USB-C. Lựa chọn hoàn hảo tầm trung cao.',19990000,'/images/products/iphone-15.jpg',1,'Apple','iphone-15',1,0,40);
DECLARE @p2 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p2,'Apple A16 Bionic',6,128,'6.1 inch','Super Retina XDR OLED',60,'48MP f/1.6 + 12MP ultrawide','12MP TrueDepth',3877,1,1,1,1,'iOS 18',N'Đen,Xanh,Vàng,Hồng,Xanh lá','147.6 x 71.5 x 7.8 mm',171,2023);
INSERT INTO ProductVariants VALUES(@p2,N'Đen',128,6,19990000,15,1),(@p2,N'Xanh',128,6,19990000,10,1),(@p2,N'Vàng',256,6,23490000,10,1),(@p2,N'Đen',256,6,23490000,5,1);
INSERT INTO UseCaseTags VALUES(@p2,'student',8),(@p2,'camera',8),(@p2,'business',7),(@p2,'value',8);

-- 3. iPhone 15 Plus
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 15 Plus',N'Màn hình 6.7 inch, pin trâu nhất dòng 15, lý tưởng cho người thích màn lớn.',24490000,'/images/products/iphone-15-plus.jpg',1,'Apple','iphone-15-plus',1,0,25);
DECLARE @p3 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p3,'Apple A16 Bionic',6,128,'6.7 inch','Super Retina XDR OLED',60,'48MP f/1.6 + 12MP ultrawide','12MP TrueDepth',4383,1,1,1,1,'iOS 18',N'Đen,Xanh,Vàng,Hồng,Xanh lá','160.9 x 77.8 x 7.8 mm',201,2023);
INSERT INTO ProductVariants VALUES(@p3,N'Đen',128,6,24490000,10,1),(@p3,N'Xanh',128,6,24490000,8,1),(@p3,N'Đen',256,6,28490000,7,1);
INSERT INTO UseCaseTags VALUES(@p3,'battery_life',9),(@p3,'student',7),(@p3,'business',7),(@p3,'gaming',6);

-- 4. iPhone 15 Pro
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 15 Pro',N'Chip A17 Pro, khung Titan, camera 48MP ProRAW, Action Button. Dành cho người dùng chuyên nghiệp.',27990000,'/images/products/iphone-15-pro.jpg',1,'Apple','iphone-15-pro',1,1,35);
DECLARE @p4 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p4,'Apple A17 Pro',8,128,'6.1 inch','Super Retina XDR ProMotion OLED',120,'48MP f/1.78 + 12MP ultrawide + 12MP 3x tele','12MP TrueDepth',3274,1,1,1,1,'iOS 18',N'Đen Titan,Trắng Titan,Xanh Titan,Tự Nhiên Titan','146.6 x 70.6 x 8.25 mm',187,2023);
INSERT INTO ProductVariants VALUES(@p4,N'Đen Titan',128,8,27990000,10,1),(@p4,N'Trắng Titan',256,8,32490000,10,1),(@p4,N'Xanh Titan',256,8,32490000,8,1),(@p4,N'Tự Nhiên Titan',512,8,38990000,7,1);
INSERT INTO UseCaseTags VALUES(@p4,'camera',10),(@p4,'business',9),(@p4,'gaming',8),(@p4,'value',6);

-- 5. iPhone 15 Pro Max
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 15 Pro Max',N'Camera 5x Tetraprism zoom quang học, màn 6.7 inch ProMotion 120Hz, A17 Pro đỉnh cao.',34990000,'/images/products/iphone-15-pro-max.jpg',1,'Apple','iphone-15-pro-max',1,1,20);
DECLARE @p5 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p5,'Apple A17 Pro',8,256,'6.7 inch','Super Retina XDR ProMotion OLED',120,'48MP f/1.78 + 12MP ultrawide + 12MP 5x tele','12MP TrueDepth',4422,1,1,1,1,'iOS 18',N'Đen Titan,Trắng Titan,Xanh Titan,Tự Nhiên Titan','159.9 x 76.7 x 8.25 mm',221,2023);
INSERT INTO ProductVariants VALUES(@p5,N'Đen Titan',256,8,34990000,8,1),(@p5,N'Trắng Titan',256,8,34990000,5,1),(@p5,N'Xanh Titan',512,8,41990000,4,1),(@p5,N'Tự Nhiên Titan',1024,8,49490000,3,1);
INSERT INTO UseCaseTags VALUES(@p5,'camera',10),(@p5,'business',10),(@p5,'gaming',9),(@p5,'battery_life',8);

-- 6. iPhone 16
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 16',N'Chip A18, Apple Intelligence, Camera Control mới, sạc MagSafe cải tiến. Lựa chọn tốt nhất tầm giá trung cao.',22990000,'/images/products/iphone-16.jpg',1,'Apple','iphone-16',1,1,60);
DECLARE @p6 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p6,'Apple A18',8,128,'6.1 inch','Super Retina XDR OLED',60,'48MP f/1.6 + 12MP ultrawide','12MP TrueDepth',3561,1,1,1,1,'iOS 18',N'Đen,Trắng,Hồng,Mòng két,Xanh da trời','147.6 x 71.5 x 7.8 mm',170,2024);
INSERT INTO ProductVariants VALUES(@p6,N'Đen',128,8,22990000,20,1),(@p6,N'Trắng',128,8,22990000,15,1),(@p6,N'Hồng',128,8,22990000,10,1),(@p6,N'Đen',256,8,26490000,10,1),(@p6,N'Mòng két',256,8,26490000,5,1);
INSERT INTO UseCaseTags VALUES(@p6,'business',9),(@p6,'camera',8),(@p6,'student',8),(@p6,'gaming',7),(@p6,'value',8);

-- 7. iPhone 16 Plus
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 16 Plus',N'Apple Intelligence trên màn 6.7 inch, pin cực trâu 4674mAh, sạc nhanh 25W.',27990000,'/images/products/iphone-16-plus.jpg',1,'Apple','iphone-16-plus',1,0,30);
DECLARE @p7 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p7,'Apple A18',8,128,'6.7 inch','Super Retina XDR OLED',60,'48MP f/1.6 + 12MP ultrawide','12MP TrueDepth',4674,1,1,1,1,'iOS 18',N'Đen,Trắng,Hồng,Mòng két,Xanh da trời','160.9 x 77.8 x 7.8 mm',203,2024);
INSERT INTO ProductVariants VALUES(@p7,N'Đen',128,8,27990000,12,1),(@p7,N'Trắng',128,8,27990000,8,1),(@p7,N'Đen',256,8,31990000,10,1);
INSERT INTO UseCaseTags VALUES(@p7,'battery_life',10),(@p7,'business',8),(@p7,'gaming',6),(@p7,'student',7);

-- 8. iPhone 16 Pro
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 16 Pro',N'A18 Pro, màn 6.3 inch ProMotion 120Hz, camera 48MP + 5x zoom, quay 4K 120fps ProRes. Flagship đỉnh 2024.',33990000,'/images/products/iphone-16-pro.jpg',1,'Apple','iphone-16-pro',1,1,45);
DECLARE @p8 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p8,'Apple A18 Pro',8,128,'6.3 inch','Super Retina XDR ProMotion OLED',120,'48MP f/1.78 + 48MP ultrawide + 12MP 5x tele','12MP TrueDepth',3582,1,1,1,1,'iOS 18',N'Đen Titan,Trắng Titan,Sa mạc Titan,Tự Nhiên Titan','149.6 x 71.5 x 8.25 mm',199,2024);
INSERT INTO ProductVariants VALUES(@p8,N'Đen Titan',128,8,33990000,15,1),(@p8,N'Trắng Titan',256,8,38990000,12,1),(@p8,N'Sa mạc Titan',256,8,38990000,8,1),(@p8,N'Tự Nhiên Titan',512,8,45990000,6,1),(@p8,N'Đen Titan',1024,8,54490000,4,1);
INSERT INTO UseCaseTags VALUES(@p8,'camera',10),(@p8,'gaming',9),(@p8,'business',10),(@p8,'battery_life',7);

-- 9. iPhone 16 Pro Max
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'iPhone 16 Pro Max',N'Đỉnh tuyệt đối 2024. Màn 6.9 inch lớn nhất, pin 4685mAh, A18 Pro siêu mạnh, camera 5x ProRes Cinema.',39990000,'/images/products/iphone-16-pro-max.jpg',1,'Apple','iphone-16-pro-max',1,1,30);
DECLARE @p9 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p9,'Apple A18 Pro',8,256,'6.9 inch','Super Retina XDR ProMotion OLED',120,'48MP f/1.78 + 48MP ultrawide + 12MP 5x tele','12MP TrueDepth',4685,1,1,1,1,'iOS 18',N'Đen Titan,Trắng Titan,Sa mạc Titan,Tự Nhiên Titan','163 x 77.6 x 8.25 mm',227,2024);
INSERT INTO ProductVariants VALUES(@p9,N'Đen Titan',256,8,39990000,10,1),(@p9,N'Trắng Titan',256,8,39990000,8,1),(@p9,N'Sa mạc Titan',512,8,46990000,6,1),(@p9,N'Tự Nhiên Titan',1024,8,55490000,4,1);
INSERT INTO UseCaseTags VALUES(@p9,'camera',10),(@p9,'gaming',10),(@p9,'business',10),(@p9,'battery_life',9);

-- ============================================================
--  SAMSUNG
-- ============================================================

-- 10. Samsung Galaxy A35 5G
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy A35 5G',N'Thiết kế cao cấp tầm trung, Super AMOLED 120Hz, camera 50MP, pin 5000mAh. 5G giá tốt nhất.',8490000,'/images/products/samsung-a35.jpg',2,'Samsung','samsung-galaxy-a35-5g',1,0,80);
DECLARE @p10 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p10,'Exynos 1380',6,128,'6.6 inch','Super AMOLED FHD+ 120Hz',120,'50MP f/1.8 + 8MP ultrawide + 5MP macro','13MP',5000,1,0,1,1,'Android 14 / One UI 6.1',N'Xanh nhung,Đen ánh bạc,Oải hương','161.7 x 78 x 8.2 mm',210,2024);
INSERT INTO ProductVariants VALUES(@p10,N'Xanh nhung',128,6,8490000,30,1),(@p10,N'Đen ánh bạc',128,6,8490000,25,1),(@p10,N'Oải hương',128,6,8490000,15,1),(@p10,N'Xanh nhung',256,8,9990000,10,1);
INSERT INTO UseCaseTags VALUES(@p10,'student',10),(@p10,'value',10),(@p10,'battery_life',8),(@p10,'camera',6);

-- 11. Samsung Galaxy A55 5G
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy A55 5G',N'Flagship tầm trung 2024. Khung nhôm sang trọng, AMOLED 120Hz, camera OIS 50MP, pin 5000mAh sạc 45W.',11990000,'/images/products/samsung-a55.jpg',2,'Samsung','samsung-galaxy-a55-5g',1,1,65);
DECLARE @p11 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p11,'Exynos 1480',8,128,'6.6 inch','Super AMOLED FHD+ 120Hz',120,'50MP OIS f/1.8 + 12MP ultrawide + 5MP macro','32MP',5000,1,0,1,1,'Android 14 / One UI 6.1',N'Xanh sapphire,Đen ánh bạc,Tím','161.7 x 77.4 x 8.2 mm',213,2024);
INSERT INTO ProductVariants VALUES(@p11,N'Xanh sapphire',128,8,11990000,25,1),(@p11,N'Đen ánh bạc',128,8,11990000,20,1),(@p11,N'Tím',256,8,13990000,10,1),(@p11,N'Xanh sapphire',256,8,13990000,10,1);
INSERT INTO UseCaseTags VALUES(@p11,'student',9),(@p11,'camera',8),(@p11,'battery_life',9),(@p11,'business',7),(@p11,'value',9);

-- 12. Samsung Galaxy S24 FE
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy S24 FE',N'Fan Edition với Exynos 2500, Galaxy AI, Dynamic AMOLED 120Hz, camera 50MP bộ 3. Trải nghiệm Galaxy S giá tốt.',16990000,'/images/products/samsung-s24-fe.jpg',2,'Samsung','samsung-galaxy-s24-fe',1,1,45);
DECLARE @p12 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p12,'Exynos 2500',8,128,'6.7 inch','Dynamic AMOLED 2X FHD+ 120Hz',120,'50MP OIS f/1.8 + 8MP ultrawide + 8MP 3x tele','10MP',4700,1,1,1,1,'Android 15 / One UI 7',N'Xanh băng,Xám,Bạc,Vàng','162.1 x 77.3 x 8 mm',213,2024);
INSERT INTO ProductVariants VALUES(@p12,N'Xanh băng',128,8,16990000,15,1),(@p12,N'Xám',128,8,16990000,15,1),(@p12,N'Vàng',256,8,19490000,10,1),(@p12,N'Bạc',256,8,19490000,5,1);
INSERT INTO UseCaseTags VALUES(@p12,'value',9),(@p12,'camera',7),(@p12,'gaming',7),(@p12,'business',8),(@p12,'student',8);

-- 13. Samsung Galaxy S25
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy S25',N'Snapdragon 8 Elite mạnh nhất Android, Galaxy AI toàn diện, camera 50MP tinh tế, thiết kế mỏng nhẹ.',22990000,'/images/products/samsung-s25.jpg',2,'Samsung','samsung-galaxy-s25',1,1,55);
DECLARE @p13 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p13,'Snapdragon 8 Elite',12,128,'6.2 inch','Dynamic AMOLED 2X QHD+ 120Hz',120,'50MP OIS f/1.8 + 12MP ultrawide + 10MP 3x tele','12MP',4000,1,1,1,1,'Android 15 / One UI 7.1',N'Xanh băng,Xám bạc,Trắng bạc,Vàng hoa anh thảo','146.9 x 70.5 x 7.2 mm',162,2025);
INSERT INTO ProductVariants VALUES(@p13,N'Xanh băng',128,12,22990000,20,1),(@p13,N'Xám bạc',128,12,22990000,15,1),(@p13,N'Trắng bạc',256,12,26490000,10,1),(@p13,N'Xanh băng',256,12,26490000,10,1);
INSERT INTO UseCaseTags VALUES(@p13,'gaming',9),(@p13,'business',9),(@p13,'camera',8),(@p13,'student',7),(@p13,'value',7);

-- 14. Samsung Galaxy S25+
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy S25+',N'Màn 6.7 inch QHD+ rộng rãi, Snapdragon 8 Elite, pin 4900mAh sạc 45W, bộ 3 camera nâng cấp.',28490000,'/images/products/samsung-s25-plus.jpg',2,'Samsung','samsung-galaxy-s25-plus',1,1,35);
DECLARE @p14 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p14,'Snapdragon 8 Elite',12,256,'6.7 inch','Dynamic AMOLED 2X QHD+ 120Hz',120,'50MP OIS f/1.8 + 12MP ultrawide + 10MP 3x tele','12MP',4900,1,1,1,1,'Android 15 / One UI 7.1',N'Xanh băng,Xám bạc,Trắng bạc,Xanh hải quân','158.4 x 75.8 x 7.3 mm',190,2025);
INSERT INTO ProductVariants VALUES(@p14,N'Xanh băng',256,12,28490000,12,1),(@p14,N'Xám bạc',256,12,28490000,10,1),(@p14,N'Trắng bạc',512,12,33990000,8,1),(@p14,N'Xanh hải quân',512,12,33990000,5,1);
INSERT INTO UseCaseTags VALUES(@p14,'gaming',9),(@p14,'business',10),(@p14,'camera',8),(@p14,'battery_life',8);

-- 15. Samsung Galaxy S25 Ultra
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy S25 Ultra',N'Đỉnh cao Android 2025. S Pen tích hợp, camera 200MP 50x zoom, màn 6.9 inch Titanium, Snapdragon 8 Elite.',35990000,'/images/products/samsung-s25-ultra.jpg',2,'Samsung','samsung-galaxy-s25-ultra',1,1,25);
DECLARE @p15 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p15,'Snapdragon 8 Elite',12,256,'6.9 inch','Dynamic AMOLED 2X QHD+ 120Hz',120,'200MP OIS f/1.7 + 50MP ultrawide + 10MP 3x + 50MP 5x','12MP',5000,1,1,1,1,'Android 15 / One UI 7.1',N'Đen Titan,Xám Titan,Trắng Titan,Xanh Titan','162.8 x 77.6 x 8.2 mm',218,2025);
INSERT INTO ProductVariants VALUES(@p15,N'Đen Titan',256,12,35990000,8,1),(@p15,N'Xám Titan',256,12,35990000,6,1),(@p15,N'Trắng Titan',512,12,42990000,5,1),(@p15,N'Xanh Titan',512,12,42990000,4,1),(@p15,N'Đen Titan',1024,12,50990000,2,1);
INSERT INTO UseCaseTags VALUES(@p15,'camera',10),(@p15,'gaming',10),(@p15,'business',10),(@p15,'battery_life',9);

-- 16. Samsung Galaxy Z Flip 6
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy Z Flip 6',N'Điện thoại gập thời thượng 2024. Snapdragon 8 Gen 3, màn phụ FlexWindow 3.4 inch, camera 50MP AI.',24990000,'/images/products/samsung-z-flip6.jpg',2,'Samsung','samsung-galaxy-z-flip6',1,1,20);
DECLARE @p16 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p16,'Snapdragon 8 Gen 3',12,256,'6.7 inch + 3.4 inch','Dynamic AMOLED 2X 120Hz',120,'50MP OIS f/1.7 + 12MP ultrawide','10MP',4000,1,1,1,1,'Android 14 / One UI 6.1',N'Bạc bóng,Vàng vangarde,Xanh craft,Trắng peach,Xanh mint','85.1 x 71.9 x 14.9 mm (gập)',187,2024);
INSERT INTO ProductVariants VALUES(@p16,N'Bạc bóng',256,12,24990000,6,1),(@p16,N'Vàng vangarde',256,12,24990000,5,1),(@p16,N'Xanh craft',256,12,24990000,5,1),(@p16,N'Xanh mint',512,12,28990000,4,1);
INSERT INTO UseCaseTags VALUES(@p16,'business',9),(@p16,'camera',7),(@p16,'student',6),(@p16,'value',5);

-- 17. Samsung Galaxy Z Fold 6
INSERT INTO Products (Name,Description,Price,Image,CategoryId,Brand,Slug,Status,Featured,Stock) VALUES
(N'Samsung Galaxy Z Fold 6',N'Điện thoại gập màn lớn đỉnh nhất 2024. Màn trong 7.6 inch, Snapdragon 8 Gen 3, Galaxy AI đa nhiệm xuất sắc.',49990000,'/images/products/samsung-z-fold6.jpg',2,'Samsung','samsung-galaxy-z-fold6',1,1,10);
DECLARE @p17 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(@p17,'Snapdragon 8 Gen 3',12,256,'7.6 inch / 6.3 inch','Dynamic AMOLED 2X 120Hz',120,'50MP OIS f/1.8 + 12MP ultrawide + 10MP 3x','10MP + 4MP under display',4400,1,1,1,1,'Android 14 / One UI 6.1',N'Đen bóng,Xám bạc,Trắng hồng','153.5 x 132.1 x 5.6 mm (mở)',239,2024);
INSERT INTO ProductVariants VALUES(@p17,N'Đen bóng',256,12,49990000,4,1),(@p17,N'Xám bạc',512,12,57990000,3,1),(@p17,N'Trắng hồng',512,12,57990000,3,1);
INSERT INTO UseCaseTags VALUES(@p17,'business',10),(@p17,'gaming',8),(@p17,'camera',7),(@p17,'student',5);

-- ============================================================
--  COUPONS
-- ============================================================
INSERT INTO Coupons (Code,Type,Value,MinOrder,MaxDiscount,MaxUsage,Status,ExpiresAt) VALUES
('WELCOME10','percent',10,500000,500000,1000,1,'2026-12-31'),
('SAMSUNG15','percent',15,5000000,2000000,200,1,'2026-08-31'),
('IPHONE200K','fixed',200000,10000000,200000,500,1,'2026-12-31'),
('GENZ20','percent',20,3000000,1500000,100,1,'2026-07-31');

GO
PRINT 'HutechStore v3 - Done! 17 san pham (9 iPhone + 8 Samsung) | Orders co PaymentPaidAt + SepayTransactionId';
