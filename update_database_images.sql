-- ============================================================
--  HutechStore - Update Database Images & Add New Products
--  Đồng bộ ảnh sản phẩm từ cấu trúc thư mục mới
-- ============================================================
USE HutechStore;
GO

-- ============================================================
--  PHASE 1: Cập nhật ảnh đại diện cho 17 sản phẩm hiện có
-- ============================================================
UPDATE Products SET Image = '/images/products/iphone-16e-lineup.jpg' WHERE Id = 1;   -- iPhone 16e
UPDATE Products SET Image = '/images/ip_15_details/Apple-iPhone-15-lineup-design-230912_big.jpg.large_2x.jpg' WHERE Id = 2;   -- iPhone 15
UPDATE Products SET Image = '/images/products/iphone_15_plus_lineup.jpg' WHERE Id = 3;   -- iPhone 15 Plus
UPDATE Products SET Image = '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-design-230912_big.jpg.large_2x.jpg' WHERE Id = 4;   -- iPhone 15 Pro
UPDATE Products SET Image = '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-design-230912_big.jpg.large_2x.jpg' WHERE Id = 5;   -- iPhone 15 Pro Max
UPDATE Products SET Image = '/images/products/iphone-16-lineup.jpg' WHERE Id = 6;   -- iPhone 16
UPDATE Products SET Image = '/images/products/iphone-16-lineup.jpg' WHERE Id = 7;   -- iPhone 16 Plus
UPDATE Products SET Image = '/images/products/iphone-16-Pro-lineup.jpg' WHERE Id = 8;   -- iPhone 16 Pro
UPDATE Products SET Image = '/images/products/iphone-16-Pro-lineup.jpg' WHERE Id = 9;   -- iPhone 16 Pro Max
UPDATE Products SET Image = '/images/a35_details/levant-feature-awesome-inside-and-out-540234736.avif' WHERE Id = 10;  -- Galaxy A35
UPDATE Products SET Image = '/images/a55_details/levant-feature-metal-meets-glass--awesome-540234927.avif' WHERE Id = 11;  -- Galaxy A55
UPDATE Products SET Image = '/images/s24_fe_details/uk-feature-graphite-543465844.avif' WHERE Id = 12;  -- Galaxy S24 FE
UPDATE Products SET Image = '/images/s25_details/p1_163x346_SilverShadow.webp' WHERE Id = 13;  -- Galaxy S25
UPDATE Products SET Image = '/images/s25_plus_details/p2_163x346_SilverShadow.avif' WHERE Id = 14;  -- Galaxy S25+
UPDATE Products SET Image = '/images/s25_ultra_details/p3_163x346_TitaniumGray.avif' WHERE Id = 15;  -- Galaxy S25 Ultra
UPDATE Products SET Image = '/images/zflip6_details/163x346_Silver-Shadow.webp' WHERE Id = 16;  -- Galaxy Z Flip 6
UPDATE Products SET Image = '/images/zfold6_details/163x346_Silver-Shadow.avif' WHERE Id = 17;  -- Galaxy Z Fold 6

PRINT 'Phase 1: Updated product images for 17 existing products.';

-- ============================================================
--  PHASE 2: Xóa ProductImages cũ và chèn ảnh chi tiết mới
-- ============================================================
DELETE FROM ProductImages;

-- === iPhone 16e (Id=1) - ip_16_e_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(1, '/images/products/iphone-16e-lineup.jpg', 0, 1),
(1, '/images/ip_16_e_details/Apple-iPhone-16e-hero-250219_inline.jpg.large_2x.jpg', 1, 0),
(1, '/images/ip_16_e_details/Apple-iPhone-16e-48MP-Fusion-photography-250219_big.jpg.large_2x.jpg', 2, 0);

-- === iPhone 15 (Id=2) - ip_15_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(2, '/images/ip_15_details/Apple-iPhone-15-lineup-design-230912_big.jpg.large_2x.jpg', 0, 1),
(2, '/images/ip_15_details/Apple-iPhone-15-48MP-01-230912_big.jpg.large_2x.jpg', 1, 0),
(2, '/images/ip_15_details/Apple-iPhone-15-lineup-Dynamic-Island-incoming-call-230912.jpg', 2, 0),
(2, '/images/ip_15_details/Apple-iPhone-15-lineup-gaming-230912.jpg', 3, 0),
(2, '/images/ip_15_details/Apple-iPhone-15-lineup-Portrait-mode-demo-230912.jpg', 4, 0);

-- === iPhone 15 Plus (Id=3) - ip_15_plus_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(3, '/images/products/iphone_15_plus_lineup.jpg', 0, 1),
(3, '/images/ip_15_plus_details/camera.jpg', 1, 0),
(3, '/images/ip_15_plus_details/dynamic_island.jpg', 2, 0),
(3, '/images/ip_15_plus_details/gaming.jpg', 3, 0),
(3, '/images/ip_15_plus_details/photo.jpg', 4, 0),
(3, '/images/ip_15_plus_details/portrait_mode.jpg', 5, 0),
(3, '/images/ip_15_plus_details/AirPods-Pro-2nd-generation-USB-C-connection.jpg', 6, 0),
(3, '/images/ip_15_plus_details/Precision-Finding-3.jpg', 7, 0);

-- === iPhone 15 Pro (Id=4) - ip_15_pro_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-design-230912_big.jpg.large_2x.jpg', 0, 1),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-camera-system-230912_big.jpg.large_2x.jpg', 1, 0),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-Max-48MP-camera-02-230912_big.jpg.large_2x.jpg', 2, 0),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-Action-button-230912.jpg', 3, 0),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-Camera-optical-zoom-230912.jpg', 4, 0),
(4, '/images/ip_15_pro_details/Apple-iPhone-15-Pro-lineup-A17-Pro-chip-gaming-230912.jpg', 5, 0);

-- === iPhone 15 Pro Max (Id=5) - ip_15_promax_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-design-230912_big.jpg.large_2x.jpg', 0, 1),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-camera-system-230912_big.jpg.large_2x.jpg', 1, 0),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-Max-48MP-camera-02-230912_big.jpg.large_2x.jpg', 2, 0),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-Action-button-230912.jpg', 3, 0),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-Camera-optical-zoom-230912.jpg', 4, 0),
(5, '/images/ip_15_promax_details/Apple-iPhone-15-Pro-lineup-A17-Pro-chip-gaming-230912.jpg', 5, 0);

-- === iPhone 16 (Id=6) - ip_16_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(6, '/images/products/iphone-16-lineup.jpg', 0, 1),
(6, '/images/ip_16_details/Apple-iPhone-16-Camera-Control-01-240909_inline.jpg.large_2x.jpg', 1, 0),
(6, '/images/ip_16_details/Apple-iPhone-16-Photographic-Styles-01-240909_inline.jpg.large_2x.jpg', 2, 0),
(6, '/images/ip_16_details/Apple-iPhone-16-Ultra-Wide-photography-01-240909_big.jpg.large_2x.jpg', 3, 0);

-- === iPhone 16 Plus (Id=7) - ip_16_plus_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(7, '/images/products/iphone-16-lineup.jpg', 0, 1),
(7, '/images/ip_16_plus_details/Apple-iPhone-16-Camera-Control-01-240909_inline.jpg.large_2x.jpg', 1, 0),
(7, '/images/ip_16_plus_details/Apple-iPhone-16-Photographic-Styles-01-240909_inline.jpg.large_2x.jpg', 2, 0),
(7, '/images/ip_16_plus_details/Apple-iPhone-16-Ultra-Wide-photography-01-240909_big.jpg.large_2x.jpg', 3, 0);

-- === iPhone 16 Pro (Id=8) - ip_16_pro_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(8, '/images/products/iphone-16-Pro-lineup.jpg', 0, 1),
(8, '/images/ip_16_pro_details/Apple-iPhone-16-Pro-camera-system-240909_inline.jpg.large_2x.jpg', 1, 0),
(8, '/images/ip_16_pro_details/Apple-iPhone-16-Pro-Vibrant-photography-240909_big.jpg.large_2x.jpg', 2, 0);

-- === iPhone 16 Pro Max (Id=9) - ip_16_promax_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(9, '/images/products/iphone-16-Pro-lineup.jpg', 0, 1),
(9, '/images/ip_16_promax_details/Apple-iPhone-16-Pro-camera-system-240909_inline.jpg.large_2x.jpg', 1, 0),
(9, '/images/ip_16_promax_details/Apple-iPhone-16-Pro-Vibrant-photography-240909_big.jpg.large_2x.jpg', 2, 0);

-- === Samsung Galaxy A35 5G (Id=10) - a35_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(10, '/images/a35_details/levant-feature-awesome-inside-and-out-540234736.avif', 0, 1),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234724.avif', 1, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234726.avif', 2, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234728.avif', 3, 0),
(10, '/images/a35_details/levant-feature-trendy-colors-that-look-amazing-540234730.avif', 4, 0);

-- === Samsung Galaxy A55 5G (Id=11) - a55_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(11, '/images/a55_details/levant-feature-metal-meets-glass--awesome-540234927.avif', 0, 1),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234966.avif', 1, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234968.avif', 2, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234970.avif', 3, 0),
(11, '/images/a55_details/levant-feature-offered-in-delightful-colors-540234972.avif', 4, 0);

-- === Samsung Galaxy S24 FE (Id=12) - s24_fe_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(12, '/images/s24_fe_details/uk-feature-graphite-543465844.avif', 0, 1),
(12, '/images/s24_fe_details/uk-feature-blue-543465842.avif', 1, 0),
(12, '/images/s24_fe_details/uk-feature-mint-543465840.avif', 2, 0),
(12, '/images/s24_fe_details/uk-feature-yellow-543465838.avif', 3, 0);

-- === Samsung Galaxy S25 (Id=13) - s25_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(13, '/images/s25_details/p1_163x346_SilverShadow.webp', 0, 1),
(13, '/images/s25_details/p1_163x346_Icyblue.avif', 1, 0),
(13, '/images/s25_details/p1_163x346_Blueblack.avif', 2, 0),
(13, '/images/s25_details/p1_163x346_Coralred.webp', 3, 0),
(13, '/images/s25_details/p1_163x346_Mint.avif', 4, 0),
(13, '/images/s25_details/p1_163x346_Navy.avif', 5, 0),
(13, '/images/s25_details/p1_163x346_Pinkgold.webp', 6, 0);

-- === Samsung Galaxy S25+ (Id=14) - s25_plus_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(14, '/images/s25_plus_details/p2_163x346_SilverShadow.avif', 0, 1),
(14, '/images/s25_plus_details/p2_163x346_Icyblue.avif', 1, 0),
(14, '/images/s25_plus_details/p2_163x346_Blueblack.avif', 2, 0),
(14, '/images/s25_plus_details/p2_163x346_Coralred.webp', 3, 0),
(14, '/images/s25_plus_details/p2_163x346_Mint.avif', 4, 0),
(14, '/images/s25_plus_details/p2_163x346_Navy.avif', 5, 0),
(14, '/images/s25_plus_details/p2_163x346_Pinkgold.avif', 6, 0);

-- === Samsung Galaxy S25 Ultra (Id=15) - s25_ultra_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(15, '/images/s25_ultra_details/p3_163x346_TitaniumGray.avif', 0, 1),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumBlack.webp', 1, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumWhitesilver.avif', 2, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumJadegreen.avif', 3, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumJetblack.webp', 4, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumSilverblue.avif', 5, 0),
(15, '/images/s25_ultra_details/p3_163x346_TitaniumPinkgold.avif', 6, 0);

-- === Samsung Galaxy Z Flip 6 (Id=16) - zflip6_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(16, '/images/zflip6_details/163x346_Silver-Shadow.webp', 0, 1),
(16, '/images/zflip6_details/163x346_Blue.avif', 1, 0),
(16, '/images/zflip6_details/163x346_Crafted-Black.avif', 2, 0),
(16, '/images/zflip6_details/163x346_Mint.webp', 3, 0),
(16, '/images/zflip6_details/163x346_Peach.avif', 4, 0),
(16, '/images/zflip6_details/163x346_White.avif', 5, 0),
(16, '/images/zflip6_details/163x346_Yellow.avif', 6, 0);

-- === Samsung Galaxy Z Fold 6 (Id=17) - zfold6_details ===
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
(17, '/images/zfold6_details/163x346_Silver-Shadow.avif', 0, 1),
(17, '/images/zfold6_details/163x346_Crafted-Black.avif', 1, 0),
(17, '/images/zfold6_details/163x346_Navy.avif', 2, 0),
(17, '/images/zfold6_details/163x346_Pink.avif', 3, 0),
(17, '/images/zfold6_details/163x346_White.avif', 4, 0);

PRINT 'Phase 2: Inserted product images for 17 existing products.';

-- ============================================================
--  PHASE 3: Thêm các sản phẩm mới - iPhone 17 Series & iPhone Air
-- ============================================================

-- 18. iPhone 17e
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone 17e',
    N'Chip A19, camera 48MP tiên tiến, Apple Intelligence toàn diện, thiết kế mỏng nhẹ với giá tốt nhất dòng iPhone 17.',
    18990000,
    '/images/products/iphone_17e_lineup.jpg',
    1, 'Apple', 'iphone-17e', 1, 0, 50
);
DECLARE @p18 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p18, 'Apple A19', 8, 128, '6.1 inch', 'Super Retina XDR OLED', 60,
    '48MP f/1.6 Fusion', '12MP TrueDepth', 3500, 1, 1, 1, 1,
    'iOS 26', N'Đen,Trắng,Xanh,Hồng', '138.5 x 67.0 x 7.25 mm', 163, 2025
);
INSERT INTO ProductVariants VALUES
    (@p18, N'Đen', 128, 8, 18990000, 20, 1),
    (@p18, N'Trắng', 128, 8, 18990000, 15, 1),
    (@p18, N'Xanh', 256, 8, 21990000, 10, 1),
    (@p18, N'Hồng', 256, 8, 21990000, 5, 1);
INSERT INTO UseCaseTags VALUES
    (@p18, 'student', 9), (@p18, 'value', 9), (@p18, 'camera', 7), (@p18, 'business', 7);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p18, '/images/products/iphone_17e_lineup.jpg', 0, 1),
    (@p18, '/images/ip_17_e_details/lockscreen.jpg', 1, 0),
    (@p18, '/images/ip_17_e_details/photography.jpg', 2, 0),
    (@p18, '/images/ip_17_e_details/video-4k-60-fps-Dolby-vision.jpg', 3, 0),
    (@p18, '/images/ip_17_e_details/accessories.jpg', 4, 0);

-- 19. iPhone 17
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone 17',
    N'Chip A19, Dynamic Island, camera 48MP nâng cấp, Ceramic Shield 2, Apple Intelligence. Chuẩn mực mới cho iPhone tiêu chuẩn.',
    24990000,
    '/images/products/iPhone-17-lineup.jpg',
    1, 'Apple', 'iphone-17', 1, 1, 45
);
DECLARE @p19 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p19, 'Apple A19', 8, 128, '6.1 inch', 'Super Retina XDR OLED', 120,
    '48MP f/1.6 + 12MP ultrawide', '24MP TrueDepth', 3800, 1, 1, 1, 1,
    'iOS 26', N'Đen,Trắng,Xanh,Hồng,Xanh lá', '147.0 x 71.0 x 7.3 mm', 170, 2025
);
INSERT INTO ProductVariants VALUES
    (@p19, N'Đen', 128, 8, 24990000, 15, 1),
    (@p19, N'Trắng', 128, 8, 24990000, 12, 1),
    (@p19, N'Xanh', 256, 8, 28990000, 10, 1),
    (@p19, N'Hồng', 256, 8, 28990000, 8, 1);
INSERT INTO UseCaseTags VALUES
    (@p19, 'camera', 8), (@p19, 'student', 8), (@p19, 'business', 8), (@p19, 'gaming', 7);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p19, '/images/products/iPhone-17-lineup.jpg', 0, 1),
    (@p19, '/images/ip_17_details/Apple-iPhone-17-hero-250909_inline.jpg.large_2x.jpg', 1, 0),
    (@p19, '/images/ip_17_details/Apple-iPhone-17-Lock-Screen-250909_inline.jpg.large_2x.jpg', 2, 0),
    (@p19, '/images/ip_17_details/Apple-iPhone-17-Ultra-Wide-photography-01-250909_big.jpg.large_2x.jpg', 3, 0);

-- 20. iPhone 17 Plus
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone 17 Plus',
    N'Màn hình 6.7 inch lớn nhất dòng 17 tiêu chuẩn, pin trâu cả ngày, camera 48MP Ultra Wide, Ceramic Shield 2.',
    28990000,
    '/images/products/iPhone-17-lineup.jpg',
    1, 'Apple', 'iphone-17-plus', 1, 0, 30
);
DECLARE @p20 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p20, 'Apple A19', 8, 128, '6.7 inch', 'Super Retina XDR OLED', 120,
    '48MP f/1.6 + 12MP ultrawide', '24MP TrueDepth', 4500, 1, 1, 1, 1,
    'iOS 26', N'Đen,Trắng,Xanh,Hồng,Xanh lá', '160.5 x 77.5 x 7.3 mm', 199, 2025
);
INSERT INTO ProductVariants VALUES
    (@p20, N'Đen', 128, 8, 28990000, 12, 1),
    (@p20, N'Trắng', 128, 8, 28990000, 8, 1),
    (@p20, N'Đen', 256, 8, 32990000, 10, 1);
INSERT INTO UseCaseTags VALUES
    (@p20, 'battery_life', 9), (@p20, 'camera', 8), (@p20, 'student', 7), (@p20, 'gaming', 7);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p20, '/images/products/iPhone-17-lineup.jpg', 0, 1),
    (@p20, '/images/ip_17_plus_details/Apple-iPhone-17-hero-250909_inline.jpg.large_2x.jpg', 1, 0),
    (@p20, '/images/ip_17_plus_details/Apple-iPhone-17-Lock-Screen-250909_inline.jpg.large_2x.jpg', 2, 0),
    (@p20, '/images/ip_17_plus_details/Apple-iPhone-17-Ultra-Wide-photography-01-250909_big.jpg.large_2x.jpg', 3, 0);

-- 21. iPhone 17 Pro
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone 17 Pro',
    N'Chip A19 Pro, khung Forged Titanium cấp 5, camera 48MP ProRAW + Center Stage, Ceramic Shield 2, iOS 26 Liquid Glass.',
    33990000,
    '/images/products/iphone_17_Pro_lineup.jpg',
    1, 'Apple', 'iphone-17-pro', 1, 1, 40
);
DECLARE @p21 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p21, 'Apple A19 Pro', 12, 256, '6.3 inch', 'Super Retina XDR ProMotion OLED', 120,
    '48MP f/1.78 + 48MP ultrawide + 12MP 5x tele', '24MP TrueDepth', 3700, 1, 1, 1, 1,
    'iOS 26', N'Đen Titan,Trắng Titan,Sa mạc Titan,Tự Nhiên Titan', '149.0 x 71.0 x 8.0 mm', 194, 2025
);
INSERT INTO ProductVariants VALUES
    (@p21, N'Đen Titan', 256, 12, 33990000, 12, 1),
    (@p21, N'Trắng Titan', 256, 12, 33990000, 10, 1),
    (@p21, N'Sa mạc Titan', 512, 12, 40990000, 8, 1),
    (@p21, N'Tự Nhiên Titan', 1024, 12, 48990000, 5, 1);
INSERT INTO UseCaseTags VALUES
    (@p21, 'camera', 10), (@p21, 'gaming', 9), (@p21, 'business', 10), (@p21, 'battery_life', 8);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p21, '/images/products/iphone_17_Pro_lineup.jpg', 0, 1),
    (@p21, '/images/ip_17_pro_details/Apple-iPhone-17-Pro-iOS-26-Liquid-Glass-Lock-Screen-250909_inline.jpg.large_2x.jpg', 1, 0),
    (@p21, '/images/ip_17_pro_details/Apple-iPhone-17-Pro-Photographic-Styles-Bright-style-250909_big.jpg.large_2x.jpg', 2, 0);

-- 22. iPhone 17 Pro Max
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone 17 Pro Max',
    N'Đỉnh tuyệt đối 2025. Màn 6.9 inch ProMotion, A19 Pro siêu mạnh, camera Tetraprism 5x zoom, khung Forged Titanium, eSIM kép, pin lớn nhất.',
    41990000,
    '/images/products/iphone_17_Pro_lineup.jpg',
    1, 'Apple', 'iphone-17-pro-max', 1, 1, 25
);
DECLARE @p22 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p22, 'Apple A19 Pro', 12, 256, '6.9 inch', 'Super Retina XDR ProMotion OLED', 120,
    '48MP f/1.78 + 48MP ultrawide + 12MP 5x Tetraprism', '24MP TrueDepth Center Stage', 4800, 1, 1, 1, 1,
    'iOS 26', N'Đen Titan,Trắng Titan,Sa mạc Titan,Tự Nhiên Titan', '163.0 x 77.2 x 8.0 mm', 225, 2025
);
INSERT INTO ProductVariants VALUES
    (@p22, N'Đen Titan', 256, 12, 41990000, 8, 1),
    (@p22, N'Trắng Titan', 256, 12, 41990000, 6, 1),
    (@p22, N'Sa mạc Titan', 512, 12, 48990000, 5, 1),
    (@p22, N'Tự Nhiên Titan', 1024, 12, 57990000, 3, 1);
INSERT INTO UseCaseTags VALUES
    (@p22, 'camera', 10), (@p22, 'gaming', 10), (@p22, 'business', 10), (@p22, 'battery_life', 9);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p22, '/images/products/iphone_17_Pro_lineup.jpg', 0, 1),
    (@p22, '/images/ip_17_promax_details/Apple-iPhone-17-Pro-iOS-26-Liquid-Glass-Lock-Screen-250909_inline.jpg.large_2x.jpg', 1, 0),
    (@p22, '/images/ip_17_promax_details/Apple-iPhone-17-Pro-Photographic-Styles-Bright-style-250909_big.jpg.large_2x.jpg', 2, 0);

-- 23. iPhone Air
INSERT INTO Products (Name, Description, Price, Image, CategoryId, Brand, Slug, Status, Featured, Stock)
VALUES (
    N'iPhone Air',
    N'Siêu mỏng nhẹ tựa hư vô. Chip A19, camera 48MP, Ceramic Shield 2, thiết kế tối giản Quiet Luxury. Mỏng nhất từ trước đến nay.',
    27990000,
    '/images/products/iphone_Air_lineup.jpg',
    1, 'Apple', 'iphone-air', 1, 1, 35
);
DECLARE @p23 INT = SCOPE_IDENTITY();
INSERT INTO PhoneSpecs VALUES(
    @p23, 'Apple A19', 8, 128, '6.6 inch', 'Super Retina XDR OLED', 120,
    '48MP f/1.6 Fusion', '12MP TrueDepth', 3600, 1, 1, 1, 1,
    'iOS 26', N'Đen Không Gian,Bạc Ánh Trăng,Xanh Bầu Trời,Xám Đá', '155.0 x 73.5 x 5.8 mm', 145, 2025
);
INSERT INTO ProductVariants VALUES
    (@p23, N'Đen Không Gian', 128, 8, 27990000, 12, 1),
    (@p23, N'Bạc Ánh Trăng', 128, 8, 27990000, 10, 1),
    (@p23, N'Xanh Bầu Trời', 256, 8, 31990000, 8, 1),
    (@p23, N'Xám Đá', 256, 8, 31990000, 5, 1);
INSERT INTO UseCaseTags VALUES
    (@p23, 'business', 10), (@p23, 'camera', 8), (@p23, 'student', 8), (@p23, 'value', 7);
INSERT INTO ProductImages (ProductId, ImageUrl, SortOrder, IsMain) VALUES
    (@p23, '/images/products/iphone_Air_lineup.jpg', 0, 1),
    (@p23, '/images/ip_air_details/Apple-iPhone-Air-48MP-250909_big.jpg.large_2x.jpg', 1, 0),
    (@p23, '/images/ip_air_details/Apple-iPhone-Air-iOS-Home-Screen-250909_inline.jpg.large_2x.jpg', 2, 0);

PRINT 'Phase 3: Added 6 new products (iPhone 17e, 17, 17 Plus, 17 Pro, 17 Pro Max, Air).';
PRINT 'Done! Total products: 23.';
GO
