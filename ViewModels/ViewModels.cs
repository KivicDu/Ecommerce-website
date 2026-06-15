using System.ComponentModel.DataAnnotations;

namespace HutechStore.ViewModels;

// ── AUTH ──────────────────────────────────────────────────────────────────────
public class LoginViewModel
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Phone   { get; set; }
    public string? Address { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpViewModel
{
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ── PRODUCT ───────────────────────────────────────────────────────────────────
public class ProductIndexViewModel
{
    public List<Models.Product> Products     { get; set; } = new();
    public List<Models.Category> Categories { get; set; } = new();

    // Filter params
    public string? Search   { get; set; }
    public string? Brand    { get; set; }     // "Apple" | "Samsung"
    public int?    Category { get; set; }
    public string  Sort     { get; set; } = "latest";

    // Price range filter
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Use case filter
    public string? UseCase { get; set; }      // "gaming" | "camera" | ...

    // Specs filter
    public int? MinRam    { get; set; }
    public bool? Has5G    { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages  { get; set; } = 1;
    public int TotalItems  { get; set; } = 0;
    public int PageSize    { get; set; } = 12;
}

public class ProductFormViewModel
{
    public int    Id   { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public string Brand { get; set; } = "Apple";

    public string? Slug      { get; set; }
    public int     Status    { get; set; } = 1;
    public int     Featured  { get; set; } = 0;
    public int     Stock     { get; set; } = 0;

    public string?   ExistingImage { get; set; }
    public IFormFile? Image        { get; set; }
    public bool      RemoveImage   { get; set; } = false;

    // Phone Specs
    public string? Chipset       { get; set; }
    public int     RamGb         { get; set; }
    public int     StorageGb     { get; set; }
    public string? DisplaySize   { get; set; }
    public string? DisplayType   { get; set; }
    public int     RefreshRate   { get; set; } = 60;
    public string? MainCamera    { get; set; }
    public string? FrontCamera   { get; set; }
    public int     BatteryMah    { get; set; }
    public bool    FastCharge    { get; set; }
    public bool    WirelessCharge { get; set; }
    public bool    Has5G         { get; set; } = true;
    public bool    HasNfc        { get; set; } = true;
    public string? Os            { get; set; }
    public string? Colors        { get; set; }
    public string? Dimensions    { get; set; }
    public int     WeightGrams   { get; set; }
    public int     ReleaseYear   { get; set; } = DateTime.Now.Year;

    // Use case tags (comma-separated)
    public string? UseCaseTags { get; set; }

    // Variants JSON serialized list
    public string? VariantsJson { get; set; }
}

// Variant form item (for JSON serialization in admin form)
public class VariantFormItem
{
    public int     Id        { get; set; }
    public string? Color     { get; set; }
    public int     StorageGb { get; set; }
    public int     RamGb     { get; set; }
    public decimal Price     { get; set; }
    public int     Stock     { get; set; }
}

public class ProductDetailViewModel
{
    public Models.Product  Product     { get; set; } = null!;
    public double          AvgRating   { get; set; }
    public bool            IsLoggedIn  { get; set; }
    public bool            InWishlist  { get; set; }
    public List<Models.Product> Related { get; set; } = new();
}

// ── CART & ORDER ──────────────────────────────────────────────────────────────
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string Address { get; set; } = string.Empty;

    public string  PaymentMethod { get; set; } = "cod";
    public string? Note          { get; set; }
    public string? CouponCode    { get; set; }
}

// ── WISHLIST ──────────────────────────────────────────────────────────────────
public class WishlistToggleRequest
{
    public int ProductId { get; set; }
}

// ── ADMIN DASHBOARD ───────────────────────────────────────────────────────────
public class DashboardViewModel
{
    public int TotalOrders     { get; set; }
    public int TotalProducts   { get; set; }
    public int TotalCategories { get; set; }
    public int TotalUsers      { get; set; }
    public decimal TotalRevenue { get; set; }

    public List<Models.Order>   RecentOrders  { get; set; } = new();
    public List<RevenueItem>    ChartData     { get; set; } = new();
    public List<TopProduct>     TopProducts   { get; set; } = new();
    public List<RevenueByStatus> RevenueByStatus { get; set; } = new();

    public string ChartType      { get; set; } = "month";
    public int    SelectedYear   { get; set; }
    public List<int> AvailableYears { get; set; } = new();
}

public class RevenueItem    { public string Label { get; set; } = ""; public decimal Revenue { get; set; } public int OrderCount { get; set; } }
public class RevenueByStatus { public string Status { get; set; } = ""; public decimal Total { get; set; } public int Count { get; set; } }
public class TopProduct      { public string Name { get; set; } = ""; public int Sold { get; set; } public decimal Revenue { get; set; } }

// ── ADMIN FORMS ───────────────────────────────────────────────────────────────
public class CategoryFormViewModel
{
    public int    Id     { get; set; }
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    public string Name   { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int    Status { get; set; } = 1;
}

public class CouponFormViewModel
{
    public int    Id     { get; set; }
    [Required, MaxLength(50)]
    public string Code   { get; set; } = string.Empty;
    public string Type   { get; set; } = "percent";
    [Range(0.01, double.MaxValue)]
    public decimal Value       { get; set; }
    public decimal MinOrder    { get; set; } = 0;
    public decimal MaxDiscount { get; set; } = 0;
    public int     MaxUsage    { get; set; } = 0;
    public int     Status      { get; set; } = 1;
    public DateTime? ExpiresAt { get; set; }
}

// ── PROFILE ───────────────────────────────────────────────────────────────────
public class ProfileViewModel
{
    [Required] public string  Name    { get; set; } = string.Empty;
    public string? Phone   { get; set; }
    public string? Address { get; set; }
    public string? Avatar  { get; set; }
    public string  AuthProvider { get; set; } = "local";
}

public class ChangePasswordViewModel
{
    [Required] public string OldPassword { get; set; } = string.Empty;
    [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
    [Compare("NewPassword", ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
