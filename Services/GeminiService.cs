using System.Text;
using System.Text.Json;
using HutechStore.Data;
using HutechStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HutechStore.Services;

public class GeminiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration     _config;
    private readonly AppDbContext        _db;
    private readonly IMemoryCache       _cache;
    private const string ProductCacheKey = "GeminiProductContext";

    public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration config, AppDbContext db, IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _db     = db;
        _cache  = cache;
    }

    // ── Lay context san pham tu Cache hoac DB inject vao system prompt ─────────────────
    private async Task<string> BuildProductContextAsync()
    {
        // Kiem tra xem danh sach san pham da co trong bo nho dem (Cache) chua
        if (_cache.TryGetValue(ProductCacheKey, out string? cachedContext) && cachedContext != null)
        {
            return cachedContext;
        }

        // Neu chua co, tien hanh quet tu Database
        var products = await _db.Products
            .Include(p => p.PhoneSpecs)
            .Include(p => p.UseCaseTags)
            .Include(p => p.ProductVariants)
            .Where(p => p.Status == 1 && p.Stock > 0)
            .OrderBy(p => p.Price)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("=== DANH SACH SAN PHAM DANG BAN ===");

        foreach (var p in products)
        {
            sb.AppendLine($"\n[{p.Id}] {p.Name}");
            sb.AppendLine($"  Hang: {p.Brand} | Gia: {p.Price:N0}d | Ton kho: {p.Stock}");

            if (p.PhoneSpecs != null)
            {
                var s = p.PhoneSpecs;
                sb.AppendLine($"  Chip: {s.Chipset} | RAM: {s.RamGb}GB | Luu tru: {s.StorageGb}GB");
                sb.AppendLine($"  Man hinh: {s.DisplaySize} {s.DisplayType} {s.RefreshRate}Hz");
                sb.AppendLine($"  Camera: {s.MainCamera} | Pin: {s.BatteryMah}mAh");
                sb.AppendLine($"  5G: {(s.Has5G ? "Co" : "Khong")} | OS: {s.Os}");
            }

            if (p.ProductVariants.Any())
            {
                var variants = p.ProductVariants.Where(v => v.Status == 1).Select(v =>
                    $"{v.StorageGb}GB{(string.IsNullOrEmpty(v.Color) ? "" : "/" + v.Color)} ({v.Price:N0}d)");
                sb.AppendLine($"  Cac phien ban: {string.Join(", ", variants)}");
            }

            if (p.UseCaseTags.Any())
            {
                var tags = p.UseCaseTags.OrderByDescending(t => t.Score).Select(t => t.Tag);
                sb.AppendLine($"  Phu hop cho: {string.Join(", ", tags)}");
            }
        }

        var resultContext = sb.ToString();

        // Luu giu du lieu vao bo nho dem trong vong 10 phut de tranh truy van DB lien tuc
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        
        _cache.Set(ProductCacheKey, resultContext, cacheOptions);

        return resultContext;
    }

    // ── Build system prompt ───────────────────────────────────────────────────
    private async Task<string> BuildSystemPromptAsync()
    {
        var productContext = await BuildProductContextAsync();

        return $"""
            Ban la tro ly tu van dien thoai thong minh cua HutechStore - cua hang chuyen iPhone va Samsung.
            
            NGUYEN TAC TU VAN:
            1. CHI tu van cac san pham co trong danh sach ben duoi (dang con hang).
            2. Dua vao ngan sach khach neu de loc phu hop.
            3. Biet tu van dien thoai phu hop cho GAMING: uu tien chip manh, RAM cao, man hinh 120Hz.
            4. Tu van cho CHUP ANH: uu tien camera chinh do phan giai cao, khau do rong, front camera.
            5. Tu van cho DUNG LAU PIN: uu tien dung luong pin lon, sac nhanh.
            6. Tu van cho DOANH NHAN/SINH VIEN: can bang gia va hieu nang.
            7. So sanh thang than uu/nhuoc diem iPhone vs Samsung khi duoc hoi.
            8. Tra loi NGAN GON, de hieu bang tieng Viet. Dung emoji vua phai 📱.
            9. Cuoi moi tu van goi y khach xem chi tiet san pham tren trang web.
            10. Neu khong co san pham phu hop voi yeu cau, hay noi that va goi y san pham gan nhat.
            
            {productContext}
            """;
    }

    // ── Goi Gemini API ────────────────────────────────────────────────────────
    public async Task<string> ChatAsync(string userMessage, List<ChatMessage> history)
    {
        var apiKey = _config["GeminiApi:ApiKey"];
        var model  = _config["GeminiApi:Model"] ?? "gemini-2.5-flash-lite";

        var systemPrompt = await BuildSystemPromptAsync();

        // Build contents array (lich su + tin moi)
        var contents = new List<object>();

        // Them lich su hoi thoai (toi da 10 tin gan nhat de tiet kiem token)
        foreach (var msg in history.TakeLast(10))
        {
            contents.Add(new
            {
                role  = msg.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = msg.Content } }
            });
        }

        // Them tin nhan moi
        contents.Add(new
        {
            role  = "user",
            parts = new[] { new { text = userMessage } }
        });

        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents,
            generationConfig = new
            {
                temperature     = 0.7,
                maxOutputTokens = 800,
                topP            = 0.8
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT",         threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH",       threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
            }
        };

        var client  = _httpClientFactory.CreateClient("GeminiClient");
        var url     = $"v1beta/models/{model}:generateContent?key={apiKey}";
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            var json     = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return "Xin loi, toi dang gap su co ky thuat. Vui long thu lai sau nhe!";
            }

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Xin loi, toi khong hieu cau hoi. Ban co the hoi lai khong?";
        }
        catch
        {
            return "Xin loi, toi dang gap su co ket noi. Vui long thu lai sau!";
        }
    }
}

// ── DTO cho lich su chat ──────────────────────────────────────────────────────
public class ChatMessage
{
    public string Role    { get; set; } = "user"; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
}