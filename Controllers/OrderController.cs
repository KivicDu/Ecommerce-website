using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;

namespace HutechStore.Controllers;

public class OrderController : Controller
{
    private readonly AppDbContext _db;
    public OrderController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> History()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var orders = await _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Detail(int id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var order  = await _db.Orders
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null) { TempData["Error"] = "Không tìm thấy đơn hàng"; return RedirectToAction("History"); }
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var userId = SessionHelper.GetUserId(HttpContext.Session)!.Value;
        var order  = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order == null || order.Status != "pending")
        {
            TempData["Error"] = "Không thể hủy đơn hàng này";
            return RedirectToAction("History");
        }

        order.Status = "cancelled";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã hủy đơn hàng thành công";
        return RedirectToAction("History");
    }
}
