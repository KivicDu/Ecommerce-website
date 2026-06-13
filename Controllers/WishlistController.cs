using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.ViewModels;

namespace HutechStore.Controllers;

public class WishlistController : Controller
{
    private readonly AppDbContext _db;
    public WishlistController(AppDbContext db) { _db = db; }

    // GET /Wishlist
    public async Task<IActionResult> Index()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var items  = await _db.Wishlists
            .Include(w => w.Product)
                .ThenInclude(p => p!.Reviews)
            .Include(w => w.Product)
                .ThenInclude(p => p!.PhoneSpecs)
            .Include(w => w.Product)
                .ThenInclude(p => p!.ProductVariants)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        // Lọc bỏ item có product null (product bị xóa)
        items = items.Where(w => w.Product != null).ToList();

        return View(items);
    }

    // POST /Wishlist/Toggle  (AJAX)
    [HttpPost]
    public async Task<IActionResult> Toggle([FromBody] WishlistToggleRequest req)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return Json(new { success = false, requireLogin = true });

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var exist  = await _db.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == req.ProductId);

        bool added;
        if (exist != null)
        {
            _db.Wishlists.Remove(exist);
            added = false;
        }
        else
        {
            _db.Wishlists.Add(new Wishlist { UserId = userId, ProductId = req.ProductId });
            added = true;
        }

        await _db.SaveChangesAsync();
        return Json(new { success = true, added });
    }

    // POST /Wishlist/Remove/5
    [HttpPost]
    public async Task<IActionResult> Remove(int id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var item   = await _db.Wishlists
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (item != null)
        {
            _db.Wishlists.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa khỏi danh sách yêu thích";
        }

        return RedirectToAction("Index");
    }
}