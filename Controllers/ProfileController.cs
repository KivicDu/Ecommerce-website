using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HutechStore.Data;
using HutechStore.Helpers;
using HutechStore.Models;
using HutechStore.ViewModels;

namespace HutechStore.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    public ProfileController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(SessionHelper.GetUserId(HttpContext.Session));
        if (user == null) return RedirectToAction("Login", "Auth");

        return View(new ProfileViewModel
        {
            Name         = user.Name,
            Phone        = user.Phone,
            Address      = user.Address,
            Avatar       = user.Avatar,
            AuthProvider = user.AuthProvider
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _db.Users.FindAsync(SessionHelper.GetUserId(HttpContext.Session));
        if (user == null) return RedirectToAction("Login", "Auth");

        user.Name    = vm.Name;
        user.Phone   = vm.Phone;
        user.Address = vm.Address;
        await _db.SaveChangesAsync();

        // Cập nhật session name
        HttpContext.Session.SetString("UserName", user.Name);

        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(SessionHelper.GetUserId(HttpContext.Session));
        if (user == null) return RedirectToAction("Login", "Auth");

        if (user.AuthProvider == "google")
        {
            TempData["Error"] = "Tài khoản Google không thể đổi mật khẩu tại đây";
            return RedirectToAction("Index");
        }

        var hasher = new PasswordHasher<User>();
        bool isOldValid = false;

        if (!string.IsNullOrEmpty(user.Password))
        {
            PasswordVerificationResult result;
            try
            {
                result = hasher.VerifyHashedPassword(user, user.Password, vm.OldPassword);
            }
            catch (FormatException)
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                isOldValid = true;
            }
            else if (user.Password == vm.OldPassword) // Fallback for legacy plaintext password
            {
                isOldValid = true;
            }
        }

        if (!isOldValid)
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng";
            return RedirectToAction("Index");
        }

        user.Password = hasher.HashPassword(user, vm.NewPassword);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Index");
    }
}
