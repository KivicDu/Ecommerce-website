using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using HutechStore.Data;
using HutechStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<CookiePolicyOptions>(o => {
    o.MinimumSameSitePolicy = SameSiteMode.Lax;
    o.Secure = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kich hoat bo nho dem cuc bo (IMemoryCache) cho GeminiService dung de cache san pham
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddCookie("GoogleTempCookie", options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(options =>
{
    options.ClientId     = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SaveTokens   = true;
    options.SignInScheme = "GoogleTempCookie";
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpClient("GeminiClient", client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<GeminiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // For [ApiController] routes (e.g. /api/payment/webhook)
app.MapControllerRoute(name: "admin",   pattern: "Admin/{action=Index}/{id?}",   defaults: new { controller = "Admin" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); } catch { }

    try
    {
        var products = db.Products.ToList();
        foreach (var p in products)
        {
            var targetImage = p.Name switch
            {
                "iPhone 16e" => "/images/16e.jpg",
                "iPhone 15" => "/images/15.jpg",
                "iPhone 15 Plus" => "/images/15plus.jpg",
                "iPhone 15 Pro" => "/images/15pro.jpg",
                "iPhone 15 Pro Max" => "/images/15prmax.jpg",
                "iPhone 16" => "/images/16.jpg",
                "iPhone 16 Plus" => "/images/16.plus.webp",
                "iPhone 16 Pro" => "/images/16pro.jpg",
                "iPhone 16 Pro Max" => "/images/16prmax.jpg",
                "Samsung Galaxy A35 5G" => "/images/a35.jpg",
                "Samsung Galaxy A55 5G" => "/images/a55_5g.jpg",
                "Samsung Galaxy S24 FE" => "/images/s24fe.jpg",
                "Samsung Galaxy S25" => "/images/s25.jpg",
                "Samsung Galaxy S25+" => "/images/s25+.jpg",
                "Samsung Galaxy S25 Ultra" => "/images/s25ultra.jpg",
                "Samsung Galaxy Z Flip 6" => "/images/flip6.jpg",
                "Samsung Galaxy Z Fold 6" => "/images/fold6.jpg",
                _ => p.Image
            };
            if (p.Image != targetImage)
            {
                p.Image = targetImage;
            }
        }
        db.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error seeding database image paths: " + ex.Message);
    }
}

app.Run();