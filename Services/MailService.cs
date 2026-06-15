using MailKit.Net.Smtp;
using MimeKit;

namespace HutechStore.Services;

public class MailService
{
    private readonly IConfiguration _config;
    public MailService(IConfiguration config) { _config = config; }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var cfg  = _config.GetSection("MailSettings");
        var msg  = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(cfg["From"]));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = "Mã xác nhận đặt lại mật khẩu - HutechStore";
        msg.Body    = new TextPart("html") { Text = BuildOtpEmail(otp) };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(cfg["Host"], int.Parse(cfg["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(cfg["Username"], cfg["Password"]);
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
}
