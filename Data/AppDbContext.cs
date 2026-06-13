using Microsoft.EntityFrameworkCore;
using HutechStore.Models;

namespace HutechStore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Core ──────────────────────────────────────────────────────────────────
    public DbSet<User>    Users    => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product>  Products   => Set<Product>();
    public DbSet<Review>   Reviews    => Set<Review>();

    // ── Phone specific ────────────────────────────────────────────────────────
    public DbSet<PhoneSpecs>     PhoneSpecs     => Set<PhoneSpecs>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<UseCaseTag>     UseCaseTags     => Set<UseCaseTag>();
    public DbSet<ProductImage>   ProductImages   => Set<ProductImage>();

    // ── Cart & Order ──────────────────────────────────────────────────────────
    public DbSet<Cart>      Carts      => Set<Cart>();
    public DbSet<CartItem>  CartItems  => Set<CartItem>();
    public DbSet<Order>     Orders     => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // ── Extras ────────────────────────────────────────────────────────────────
    public DbSet<Coupon>      Coupons      => Set<Coupon>();
    public DbSet<Wishlist>    Wishlists    => Set<Wishlist>();
    public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();
    public DbSet<PasswordOtp> PasswordOtps => Set<PasswordOtp>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── User ──────────────────────────────────────────────────────────────
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<User>().HasIndex(u => u.GoogleId);

        // ── Category ──────────────────────────────────────────────────────────
        mb.Entity<Category>().HasIndex(c => c.Name);

        // ── Product ───────────────────────────────────────────────────────────
        mb.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        mb.Entity<Product>().HasIndex(p => p.Brand);

        mb.Entity<Product>()
          .HasOne(p => p.Category)
          .WithMany(c => c.Products)
          .HasForeignKey(p => p.CategoryId)
          .OnDelete(DeleteBehavior.Restrict);

        // ── PhoneSpecs (1-1 với Product) ──────────────────────────────────────
        mb.Entity<PhoneSpecs>()
          .HasOne(s => s.Product)
          .WithOne(p => p.PhoneSpecs)
          .HasForeignKey<PhoneSpecs>(s => s.ProductId)
          .OnDelete(DeleteBehavior.Cascade);

        // ── ProductVariant ────────────────────────────────────────────────────
        mb.Entity<ProductVariant>()
          .HasOne(v => v.Product)
          .WithMany(p => p.ProductVariants)
          .HasForeignKey(v => v.ProductId)
          .OnDelete(DeleteBehavior.Cascade);

        // ── UseCaseTag ────────────────────────────────────────────────────────
        mb.Entity<UseCaseTag>()
          .HasOne(t => t.Product)
          .WithMany(p => p.UseCaseTags)
          .HasForeignKey(t => t.ProductId)
          .OnDelete(DeleteBehavior.Cascade);
        mb.Entity<UseCaseTag>().HasIndex(t => new { t.ProductId, t.Tag }).IsUnique();

        // ── ProductImage ──────────────────────────────────────────────────────
        mb.Entity<ProductImage>()
          .HasOne(i => i.Product)
          .WithMany(p => p.ProductImages)
          .HasForeignKey(i => i.ProductId)
          .OnDelete(DeleteBehavior.Cascade);

        // ── Review ────────────────────────────────────────────────────────────
        mb.Entity<Review>()
          .HasOne(r => r.Product).WithMany(p => p.Reviews)
          .HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Review>()
          .HasOne(r => r.User).WithMany(u => u.Reviews)
          .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Review>().HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();

        // ── Cart ──────────────────────────────────────────────────────────────
        mb.Entity<Cart>()
          .HasOne(c => c.User).WithMany(u => u.Carts)
          .HasForeignKey(c => c.UserId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        mb.Entity<CartItem>()
          .HasOne(ci => ci.Cart).WithMany(c => c.CartItems)
          .HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<CartItem>()
          .HasOne(ci => ci.Product).WithMany(p => p.CartItems)
          .HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<CartItem>()
          .HasOne(ci => ci.Variant).WithMany()
          .HasForeignKey(ci => ci.VariantId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        // ── Order ─────────────────────────────────────────────────────────────
        mb.Entity<Order>()
          .HasOne(o => o.User).WithMany(u => u.Orders)
          .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

        mb.Entity<OrderItem>()
          .HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
          .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<OrderItem>()
          .HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
          .HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<OrderItem>()
          .HasOne(oi => oi.Variant).WithMany()
          .HasForeignKey(oi => oi.VariantId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        // ── Wishlist ──────────────────────────────────────────────────────────
        mb.Entity<Wishlist>()
          .HasOne(w => w.User).WithMany(u => u.Wishlists)
          .HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Wishlist>()
          .HasOne(w => w.Product).WithMany(p => p.Wishlists)
          .HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Wishlist>().HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();

        // ── ChatHistory ───────────────────────────────────────────────────────
        mb.Entity<ChatHistory>()
          .HasOne(c => c.User).WithMany(u => u.ChatHistories)
          .HasForeignKey(c => c.UserId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        mb.Entity<ChatHistory>().HasIndex(c => c.SessionId);

        // ── Coupon ────────────────────────────────────────────────────────────
        mb.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();

        // ── PasswordOtp ───────────────────────────────────────────────────────
        mb.Entity<PasswordOtp>().HasIndex(p => p.Email);
    }
}
