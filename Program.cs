using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HutechStore.Data;
using HutechStore.Services;
using HutechStore.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<CookiePolicyOptions>(o => {
    o.MinimumSameSitePolicy = SameSiteMode.Lax;
    o.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kich hoat bo nho dem cuc bo (IMemoryCache) cho GeminiService dung de cache san pham
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddCookie("GoogleTempCookie", options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(options =>
{
    options.ClientId     = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SaveTokens   = true;
    options.SignInScheme = "GoogleTempCookie";
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpClient("GeminiClient", client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<GeminiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // For [ApiController] routes (e.g. /api/payment/webhook)
app.MapControllerRoute(name: "admin",   pattern: "Admin/{action=Index}/{id?}",   defaults: new { controller = "Admin" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); } catch { }

    // 1. Seed database with SQL scripts if new products (like iPhone Air) are missing
    if (!db.Products.Any(p => p.Slug == "iphone-air"))
    {
        try
        {
            Console.WriteLine("Seeding HutechStore database from SQL scripts...");
            ExecuteSqlScript(db, "update_database_images.sql");
            ExecuteSqlScript(db, "update_database_images_v2.sql");
            ExecuteSqlScript(db, "update_product_descriptions.sql");
            Console.WriteLine("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error seeding database: " + ex.Message);
        }
    }

    // Auto-create Minigame & Survey tables if they don't exist
    try
    {
        db.Database.ExecuteSqlRaw(@"
            IF OBJECT_ID('SurveyQuestions', 'U') IS NULL
            BEGIN
                CREATE TABLE SurveyQuestions (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    QuestionText NVARCHAR(500) NOT NULL,
                    Options NVARCHAR(500) NOT NULL,
                    IsActive BIT NOT NULL DEFAULT 1,
                    DisplayOrder INT NOT NULL DEFAULT 0,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );
            END

            IF OBJECT_ID('Surveys', 'U') IS NULL
            BEGIN
                CREATE TABLE Surveys (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NULL,
                    IpAddress NVARCHAR(50) NULL,
                    Age INT NOT NULL,
                    Gender NVARCHAR(10) NOT NULL DEFAULT 'Other',
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT FK_Surveys_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
                );
            END

            IF OBJECT_ID('SurveyResponses', 'U') IS NULL
            BEGIN
                CREATE TABLE SurveyResponses (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    SurveyId INT NOT NULL,
                    QuestionId INT NOT NULL,
                    AnswerText NVARCHAR(255) NOT NULL,
                    CONSTRAINT FK_SurveyResponses_Surveys FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_SurveyResponses_SurveyQuestions FOREIGN KEY (QuestionId) REFERENCES SurveyQuestions(Id) ON DELETE CASCADE
                );
            END

            IF OBJECT_ID('LuckyWheelPrizes', 'U') IS NULL
            BEGIN
                CREATE TABLE LuckyWheelPrizes (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    CouponType NVARCHAR(20) NOT NULL DEFAULT 'percent',
                    CouponValue DECIMAL(18,2) NOT NULL DEFAULT 0,
                    Weight INT NOT NULL DEFAULT 10,
                    ColorHex NVARCHAR(7) NOT NULL DEFAULT '#BEB280',
                    Status INT NOT NULL DEFAULT 1
                );

                -- Seed default prizes
                INSERT INTO LuckyWheelPrizes (Name, CouponType, CouponValue, Weight, ColorHex, Status) VALUES 
                (N'Giảm 10%', 'percent', 10.00, 10, '#0a1e33', 1),
                (N'Giảm 5%', 'percent', 5.00, 20, '#c2b29c', 1),
                (N'Miễn Phí Vận Chuyển', 'percent', 0.00, 15, '#1b365d', 1),
                (N'Chúc may mắn lần sau', 'none', 0.00, 30, '#8e8e93', 1),
                (N'Giảm 15%', 'percent', 15.00, 5, '#dfdaf2', 1),
                (N'Giảm 50k', 'fixed', 50000.00, 10, '#BEB280', 1);
            END

            IF OBJECT_ID('LuckyWheelPlays', 'U') IS NULL
            BEGIN
                CREATE TABLE LuckyWheelPlays (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NULL,
                    IpAddress NVARCHAR(50) NULL,
                    PlayedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CONSTRAINT FK_LuckyWheelPlays_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
                );
            END
            
            -- Seed some default questions if empty
            IF NOT EXISTS (SELECT * FROM SurveyQuestions)
            BEGIN
                INSERT INTO SurveyQuestions (QuestionText, Options, IsActive, DisplayOrder) VALUES
                (N'Bạn biết đến HUTECHSTORE qua kênh nào?', N'Mạng xã hội,Bạn bạn giới thiệu,Quảng cáo,Khác', 1, 0),
                (N'Yếu tố nào quan trọng nhất khi bạn chọn mua điện thoại?', N'Thiết kế sang trọng,Cấu hình mạnh mẽ,Giá cả hợp lý,Chế độ bảo hành', 1, 1),
                (N'Bạn đánh giá thế nào về trải nghiệm mua sắm trên website?', N'Rất mượt mà,Bình thường,Còn chậm,Cần cải thiện giao diện', 1, 2),
                (N'Bạn có dự định nâng cấp điện thoại trong 6 tháng tới không?', N'Chắc chắn có,Có thể,Không có nhu cầu', 1, 3),
                (N'Dòng sản phẩm nào của Apple làm bạn ấn tượng nhất?', N'iPhone Pro/Pro Max,iPhone tiêu chuẩn,iPhone Air siêu mỏng', 1, 4),
                (N'Bạn mong muốn HUTECHSTORE cải thiện dịch vụ nào nhất?', N'Tốc độ giao hàng,Chăm sóc khách hàng,Nhiều chương trình khuyến mãi,Chất lượng tư vấn', 1, 5);
            END
        ");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error creating Minigame/Survey tables: " + ex.Message);
    }

    // 2. Auto-heal corrupted Vietnamese encoding in database
    try
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        string FixEncoding(string? val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            bool isCorrupted = val.Contains('Ä') || val.Contains('Æ') || val.Contains('»') || val.Contains('º') || (val.Contains('á') && val.Contains('º'));
            if (!isCorrupted) return val;
            try
            {
                var encoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
                byte[] bytes = encoding.GetBytes(val);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return val;
            }
        }

        bool changed = false;
        var variants = db.ProductVariants.ToList();
        foreach (var v in variants)
        {
            var fixedColor = FixEncoding(v.Color);
            if (v.Color != fixedColor)
            {
                v.Color = fixedColor;
                changed = true;
            }
        }

        var specs = db.PhoneSpecs.ToList();
        foreach (var s in specs)
        {
            var fixedColors = FixEncoding(s.Colors);
            if (s.Colors != fixedColors)
            {
                s.Colors = fixedColors;
                changed = true;
            }
        }

        var products = db.Products.ToList();
        foreach (var p in products)
        {
            var fixedName = FixEncoding(p.Name);
            if (p.Name != fixedName)
            {
                p.Name = fixedName;
                changed = true;
            }
            var fixedDesc = FixEncoding(p.Description);
            if (p.Description != fixedDesc)
            {
                p.Description = fixedDesc;
                changed = true;
            }
        }

        if (changed)
        {
            db.SaveChanges();
            Console.WriteLine("Database Vietnamese characters encoding healed successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error healing database encoding: " + ex.Message);
    }

    // 3. Align lineup product image paths
    try
    {
        var products = db.Products.ToList();
        foreach (var p in products)
        {
            var targetImage = p.Name switch
            {
                "iPhone 16e" => "/images/products/iphone-16e-lineup.jpg",
                "iPhone 15" => "/images/products/iphone_15_lineup.jpg",
                "iPhone 15 Plus" => "/images/products/iphone_15_plus_lineup.jpg",
                "iPhone 15 Pro" => "/images/products/iphone-15-Pro-lineup.jpg",
                "iPhone 15 Pro Max" => "/images/products/iphone-15-Promax-lineup.jpg",
                "iPhone 16" => "/images/products/iphone-16-lineup.jpg",
                "iPhone 16 Plus" => "/images/products/iphone-16-plus-lineup.jpg",
                "iPhone 16 Pro" => "/images/products/iphone-16-Pro-lineup.jpg",
                "iPhone 16 Pro Max" => "/images/products/iphone-16-Promax-lineup.jpg",
                "Samsung Galaxy A35 5G" => "/images/products/samsung_a35_lineup.png",
                "Samsung Galaxy A55 5G" => "/images/products/samsung_a55_lineup.png",
                "Samsung Galaxy S24 FE" => "/images/products/samsung_s24_fe_lineup.png",
                "Samsung Galaxy S25" => "/images/products/samsung_s25_lineup.png",
                "Samsung Galaxy S25+" => "/images/products/samsung_s25_plus_lineup.png",
                "Samsung Galaxy S25 Ultra" => "/images/products/samsung_s25_ultra_lineup.png",
                "Samsung Galaxy Z Flip 6" => "/images/products/samsung_zflip6_lineup.jfif",
                "Samsung Galaxy Z Fold 6" => "/images/products/samsung_zfold6_lineup.jfif",
                "iPhone 17e" => "/images/products/iphone_17e_lineup.jpg",
                "iPhone 17" => "/images/products/iPhone-17-lineup.jpg",
                "iPhone 17 Plus" => "/images/products/iPhone-17-plus-lineup.jpg",
                "iPhone 17 Pro" => "/images/products/iphone_17_Pro_lineup.jpg",
                "iPhone 17 Pro Max" => "/images/products/iphone_17_Promax_lineup.jpg",
                "iPhone Air" => "/images/products/iphone_Air_lineup.jpg",
                _ => p.Image
            };
            if (p.Image != targetImage)
            {
                p.Image = targetImage;
            }
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error seeding database image paths: " + ex.Message);
    }

    // Call variant and price sync on startup
    SyncProductVariantsAndPrices(db);
}

app.Run();

// Helper method to execute multi-batch SQL files
static void ExecuteSqlScript(AppDbContext context, string filePath)
{
    if (!System.IO.File.Exists(filePath)) return;
    string script = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
    var regex = new System.Text.RegularExpressions.Regex(@"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
    var batches = regex.Split(script);
    foreach (var batch in batches)
    {
        if (string.IsNullOrWhiteSpace(batch)) continue;
        context.Database.ExecuteSqlRaw(batch);
    }
}

static void SyncProductVariantsAndPrices(AppDbContext db)
{
    try
    {
        Console.WriteLine("Synchronizing product variants and prices...");
        var products = db.Products.Include(p => p.PhoneSpecs).Include(p => p.ProductVariants).ToList();
        
        foreach (var p in products)
        {
            List<int> storages = new List<int>();
            List<string> colors = new List<string>();
            
            // Extract colors from PhoneSpecs or use fallback
            if (p.PhoneSpecs != null && !string.IsNullOrEmpty(p.PhoneSpecs.Colors))
            {
                colors = p.PhoneSpecs.Colors.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(c => c.Trim())
                                           .ToList();
            }
            if (!colors.Any())
            {
                colors.Add("Mặc định");
            }

            // Determine variants based on brand and model name
            if (p.Brand == "Apple")
            {
                // All iPhones must have 128, 256, 512, 1024 (1TB)
                storages = new List<int> { 128, 256, 512, 1024 };
            }
            else if (p.Brand == "Samsung")
            {
                if (p.Name.Contains("Ultra") || p.Name.Contains("Fold 6"))
                {
                    storages = new List<int> { 256, 512, 1024 };
                }
                else if (p.Name.Contains("S25+") || p.Name.Contains("Z Flip 6"))
                {
                    storages = new List<int> { 256, 512 };
                }
                else // A35, A55, S24 FE, S25
                {
                    storages = new List<int> { 128, 256 };
                }
            }

            if (!storages.Any()) continue;

            // Update main product price to reflect minimum variant price
            decimal basePrice = CalculatePrice(p.Name, p.Brand, storages.Min());
            if (p.Price != basePrice)
            {
                p.Price = basePrice;
            }

            // Delete variants that don't match the new storages or colors to clean up
            var toDelete = p.ProductVariants.Where(v => !storages.Contains(v.StorageGb) || !colors.Contains(v.Color ?? "")).ToList();
            if (toDelete.Any())
            {
                db.ProductVariants.RemoveRange(toDelete);
            }

            // Create or update valid variants
            foreach (var color in colors)
            {
                foreach (var storage in storages)
                {
                    decimal price = CalculatePrice(p.Name, p.Brand, storage);
                    
                    var existing = p.ProductVariants.FirstOrDefault(v => v.Color == color && v.StorageGb == storage);
                    if (existing != null)
                    {
                        existing.Price = price;
                        existing.Stock = 15; // Reset stock
                        existing.Status = 1;
                    }
                    else
                    {
                        db.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = p.Id,
                            Color = color,
                            StorageGb = storage,
                            RamGb = GetRamForModel(p.Name, storage),
                            Price = price,
                            Stock = 15,
                            Status = 1
                        });
                    }
                }
            }
        }
        db.SaveChanges();
        Console.WriteLine("Product variants synchronization completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error synchronizing product variants: " + ex.Message);
    }
}

static int GetRamForModel(string name, int storage)
{
    if (name.Contains("17 Pro") || name.Contains("17 Pro Max")) return 12;
    if (name.Contains("15 Pro") || name.Contains("16 Pro") || name.Contains("17") || name.Contains("Air")) return 8;
    if (name.Contains("15") || name.Contains("16")) return 8;
    if (name.Contains("S25 Ultra") || name.Contains("Fold 6") || name.Contains("S25+")) return 12;
    if (name.Contains("S25") || name.Contains("Flip 6")) return 12;
    if (name.Contains("A55") || name.Contains("A35") || name.Contains("S24 FE")) return 8;
    return 8;
}

static decimal CalculatePrice(string name, string brand, int storage)
{
    // Default base prices in VND
    decimal basePrice = 19990000;
    
    if (brand == "Apple")
    {
        if (name.Contains("16e")) basePrice = 18990000;
        else if (name.Contains("15 Plus")) basePrice = 22990000;
        else if (name.Contains("15 Pro Max")) basePrice = 29990000;
        else if (name.Contains("15 Pro")) basePrice = 24990000;
        else if (name.Contains("15")) basePrice = 19990000;
        else if (name.Contains("16 Plus")) basePrice = 25990000;
        else if (name.Contains("16 Pro Max")) basePrice = 31990000;
        else if (name.Contains("16 Pro")) basePrice = 28990000;
        else if (name.Contains("16")) basePrice = 22990000;
        else if (name.Contains("17e")) basePrice = 18990000;
        else if (name.Contains("17 Plus")) basePrice = 28990000;
        else if (name.Contains("17 Pro Max")) basePrice = 35990000;
        else if (name.Contains("17 Pro")) basePrice = 29990000;
        else if (name.Contains("17")) basePrice = 24990000;
        else if (name.Contains("Air")) basePrice = 27990000;

        // Price increments for storage upgrades:
        // +3M for 256GB, +9M for 512GB, +15M for 1TB (standard Apple pricing curve)
        return storage switch
        {
            256 => basePrice + 3000000,
            512 => basePrice + 9000000,
            1024 => basePrice + 15000000,
            _ => basePrice
        };
    }
    else // Samsung
    {
        if (name.Contains("A35"))
        {
            return storage == 256 ? 9490000 : 7990000;
        }
        else if (name.Contains("A55"))
        {
            return storage == 256 ? 11490000 : 9990000;
        }
        else if (name.Contains("S24 FE"))
        {
            return storage == 256 ? 16990000 : 14990000;
        }
        else if (name.Contains("S25+"))
        {
            return storage == 512 ? 29990000 : 26990000;
        }
        else if (name.Contains("S25 Ultra"))
        {
            return storage switch
            {
                512 => 38990000,
                1024 => 44990000,
                _ => 34990000
            };
        }
        else if (name.Contains("S25"))
        {
            return storage == 256 ? 23990000 : 21990000;
        }
        else if (name.Contains("Z Flip 6"))
        {
            return storage == 512 ? 26990000 : 23990000;
        }
        else if (name.Contains("Z Fold 6"))
        {
            return storage switch
            {
                512 => 43990000,
                1024 => 49990000,
                _ => 41990000
            };
        }
    }
    return basePrice;
}