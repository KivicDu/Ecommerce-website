using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HutechStore.Models;

// ══════════════════════════════════════════════════════════════════════════════
//  USER
// ══════════════════════════════════════════════════════════════════════════════
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    // nullable vì user đăng nhập Google không có password
    public string? Password { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = "user"; // "user" | "admin"

    // Google OAuth
    [MaxLength(100)]
    public string? GoogleId { get; set; }

    [MaxLength(255)]
    public string? Avatar { get; set; }

    // đăng nhập bằng gì: "local" | "google"
    [MaxLength(20)]
    public string AuthProvider { get; set; } = "local";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Order>    Orders    { get; set; } = new List<Order>();
    public ICollection<Review>   Reviews   { get; set; } = new List<Review>();
    public ICollection<Cart>     Carts     { get; set; } = new List<Cart>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
}

// ══════════════════════════════════════════════════════════════════════════════
//  CATEGORY
// ══════════════════════════════════════════════════════════════════════════════
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public int Status { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

// ══════════════════════════════════════════════════════════════════════════════
//  PRODUCT
// ══════════════════════════════════════════════════════════════════════════════
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Ảnh chính (thumbnail)
    [MaxLength(500)]
    public string? Image { get; set; }

    public int CategoryId { get; set; }

    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty; // "Apple" | "Samsung"

    // URL thân thiện SEO
    [MaxLength(250)]
    public string? Slug { get; set; }

    public int Status   { get; set; } = 1; // 1=active 0=inactive
    public int Featured { get; set; } = 0; // 1=featured
    public int Stock    { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category?   Category   { get; set; }
    public PhoneSpecs? PhoneSpecs { get; set; }

    public ICollection<Review>         Reviews         { get; set; } = new List<Review>();
    public ICollection<CartItem>       CartItems       { get; set; } = new List<CartItem>();
    public ICollection<OrderItem>      OrderItems      { get; set; } = new List<OrderItem>();
    public ICollection<ProductImage>   ProductImages   { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<UseCaseTag>     UseCaseTags     { get; set; } = new List<UseCaseTag>();
    public ICollection<Wishlist>       Wishlists       { get; set; } = new List<Wishlist>();
}

// ══════════════════════════════════════════════════════════════════════════════
//  PHONE SPECS  (cho AI tư vấn)
// ══════════════════════════════════════════════════════════════════════════════
public class PhoneSpecs
{
    public int Id { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }

    // Hiệu năng
    [MaxLength(100)]
    public string? Chipset { get; set; }        // "Apple A18 Pro", "Snapdragon 8 Elite"

    public int RamGb      { get; set; }          // 8, 12, 16
    public int StorageGb  { get; set; }          // 128, 256, 512, 1024

    // Màn hình
    [MaxLength(20)]
    public string? DisplaySize { get; set; }    // "6.1 inch"

    [MaxLength(50)]
    public string? DisplayType { get; set; }    // "Super Retina XDR OLED"
    public int RefreshRate    { get; set; }      // 60, 120

    // Camera
    [MaxLength(100)]
    public string? MainCamera { get; set; }     // "48MP f/1.78"
    [MaxLength(100)]
    public string? FrontCamera { get; set; }    // "12MP TrueDepth"

    // Pin
    public int BatteryMah { get; set; }         // 4685
    public bool FastCharge { get; set; }
    public bool WirelessCharge { get; set; }

    // Kết nối
    public bool Has5G        { get; set; }
    public bool HasNfc       { get; set; }

    // OS
    [MaxLength(50)]
    public string? Os { get; set; }             // "iOS 18", "Android 15 / One UI 7"

    // Màu sắc có sẵn (JSON array string)
    [MaxLength(500)]
    public string? Colors { get; set; }         // "Black Titanium,White Titanium,Desert Titanium"

    // Kích thước & trọng lượng
    [MaxLength(100)]
    public string? Dimensions { get; set; }     // "147.6 x 71.5 x 8.25 mm"
    public int WeightGrams { get; set; }

    // Năm phát hành
    public int ReleaseYear { get; set; }

    // Navigation
    public Product? Product { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  PRODUCT VARIANTS  (128GB / 256GB / màu sắc)
// ══════════════════════════════════════════════════════════════════════════════
public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }          // "Đen Titan"

    public int StorageGb { get; set; }           // 128, 256, 512
    public int RamGb     { get; set; }           // 0 = không áp dụng (iPhone)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Stock     { get; set; } = 0;
    public int Status    { get; set; } = 1;

    public Product? Product { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  USE CASE TAGS  (AI tư vấn: gaming / camera / business / student…)
// ══════════════════════════════════════════════════════════════════════════════
public class UseCaseTag
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    // "gaming" | "camera" | "business" | "student" | "battery_life" | "value"
    [MaxLength(50)]
    public string Tag { get; set; } = string.Empty;

    // Điểm phù hợp 1-10 (AI dùng để rank)
    public int Score { get; set; } = 8;

    public Product? Product { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  PRODUCT IMAGES  (gallery nhiều ảnh)
// ══════════════════════════════════════════════════════════════════════════════
public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0; // thứ tự hiển thị

    public bool IsMain { get; set; } = false;

    public Product? Product { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  WISHLIST
// ══════════════════════════════════════════════════════════════════════════════
public class Wishlist
{
    public int Id { get; set; }

    public int UserId    { get; set; }
    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User?    User    { get; set; }
    public Product? Product { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  CHAT HISTORY  (lưu lịch sử chat AI)
// ══════════════════════════════════════════════════════════════════════════════
public class ChatHistory
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;

    public int? UserId { get; set; } // nullable - khách vãng lai

    // "user" | "assistant"
    [MaxLength(20)]
    public string Role { get; set; } = "user";

    public string Message { get; set; } = string.Empty;

    // Sản phẩm được AI đề cập (JSON array of ProductId)
    [MaxLength(500)]
    public string? ProductsMentioned { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  REVIEW
// ══════════════════════════════════════════════════════════════════════════════
public class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public int UserId    { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
    public User?    User    { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  CART
// ══════════════════════════════════════════════════════════════════════════════
public class Cart
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }

    public int CartId    { get; set; }
    public int ProductId { get; set; }

    public int? VariantId { get; set; } // nullable - chọn biến thể cụ thể

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cart?           Cart    { get; set; }
    public Product?        Product { get; set; }
    public ProductVariant? Variant { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  ORDER
// ══════════════════════════════════════════════════════════════════════════════
public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(255)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ShippingName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "cod"; // "cod" | "vnpay" | "momo"

    [MaxLength(500)]
    public string? Note { get; set; }

    [MaxLength(50)]
    public string? CouponCode { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    // pending | confirmed | shipping | delivered | cancelled
    [MaxLength(50)]
    public string Status { get; set; } = "pending";

    // Bank transfer tracking
    public DateTime? PaymentPaidAt { get; set; }

    [MaxLength(100)]
    public string? SepayTransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId   { get; set; }
    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty; // snapshot tên lúc đặt

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public Order?          Order   { get; set; }
    public Product?        Product { get; set; }
    public ProductVariant? Variant { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
//  COUPON
// ══════════════════════════════════════════════════════════════════════════════
public class Coupon
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Type { get; set; } = "percent"; // "percent" | "fixed"

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinOrder { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxDiscount { get; set; } = 0;

    public int MaxUsage  { get; set; } = 0;
    public int UsedCount { get; set; } = 0;
    public int Status    { get; set; } = 1;

    public DateTime? ExpiresAt { get; set; }
    public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
}

// ══════════════════════════════════════════════════════════════════════════════
//  PASSWORD OTP
// ══════════════════════════════════════════════════════════════════════════════
public class PasswordOtp
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(6)]
    public string Otp { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ══════════════════════════════════════════════════════════════════════════════
//  ERROR VIEW MODEL
// ══════════════════════════════════════════════════════════════════════════════
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
