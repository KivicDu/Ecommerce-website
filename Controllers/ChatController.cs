using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.Services;

namespace HutechStore.Controllers;

public class ChatController : Controller
{
    private readonly AppDbContext  _db;
    private readonly GeminiService _gemini;

    public ChatController(AppDbContext db, GeminiService gemini)
    {
        _db = db; 
        _gemini = gemini;
    }

    // POST /Chat/Send
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ChatRequest request)
    {
        // 1. Kiểm tra tin nhắn đầu vào từ giao diện
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Json(new { success = false, message = "Tin nhắn không được rỗng" });
        }

        var sessionId = HttpContext.Session.Id;
        var userId    = SessionHelper.GetUserId(HttpContext.Session);

        // 2. Lấy lịch sử hội thoại hợp lệ từ DB (tối đa 10 tin gần nhất)
        var dbHistory = await _db.ChatHistories
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var history = dbHistory.Select(h => new ChatMessage
        {
            Role    = h.Role,
            Content = h.Message
        }).ToList();

        // 3. Gọi dịch vụ AI Gemini để lấy phản hồi
        var reply = await _gemini.ChatAsync(request.Message, history);

        // 4. KIỂM TRA PHẢN HỒI: Nếu dính câu thông báo lỗi kỹ thuật hoặc rỗng, chặn không cho lưu vào DB
        if (string.IsNullOrEmpty(reply) || 
            reply.Contains("sự cố kỹ thuật") || 
            reply.Contains("sự cố kết nối") || 
            reply.Contains("không hiểu câu hỏi"))
        {
            // Trả về lỗi trực tiếp cho Frontend (chat.js) hiển thị cảnh báo, không ghi nhận vào lịch sử
            return Json(new { success = false, message = reply });
        }

        // 5. CHỈ LƯU VÀO DATABASE KHI AI PHẢN HỒI THÀNH CÔNG THỰC SỰ
        // Lưu tin nhắn của Người dùng (User)
        _db.ChatHistories.Add(new ChatHistory
        {
            SessionId = sessionId,
            UserId    = userId,
            Role      = "user",
            Message   = request.Message
        });

        // Lưu phản hồi hợp lệ của Trợ lý AI (Assistant)
        _db.ChatHistories.Add(new ChatHistory
        {
            SessionId = sessionId,
            UserId    = userId,
            Role      = "assistant",
            Message   = reply
        });

        // Xác nhận lưu các thay đổi vào SQL Server
        await _db.SaveChangesAsync();

        // Trả kết quả thành công về cho giao diện hiển thị
        return Json(new { success = true, message = reply });
    }

    // POST /Chat/Clear - xóa lịch sử hội thoại hiện tại
    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var sessionId = HttpContext.Session.Id;
        var toDelete  = _db.ChatHistories.Where(c => c.SessionId == sessionId);
        
        _db.ChatHistories.RemoveRange(toDelete);
        await _db.SaveChangesAsync();
        
        return Json(new { success = true });
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}