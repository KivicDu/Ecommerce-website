using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.Services;
using HutechStore.ViewModels;
using BCrypt.Net;

namespace HutechStore.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly MailService  _mail;
    private readonly CartService  _cart;

    public AuthController(AppDbContext db, MailService mail, CartService cart)
    {
        _db = db; _mail = mail; _cart = cart;
    }

    public IActionResult Login()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);

        if (user == null)
        {
            ModelState.AddModelError("", "Email không tồn tại");
            return View(vm);
        }

        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(vm.Password, user.Password);
        
        if (!isPasswordCorrect)
        {
            ModelState.AddModelError("", "Mật khẩu không chính xác");
            return View(vm);
        }

        SessionHelper.SetUser(HttpContext.Session, user);
        await _cart.MergeSessionCartAsync(user.Id);
        return RedirectAfterLogin(user);
    }

   public IActionResult LoginWithGoogle(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        HttpContext.Session.SetString("RedirectAfterLogin", returnUrl);

        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback", "Auth", null, Request.Scheme)
        };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

  public async Task<IActionResult> GoogleCallback()
{
    var result = await HttpContext.AuthenticateAsync("GoogleTempCookie");
    if (!result.Succeeded)
    {
        TempData["Error"] = "Đăng nhập Google thất bại.";
        return RedirectToAction("Login");
    }

    var claims   = result.Principal?.Claims;
    var email    = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
    var name     = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
    var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    var avatar   = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value ?? "";

    if (string.IsNullOrEmpty(email))
    {
        TempData["Error"] = "Không lấy được email từ Google.";
        return RedirectToAction("Login");
    }

    var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user == null)
    {
        user = new User { Name = name, Email = email, GoogleId = googleId, Avatar = avatar, AuthProvider = "google", Role = "user" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    await HttpContext.SignOutAsync("GoogleTempCookie");
    SessionHelper.SetUser(HttpContext.Session, user);
    await _cart.MergeSessionCartAsync(user.Id);
    return RedirectAfterLogin(user);
}

    public IActionResult Register()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng");
            return View(vm);
        }

        string hashPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password);

        var user = new User { Name = vm.Name, Email = vm.Email, Password = hashPassword, Phone = vm.Phone, Address = vm.Address };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        SessionHelper.Clear(HttpContext.Session);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
        if (user == null) { ModelState.AddModelError("", "Email không tồn tại"); return View(vm); }
        if (user.AuthProvider == "google") { ModelState.AddModelError("", "Tài khoản Google không có mật khẩu."); return View(vm); }
        var otp = new Random().Next(100000, 999999).ToString();
        _db.PasswordOtps.RemoveRange(_db.PasswordOtps.Where(p => p.Email == vm.Email));
        _db.PasswordOtps.Add(new PasswordOtp { Email = vm.Email, Otp = otp, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await _db.SaveChangesAsync();
        try { await _mail.SendOtpAsync(vm.Email, otp); } catch { }
        TempData["OtpEmail"] = vm.Email;
        return RedirectToAction("VerifyOtp");
    }

    public IActionResult VerifyOtp()
    {
        var email = TempData.Peek("OtpEmail")?.ToString();
        if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
        return View(new VerifyOtpViewModel { Email = email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var record = await _db.PasswordOtps.FirstOrDefaultAsync(p => p.Email == vm.Email && p.Otp == vm.Otp);
        if (record == null || record.ExpiresAt < DateTime.UtcNow) { ModelState.AddModelError("Otp", "Mã OTP không hợp lệ hoặc đã hết hạn"); return View(vm); }
        TempData["ResetEmail"] = vm.Email;
        return RedirectToAction("ResetPassword");
    }

    public IActionResult ResetPassword()
    {
        var email = TempData.Peek("ResetEmail")?.ToString();
        if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");
        return View(new ResetPasswordViewModel { Email = email });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);
        if (user == null) return RedirectToAction("Login");
        user.Password = vm.NewPassword;
        _db.PasswordOtps.RemoveRange(_db.PasswordOtps.Where(p => p.Email == vm.Email));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Login");
    }

    private IActionResult RedirectAfterLogin(User user)
    {
        var redirect = HttpContext.Session.GetString("RedirectAfterLogin");
        if (!string.IsNullOrEmpty(redirect)) { HttpContext.Session.Remove("RedirectAfterLogin"); return Redirect(redirect); }
        return user.Role == "admin" ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
    }
}