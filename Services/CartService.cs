using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;

namespace HutechStore.Services;

public class CartService
{
    private readonly AppDbContext        _db;
    private readonly IHttpContextAccessor _http;

    public CartService(AppDbContext db, IHttpContextAccessor http)
    { _db = db; _http = http; }

    private ISession Session => _http.HttpContext!.Session;

    // Lấy hoặc tạo cart mới
    public async Task<Cart> GetOrCreateCartAsync()
    {
        var userId    = SessionHelper.GetUserId(Session);
        var sessionId = Session.Id;

        Cart? cart = null;

        if (userId.HasValue)
            cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId.Value);

        cart ??= await _db.Carts.FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId, SessionId = sessionId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }

    // Lấy cart hiện tại (không tạo mới)
    public async Task<Cart?> GetCurrentCartAsync()
    {
        var userId    = SessionHelper.GetUserId(Session);
        var sessionId = Session.Id;

        if (userId.HasValue)
            return await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId.Value);

        return await _db.Carts.FirstOrDefaultAsync(c => c.SessionId == sessionId);
    }

    // Lấy danh sách items kèm product info
    public async Task<List<CartItem>> GetCartItemsAsync(int cartId)
    {
        return await _db.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p!.PhoneSpecs)
            .Include(ci => ci.Variant)
            .Where(ci => ci.CartId == cartId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync();
    }

    // Đếm số lượng items trong cart
    public async Task<int> GetCartCountAsync()
    {
        var cart = await GetCurrentCartAsync();
        if (cart == null) return 0;

        return await _db.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .SumAsync(ci => ci.Quantity);
    }

    // Merge cart ẩn danh vào cart của user sau khi đăng nhập
    public async Task MergeSessionCartAsync(int userId)
    {
        var sessionId   = Session.Id;
        var sessionCart = await _db.Carts.FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null);
        if (sessionCart == null) return;

        var userCart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (userCart == null)
        {
            sessionCart.UserId = userId;
            await _db.SaveChangesAsync();
            return;
        }

        // Merge items
        var sessionItems = await _db.CartItems
            .Where(ci => ci.CartId == sessionCart.Id)
            .ToListAsync();

        foreach (var item in sessionItems)
        {
            var existing = await _db.CartItems.FirstOrDefaultAsync(ci =>
                ci.CartId    == userCart.Id &&
                ci.ProductId == item.ProductId &&
                ci.VariantId == item.VariantId);

            if (existing != null)
                existing.Quantity = Math.Min(99, existing.Quantity + item.Quantity);
            else
                _db.CartItems.Add(new CartItem
                {
                    CartId    = userCart.Id,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity  = item.Quantity,
                    Price     = item.Price
                });
        }

        _db.CartItems.RemoveRange(sessionItems);
        _db.Carts.Remove(sessionCart);
        await _db.SaveChangesAsync();

        var count = await _db.CartItems
            .Where(ci => ci.CartId == userCart.Id)
            .SumAsync(ci => ci.Quantity);
        Session.SetInt32("CartCount", count);
    }
}
