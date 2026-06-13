using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using System.Text.Json.Serialization;

namespace HutechStore.Controllers;

// ══════════════════════════════════════════════════════════════════════════════
//  SEPAY WEBHOOK  –  POST /api/payment/webhook
//  Sepay gọi endpoint này mỗi khi phát hiện giao dịch vào tài khoản MB Bank.
//  Docs: https://docs.sepay.vn/webhooks.html
// ══════════════════════════════════════════════════════════════════════════════
[ApiController]
[Route("api/payment")]
public class PaymentWebhookController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentWebhookController> _logger;
    private readonly IWebHostEnvironment _env;

    public PaymentWebhookController(AppDbContext db, IConfiguration config,
        ILogger<PaymentWebhookController> logger, IWebHostEnvironment env)
    {
        _db = db;
        _config = config;
        _logger = logger;
        _env = env;
    }

    // ── Sepay Webhook ──────────────────────────────────────────────────────────
    [HttpPost("webhook")]
    public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookPayload payload)
    {
        // 1. Xác thực token từ Sepay (cấu hình trong appsettings.json)
        var expectedToken = _config["BankPayment:SepayWebhookToken"];
        if (!string.IsNullOrEmpty(expectedToken))
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                authHeader.ToString() != $"Apikey {expectedToken}")
            {
                _logger.LogWarning("Sepay webhook: invalid token");
                return Unauthorized(new { error = "Invalid token" });
            }
        }

        _logger.LogInformation("Sepay webhook received: txId={TxId}, content={Content}, amount={Amount}",
            payload.Id, payload.TransactionContent, payload.TransferAmount);

        // 2. Chỉ xử lý giao dịch tiền VÀO (credit)
        if (payload.TransferType != "in")
            return Ok(new { success = true, message = "Ignored (not credit)" });

        // 3. Tìm đơn hàng theo nội dung chuyển khoản (OrderNumber)
        //    Nội dung CK format: "HS20260608004358662" hoặc "Thanh toan don hang HS..."
        var content = payload.TransactionContent ?? "";
        var order = await _db.Orders
            .Where(o => o.PaymentMethod == "bank_transfer"
                     && o.Status == "pending"
                     && content.Contains(o.OrderNumber))
            .FirstOrDefaultAsync();

        if (order == null)
        {
            _logger.LogWarning("Sepay webhook: no matching order for content '{Content}'", content);
            return Ok(new { success = false, message = "No matching order" });
        }

        // 4. Kiểm tra số tiền (cho phép sai lệch ±1000đ để tránh lỗi làm tròn)
        var expectedAmount = (long)order.TotalAmount;
        var receivedAmount = payload.TransferAmount;
        if (Math.Abs(receivedAmount - expectedAmount) > 1000)
        {
            _logger.LogWarning("Sepay webhook: amount mismatch. Expected {Expected}, got {Received}",
                expectedAmount, receivedAmount);
            return Ok(new { success = false, message = "Amount mismatch" });
        }

        // 5. Cập nhật trạng thái đơn hàng → confirmed
        order.Status              = "confirmed";
        order.PaymentPaidAt       = DateTime.UtcNow;
        order.SepayTransactionId  = payload.Id?.ToString();
        await _db.SaveChangesAsync();

        _logger.LogInformation("Order #{OrderNumber} confirmed via bank transfer (txId={TxId})",
            order.OrderNumber, payload.Id);

        return Ok(new { success = true, message = $"Order {order.OrderNumber} confirmed" });
    }

    // ── Polling API  –  GET /api/payment/status/{orderId} ─────────────────────
    //  Frontend gọi mỗi 10s để kiểm tra trạng thái đơn hàng
    [HttpGet("status/{orderId:int}")]
    public async Task<IActionResult> GetOrderStatus(int orderId)
    {
        var order = await _db.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.Status,
                o.PaymentMethod,
                o.PaymentPaidAt,
                o.TotalAmount
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound(new { error = "Order not found" });

        return Ok(new
        {
            orderId       = order.Id,
            orderNumber   = order.OrderNumber,
            status        = order.Status,
            paymentMethod = order.PaymentMethod,
            paidAt        = order.PaymentPaidAt,
            isPaid        = order.Status != "pending" && order.Status != "cancelled"
        });
    }

    // ── Dev-only: POST /api/payment/dev-confirm/{orderId} ─────────────────────
    //  Giả lập xác nhận thanh toán khi test local (không có Sepay webhook)
    //  Chỉ hoạt động ở môi trường Development
    [HttpPost("dev-confirm/{orderId:int}")]
    public async Task<IActionResult> DevConfirm(int orderId)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound(new { success = false, message = "Order not found" });

        order.Status             = "confirmed";
        order.PaymentPaidAt      = DateTime.UtcNow;
        order.SepayTransactionId = "DEV-SIMULATED";
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = $"Order {order.OrderNumber} confirmed (dev mode)" });
    }
}

// ── DTO Sepay Webhook Payload ─────────────────────────────────────────────────
// Tham khảo: https://docs.sepay.vn/webhooks.html#payload
public class SepayWebhookPayload
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    [JsonPropertyName("transactionDate")]
    public string? TransactionDate { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("subAccount")]
    public string? SubAccount { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    // Sepay dùng "content" hoặc "transferContent" tuỳ version
    [JsonPropertyName("transferContent")]
    public string? TransferContent { get; set; }

    public string? TransactionContent => Content ?? TransferContent;

    [JsonPropertyName("transferAmount")]
    public long TransferAmount { get; set; }

    [JsonPropertyName("accumulated")]
    public long Accumulated { get; set; }

    // "in" = tiền vào, "out" = tiền ra
    [JsonPropertyName("transferType")]
    public string TransferType { get; set; } = "in";

    [JsonPropertyName("referenceCode")]
    public string? ReferenceCode { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}