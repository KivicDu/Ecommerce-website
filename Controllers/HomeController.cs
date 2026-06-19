using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.Services;

namespace HutechStore.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService  _cart;

    public HomeController(AppDbContext db, CartService cart)
    { _db = db; _cart = cart; }

    public async Task<IActionResult> Index()
    {
        // Cập nhật CartCount vào session
        var count = await _cart.GetCartCountAsync();
        HttpContext.Session.SetInt32("CartCount", count);

        var featured = await _db.Products
            .Include(p => p.Reviews)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.ProductVariants.Where(v => v.Status == 1))
            .Where(p => p.Status == 1 && p.Featured == 1)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        var iphones = await _db.Products
            .Include(p => p.Reviews)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.ProductVariants.Where(v => v.Status == 1))
            .Where(p => p.Status == 1 && p.Brand == "Apple")
            .OrderByDescending(p => p.Price)
            .Take(10)
            .ToListAsync();

        var samsungs = await _db.Products
            .Include(p => p.Reviews)
            .Include(p => p.PhoneSpecs)
            .Include(p => p.ProductVariants.Where(v => v.Status == 1))
            .Where(p => p.Status == 1 && p.Brand == "Samsung")
            .OrderByDescending(p => p.Price)
            .Take(10)
            .ToListAsync();

        ViewBag.Featured = featured;
        ViewBag.IPhones  = iphones;
        ViewBag.Samsungs = samsungs;

        // Wishlist ids để ProductCard hiện trạng thái tim đúng
        ViewBag.WishlistIds = await GetWishlistIdsAsync();

        return View();
    }

    private async Task<HashSet<int>> GetWishlistIdsAsync()
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return new HashSet<int>();
        return (await _db.Wishlists
            .Where(w => w.UserId == userId.Value)
            .Select(w => w.ProductId)
            .ToListAsync()).ToHashSet();
    }

    public IActionResult About()   => View();
    public IActionResult Contact() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
