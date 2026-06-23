using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using System.Text.Json;

namespace HutechStore.Controllers;

[ApiController]
[Route("api/order")]
public class OrderApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrderApiController(AppDbContext db) { _db = db; }

    // ── Helper: lấy secret key cho mã hóa dựa trên order + session ──────
    private string GetSecretKey(Order order)
    {
        var sessionId = HttpContext.Session.Id;
        return SecurityHelper.GetOrderSecretKey(order.OrderNumber, sessionId);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/order/tracking/{id}  — Trả về dữ liệu tracking đã mã hóa
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("tracking/{id}")]
    public async Task<IActionResult> GetTracking(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return Unauthorized();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();

        var key = GetSecretKey(order);

        // Tọa độ cửa hàng HutechStore (Trường HUTECH, Bình Thạnh, TP.HCM)
        const double storeLat = 10.8018;
        const double storeLng = 106.7118;

        var trackingData = new
        {
            orderId      = order.Id,
            orderNumber  = order.OrderNumber,
            status       = order.Status,
            storeLat,
            storeLng,
            // Mã hóa tọa độ khách hàng
            customerLat  = SecurityHelper.Encrypt((order.Latitude  ?? 10.7769).ToString(), key),
            customerLng  = SecurityHelper.Encrypt((order.Longitude ?? 106.7009).ToString(), key),
            address      = order.ShippingAddress,
            shipperName  = order.ShippingName,
            pod          = order.ProofOfDeliveryImage,
            shipperRating = order.ShipperRating,
            createdAt    = order.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        return Ok(trackingData);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GET /api/order/decrypt-key/{id}  — Trả về khóa giải mã cho client
    //  (Chỉ trả về 1 lần cho đúng user sở hữu đơn hàng)
    // ══════════════════════════════════════════════════════════════════════
    [HttpGet("decrypt-key/{id}")]
    public async Task<IActionResult> GetDecryptKey(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return Unauthorized();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();

        var key = GetSecretKey(order);
        var (keyHex, saltHex) = SecurityHelper.GetKeyMaterialForClient(key);

        return Ok(new { keyHex, saltHex, iterations = 10000 });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POST /api/order/shipper-simulate/{id}  — Giả lập shipper giao hàng
    //  Dùng cho nút "Bắt đầu giả lập giao hàng" trên trang Detail
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost("shipper-simulate/{id}")]
    public async Task<IActionResult> ShipperSimulate(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return Unauthorized();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

        // Chuyển trạng thái thành shipping
        if (order.Status == "pending" || order.Status == "confirmed")
        {
            order.Status = "shipping";
            await _db.SaveChangesAsync();
        }

        return Ok(new { success = true, status = order.Status });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POST /api/order/complete-delivery/{id}  — Shipper xác nhận giao xong
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost("complete-delivery/{id}")]
    public async Task<IActionResult> CompleteDelivery(int id)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return Unauthorized();

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();

        order.Status = "delivered";
        // Giả lập ảnh chụp POD
        order.ProofOfDeliveryImage = "/images/pod/proof_of_delivery.png";
        await _db.SaveChangesAsync();

        return Ok(new { success = true, podImage = order.ProofOfDeliveryImage });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POST /api/order/submit-review/{id}  — Đánh giá kép (sản phẩm + shipper)
    // ══════════════════════════════════════════════════════════════════════
    [HttpPost("submit-review/{id}")]
    public async Task<IActionResult> SubmitReview(int id, [FromBody] ReviewSubmitRequest req)
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        if (userId == null) return Unauthorized();

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();
        if (order.Status != "delivered") return BadRequest(new { message = "Đơn hàng chưa được giao" });

        // 1. Đánh giá shipper
        if (req.ShipperRating is >= 1 and <= 5)
        {
            order.ShipperRating   = req.ShipperRating;
            order.ShipperFeedback = req.ShipperFeedback;
        }

        // 2. Đánh giá sản phẩm (cho tất cả sản phẩm trong đơn)
        if (req.ProductRating is >= 1 and <= 5)
        {
            foreach (var item in order.OrderItems)
            {
                // Kiểm tra nếu đã review chưa
                var existingReview = await _db.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == item.ProductId && r.UserId == userId.Value);

                if (existingReview == null)
                {
                    _db.Reviews.Add(new Review
                    {
                        ProductId = item.ProductId,
                        UserId    = userId.Value,
                        Rating    = req.ProductRating.Value,
                        Comment   = req.ProductComment
                    });
                }
                else
                {
                    existingReview.Rating  = req.ProductRating.Value;
                    existingReview.Comment = req.ProductComment;
                }
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
    }
}

public class ReviewSubmitRequest
{
    public int?    ProductRating   { get; set; }
    public string? ProductComment  { get; set; }
    public int?    ShipperRating   { get; set; }
    public string? ShipperFeedback { get; set; }
}
