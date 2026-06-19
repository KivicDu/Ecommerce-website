using MailKit.Net.Smtp;
using MimeKit;
using System.Text;
using HutechStore.Models;

namespace HutechStore.Services;

public class MailService
{
    private readonly IConfiguration _config;
    public MailService(IConfiguration config) { _config = config; }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var cfg  = _config.GetSection("MailSettings");
        string from = cfg["From"] ?? "no-reply@hutechstore.com";
        string host = cfg["Host"] ?? "localhost";
        int port = int.Parse(cfg["Port"] ?? "25");
        string username = cfg["Username"] ?? "";
        string password = cfg["Password"] ?? "";

        var msg  = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(from));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = "Mã xác nhận đặt lại mật khẩu - HutechStore";
        msg.Body    = new TextPart("html") { Text = BuildOtpEmail(otp) };

        using var smtp = new SmtpClient();
        smtp.CheckCertificateRevocation = false;
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendOrderConfirmationEmailAsync(string toEmail, Order order, List<OrderItem> items)
    {
        var cfg  = _config.GetSection("MailSettings");
        string from = cfg["From"] ?? "no-reply@hutechstore.com";
        string host = cfg["Host"] ?? "localhost";
        int port = int.Parse(cfg["Port"] ?? "25");
        string username = cfg["Username"] ?? "";
        string password = cfg["Password"] ?? "";

        var msg  = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(from));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = $"Xác nhận đơn hàng #{order.OrderNumber} - HutechStore";
        msg.Body    = new TextPart("html") { Text = BuildOrderConfirmationEmail(order, items) };

        using var smtp = new SmtpClient();
        smtp.CheckCertificateRevocation = false;
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }

    private static string BuildOtpEmail(string otp) => $"""
        <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:24px">
          <div style="background:#dc3545;color:#fff;padding:16px 24px;border-radius:8px 8px 0 0;text-align:center">
            <h2 style="margin:0">HutechStore</h2>
          </div>
          <div style="background:#fff;padding:24px;border:1px solid #dee2e6;border-top:none;border-radius:0 0 8px 8px">
            <p>Bạn đã yêu cầu đặt lại mật khẩu. Dùng mã OTP dưới đây:</p>
            <div style="text-align:center;margin:24px 0">
              <span style="font-size:36px;font-weight:900;letter-spacing:10px;color:#dc3545">{otp}</span>
            </div>
            <p style="color:#666;font-size:.9rem">Mã có hiệu lực trong <strong>10 phút</strong>.</p>
            <p style="color:#666;font-size:.9rem">Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
          </div>
        </div>
        """;

    private static string BuildOrderConfirmationEmail(Order order, List<OrderItem> items)
    {
        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.Append($"""
                <tr>
                  <td style="padding: 12px; border-bottom: 1px solid #E8E6DF; font-family: 'Inter', sans-serif; font-size: 14px; color: #1D1D1F;">{item.ProductName}</td>
                  <td style="padding: 12px; border-bottom: 1px solid #E8E6DF; font-family: 'Inter', sans-serif; font-size: 14px; color: #1D1D1F; text-align: center;">{item.Quantity}</td>
                  <td style="padding: 12px; border-bottom: 1px solid #E8E6DF; font-family: 'Inter', sans-serif; font-size: 14px; color: #1D1D1F; text-align: right;">{item.Price.ToString("N0")}₫</td>
                  <td style="padding: 12px; border-bottom: 1px solid #E8E6DF; font-family: 'Inter', sans-serif; font-size: 14px; color: #0F2A44; font-weight: bold; text-align: right;">{item.Total.ToString("N0")}₫</td>
                </tr>
            """);
        }

        return $"""
        <div style="background-color: #F2F0E8; padding: 40px 20px; font-family: 'Inter', -apple-system, sans-serif;">
          <div style="max-width: 600px; margin: 0 auto; background-color: #FAF9F5; border-radius: 16px; border: 1px solid rgba(194, 178, 128, 0.25); overflow: hidden; box-shadow: 0 4px 16px rgba(15, 42, 68, 0.03);">
            
            <!-- Logo Header -->
            <div style="background-color: #0F2A44; padding: 24px; text-align: center; border-bottom: 1px solid rgba(194, 178, 128, 0.1);">
              <h1 style="margin: 0; font-family: 'Cormorant Garamond', Georgia, serif; font-size: 26px; color: #E8E6DF; letter-spacing: 0.05em; font-weight: 600;">HUTECHSTORE</h1>
              <p style="margin: 4px 0 0 0; font-size: 12px; color: #C2B280; text-transform: uppercase; letter-spacing: 0.15em;">Đặc quyền công nghệ xa xỉ</p>
            </div>

            <div style="padding: 30px;">
              <h2 style="font-family: 'Cormorant Garamond', Georgia, serif; font-size: 22px; color: #0F2A44; margin-top: 0; margin-bottom: 12px; font-weight: 600;">Kính chào Quý khách,</h2>
              <p style="font-size: 14px; color: #6B6B6F; line-height: 1.6; margin-bottom: 24px;">
                Chân thành cảm ơn Quý khách đã gửi trọn niềm tin lựa chọn <strong>HutechStore</strong>. Đơn hàng của Quý khách đã được ghi nhận thành công và đang được bộ phận chuyên trách của chúng tôi chuẩn bị với sự trân trọng tối đa.
              </p>

              <!-- Thẻ thông tin đơn hàng -->
              <div style="background-color: #F2F0E8; border-radius: 12px; padding: 18px; margin-bottom: 24px; border: 1px solid rgba(194, 178, 128, 0.15);">
                <div style="font-size: 13px; color: #9B9B9F; text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 10px; font-weight: 600;">Thông tin đơn hàng</div>
                <table style="width: 100%; border-collapse: collapse; font-size: 14px;">
                  <tr>
                    <td style="padding: 4px 0; color: #6B6B6F; width: 120px;">Mã đơn hàng:</td>
                    <td style="padding: 4px 0; color: #0F2A44; font-weight: bold;">#{order.OrderNumber}</td>
                  </tr>
                  <tr>
                    <td style="padding: 4px 0; color: #6B6B6F;">Khách hàng:</td>
                    <td style="padding: 4px 0; color: #1D1D1F; font-weight: 500;">{order.ShippingName}</td>
                  </tr>
                  <tr>
                    <td style="padding: 4px 0; color: #6B6B6F;">Số điện thoại:</td>
                    <td style="padding: 4px 0; color: #1D1D1F;">{order.ShippingPhone}</td>
                  </tr>
                  <tr>
                    <td style="padding: 4px 0; color: #6B6B6F;">Địa chỉ giao:</td>
                    <td style="padding: 4px 0; color: #1D1D1F; line-height: 1.4;">{order.ShippingAddress}</td>
                  </tr>
                  <tr>
                    <td style="padding: 4px 0; color: #6B6B6F;">Thanh toán:</td>
                    <td style="padding: 4px 0; color: #1D1D1F; text-transform: uppercase; font-weight: 600; font-size: 12px;">{order.PaymentMethod}</td>
                  </tr>
                </table>
              </div>

              <!-- Bảng sản phẩm -->
              <table style="width: 100%; border-collapse: collapse; margin-bottom: 24px;">
                <thead>
                  <tr style="background-color: #0F2A44; color: #E8E6DF;">
                    <th style="padding: 10px 12px; font-family: 'Inter', sans-serif; font-size: 12px; font-weight: 600; text-align: left; border-radius: 6px 0 0 6px;">Sản phẩm</th>
                    <th style="padding: 10px 12px; font-family: 'Inter', sans-serif; font-size: 12px; font-weight: 600; text-align: center; width: 60px;">SL</th>
                    <th style="padding: 10px 12px; font-family: 'Inter', sans-serif; font-size: 12px; font-weight: 600; text-align: right; width: 100px;">Đơn giá</th>
                    <th style="padding: 10px 12px; font-family: 'Inter', sans-serif; font-size: 12px; font-weight: 600; text-align: right; width: 110px; border-radius: 0 6px 6px 0;">Tạm tính</th>
                  </tr>
                </thead>
                <tbody>
                  {sb.ToString()}
                </tbody>
              </table>

              <!-- Tổng kết tài chính -->
              <div style="width: 250px; margin-left: auto; font-size: 14px;">
                <table style="width: 100%; border-collapse: collapse;">
                  {(order.DiscountAmount > 0 ? $"""
                  <tr>
                    <td style="padding: 6px 0; color: #6B6B6F;">Giảm giá:</td>
                    <td style="padding: 6px 0; text-align: right; color: #27AE60; font-weight: 500;">-{order.DiscountAmount.ToString("N0")}₫</td>
                  </tr>
                  """ : "")}
                  <tr style="border-top: 1px solid #C2B280; font-size: 16px; font-weight: bold;">
                    <td style="padding: 10px 0; color: #0F2A44;">Tổng thanh toán:</td>
                    <td style="padding: 10px 0; text-align: right; color: #0F2A44;">{order.TotalAmount.ToString("N0")}₫</td>
                  </tr>
                </table>
              </div>

            </div>

            <!-- Footer -->
            <div style="background-color: #F2F0E8; padding: 20px; text-align: center; font-size: 12px; color: #9B9B9F; border-top: 1px solid rgba(194, 178, 128, 0.15);">
              <p style="margin: 0; font-weight: bold; color: #0F2A44;">HUTECHSTORE — ĐẶC QUYỀN HỘI VIÊN</p>
              <p style="margin: 4px 0 0 0;">Hotline: 1900 xxxx | Hỗ trợ chuyên nghiệp 24/7</p>
              <p style="margin: 8px 0 0 0; font-size: 10px; color: #9B9B9F;">Email này được gửi tự động từ hệ thống chăm sóc khách hàng HutechStore.</p>
            </div>

          </div>
        </div>
        """;
    }
}
