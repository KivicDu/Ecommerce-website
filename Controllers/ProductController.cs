using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.ViewModels;

namespace HutechStore.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _db;
    public ProductController(AppDbContext db) { _db = db; }

    // GET /Product  — danh sách + filter nâng cao
    public async Task<IActionResult> Index(ProductIndexViewModel filter)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.ProductVariants.Where(v => v.Status == 1))
            .Include(p => p.UseCaseTags)
            .Where(p => p.Status == 1)
            .AsQueryable();

        // ── FILTERS ──────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand == filter.Brand);

        if (filter.Category.HasValue)
            query = query.Where(p => p.CategoryId == filter.Category.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.UseCase))
            query = query.Where(p => p.UseCaseTags.Any(t => t.Tag == filter.UseCase));

        if (filter.MinRam.HasValue)
            query = query.Where(p => p.PhoneSpecs != null && p.PhoneSpecs.RamGb >= filter.MinRam.Value);

        if (filter.Has5G == true)
            query = query.Where(p => p.PhoneSpecs != null && p.PhoneSpecs.Has5G);

        // ── SORT ──────────────────────────────────────────────────────
        query = filter.Sort switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name"       => query.OrderBy(p => p.Name),
            "rating"     => query.OrderByDescending(p => p.Reviews.Average(r => (double?)r.Rating) ?? 0),
            _            => query.OrderByDescending(p => p.CreatedAt)  // latest
        };

        // ── PAGINATION ────────────────────────────────────────────────
        const int pageSize = 12;
        var total      = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);
        var page       = Math.Max(1, filter.CurrentPage);

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await _db.Categories.Where(c => c.Status == 1).ToListAsync();

        filter.Products    = products;
        filter.Categories  = categories;
        filter.TotalItems  = total;
        filter.TotalPages  = totalPages;
        filter.CurrentPage = page;
        filter.PageSize    = pageSize;

        // Wishlist ids để ProductCard hiện trạng thái tim đúng
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        ViewBag.WishlistIds = userId == null
            ? new HashSet<int>()
            : (await _db.Wishlists
                .Where(w => w.UserId == userId.Value)
                .Select(w => w.ProductId)
                .ToListAsync()).ToHashSet();

        return View(filter);
    }

    // GET /Product/Detail/{slug-or-id}
    public async Task<IActionResult> Detail(string id)
    {
        // Thử tìm theo slug trước, sau đó theo Id
        Product? product = null;

        if (int.TryParse(id, out int productId))
            product = await GetProductWithIncludes().FirstOrDefaultAsync(p => p.Id == productId);

        product ??= await GetProductWithIncludes().FirstOrDefaultAsync(p => p.Slug == id);

        if (product == null || product.Status != 1)
        {
            TempData["Error"] = "Sản phẩm không tồn tại";
            return RedirectToAction("Index");
        }

        // Wishlist check
        bool inWishlist = false;
        var  userId     = SessionHelper.GetUserId(HttpContext.Session);
        if (userId.HasValue)
            inWishlist = await _db.Wishlists
                .AnyAsync(w => w.UserId == userId.Value && w.ProductId == product.Id);

        // Related (cùng category, khác sản phẩm)
        var related = await _db.Products
            .Include(p => p.Reviews)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.ProductVariants.Where(v => v.Status == 1))
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == 1)
            .OrderByDescending(p => p.Featured)
            .Take(4)
            .ToListAsync();

        var vm = new ProductDetailViewModel
        {
            Product    = product,
            AvgRating  = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            IsLoggedIn = SessionHelper.IsLoggedIn(HttpContext.Session),
            InWishlist = inWishlist,
            Related    = related
        };

        return View(vm);
    }

    // POST /Product/AddReview
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int productId, int rating, string? comment)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;

        var existing = await _db.Reviews
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

        if (existing)
        {
            TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi";
        }
        else if (rating < 1 || rating > 5)
        {
            TempData["Error"] = "Vui lòng chọn số sao";
        }
        else
        {
            _db.Reviews.Add(new Review
            {
                ProductId = productId,
                UserId    = userId,
                Rating    = rating,
                Comment   = comment
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        var slug    = product?.Slug ?? productId.ToString();
        return RedirectToAction("Detail", new { id = slug });
    }

    // GET /Product/Compare?ids=1,2,3
    public async Task<IActionResult> Compare(string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            TempData["Error"] = "Vui lòng chọn sản phẩm để so sánh";
            return RedirectToAction("Index");
        }

        var idList = ids.Split(',')
                        .Select(x => int.TryParse(x.Trim(), out int val) ? val : 0)
                        .Where(val => val > 0)
                        .Distinct()
                        .Take(3)
                        .ToList();

        if (!idList.Any())
        {
            TempData["Error"] = "Sản phẩm so sánh không hợp lệ";
            return RedirectToAction("Index");
        }

        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.Reviews)
            .Where(p => idList.Contains(p.Id) && p.Status == 1)
            .ToListAsync();

        // Sắp xếp lại theo thứ tự ids truyền vào ban đầu
        var orderedProducts = idList
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Cast<Product>()
            .ToList();

        if (!orderedProducts.Any())
        {
            TempData["Error"] = "Không tìm thấy sản phẩm cần so sánh";
            return RedirectToAction("Index");
        }

        return View(orderedProducts);
    }

    // GET /Product/GetCompareList?currentProductId=5
    [HttpGet]
    public async Task<IActionResult> GetCompareList(int currentProductId)
    {
        var list = await _db.Products
            .Where(p => p.Status == 1 && p.Id != currentProductId)
            .Select(p => new {
                id = p.Id,
                name = p.Name,
                image = p.Image ?? "/images/no-image.png",
                brand = p.Brand
            })
            .ToListAsync();

        return Json(list);
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private IQueryable<Product> GetProductWithIncludes() =>
        _db.Products
           .Include(p => p.Category)
           .Include(p => p.Reviews).ThenInclude(r => r.User)
           .Include(p => p.PhoneSpecs)
           .Include(p => p.ProductVariants.Where(v => v.Status == 1))
           .Include(p => p.UseCaseTags)
           .Include(p => p.ProductImages.OrderBy(i => i.SortOrder));
}
