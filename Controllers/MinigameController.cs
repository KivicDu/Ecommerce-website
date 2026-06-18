using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;

namespace HutechStore.Controllers;

public class MinigameController : Controller
{
    private readonly AppDbContext _db;

    public MinigameController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Minigame
    public async Task<IActionResult> Index()
    {
        var userId = SessionHelper.GetUserId(HttpContext.Session);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Check if played in the last 24 hours
        bool played = false;
        if (userId.HasValue)
        {
            played = await _db.LuckyWheelPlays.AnyAsync(p => p.UserId == userId.Value && p.PlayedAt > DateTime.UtcNow.AddDays(-1));
        }
        else
        {
            played = await _db.LuckyWheelPlays.AnyAsync(p => p.IpAddress == ip && p.PlayedAt > DateTime.UtcNow.AddDays(-1));
            if (!played)
            {
                // check cookie play limit
                if (HttpContext.Request.Cookies.TryGetValue("wheel_played_today", out _))
                {
                    played = true;
                }
            }
        }

        ViewBag.Played = played;

        // Fetch random active questions (limit to 5)
        var questions = await _db.SurveyQuestions
            .Where(q => q.IsActive)
            .OrderBy(q => EF.Functions.Random())
            .Take(5)
            .ToListAsync();

        if (questions.Count == 0)
        {
            questions = await _db.SurveyQuestions
                .Where(q => q.IsActive)
                .Take(5)
                .ToListAsync();
        }

        // Fetch active wheel prizes to render on the canvas
        var prizes = await _db.LuckyWheelPrizes
            .Where(p => p.Status == 1)
            .ToListAsync();

        ViewBag.Prizes = prizes;

        return View(questions);
    }

    // POST: /Minigame/SubmitSurveyAndSpin
    [HttpPost]
    public async Task<IActionResult> SubmitSurveyAndSpin([FromBody] SurveySubmissionInput input)
    {
        if (input == null)
            return Json(new { success = false, message = "Dữ liệu khảo sát không hợp lệ." });

        var userId = SessionHelper.GetUserId(HttpContext.Session);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // 1. Double check play limit
        bool played = false;
        if (userId.HasValue)
        {
            played = await _db.LuckyWheelPlays.AnyAsync(p => p.UserId == userId.Value && p.PlayedAt > DateTime.UtcNow.AddDays(-1));
        }
        else
        {
            played = await _db.LuckyWheelPlays.AnyAsync(p => p.IpAddress == ip && p.PlayedAt > DateTime.UtcNow.AddDays(-1));
            if (!played && HttpContext.Request.Cookies.TryGetValue("wheel_played_today", out _))
            {
                played = true;
            }
        }

        if (played)
        {
            return Json(new { success = false, message = "Bạn đã tham gia hôm nay rồi. Hãy quay lại vào ngày mai!" });
        }

        // 2. Save Survey responses
        var survey = new Survey
        {
            UserId = userId,
            IpAddress = ip,
            Age = input.Age,
            Gender = input.Gender,
            CreatedAt = DateTime.UtcNow
        };

        _db.Surveys.Add(survey);
        await _db.SaveChangesAsync(); // save to generate survey ID

        if (input.Responses != null && input.Responses.Any())
        {
            foreach (var r in input.Responses)
            {
                _db.SurveyResponses.Add(new SurveyResponse
                {
                    SurveyId = survey.Id,
                    QuestionId = r.QuestionId,
                    AnswerText = r.AnswerText
                });
            }
            await _db.SaveChangesAsync();
        }

        // 3. Determine Prize (Weighted Random Algorithm)
        var prizes = await _db.LuckyWheelPrizes
            .Where(p => p.Status == 1)
            .ToListAsync();

        if (!prizes.Any())
        {
            return Json(new { success = false, message = "Không tìm thấy giải thưởng hoạt động." });
        }

        int totalWeight = prizes.Sum(p => p.Weight);
        var rand = new Random();
        int rVal = rand.Next(totalWeight);
        
        LuckyWheelPrize winningPrize = prizes.First();
        int currentSum = 0;
        int winningIndex = 0;

        for (int i = 0; i < prizes.Count; i++)
        {
            currentSum += prizes[i].Weight;
            if (rVal < currentSum)
            {
                winningPrize = prizes[i];
                winningIndex = i;
                break;
            }
        }

        // 4. Generate Coupon if winning prize yields a coupon
        string? couponCode = null;
        if (winningPrize.CouponType != "none" && winningPrize.CouponValue > 0)
        {
            couponCode = GenerateRandomCouponCode();
            
            var newCoupon = new Coupon
            {
                Code = couponCode,
                Type = winningPrize.CouponType,
                Value = winningPrize.CouponValue,
                MinOrder = 0,
                MaxDiscount = winningPrize.CouponType == "percent" ? 500000 : winningPrize.CouponValue, // max 500k for percent coupons
                MaxUsage = 1,
                UsedCount = 0,
                Status = 1,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            _db.Coupons.Add(newCoupon);
            await _db.SaveChangesAsync();

            // Save coupon to cookie or session if guest
            if (!userId.HasValue)
            {
                HttpContext.Response.Cookies.Append("wheel_guest_coupon", couponCode, new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddHours(24),
                    HttpOnly = false, // allow read in JS
                    SameSite = SameSiteMode.Lax
                });
            }
        }

        // 5. Save Play Record
        var play = new LuckyWheelPlay
        {
            UserId = userId,
            IpAddress = ip,
            PlayedAt = DateTime.UtcNow
        };
        _db.LuckyWheelPlays.Add(play);
        await _db.SaveChangesAsync();

        // Save play cookie indicator (expires in 24 hours)
        HttpContext.Response.Cookies.Append("wheel_played_today", "true", new CookieOptions
        {
            Expires = DateTime.UtcNow.AddHours(24),
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Json(new {
            success = true,
            prizeIndex = winningIndex,
            prizeName = winningPrize.Name,
            couponCode = couponCode,
            isLoggedIn = userId.HasValue
        });
    }

    private string GenerateRandomCouponCode()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[6];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return "WHEEL-" + new string(stringChars);
    }
}

public class SurveySubmissionInput
{
    public int Age { get; set; }
    public string Gender { get; set; } = "Other";
    public List<SurveyResponseInput> Responses { get; set; } = new();
}

public class SurveyResponseInput
{
    public int QuestionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
}
