using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.ViewModels;
using System.Text.Json;

namespace HutechStore.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) { _db = db; }

    // ── AUTH GUARD ────────────────────────────────────────────────
    private bool IsAdmin() => SessionHelper.IsAdmin(HttpContext.Session);
    private IActionResult Deny() => RedirectToAction("Login", "Auth");

    // ══════════════════════════════════════════════════════════════
    //  DASHBOARD
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Index(string chartType = "month", int year = 0)
    {
        if (!IsAdmin()) return Deny();
        if (year == 0) year = DateTime.Now.Year;

        var now         = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Counts
        var totalOrders     = await _db.Orders.CountAsync();
        var totalProducts   = await _db.Products.CountAsync(p => p.Status == 1);
        var totalCategories = await _db.Categories.CountAsync(c => c.Status == 1);
        var totalUsers      = await _db.Users.CountAsync(u => u.Role == "user");
        var totalRevenue    = await _db.Orders
            .Where(o => o.Status == "delivered")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        // Revenue chart data
        List<RevenueItem> chartData;
        if (chartType == "week")
        {
            var startWeek = now.AddDays(-6).Date;
            chartData = Enumerable.Range(0, 7).Select(i =>
            {
                var d = startWeek.AddDays(i);
                var rev = _db.Orders
                    .Where(o => o.CreatedAt.Date == d && o.Status != "cancelled")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                return new RevenueItem { Label = d.ToString("dd/MM"), Revenue = rev };
            }).ToList();
        }
        else if (chartType == "year")
        {
            var years = await _db.Orders.Select(o => o.CreatedAt.Year).Distinct().OrderByDescending(y => y).Take(5).ToListAsync();
            chartData = years.Select(y =>
            {
                var rev = _db.Orders.Where(o => o.CreatedAt.Year == y && o.Status != "cancelled").Sum(o => (decimal?)o.TotalAmount) ?? 0;
                return new RevenueItem { Label = y.ToString(), Revenue = rev };
            }).ToList();
        }
        else // month
        {
            chartData = Enumerable.Range(1, 12).Select(m =>
            {
                var rev = _db.Orders
                    .Where(o => o.CreatedAt.Year == year && o.CreatedAt.Month == m && o.Status != "cancelled")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                var cnt = _db.Orders.Count(o => o.CreatedAt.Year == year && o.CreatedAt.Month == m);
                return new RevenueItem { Label = $"T{m}", Revenue = rev, OrderCount = cnt };
            }).ToList();
        }

        // Top products
        var topProducts = await _db.OrderItems
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProduct
            {
                Name    = g.Key.ProductName,
                Sold    = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Total)
            })
            .OrderByDescending(t => t.Sold)
            .Take(5)
            .ToListAsync();

        // Revenue by status
        var revByStatus = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new RevenueByStatus
            {
                Status  = g.Key,
                Total   = g.Sum(o => o.TotalAmount),
                Count   = g.Count()
            })
            .ToListAsync();

        var recentOrders = await _db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .ToListAsync();

        var availableYears = await _db.Orders
            .Select(o => o.CreatedAt.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();
        if (!availableYears.Contains(DateTime.Now.Year))
            availableYears.Insert(0, DateTime.Now.Year);

        var vm = new DashboardViewModel
        {
            TotalOrders      = totalOrders,
            TotalProducts    = totalProducts,
            TotalCategories  = totalCategories,
            TotalUsers       = totalUsers,
            TotalRevenue     = totalRevenue,
            RecentOrders     = recentOrders,
            ChartData        = chartData,
            TopProducts      = topProducts,
            RevenueByStatus  = revByStatus,
            ChartType        = chartType,
            SelectedYear     = year,
            AvailableYears   = availableYears
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetRevenueData(string chartType = "month", int year = 0)
    {
        if (!IsAdmin()) return Json(new { success = false, message = "Access denied" });
        if (year == 0) year = DateTime.Now.Year;

        List<RevenueItem> chartData;
        var now = DateTime.UtcNow;

        if (chartType == "week")
        {
            var startWeek = now.AddDays(-6).Date;
            chartData = Enumerable.Range(0, 7).Select(i =>
            {
                var d = startWeek.AddDays(i);
                var rev = _db.Orders
                    .Where(o => o.CreatedAt.Date == d && o.Status != "cancelled")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                return new RevenueItem { Label = d.ToString("dd/MM"), Revenue = rev };
            }).ToList();
        }
        else if (chartType == "year")
        {
            var years = await _db.Orders.Select(o => o.CreatedAt.Year).Distinct().OrderByDescending(y => y).Take(5).ToListAsync();
            chartData = years.Select(y =>
            {
                var rev = _db.Orders.Where(o => o.CreatedAt.Year == y && o.Status != "cancelled").Sum(o => (decimal?)o.TotalAmount) ?? 0;
                return new RevenueItem { Label = y.ToString(), Revenue = rev };
            }).ToList();
        }
        else // month
        {
            chartData = Enumerable.Range(1, 12).Select(m =>
            {
                var rev = _db.Orders
                    .Where(o => o.CreatedAt.Year == year && o.CreatedAt.Month == m && o.Status != "cancelled")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                var cnt = _db.Orders.Count(o => o.CreatedAt.Year == year && o.CreatedAt.Month == m);
                return new RevenueItem { Label = $"T{m}", Revenue = rev, OrderCount = cnt };
            }).ToList();
        }

        return Json(new { 
            success = true, 
            labels = chartData.Select(d => d.Label).ToList(), 
            values = chartData.Select(d => d.Revenue).ToList() 
        });
    }

    // ══════════════════════════════════════════════════════════════
    //  PRODUCTS
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Products(string? search, string? brand, int page = 1)
    {
        if (!IsAdmin()) return Deny();
        const int pageSize = 15;

        var query = _db.Products.Include(p => p.Category).Include(p => p.PhoneSpecs).AsQueryable();
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search));
        if (!string.IsNullOrEmpty(brand))  query = query.Where(p => p.Brand == brand);

        var total    = await query.CountAsync();
        var products = await query.OrderByDescending(p => p.CreatedAt)
                                  .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search     = search;
        ViewBag.Brand      = brand;
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total      = total;
        return View(products);
    }

    public async Task<IActionResult> ProductCreate()
    {
        if (!IsAdmin()) return Deny();
        ViewBag.Categories = await _db.Categories.Where(c => c.Status == 1).ToListAsync();
        return View(new ProductFormViewModel { ReleaseYear = DateTime.Now.Year });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(ProductFormViewModel vm, IFormFile? Image)
    {
        if (!IsAdmin()) return Deny();
        ViewBag.Categories = await _db.Categories.Where(c => c.Status == 1).ToListAsync();
        if (!ModelState.IsValid) return View(vm);

        // Slug
        var slug = GenerateSlug(vm.Name);
        if (await _db.Products.AnyAsync(p => p.Slug == slug))
            slug = slug + "-" + Guid.NewGuid().ToString("N")[..6];

        var product = new Product
        {
            Name        = vm.Name,
            Description = vm.Description,
            Price       = vm.Price,
            CategoryId  = vm.CategoryId,
            Brand       = vm.Brand,
            Slug        = slug,
            Status      = vm.Status,
            Featured    = vm.Featured,
            Stock       = vm.Stock,
            Image       = await SaveImageAsync(Image)
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // PhoneSpecs
        _db.PhoneSpecs.Add(new PhoneSpecs
        {
            ProductId      = product.Id,
            Chipset        = vm.Chipset,
            RamGb          = vm.RamGb,
            StorageGb      = vm.StorageGb,
            DisplaySize    = vm.DisplaySize,
            DisplayType    = vm.DisplayType,
            RefreshRate    = vm.RefreshRate,
            MainCamera     = vm.MainCamera,
            FrontCamera    = vm.FrontCamera,
            BatteryMah     = vm.BatteryMah,
            FastCharge     = vm.FastCharge,
            WirelessCharge = vm.WirelessCharge,
            Has5G          = vm.Has5G,
            HasNfc         = vm.HasNfc,
            Os             = vm.Os,
            Colors         = vm.Colors,
            Dimensions     = vm.Dimensions,
            WeightGrams    = vm.WeightGrams,
            ReleaseYear    = vm.ReleaseYear
        });

        // UseCaseTags
        if (!string.IsNullOrEmpty(vm.UseCaseTags))
        {
            foreach (var tag in vm.UseCaseTags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                _db.UseCaseTags.Add(new UseCaseTag { ProductId = product.Id, Tag = tag.Trim(), Score = 8 });
        }

        // ProductVariants
        if (!string.IsNullOrEmpty(vm.VariantsJson))
        {
            var varItems = JsonSerializer.Deserialize<List<VariantFormItem>>(vm.VariantsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (varItems != null)
            {
                foreach (var vi in varItems)
                {
                    _db.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Color     = vi.Color?.Trim(),
                        StorageGb = vi.StorageGb,
                        RamGb     = vi.RamGb,
                        Price     = vi.Price,
                        Stock     = vi.Stock,
                        Status    = 1
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm sản phẩm thành công!";
        return RedirectToAction("Products");
    }

    public async Task<IActionResult> ProductEdit(int id)
    {
        if (!IsAdmin()) return Deny();
        var p = await _db.Products
                         .Include(x => x.PhoneSpecs)
                         .Include(x => x.UseCaseTags)
                         .Include(x => x.ProductVariants)
                         .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        ViewBag.Categories = await _db.Categories.Where(c => c.Status == 1).ToListAsync();
        var s = p.PhoneSpecs;

        var variantItems = p.ProductVariants.Select(v => new VariantFormItem
        {
            Id        = v.Id,
            Color     = v.Color,
            StorageGb = v.StorageGb,
            RamGb     = v.RamGb,
            Price     = v.Price,
            Stock     = v.Stock
        }).ToList();

        return View("ProductCreate", new ProductFormViewModel
        {
            Id             = p.Id,
            Name           = p.Name,
            Description    = p.Description,
            Price          = p.Price,
            CategoryId     = p.CategoryId,
            Brand          = p.Brand,
            Slug           = p.Slug,
            Status         = p.Status,
            Featured       = p.Featured,
            Stock          = p.Stock,
            ExistingImage  = p.Image,
            Chipset        = s?.Chipset,
            RamGb          = s?.RamGb ?? 0,
            StorageGb      = s?.StorageGb ?? 0,
            DisplaySize    = s?.DisplaySize,
            DisplayType    = s?.DisplayType,
            RefreshRate    = s?.RefreshRate ?? 60,
            MainCamera     = s?.MainCamera,
            FrontCamera    = s?.FrontCamera,
            BatteryMah     = s?.BatteryMah ?? 0,
            FastCharge     = s?.FastCharge ?? false,
            WirelessCharge = s?.WirelessCharge ?? false,
            Has5G          = s?.Has5G ?? true,
            HasNfc         = s?.HasNfc ?? true,
            Os             = s?.Os,
            Colors         = s?.Colors,
            Dimensions     = s?.Dimensions,
            WeightGrams    = s?.WeightGrams ?? 0,
            ReleaseYear    = s?.ReleaseYear ?? DateTime.Now.Year,
            UseCaseTags    = string.Join(",", p.UseCaseTags.Select(t => t.Tag)),
            VariantsJson   = JsonSerializer.Serialize(variantItems)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductEdit(int id, ProductFormViewModel vm, IFormFile? Image)
    {
        if (!IsAdmin()) return Deny();
        ViewBag.Categories = await _db.Categories.Where(c => c.Status == 1).ToListAsync();
        if (!ModelState.IsValid) return View("ProductCreate", vm);

        var product = await _db.Products.Include(p => p.PhoneSpecs).Include(p => p.UseCaseTags)
                                        .Include(p => p.ProductVariants)
                                        .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        product.Name        = vm.Name;
        product.Description = vm.Description;
        product.Price       = vm.Price;
        product.CategoryId  = vm.CategoryId;
        product.Brand       = vm.Brand;
        product.Status      = vm.Status;
        product.Featured    = vm.Featured;
        product.Stock       = vm.Stock;

        if (vm.RemoveImage)        product.Image = null;
        else if (Image != null)    product.Image = await SaveImageAsync(Image);

        // Upsert PhoneSpecs
        var specs = product.PhoneSpecs ?? new PhoneSpecs { ProductId = product.Id };
        specs.Chipset = vm.Chipset; specs.RamGb = vm.RamGb; specs.StorageGb = vm.StorageGb;
        specs.DisplaySize = vm.DisplaySize; specs.DisplayType = vm.DisplayType; specs.RefreshRate = vm.RefreshRate;
        specs.MainCamera = vm.MainCamera; specs.FrontCamera = vm.FrontCamera;
        specs.BatteryMah = vm.BatteryMah; specs.FastCharge = vm.FastCharge; specs.WirelessCharge = vm.WirelessCharge;
        specs.Has5G = vm.Has5G; specs.HasNfc = vm.HasNfc; specs.Os = vm.Os;
        specs.Colors = vm.Colors; specs.Dimensions = vm.Dimensions;
        specs.WeightGrams = vm.WeightGrams; specs.ReleaseYear = vm.ReleaseYear;
        if (product.PhoneSpecs == null) _db.PhoneSpecs.Add(specs);

        // Update UseCaseTags
        _db.UseCaseTags.RemoveRange(product.UseCaseTags);
        if (!string.IsNullOrEmpty(vm.UseCaseTags))
        {
            foreach (var tag in vm.UseCaseTags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                _db.UseCaseTags.Add(new UseCaseTag { ProductId = product.Id, Tag = tag.Trim(), Score = 8 });
        }

        // Update ProductVariants - remove old, add new
        _db.ProductVariants.RemoveRange(product.ProductVariants);
        if (!string.IsNullOrEmpty(vm.VariantsJson))
        {
            var varItems = JsonSerializer.Deserialize<List<VariantFormItem>>(vm.VariantsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (varItems != null)
            {
                foreach (var vi in varItems)
                {
                    _db.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.Id,
                        Color     = vi.Color?.Trim(),
                        StorageGb = vi.StorageGb,
                        RamGb     = vi.RamGb,
                        Price     = vi.Price,
                        Stock     = vi.Stock,
                        Status    = 1
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật sản phẩm thành công!";
        return RedirectToAction("Products");
    }

    [HttpPost]
    public async Task<IActionResult> ProductDelete(int id)
    {
        if (!IsAdmin()) return Deny();
        var p = await _db.Products.FindAsync(id);
        if (p != null) { p.Status = 0; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Đã ẩn sản phẩm";
        return RedirectToAction("Products");
    }

    // ══════════════════════════════════════════════════════════════
    //  ORDERS
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Orders(string? status, int page = 1)
    {
        if (!IsAdmin()) return Deny();
        const int pageSize = 15;

        var query = _db.Orders.Include(o => o.User).AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

        var total  = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
                                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Status     = status;
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total      = total;
        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequest req)
    {
        if (!IsAdmin()) return Json(new { success = false });
        var order = await _db.Orders.FindAsync(req.Id);
        if (order == null) return Json(new { success = false });
        order.Status = req.Status;
        await _db.SaveChangesAsync();
        return Json(new { success = true, status = req.Status });
    }

    // ══════════════════════════════════════════════════════════════
    //  REVIEWS
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Reviews(string? search, int? rating, int page = 1)
    {
        if (!IsAdmin()) return Deny();
        const int pageSize = 15;

        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Product!.Name.Contains(search) || r.User!.Name.Contains(search) || (r.Comment != null && r.Comment.Contains(search)));
        if (rating.HasValue)
            query = query.Where(r => r.Rating == rating.Value);

        var total  = await query.CountAsync();
        var reviews = await query.OrderByDescending(r => r.CreatedAt)
                                 .Skip((page - 1) * pageSize).Take(pageSize)
                                 .ToListAsync();

        ViewBag.Search     = search;
        ViewBag.Rating     = rating;
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total      = total;
        return View(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReview([FromBody] DeleteByIdRequest req)
    {
        if (!IsAdmin()) return Json(new { success = false });
        var review = await _db.Reviews.FindAsync(req.Id);
        if (review == null) return Json(new { success = false });
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // ══════════════════════════════════════════════════════════════
    //  USERS
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Users(string? search, int page = 1)
    {
        if (!IsAdmin()) return Deny();
        const int pageSize = 15;

        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();
        var users = await query.OrderByDescending(u => u.CreatedAt)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Search     = search;
        ViewBag.Page       = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Total      = total;
        return View(users);
    }

    // ══════════════════════════════════════════════════════════════
    //  CATEGORIES
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Categories()
    {
        if (!IsAdmin()) return Deny();
        var cats = await _db.Categories.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(cats);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CategorySave(CategoryFormViewModel vm)
    {
        if (!IsAdmin()) return Deny();
        if (vm.Id == 0)
        {
            _db.Categories.Add(new Category { Name = vm.Name, Description = vm.Description, Status = vm.Status });
        }
        else
        {
            var cat = await _db.Categories.FindAsync(vm.Id);
            if (cat != null) { cat.Name = vm.Name; cat.Description = vm.Description; cat.Status = vm.Status; }
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = vm.Id == 0 ? "Thêm danh mục thành công!" : "Cập nhật danh mục thành công!";
        return RedirectToAction("Categories");
    }

    // ══════════════════════════════════════════════════════════════
    //  COUPONS
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Coupons()
    {
        if (!IsAdmin()) return Deny();
        var coupons = await _db.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(coupons);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CouponSave(CouponFormViewModel vm)
    {
        if (!IsAdmin()) return Deny();
        if (vm.Id == 0)
        {
            _db.Coupons.Add(new Coupon
            {
                Code = vm.Code, Type = vm.Type, Value = vm.Value,
                MinOrder = vm.MinOrder, MaxDiscount = vm.MaxDiscount,
                MaxUsage = vm.MaxUsage, Status = vm.Status, ExpiresAt = vm.ExpiresAt
            });
        }
        else
        {
            var c = await _db.Coupons.FindAsync(vm.Id);
            if (c != null)
            {
                c.Code = vm.Code; c.Type = vm.Type; c.Value = vm.Value;
                c.MinOrder = vm.MinOrder; c.MaxDiscount = vm.MaxDiscount;
                c.MaxUsage = vm.MaxUsage; c.Status = vm.Status; c.ExpiresAt = vm.ExpiresAt;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Lưu coupon thành công!";
        return RedirectToAction("Coupons");
    }

    // ── Helpers ───────────────────────────────────────────────────
    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant()
            .Replace("đ", "d")
            .Replace(" ", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
        Directory.CreateDirectory(folder);
        var ext      = Path.GetExtension(file.FileName);
        var fileName = Guid.NewGuid().ToString("N") + ext;
        var path     = Path.Combine(folder, fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/images/products/" + fileName;
    }

    // ══════════════════════════════════════════════════════════════
    //  MINIGAME & SURVEY MANAGEMENT
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Minigame()
    {
        if (!IsAdmin()) return Deny();
        
        var questions = await _db.SurveyQuestions
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync();
            
        var prizes = await _db.LuckyWheelPrizes
            .ToListAsync();
            
        ViewBag.Prizes = prizes;
        return View(questions);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(string questionText, string options, int displayOrder)
    {
        if (!IsAdmin()) return Deny();
        if (!string.IsNullOrWhiteSpace(questionText) && !string.IsNullOrWhiteSpace(options))
        {
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                QuestionText = questionText.Trim(),
                Options = options.Trim(),
                IsActive = true,
                DisplayOrder = displayOrder,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thêm câu hỏi khảo sát thành công!";
        }
        else
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin câu hỏi và các lựa chọn!";
        }
        return RedirectToAction("Minigame");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int id, string questionText, string options, bool isActive, int displayOrder)
    {
        if (!IsAdmin()) return Deny();
        var q = await _db.SurveyQuestions.FindAsync(id);
        if (q != null)
        {
            q.QuestionText = questionText.Trim();
            q.Options = options.Trim();
            q.IsActive = isActive;
            q.DisplayOrder = displayOrder;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật câu hỏi thành công!";
        }
        return RedirectToAction("Minigame");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        if (!IsAdmin()) return Deny();
        var q = await _db.SurveyQuestions.FindAsync(id);
        if (q != null)
        {
            _db.SurveyQuestions.Remove(q);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Xóa câu hỏi thành công!";
        }
        return RedirectToAction("Minigame");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePrize(int id, string name, int weight, string couponType, decimal couponValue, string colorHex, int status)
    {
        if (!IsAdmin()) return Deny();
        var prize = await _db.LuckyWheelPrizes.FindAsync(id);
        if (prize != null)
        {
            prize.Name = name.Trim();
            prize.Weight = weight;
            prize.CouponType = couponType.Trim();
            prize.CouponValue = couponValue;
            prize.ColorHex = colorHex.Trim();
            prize.Status = status;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật giải thưởng thành công!";
        }
        return RedirectToAction("Minigame");
    }

    public async Task<IActionResult> ExportSurveyCsv(int month, int year)
    {
        if (!IsAdmin()) return Deny();

        var surveys = await _db.Surveys
            .Include(s => s.Responses)
            .ThenInclude(r => r.Question)
            .Include(s => s.User)
            .Where(s => s.CreatedAt.Month == month && s.CreatedAt.Year == year)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        var csvBuilder = new System.Text.StringBuilder();
        // Prepend UTF-8 BOM so Excel opens it correctly with accents
        csvBuilder.Append('\uFEFF');
        csvBuilder.AppendLine("ID Khảo Sát,Ngày Tạo,Tài Khoản,Tuổi,Giới Tính,Câu Hỏi,Câu Trả Lời");

        foreach (var s in surveys)
        {
            var email = s.User?.Email;
            var ip = s.IpAddress ?? "N/A";
            var dateStr = s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            var userText = string.IsNullOrEmpty(email) ? $"Khách ({ip})" : email;
            
            foreach (var r in s.Responses)
            {
                var qText = r.Question?.QuestionText ?? "Câu hỏi đã bị xóa";
                var escapedQ = $"\"{qText.Replace("\"", "\"\"")}\"";
                var escapedAns = $"\"{r.AnswerText.Replace("\"", "\"\"")}\"";
                
                csvBuilder.AppendLine($"{s.Id},{dateStr},{userText},{s.Age},{s.Gender},{escapedQ},{escapedAns}");
            }
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(bytes, "text/csv", $"BaoCaoKhaoSat_{month:D2}_{year}.csv");
    }
}

public class UpdateOrderStatusRequest
{
    public int    Id     { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DeleteByIdRequest
{
    public int Id { get; set; }
}