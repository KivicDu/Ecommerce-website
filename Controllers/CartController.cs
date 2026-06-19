using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.Services;
using HutechStore.ViewModels;

namespace HutechStore.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext   _db;
    private readonly CartService    _cart;
    private readonly IConfiguration _config;
    private readonly MailService    _mail;

    public CartController(AppDbContext db, CartService cart, IConfiguration config, MailService mail)
    { _db = db; _cart = cart; _config = config; _mail = mail; }

    // GET /Cart
    public async Task<IActionResult> Index()
    {
        var cart  = await _cart.GetOrCreateCartAsync();
        var items = await _cart.GetCartItemsAsync(cart.Id);
        ViewBag.Cart = cart;
        return View(items);
    }

    // POST /Cart/AddItem  (AJAX — dùng trong site.js)
    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest req)
    {
        var product = await _db.Products.FindAsync(req.ProductId);
        if (product == null || product.Status != 1)
            return Json(new { success = false, message = "Sản phẩm không tồn tại" });

        // Kiểm tra tồn kho
        int stock = product.Stock;
        decimal price = product.Price;

        if (req.VariantId.HasValue)
        {
            var variant = await _db.ProductVariants.FindAsync(req.VariantId.Value);
            if (variant != null)
            {
                stock = variant.Stock;
                price = variant.Price;
            }
        }

        if (stock <= 0)
            return Json(new { success = false, message = "Sản phẩm đã hết hàng" });

        var quantity = Math.Max(1, Math.Min(99, req.Quantity));
        var cart     = await _cart.GetOrCreateCartAsync();

        var existing = await _db.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId    == cart.Id &&
            ci.ProductId == req.ProductId &&
            ci.VariantId == req.VariantId);

        if (existing != null)
            existing.Quantity = Math.Min(99, existing.Quantity + quantity);
        else
            _db.CartItems.Add(new CartItem
            {
                CartId    = cart.Id,
                ProductId = req.ProductId,
                VariantId = req.VariantId,
                Quantity  = quantity,
                Price     = price
            });

        await _db.SaveChangesAsync();

        var count = await _cart.GetCartCountAsync();
        HttpContext.Session.SetInt32("CartCount", count);

        return Json(new { success = true, cartCount = count, message = "Đã thêm vào giỏ hàng" });
    }

    // POST /Cart/Add  (form thường — backward compat)
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var req = new AddItemRequest { ProductId = productId, Quantity = quantity };
        return await AddItem(req);
    }

    // POST /Cart/Update  (AJAX)
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] CartUpdateRequest req)
    {
        var itemId   = req.ItemId;
        var quantity = Math.Max(1, Math.Min(99, req.Quantity));
        var item = await _db.CartItems.Include(ci => ci.Cart)
                                      .FirstOrDefaultAsync(ci => ci.Id == itemId);
        if (item == null) return Json(new { success = false });

        item.Quantity = quantity;
        await _db.SaveChangesAsync();

        var count     = await _cart.GetCartCountAsync();
        var total     = await _db.CartItems.Where(ci => ci.CartId == item.CartId)
                                           .SumAsync(ci => ci.Quantity * ci.Price);
        var itemTotal = item.Quantity * item.Price;

        HttpContext.Session.SetInt32("CartCount", count);

        return Json(new
        {
            success    = true,
            cart_count = count,
            total      = total.ToString("N0") + "đ",
            item_total = itemTotal.ToString("N0") + "đ",
            quantity
        });
    }

    // POST /Cart/Remove  (AJAX)
    [HttpPost]
    public async Task<IActionResult> Remove([FromBody] CartRemoveRequest req)
    {
        var itemId = req.ItemId;
        var item = await _db.CartItems.FindAsync(itemId);
        if (item == null) return Json(new { success = false });

        var cartId = item.CartId;
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        var count = await _cart.GetCartCountAsync();
        var total = await _db.CartItems.Where(ci => ci.CartId == cartId)
                                       .SumAsync(ci => ci.Quantity * ci.Price);

        HttpContext.Session.SetInt32("CartCount", count);

        return Json(new { success = true, cart_count = count, total = total.ToString("N0") + "đ" });
    }

    // POST /Cart/Clear
    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var cart = await _cart.GetCurrentCartAsync();
        if (cart != null)
        {
            _db.CartItems.RemoveRange(_db.CartItems.Where(ci => ci.CartId == cart.Id));
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("CartCount", 0);
        }
        TempData["Success"] = "Đã xóa toàn bộ giỏ hàng";
        return RedirectToAction("Index");
    }

    // GET /Cart/Checkout
    public async Task<IActionResult> Checkout()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
        {
            HttpContext.Session.SetString("RedirectAfterLogin", "/Cart/Checkout");
            return RedirectToAction("Login", "Auth");
        }

        var cart  = await _cart.GetCurrentCartAsync();
        var items = cart != null ? await _cart.GetCartItemsAsync(cart.Id) : new();

        if (!items.Any())
        {
            TempData["Error"] = "Giỏ hàng trống";
            return RedirectToAction("Index");
        }

        var user = await _db.Users.FindAsync(SessionHelper.GetUserId(HttpContext.Session));

        var discount   = HttpContext.Session.GetInt32("CouponDiscount") ?? 0;
        var total      = items.Sum(i => i.Quantity * i.Price);

        ViewBag.CartItems  = items;
        ViewBag.CartTotal  = total;
        ViewBag.Discount   = discount;
        ViewBag.FinalTotal = Math.Max(0, total - discount);
        ViewBag.CouponCode = HttpContext.Session.GetString("CouponCode");

        return View(new CheckoutViewModel
        {
            Name    = user?.Name    ?? "",
            Phone   = user?.Phone   ?? "",
            Address = user?.Address ?? ""
        });
    }

    // POST /Cart/PlaceOrder
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var cart  = await _cart.GetCurrentCartAsync();
        var items = cart != null ? await _cart.GetCartItemsAsync(cart.Id) : new();

        if (!items.Any())
        {
            TempData["Error"] = "Giỏ hàng trống";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            var t = items.Sum(i => i.Quantity * i.Price);
            var d = HttpContext.Session.GetInt32("CouponDiscount") ?? 0;
            ViewBag.CartItems  = items;
            ViewBag.CartTotal  = t;
            ViewBag.Discount   = d;
            ViewBag.FinalTotal = Math.Max(0, t - d);
            return View("Checkout", vm);
        }

        var userId        = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var couponCode    = HttpContext.Session.GetString("CouponCode");
        var discountAmt   = HttpContext.Session.GetInt32("CouponDiscount") ?? 0;
        var subTotal      = items.Sum(i => i.Quantity * i.Price);
        var finalTotal    = Math.Max(0, subTotal - discountAmt);
        var orderNumber   = "HS" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

        var order = new Order
        {
            UserId          = userId,
            OrderNumber     = orderNumber,
            TotalAmount     = finalTotal,
            ShippingAddress = vm.Address,
            ShippingPhone   = vm.Phone,
            ShippingName    = vm.Name,
            PaymentMethod   = vm.PaymentMethod,
            Note            = vm.Note,
            CouponCode      = couponCode,
            DiscountAmount  = discountAmt
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var item in items)
        {
            _db.OrderItems.Add(new OrderItem
            {
                OrderId     = order.Id,
                ProductId   = item.ProductId,
                VariantId   = item.VariantId,
                ProductName = item.Product?.Name ?? "",
                Quantity    = item.Quantity,
                Price       = item.Price,
                Total       = item.Quantity * item.Price
            });
        }

        // Giảm tồn kho
        foreach (var item in items)
        {
            if (item.VariantId.HasValue)
            {
                var v = await _db.ProductVariants.FindAsync(item.VariantId.Value);
                if (v != null) v.Stock = Math.Max(0, v.Stock - item.Quantity);
            }
            else
            {
                var p = await _db.Products.FindAsync(item.ProductId);
                if (p != null) p.Stock = Math.Max(0, p.Stock - item.Quantity);
            }
        }

        // Coupon usage
        if (!string.IsNullOrEmpty(couponCode))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
            if (coupon != null) coupon.UsedCount++;
        }

        // Xóa giỏ
        _db.CartItems.RemoveRange(_db.CartItems.Where(ci => ci.CartId == cart!.Id));
        HttpContext.Session.Remove("CouponCode");
        HttpContext.Session.Remove("CouponDiscount");
        HttpContext.Session.SetInt32("CartCount", 0);
        await _db.SaveChangesAsync();

        // Gửi email xác nhận đặt hàng (chạy nền bất đồng bộ)
        var userRecord = await _db.Users.FindAsync(userId);
        if (userRecord != null && !string.IsNullOrEmpty(userRecord.Email))
        {
            var orderItemsList = await _db.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mail.SendOrderConfirmationEmailAsync(userRecord.Email, order, orderItemsList);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi gửi email xác nhận đặt hàng: " + ex.Message);
                }
            });
        }

        TempData["Success"] = $"Đặt hàng thành công! Mã đơn: {orderNumber}";
        return RedirectToAction("Success", new { id = order.Id });
    }

    // GET /Cart/Success/5
    public async Task<IActionResult> Success(int id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var order  = await _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null)
        {
            TempData["Error"] = "Không tìm thấy đơn hàng";
            return RedirectToAction("Index", "Home");
        }

        // Bank transfer info for QR code
        ViewBag.BankCode      = _config["BankPayment:BankCode"]      ?? "MB";
        ViewBag.AccountNumber = _config["BankPayment:AccountNumber"]  ?? "0123456789";
        ViewBag.AccountName   = _config["BankPayment:AccountName"]    ?? "HUTECH STORE";
        ViewBag.BankTemplate  = _config["BankPayment:Template"]       ?? "compact";

        return View(order);
    }

    // POST /Cart/ApplyCoupon  (AJAX)
    [HttpPost]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest req)
    {
        var cart  = await _cart.GetCurrentCartAsync();
        if (cart == null) return Json(new { success = false, message = "Giỏ hàng trống" });

        var items = await _cart.GetCartItemsAsync(cart.Id);
        var total = items.Sum(i => i.Quantity * i.Price);

        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.Code == req.Code && c.Status == 1);

        if (coupon == null)
            return Json(new { success = false, message = "Mã coupon không hợp lệ" });
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return Json(new { success = false, message = "Mã coupon đã hết hạn" });
        if (coupon.MaxUsage > 0 && coupon.UsedCount >= coupon.MaxUsage)
            return Json(new { success = false, message = "Mã coupon đã hết lượt sử dụng" });
        if (total < coupon.MinOrder)
            return Json(new { success = false, message = $"Đơn hàng tối thiểu {coupon.MinOrder:N0}đ" });

        // Brand-based restrictions (e.g. iphone200k cannot be applied to Samsung)
        var codeLower = req.Code.ToLowerInvariant();
        if (codeLower.Contains("iphone") || codeLower.Contains("apple"))
        {
            var hasAppleProduct = items.Any(i => i.Product != null && i.Product.Brand.Equals("Apple", StringComparison.OrdinalIgnoreCase));
            if (!hasAppleProduct)
            {
                return Json(new { success = false, message = "Mã giảm giá này chỉ áp dụng cho các sản phẩm Apple (iPhone)." });
            }
        }
        else if (codeLower.Contains("samsung") || codeLower.Contains("galaxy"))
        {
            var hasSamsungProduct = items.Any(i => i.Product != null && i.Product.Brand.Equals("Samsung", StringComparison.OrdinalIgnoreCase));
            if (!hasSamsungProduct)
            {
                return Json(new { success = false, message = "Mã giảm giá này chỉ áp dụng cho các sản phẩm Samsung." });
            }
        }

        decimal discount = coupon.Type == "percent"
            ? total * coupon.Value / 100
            : coupon.Value;

        if (coupon.MaxDiscount > 0 && discount > coupon.MaxDiscount)
            discount = coupon.MaxDiscount;

        HttpContext.Session.SetString("CouponCode", req.Code);
        HttpContext.Session.SetInt32("CouponDiscount", (int)discount);

        return Json(new
        {
            success     = true,
            message     = "Áp dụng mã giảm giá thành công!",
            discount    = discount.ToString("N0") + "đ",
            final_total = (total - discount).ToString("N0") + "đ"
        });
    }
}

public class AddItemRequest
{
    public int  ProductId { get; set; }
    public int? VariantId { get; set; }
    public int  Quantity  { get; set; } = 1;
}

public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
}

public class CartUpdateRequest
{
    public int ItemId   { get; set; }
    public int Quantity { get; set; }
}

public class CartRemoveRequest
{
    public int ItemId { get; set; }
}