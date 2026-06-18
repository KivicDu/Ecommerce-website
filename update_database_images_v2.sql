USE HutechStore;
GO
-- ============================================================
--  HutechStore - Database Sync Update (V2)
--  Đồng bộ ảnh đại diện, ảnh màu chi tiết và các biến thể sản phẩm
-- ============================================================
USE HutechStore;
GO

-- 1. Xóa các bảng có khoá ngoại đến biến thể trước để tránh lỗi FK
DELETE FROM CartItems;
DELETE FROM OrderItems;
DELETE FROM ProductImages;
DELETE FROM ProductVariants;

-- 2. Cập nhật ảnh đại diện (Lineup) cho 23 sản phẩm trong bảng Products
UPDATE Products SET Image = '/images/products/iphone-16e-lineup.jpg' WHERE Id = 1;
UPDATE Products SET Image = '/images/products/iphone_15_lineup.jpg' WHERE Id = 2;
UPDATE Products SET Image = '/images/products/iphone_15_plus_lineup.jpg' WHERE Id = 3;
UPDATE Products SET Image = '/images/products/iphone-15-Pro-lineup.jpg' WHERE Id = 4;
UPDATE Products SET Image = '/images/products/iphone-15-Promax-lineup.jpg' WHERE Id = 5;
UPDATE Products SET Image = '/images/products/iphone-16-lineup.jpg' WHERE Id = 6;
UPDATE Products SET Image = '/images/products/iphone-16-plus-lineup.jpg' WHERE Id = 7;
UPDATE Products SET Image = '/images/products/iphone-16-Pro-lineup.jpg' WHERE Id = 8;
UPDATE Products SET Image = '/images/products/iphone-16-Promax-lineup.jpg' WHERE Id = 9;
UPDATE Products SET Image = '/images/products/samsung_a35_lineup.png' WHERE Id = 10;
UPDATE Products SET Image = '/images/products/samsung_a55_lineup.png' WHERE Id = 11;
UPDATE Products SET Image = '/images/products/samsung_s24_fe_lineup.png' WHERE Id = 12;
UPDATE Products SET Image = '/images/products/samsung_s25_lineup.png' WHERE Id = 13;
UPDATE Products SET Image = '/images/products/samsung_s25_plus_lineup.png' WHERE Id = 14;
UPDATE Products SET Image = '/images/products/samsung_s25_ultra_lineup.png' WHERE Id = 15;
UPDATE Products SET Image = '/images/products/samsung_zflip6_lineup.jfif' WHERE Id = 16;
UPDATE Products SET Image = '/images/products/samsung_zfold6_lineup.jfif' WHERE Id = 17;
UPDATE Products SET Image = '/images/products/iphone_17e_lineup.jpg' WHERE Id = 18;
UPDATE Products SET Image = '/images/products/iPhone-17-lineup.jpg' WHERE Id = 19;
UPDATE Products SET Image = '/images/products/iPhone-17-plus-lineup.jpg' WHERE Id = 20;
UPDATE Products SET Image = '/images/products/iphone_17_Pro_lineup.jpg' WHERE Id = 21;
UPDATE Products SET Image = '/images/products/iphone_17_Promax_lineup.jpg' WHERE Id = 22;
UPDATE Products SET Image = '/images/products/iphone_Air_lineup.jpg' WHERE Id = 23;

-- 3. Đồng bộ lại bảng PhoneSpecs.Colors tương ứng
UPDATE PhoneSpecs SET Colors = N'Đen,Trắng,Hồng' WHERE ProductId = 1;
UPDATE PhoneSpecs SET Colors = N'Đen,Xanh Dương,Xanh Lá,Hồng,Vàng' WHERE ProductId = 2;
UPDATE PhoneSpecs SET Colors = N'Đen,Xanh Dương,Xanh Lá,Hồng,Vàng' WHERE ProductId = 3;
UPDATE PhoneSpecs SET Colors = N'Đen Titan,Trắng Titan,Titan Tự Nhiên,Sa Mạc Titan' WHERE ProductId = 4;
UPDATE PhoneSpecs SET Colors = N'Đen Titan,Xanh Dương Titan,Titan Tự Nhiên,Trắng Titan' WHERE ProductId = 5;
UPDATE PhoneSpecs SET Colors = N'Đen,Hồng,Xanh Lưu Ly,Trắng' WHERE ProductId = 6;
UPDATE PhoneSpecs SET Colors = N'Đen,Hồng,Teal,Xanh Lưu Ly,Trắng' WHERE ProductId = 7;
UPDATE PhoneSpecs SET Colors = N'Đen Titan,Sa Mạc Titan,Titan Tự Nhiên,Trắng Titan' WHERE ProductId = 8;
UPDATE PhoneSpecs SET Colors = N'Đen Titan,Sa Mạc Titan,Titan Tự Nhiên,Trắng Titan' WHERE ProductId = 9;
UPDATE PhoneSpecs SET Colors = N'Xanh Đen,Vàng,Xanh Dương,Hồng' WHERE ProductId = 10;
UPDATE PhoneSpecs SET Colors = N'Xanh Đen,Vàng,Xanh Dương,Hồng' WHERE ProductId = 11;
UPDATE PhoneSpecs SET Colors = N'Xám,Xanh Dương,Xanh Lá,Vàng' WHERE ProductId = 12;
UPDATE PhoneSpecs SET Colors = N'Bạc Shadow,Xanh Băng Giá,Đen Xanh,Đỏ San Hô,Xanh Bạc Hà,Xanh Navy,Vàng Hồng' WHERE ProductId = 13;
UPDATE PhoneSpecs SET Colors = N'Bạc Shadow,Xanh Băng Giá,Đen Xanh,Đỏ San Hô,Xanh Bạc Hà,Xanh Navy,Vàng Hồng' WHERE ProductId = 14;
UPDATE PhoneSpecs SET Colors = N'Titan Xám,Titan Đen,Titan Trắng Bạc,Titan Xanh Ngọc,Titan Đen Bóng,Titan Xanh Bạc,Titan Vàng Hồng' WHERE ProductId = 15;
UPDATE PhoneSpecs SET Colors = N'Bạc Shadow,Xanh Dương,Đen Vân Cacbon,Xanh Bạc Hà,Hồng Đào,Trắng,Vàng' WHERE ProductId = 16;
UPDATE PhoneSpecs SET Colors = N'Bạc Shadow,Đen Vân Cacbon,Xanh Navy,Hồng,Trắng' WHERE ProductId = 17;
UPDATE PhoneSpecs SET Colors = N'Đen,Trắng,Hồng Phấn' WHERE ProductId = 18;
UPDATE PhoneSpecs SET Colors = N'Đen,Trắng,Xanh Sương Mù,Oải Hương,Xanh Xô Thơm' WHERE ProductId = 19;
UPDATE PhoneSpecs SET Colors = N'Đen,Trắng,Xanh Sương Mù,Oải Hương,Xanh Xô Thơm' WHERE ProductId = 20;
UPDATE PhoneSpecs SET Colors = N'Bạc,Xanh Sâu Thẳm,Cam Vũ Trụ' WHERE ProductId = 21;
UPDATE PhoneSpecs SET Colors = N'Bạc,Xanh Sâu Thẳm,Cam Vũ Trụ' WHERE ProductId = 22;
UPDATE PhoneSpecs SET Colors = N'Đen Không Gian,Bạc Ánh Trăng,Xanh Bầu Trời,Vàng Nhạt' WHERE ProductId = 23;

-- 4. Chèn lại các biến thể (ProductVariants) đầy đủ
-- Cột RamGb: 8, 12, 16. Mặc định là 0 nếu không quan trọng.

-- Id=1: iPhone 16e
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(1, N'Đen', 128, 8, 18990000, 15, 1), (1, N'Trắng', 128, 8, 18990000, 15, 1), (1, N'Hồng', 128, 8, 18990000, 10, 1),
(1, N'Đen', 256, 8, 21990000, 10, 1), (1, N'Trắng', 256, 8, 21990000, 10, 1), (1, N'Hồng', 256, 8, 21990000, 5, 1);

-- Id=2: iPhone 15
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(2, N'Đen', 128, 6, 19990000, 15, 1), (2, N'Xanh Dương', 128, 6, 19990000, 10, 1), (2, N'Xanh Lá', 128, 6, 19990000, 10, 1), (2, N'Hồng', 128, 6, 19990000, 5, 1), (2, N'Vàng', 128, 6, 19990000, 5, 1),
(2, N'Đen', 256, 6, 22990000, 10, 1), (2, N'Xanh Dương', 256, 6, 22990000, 8, 1), (2, N'Xanh Lá', 256, 6, 22990000, 8, 1);

-- Id=3: iPhone 15 Plus
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(3, N'Đen', 128, 6, 22990000, 12, 1), (3, N'Xanh Dương', 128, 6, 22990000, 10, 1), (3, N'Xanh Lá', 128, 6, 22990000, 10, 1), (3, N'Hồng', 128, 6, 22990000, 5, 1), (3, N'Vàng', 128, 6, 22990000, 5, 1),
(3, N'Đen', 256, 6, 25990000, 10, 1), (3, N'Xanh Dương', 256, 6, 25990000, 8, 1);

-- Id=4: iPhone 15 Pro
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(4, N'Đen Titan', 128, 8, 24990000, 10, 1), (4, N'Trắng Titan', 128, 8, 24990000, 10, 1), (4, N'Titan Tự Nhiên', 128, 8, 24990000, 10, 1), (4, N'Sa Mạc Titan', 128, 8, 24990000, 5, 1),
(4, N'Đen Titan', 256, 8, 27990000, 10, 1), (4, N'Trắng Titan', 256, 8, 27990000, 8, 1), (4, N'Titan Tự Nhiên', 256, 8, 27990000, 8, 1);

-- Id=5: iPhone 15 Pro Max
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(5, N'Đen Titan', 256, 8, 29990000, 12, 1), (5, N'Xanh Dương Titan', 256, 8, 29990000, 10, 1), (5, N'Titan Tự Nhiên', 256, 8, 29990000, 10, 1), (5, N'Trắng Titan', 256, 8, 29990000, 8, 1),
(5, N'Đen Titan', 512, 8, 35990000, 8, 1), (5, N'Titan Tự Nhiên', 512, 8, 35990000, 8, 1);

-- Id=6: iPhone 16
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(6, N'Đen', 128, 8, 22990000, 15, 1), (6, N'Trắng', 128, 8, 22990000, 12, 1), (6, N'Hồng', 128, 8, 22990000, 10, 1), (6, N'Xanh Lưu Ly', 128, 8, 22990000, 10, 1),
(6, N'Đen', 256, 8, 25990000, 10, 1), (6, N'Trắng', 256, 8, 25990000, 8, 1);

-- Id=7: iPhone 16 Plus
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(7, N'Đen', 128, 8, 25990000, 12, 1), (7, N'Trắng', 128, 8, 25990000, 10, 1), (7, N'Hồng', 128, 8, 25990000, 8, 1), (7, N'Teal', 128, 8, 25990000, 5, 1), (7, N'Xanh Lưu Ly', 128, 8, 25990000, 5, 1),
(7, N'Đen', 256, 8, 28990000, 10, 1), (7, N'Trắng', 256, 8, 28990000, 8, 1);

-- Id=8: iPhone 16 Pro
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(8, N'Đen Titan', 128, 8, 28990000, 10, 1), (8, N'Sa Mạc Titan', 128, 8, 28990000, 10, 1), (8, N'Titan Tự Nhiên', 128, 8, 28990000, 10, 1), (8, N'Trắng Titan', 128, 8, 28990000, 8, 1),
(8, N'Đen Titan', 256, 8, 31990000, 8, 1), (8, N'Sa Mạc Titan', 256, 8, 31990000, 8, 1);

-- Id=9: iPhone 16 Pro Max
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(9, N'Đen Titan', 256, 8, 34990000, 15, 1), (9, N'Sa Mạc Titan', 256, 8, 34990000, 12, 1), (9, N'Titan Tự Nhiên', 256, 8, 34990000, 12, 1), (9, N'Trắng Titan', 256, 8, 34990000, 10, 1),
(9, N'Đen Titan', 512, 8, 40990000, 10, 1), (9, N'Sa Mạc Titan', 512, 8, 40990000, 8, 1);

-- Id=10: Samsung Galaxy A35 5G
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(10, N'Xanh Đen', 128, 8, 7990000, 20, 1), (10, N'Vàng', 128, 8, 7990000, 15, 1), (10, N'Xanh Dương', 128, 8, 7990000, 15, 1), (10, N'Hồng', 128, 8, 7990000, 10, 1),
(10, N'Xanh Đen', 256, 8, 9490000, 10, 1), (10, N'Vàng', 256, 8, 9490000, 8, 1);

-- Id=11: Samsung Galaxy A55 5G
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(11, N'Xanh Đen', 128, 8, 9990000, 18, 1), (11, N'Vàng', 128, 8, 9990000, 12, 1), (11, N'Xanh Dương', 128, 8, 9990000, 12, 1), (11, N'Hồng', 128, 8, 9990000, 8, 1),
(11, N'Xanh Đen', 256, 8, 11490000, 10, 1), (11, N'Vàng', 256, 8, 11490000, 8, 1);

-- Id=12: Samsung Galaxy S24 FE
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(12, N'Xám', 128, 8, 14990000, 12, 1), (12, N'Xanh Dương', 128, 8, 14990000, 10, 1), (12, N'Xanh Lá', 128, 8, 14990000, 10, 1), (12, N'Vàng', 128, 8, 14990000, 5, 1),
(12, N'Xám', 256, 8, 16990000, 10, 1), (12, N'Xanh Dương', 256, 8, 16990000, 8, 1);

-- Id=13: Samsung Galaxy S25
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(13, N'Bạc Shadow', 128, 12, 21990000, 15, 1), (13, N'Xanh Băng Giá', 128, 12, 21990000, 15, 1), (13, N'Đen Xanh', 128, 12, 21990000, 10, 1), (13, N'Đỏ San Hô', 128, 12, 21990000, 5, 1),
(13, N'Xanh Bạc Hà', 128, 12, 21990000, 8, 1), (13, N'Xanh Navy', 128, 12, 21990000, 8, 1), (13, N'Vàng Hồng', 128, 12, 21990000, 8, 1),
(13, N'Bạc Shadow', 256, 12, 24990000, 10, 1), (13, N'Xanh Băng Giá', 256, 12, 24990000, 10, 1);

-- Id=14: Samsung Galaxy S25+
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(14, N'Bạc Shadow', 256, 12, 26990000, 12, 1), (14, N'Xanh Băng Giá', 256, 12, 26990000, 12, 1), (14, N'Đen Xanh', 256, 12, 26990000, 10, 1), (14, N'Đỏ San Hô', 256, 12, 26990000, 5, 1),
(14, N'Xanh Bạc Hà', 256, 12, 26990000, 8, 1), (14, N'Xanh Navy', 256, 12, 26990000, 8, 1), (14, N'Vàng Hồng', 256, 12, 26990000, 8, 1),
(14, N'Bạc Shadow', 512, 12, 30990000, 10, 1), (14, N'Xanh Băng Giá', 512, 12, 30990000, 10, 1);

-- Id=15: Samsung Galaxy S25 Ultra
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(15, N'Titan Xám', 256, 12, 34990000, 15, 1), (15, N'Titan Đen', 256, 12, 34990000, 10, 1), (15, N'Titan Trắng Bạc', 256, 12, 34990000, 10, 1), (15, N'Titan Xanh Ngọc', 256, 12, 34990000, 5, 1),
(15, N'Titan Đen Bóng', 256, 12, 34990000, 5, 1), (15, N'Titan Xanh Bạc', 256, 12, 34990000, 5, 1), (15, N'Titan Vàng Hồng', 256, 12, 34990000, 5, 1),
(15, N'Titan Xám', 512, 12, 39990000, 10, 1), (15, N'Titan Đen', 512, 12, 39990000, 8, 1),
(15, N'Titan Xám', 1024, 12, 45990000, 5, 1);

-- Id=16: Samsung Galaxy Z Flip 6
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(16, N'Bạc Shadow', 256, 12, 23990000, 15, 1), (16, N'Xanh Dương', 256, 12, 23990000, 10, 1), (16, N'Đen Vân Cacbon', 256, 12, 23990000, 10, 1), (16, N'Xanh Bạc Hà', 256, 12, 23990000, 8, 1),
(16, N'Hồng Đào', 256, 12, 23990000, 5, 1), (16, N'Trắng', 256, 12, 23990000, 5, 1), (16, N'Vàng', 256, 12, 23990000, 5, 1),
(16, N'Bạc Shadow', 512, 12, 26990000, 10, 1), (16, N'Xanh Dương', 512, 12, 26990000, 8, 1);

-- Id=17: Samsung Galaxy Z Fold 6
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(17, N'Bạc Shadow', 256, 12, 41990000, 10, 1), (17, N'Đen Vân Cacbon', 256, 12, 41990000, 8, 1), (17, N'Xanh Navy', 256, 12, 41990000, 8, 1), (17, N'Hồng', 256, 12, 41990000, 5, 1), (17, N'Trắng', 256, 12, 41990000, 5, 1),
(17, N'Bạc Shadow', 512, 12, 45990000, 8, 1), (17, N'Đen Vân Cacbon', 512, 12, 45990000, 5, 1),
(17, N'Bạc Shadow', 1024, 12, 51990000, 3, 1);

-- Id=18: iPhone 17e
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(18, N'Đen', 128, 8, 18990000, 20, 1), (18, N'Trắng', 128, 8, 18990000, 15, 1), (18, N'Hồng Phấn', 128, 8, 18990000, 15, 1),
(18, N'Đen', 256, 8, 21990000, 10, 1), (18, N'Trắng', 256, 8, 21990000, 10, 1), (18, N'Hồng Phấn', 256, 8, 21990000, 5, 1);

-- Id=19: iPhone 17
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(19, N'Đen', 128, 8, 24990000, 15, 1), (19, N'Trắng', 128, 8, 24990000, 12, 1), (19, N'Xanh Sương Mù', 128, 8, 24990000, 10, 1), (19, N'Oải Hương', 128, 8, 24990000, 8, 1), (19, N'Xanh Xô Thơm', 128, 8, 24990000, 8, 1),
(19, N'Đen', 256, 8, 28990000, 10, 1), (19, N'Trắng', 256, 8, 28990000, 8, 1), (19, N'Xanh Sương Mù', 256, 8, 28990000, 8, 1),
(19, N'Đen', 512, 8, 34990000, 5, 1);

-- Id=20: iPhone 17 Plus
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(20, N'Đen', 128, 8, 28990000, 12, 1), (20, N'Trắng', 128, 8, 28990000, 8, 1), (20, N'Xanh Sương Mù', 128, 8, 28990000, 10, 1), (20, N'Oải Hương', 128, 8, 28990000, 5, 1), (20, N'Xanh Xô Thơm', 128, 8, 28990000, 5, 1),
(20, N'Đen', 256, 8, 32990000, 10, 1), (20, N'Trắng', 256, 8, 32990000, 8, 1), (20, N'Xanh Sương Mù', 256, 8, 32990000, 8, 1),
(20, N'Đen', 512, 8, 38990000, 5, 1);

-- Id=21: iPhone 17 Pro
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(21, N'Bạc', 256, 12, 33990000, 12, 1), (21, N'Xanh Sâu Thẳm', 256, 12, 33990000, 10, 1), (21, N'Cam Vũ Trụ', 256, 12, 33990000, 8, 1),
(21, N'Bạc', 512, 12, 40990000, 8, 1), (21, N'Xanh Sâu Thẳm', 512, 12, 40990000, 8, 1), (21, N'Cam Vũ Trụ', 512, 12, 40990000, 6, 1),
(21, N'Bạc', 1024, 12, 48990000, 5, 1), (21, N'Xanh Sâu Thẳm', 1024, 12, 48990000, 5, 1);

-- Id=22: iPhone 17 Pro Max
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(22, N'Bạc', 256, 12, 41990000, 8, 1), (22, N'Xanh Sâu Thẳm', 256, 12, 41990000, 6, 1), (22, N'Cam Vũ Trụ', 256, 12, 41990000, 5, 1),
(22, N'Bạc', 512, 12, 48990000, 5, 1), (22, N'Xanh Sâu Thẳm', 512, 12, 48990000, 5, 1), (22, N'Cam Vũ Trụ', 512, 12, 48990000, 5, 1),
(22, N'Bạc', 1024, 12, 57990000, 3, 1), (22, N'Xanh Sâu Thẳm', 1024, 12, 57990000, 3, 1);

-- Id=23: iPhone Air
INSERT INTO ProductVariants (ProductId, Color, StorageGb, RamGb, Price, Stock, Status) VALUES
(23, N'Đen Không Gian', 128, 8, 27990000, 12, 1), (23, N'Bạc Ánh Trăng', 128, 8, 27990000, 10, 1), (23, N'Xanh Bầu Trời', 128, 8, 27990000, 8, 1), (23, N'Vàng Nhạt', 128, 8, 27990000, 5, 1),
(23, N'Đen Không Gian', 256, 8, 31990000, 10, 1), (23, N'Bạc Ánh Trăng', 256, 8, 31990000, 8, 1), (23, N'Xanh Bầu Trời', 256, 8, 31990000, 8, 1),
(23, N'Đen Không Gian', 512, 8, 37990000, 5, 1);


-- 5. Chèn dữ liệu ProductImages cho từng sản phẩm (bao gồm lineup và các ảnh màu chi tiết)

-- iPhone 16e (Id=1)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(1, '/images/products/iphone-16e-lineup.jpg', 0, 1),
(1, '/images/ip_16_e_details/Apple-iPhone-16e-hero-250219_inline.jpg.large_2x.jpg', 1, 0),
(1, '/images/ip_16_e_details/Apple-iPhone-16e-48MP-Fusion-photography-250219_big.jpg.large_2x.jpg', 2, 0);

-- iPhone 15 (Id=2)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(2, '/images/products/iphone_15_lineup.jpg', 0, 1),
(2, '/images/ip_15_details/refurb-iphone-15-black-202412.jfif', 1, 0),
(2, '/images/ip_15_details/refurb-iphone-15-blue-202412.jfif', 2, 0),
(2, '/images/ip_15_details/refurb-iphone-15-green-202412.jfif', 3, 0),
(2, '/images/ip_15_details/refurb-iphone-15-pink-202412.jfif', 4, 0),
(2, '/images/ip_15_details/refurb-iphone-15-yellow-202412.jfif', 5, 0),
(2, '/images/ip_15_details/Apple-iPhone-15-48MP-01-230912_big.jpg.large_2x.jpg', 6, 0),
(2, '/images/ip_15_details/Apple-iPhone-15-lineup-Dynamic-Island-incoming-call-230912.jpg', 7, 0);

-- iPhone 15 Plus (Id=3)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(3, '/images/products/iphone_15_plus_lineup.jpg', 0, 1),
(3, '/images/ip_15_plus_details/refurb-iphone-15-plus-black-202412.jfif', 1, 0),
(3, '/images/ip_15_plus_details/refurb-iphone-15-plus-blue-202412.jfif', 2, 0),
(3, '/images/ip_15_plus_details/refurb-iphone-15-plus-green-202412.jfif', 3, 0),
(3, '/images/ip_15_plus_details/refurb-iphone-15-plus-pink-202412.jfif', 4, 0),
(3, '/images/ip_15_plus_details/refurb-iphone-15-plus-yellow-202412.jfif', 5, 0),
(3, '/images/ip_15_plus_details/camera.jpg', 6, 0),
(3, '/images/ip_15_plus_details/dynamic_island.jpg', 7, 0);

-- iPhone 15 Pro (Id=4)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(4, '/images/products/iphone-15-Pro-lineup.jpg', 0, 1),
(4, '/images/ip_15_pro_details/refurb-iphone-16-pro-blacktitanium-202509.jfif', 1, 0),
(4, '/images/ip_15_pro_details/refurb-iphone-16-pro-whitetitanium-202509.jfif', 2, 0),
(4, '/images/ip_15_pro_details/refurb-iphone-16-pro-naturaltitanium-202509.jfif', 3, 0),
(4, '/images/ip_15_pro_details/refurb-iphone-16-pro-deserttitanium-202509.jfif', 4, 0),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-camera-system-230912_big.jpg.large_2x.jpg', 5, 0);

-- iPhone 15 Pro Max (Id=5)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(5, '/images/products/iphone-15-Promax-lineup.jpg', 0, 1),
(5, '/images/ip_15_promax_details/refurb-iphone-15-pro-max-blacktitanium-202412.jfif', 1, 0),
(5, '/images/ip_15_promax_details/refurb-iphone-15-pro-max-bluetitanium-202412.jfif', 2, 0),
(5, '/images/ip_15_promax_details/refurb-iphone-15-pro-max-naturaltitanium-202412.jfif', 3, 0),
(5, '/images/ip_15_promax_details/refurb-iphone-15-pro-max-whitetitanium-202412.jfif', 4, 0),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-camera-system-230912_big.jpg.large_2x.jpg', 5, 0);

-- iPhone 16 (Id=6)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(6, '/images/products/iphone-16-lineup.jpg', 0, 1),
(6, '/images/ip_16_details/refurb-iphone-16-black-202509.jfif', 1, 0),
(6, '/images/ip_16_details/refurb-iphone-16-pink-202509.jfif', 2, 0),
(6, '/images/ip_16_details/refurb-iphone-16-ultramarine-202509.jfif', 3, 0),
(6, '/images/ip_16_details/refurb-iphone-16-white-202509.jfif', 4, 0),
(6, '/images/ip_16_details/Apple-iPhone-16-Camera-Control-01-240909_inline.jpg.large_2x.jpg', 5, 0);

-- iPhone 16 Plus (Id=7)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(7, '/images/products/iphone-16-plus-lineup.jpg', 0, 1),
(7, '/images/ip_16_plus_details/refurb-iphone-16-plus-black-202509.jfif', 1, 0),
(7, '/images/ip_16_plus_details/refurb-iphone-16-plus-pink-202509.jfif', 2, 0),
(7, '/images/ip_16_plus_details/refurb-iphone-16-plus-teal-202509.jfif', 3, 0),
(7, '/images/ip_16_plus_details/refurb-iphone-16-plus-ultramarine-202509.jfif', 4, 0),
(7, '/images/ip_16_plus_details/refurb-iphone-16-plus-white-202509.jfif', 5, 0),
(7, '/images/ip_16_plus_details/Apple-iPhone-16-Camera-Control-01-240909_inline.jpg.large_2x.jpg', 6, 0);

-- iPhone 16 Pro (Id=8)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(8, '/images/products/iphone-16-Pro-lineup.jpg', 0, 1),
(8, '/images/ip_16_pro_details/refurb-iphone-16-pro-blacktitanium-202509.jfif', 1, 0),
(8, '/images/ip_16_pro_details/refurb-iphone-16-pro-deserttitanium-202509.jfif', 2, 0),
(8, '/images/ip_16_pro_details/refurb-iphone-16-pro-naturaltitanium-202509.jfif', 3, 0),
(8, '/images/ip_16_pro_details/refurb-iphone-16-pro-whitetitanium-202509.jfif', 4, 0),
(8, '/images/ip_16_pro_details/Apple-iPhone-16-Pro-camera-system-240909_inline.jpg.large_2x.jpg', 5, 0);

-- iPhone 16 Pro Max (Id=9)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(9, '/images/products/iphone-16-Promax-lineup.jpg', 0, 1),
(9, '/images/ip_16_promax_details/refurb-iphone-16-pro-max-blacktitanium-202509.jfif', 1, 0),
(9, '/images/ip_16_promax_details/refurb-iphone-16-pro-max-deserttitanium-202509.jfif', 2, 0),
(9, '/images/ip_16_promax_details/refurb-iphone-16-pro-max-naturaltitanium-202509.jfif', 3, 0),
(9, '/images/ip_16_promax_details/refurb-iphone-16-pro-max-whitetitanium-202509.jfif', 4, 0),
(9, '/images/ip_16_promax_details/Apple-iPhone-16-Pro-camera-system-240909_inline.jpg.large_2x.jpg', 5, 0);

-- Samsung Galaxy A35 5G (Id=10)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(10, '/images/products/samsung_a35_lineup.png', 0, 1),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234730.avif', 1, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234724.avif', 2, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234726.avif', 3, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234728.avif', 4, 0);

-- Samsung Galaxy A55 5G (Id=11)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(11, '/images/products/samsung_a55_lineup.png', 0, 1),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234972.avif', 1, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234966.avif', 2, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234968.avif', 3, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234970.avif', 4, 0);

-- Samsung Galaxy S24 FE (Id=12)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(12, '/images/products/samsung_s24_fe_lineup.png', 0, 1),
(12, '/images/s24_fe_details/uk-feature-graphite-543465844.avif', 1, 0),
(12, '/images/s24_fe_details/uk-feature-blue-543465842.avif', 2, 0),
(12, '/images/s24_fe_details/uk-feature-mint-543465840.avif', 3, 0),
(12, '/images/s24_fe_details/uk-feature-yellow-543465838.avif', 4, 0);

-- Samsung Galaxy S25 (Id=13)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(13, '/images/products/samsung_s25_lineup.png', 0, 1),
(13, '/images/s25_details/p1_163x346_SilverShadow.webp', 1, 0),
(13, '/images/s25_details/p1_163x346_Icyblue.avif', 2, 0),
(13, '/images/s25_details/p1_163x346_Blueblack.avif', 3, 0),
(13, '/images/s25_details/p1_163x346_Coralred.webp', 4, 0),
(13, '/images/s25_details/p1_163x346_Mint.avif', 5, 0),
(13, '/images/s25_details/p1_163x346_Navy.avif', 6, 0),
(13, '/images/s25_details/p1_163x346_Pinkgold.webp', 7, 0);

-- Samsung Galaxy S25+ (Id=14)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(14, '/images/products/samsung_s25_plus_lineup.png', 0, 1),
(14, '/images/s25_plus_details/p2_163x346_SilverShadow.avif', 1, 0),
(14, '/images/s25_plus_details/p2_163x346_Icyblue.avif', 2, 0),
(14, '/images/s25_plus_details/p2_163x346_Blueblack.avif', 3, 0),
(14, '/images/s25_plus_details/p2_163x346_Coralred.webp', 4, 0),
(14, '/images/s25_plus_details/p2_163x346_Mint.avif', 5, 0),
(14, '/images/s25_plus_details/p2_163x346_Navy.avif', 6, 0),
(14, '/images/s25_plus_details/p2_163x346_Pinkgold.avif', 7, 0);

-- Samsung Galaxy S25 Ultra (Id=15)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(15, '/images/products/samsung_s25_ultra_lineup.png', 0, 1),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumGray.avif', 1, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumBlack.webp', 2, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumWhitesilver.avif', 3, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumJadegreen.avif', 4, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumJetblack.webp', 5, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumSilverblue.avif', 6, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumPinkgold.avif', 7, 0);

-- Samsung Galaxy Z Flip 6 (Id=16)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(16, '/images/products/samsung_zflip6_lineup.jfif', 0, 1),
(16, '/images/zflip6_details/163x346_Silver-Shadow.webp', 1, 0),
(16, '/images/zflip6_details/163x346_Blue.avif', 2, 0),
(16, '/images/zflip6_details/163x346_Crafted-Black.avif', 3, 0),
(16, '/images/zflip6_details/163x346_Mint.webp', 4, 0),
(16, '/images/zflip6_details/163x346_Peach.avif', 5, 0),
(16, '/images/zflip6_details/163x346_White.avif', 6, 0),
(16, '/images/zflip6_details/163x346_Yellow.avif', 7, 0);

-- Samsung Galaxy Z Fold 6 (Id=17)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(17, '/images/products/samsung_zfold6_lineup.jfif', 0, 1),
(17, '/images/zfold6_details/163x346_Silver-Shadow.avif', 1, 0),
(17, '/images/zfold6_details/163x346_Crafted-Black.avif', 2, 0),
(17, '/images/zfold6_details/163x346_Navy.avif', 3, 0),
(17, '/images/zfold6_details/163x346_Pink.avif', 4, 0),
(17, '/images/zfold6_details/163x346_White.avif', 5, 0);

-- iPhone 17e (Id=18)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(18, '/images/products/iphone_17e_lineup.jpg', 0, 1),
(18, '/images/ip_17_e_details/black.webp', 1, 0),
(18, '/images/ip_17_e_details/white.webp', 2, 0),
(18, '/images/ip_17_e_details/soft-pink.webp', 3, 0),
(18, '/images/ip_17_e_details/lockscreen.jpg', 4, 0),
(18, '/images/ip_17_e_details/photography.jpg', 5, 0);

-- iPhone 17 (Id=19)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(19, '/images/products/iPhone-17-lineup.jpg', 0, 1),
(19, '/images/ip_17_details/black.webp', 1, 0),
(19, '/images/ip_17_details/white.webp', 2, 0),
(19, '/images/ip_17_details/mist_blue.webp', 3, 0),
(19, '/images/ip_17_details/lavender.webp', 4, 0),
(19, '/images/ip_17_details/sage.webp', 5, 0),
(19, '/images/ip_17_details/Apple-iPhone-17-hero-250909_inline.jpg.large_2x.jpg', 6, 0),
(19, '/images/ip_17_details/Apple-iPhone-17-Lock-Screen-250909_inline.jpg.large_2x.jpg', 7, 0);

-- iPhone 17 Plus (Id=20)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(20, '/images/products/iPhone-17-plus-lineup.jpg', 0, 1),
(20, '/images/ip_17_plus_details/black.webp', 1, 0),
(20, '/images/ip_17_plus_details/white.webp', 2, 0),
(20, '/images/ip_17_plus_details/mist_blue.webp', 3, 0),
(20, '/images/ip_17_plus_details/lavender.webp', 4, 0),
(20, '/images/ip_17_plus_details/sage.webp', 5, 0),
(20, '/images/ip_17_plus_details/Apple-iPhone-17-hero-250909_inline.jpg.large_2x.jpg', 6, 0),
(20, '/images/ip_17_plus_details/Apple-iPhone-17-Lock-Screen-250909_inline.jpg.large_2x.jpg', 7, 0);

-- iPhone 17 Pro (Id=21)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(21, '/images/products/iphone_17_Pro_lineup.jpg', 0, 1),
(21, '/images/ip_17_pro_details/silver.webp', 1, 0),
(21, '/images/ip_17_pro_details/deepblue.webp', 2, 0),
(21, '/images/ip_17_pro_details/cosmicorange.webp', 3, 0),
(21, '/images/ip_17_pro_details/Apple-iPhone-17-Pro-iOS-26-Liquid-Glass-Lock-Screen-250909_inline.jpg.large_2x.jpg', 4, 0),
(21, '/images/ip_17_pro_details/Apple-iPhone-17-Pro-Photographic-Styles-Bright-style-250909_big.jpg.large_2x.jpg', 5, 0);

-- iPhone 17 Pro Max (Id=22)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(22, '/images/products/iphone_17_Promax_lineup.jpg', 0, 1),
(22, '/images/ip_17_promax_details/silver.webp', 1, 0),
(22, '/images/ip_17_promax_details/deepblue.webp', 2, 0),
(22, '/images/ip_17_promax_details/cosmicorange.webp', 3, 0),
(22, '/images/ip_17_promax_details/Apple-iPhone-17-Pro-iOS-26-Liquid-Glass-Lock-Screen-250909_inline.jpg.large_2x.jpg', 4, 0),
(22, '/images/ip_17_promax_details/Apple-iPhone-17-Pro-Photographic-Styles-Bright-style-250909_big.jpg.large_2x.jpg', 5, 0);

-- iPhone Air (Id=23)
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(23, '/images/products/iphone_Air_lineup.jpg', 0, 1),
(23, '/images/ip_air_details/spaceblack.webp', 1, 0),
(23, '/images/ip_air_details/cloudwhite.webp', 2, 0),
(23, '/images/ip_air_details/skyblue.webp', 3, 0),
(23, '/images/ip_air_details/lightgold.webp', 4, 0),
(23, '/images/ip_air_details/Apple-iPhone-Air-48MP-250909_big.jpg.large_2x.jpg', 5, 0),
(23, '/images/ip_air_details/Apple-iPhone-Air-iOS-Home-Screen-250909_inline.jpg.large_2x.jpg', 6, 0);

PRINT 'All images and variants updated successfully!';
GO