# 📱 HutechStore - Advanced Smart E-Commerce Platform

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Three.js](https://img.shields.io/badge/Three.js-Interactive%203D-000000?logo=three.js&logoColor=white)](https://threejs.org/)

**HutechStore** là một nền tảng thương mại điện tử thông minh chuyên biệt cho điện thoại di động (iPhone & Samsung), được xây dựng trên nền tảng **ASP.NET Core MVC (NET 10.0)** và **Microsoft SQL Server**. Dự án tích hợp nhiều công nghệ hiện đại nhằm mang lại trải nghiệm tương tác trực quan và thông minh tối đa cho người dùng.

---

## ✨ Các Tính Năng Nổi Bật

*   🎨 **Trực quan hóa 3D (Interactive 3D Simulation)**: Xem chi tiết sản phẩm 3D tương tác (xoay, đổi màu, zoom cận cảnh) bằng Three.js.
*   🤖 **Trợ lý AI tư vấn (AI Chatbot Consultant)**: Chatbot thông minh tích hợp **Gemini API** tư vấn cấu hình sản phẩm, so sánh điện thoại và gợi ý mua sắm cá nhân hóa.
*   📊 **Bảng điều khiển thời gian thực (Realtime Dashboard)**: Trang quản trị (Admin) theo dõi doanh số, đơn hàng và lượng truy cập thời gian thực sử dụng **SignalR**.
*   🎮 **Mini-game tích điểm & coupon (Gamification)**: Vòng quay may mắn (Lucky Wheel) nhận mã giảm giá và Khảo sát (Survey) nhận coupon.
*   💳 **Thanh toán tự động**: Tích hợp cổng chuyển khoản ngân hàng qua quét mã QR với cơ chế Webhook nhận diện giao dịch tự động (**SePay**).

---

## 🛠️ Công Nghệ Sử Dụng

-   **Backend**: ASP.NET Core MVC (NET 10.0), Entity Framework Core 9.0.
-   **Database**: Microsoft SQL Server (2019 / 2022 / LocalDB).
-   **Frontend**: HTML5, CSS3, JavaScript (ES6+), Bootstrap 5, Three.js, SignalR.
-   **Tích hợp bên thứ ba**: Google OAuth, Gemini API (Google Generative AI), SMTP Mail Service, SePay API.

---

## 📋 Yêu Cầu Hệ Thống (Prerequisites)

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:
1.  **[.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)** trở lên.
2.  **[Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** (hoặc **SQL Server Express / LocalDB** đi kèm khi cài Visual Studio).
3.  **[SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** hoặc **[Azure Data Studio](https://learn.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio)** để thao tác với cơ sở dữ liệu.
4.  **[Git](https://git-scm.com/)** cài đặt trên máy.
5.  Một IDE/Editor: **Visual Studio 2022 (v17.12+)** (khuyên dùng), **Visual Studio Code**, hoặc **JetBrains Rider**.

---

## 🚀 Hướng Dẫn Cài Đặt Chi Tiết (Step-by-Step Setup)

Thực hiện theo 5 bước dưới đây để chạy dự án trên máy cục bộ của bạn:

### Bước 1: Clone dự án về máy tính
Mở Terminal (hoặc PowerShell / Command Prompt) và chạy lệnh sau để tải mã nguồn:
```bash
git clone https://github.com/KivicDu/Ecommerce-website.git
cd Ecommerce-website
```

### Bước 2: Tạo và cấu hình file `appsettings.json`
1. Tại thư mục gốc của dự án, sao chép file cấu hình mẫu:
   * **Trên Windows (PowerShell):**
     ```powershell
     cp appsettings.Example.json appsettings.json
     ```
   * **Trên MacOS / Linux (Terminal):**
     ```bash
     cp appsettings.Example.json appsettings.json
     ```
2. Mở file `appsettings.json` mới tạo và cấu hình các thông số sau:

   * **Chuỗi kết nối SQL Server (`ConnectionStrings:DefaultConnection`):**
     * *Nếu dùng LocalDB (Mặc định đi kèm Visual Studio):*
       ```json
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HutechStore;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
       ```
     * *Nếu dùng SQL Server Standalone / Docker (Cần khai báo Server, User, Password):*
       ```json
       "DefaultConnection": "Server=YOUR_SERVER_IP;Database=HutechStore;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;"
       ```
   * **Cấu hình Trợ lý ảo AI (`GeminiApi`):**
     Điền API Key của bạn từ Google AI Studio để chatbot hoạt động:
     ```json
     "GeminiApi": {
       "ApiKey": "YOUR_GEMINI_API_KEY",
       "Model": "gemini-2.5-flash-lite",
       "BaseUrl": "https://generativelanguage.googleapis.com/v1beta/models"
     }
     ```
   * **Cấu hình Đăng nhập Google (`Authentication:Google`):**
     Nhận ClientId và ClientSecret từ Google Cloud Console để kích hoạt tính năng đăng nhập Google:
     ```json
     "Google": {
       "ClientId": "YOUR_GOOGLE_CLIENT_ID",
       "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
     }
     ```
   * **Cấu hình SMTP gửi Mail OTP (`MailSettings`):**
     Sử dụng SMTP của Gmail để gửi mã OTP khi người dùng quên mật khẩu:
     ```json
     "MailSettings": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "Username": "YOUR_EMAIL@gmail.com",
       "Password": "YOUR_APP_PASSWORD_HERE",
       "From": "HutechStore <YOUR_EMAIL@gmail.com>"
     }
     ```

### Bước 3: Khởi tạo và nạp dữ liệu Cơ sở dữ liệu
Dự án sử dụng cơ sở dữ liệu có sẵn dữ liệu mẫu (sản phẩm, hình ảnh màu, mô tả chi tiết chuyên nghiệp). Bạn cần chạy các script SQL theo đúng thứ tự sau:

| Thứ Tự | Tên File Script | Mục Tiêu | Tác Vụ |
| :---: | :--- | :--- | :--- |
| **1️⃣** | **`database_setup_v3.sql`** | Tạo database `HutechStore`, tạo cấu trúc các bảng chính và nạp tài khoản Admin. | **Khởi tạo & Tạo bảng** |
| **2️⃣** | **`update_database_images_v2.sql`** | Nạp toàn bộ hình ảnh sản phẩm thực tế, bảng màu sắc và các cấu hình biến thể (RAM, dung lượng). | **Nạp Hình ảnh & Biến thể** |
| **3️⃣** | **`update_product_descriptions.sql`** | Nạp nội dung mô tả sản phẩm copywriting chuẩn Apple Newsroom & Samsung Galaxy. | **Nạp Mô tả Chi tiết** |

> [!WARNING]
> Không chạy file `update_database_images.sql` cũ. Hãy sử dụng file **`update_database_images_v2.sql`** để tránh các lỗi xung đột khóa ngoại (Foreign Key) và đảm bảo các biến thể lưu trữ được đồng bộ chính xác.

#### Cách 1: Sử dụng công cụ đồ họa (SSMS / Azure Data Studio) - Khuyên dùng
1. Mở SSMS hoặc Azure Data Studio và kết nối vào máy chủ SQL Server của bạn.
2. Mở lần lượt từng file script SQL trong thư mục dự án theo thứ tự đã nêu.
3. Nhấn **Execute (hoặc phím F5)** để chạy từng script.

#### Cách 2: Sử dụng dòng lệnh `sqlcmd` (Dành cho nhà phát triển thích Terminal)
Mở Terminal/PowerShell tại thư mục dự án và chạy các lệnh tương ứng:
* **Với LocalDB:**
  ```powershell
  sqlcmd -S "(localdb)\MSSQLLocalDB" -i database_setup_v3.sql
  sqlcmd -S "(localdb)\MSSQLLocalDB" -i update_database_images_v2.sql
  sqlcmd -S "(localdb)\MSSQLLocalDB" -i update_product_descriptions.sql
  ```
* **Với SQL Server Instance (Xác thực Windows):**
  ```powershell
  sqlcmd -S "." -i database_setup_v3.sql
  sqlcmd -S "." -i update_database_images_v2.sql
  sqlcmd -S "." -i update_product_descriptions.sql
  ```
* **Với SQL Server Instance (Xác thực SQL Server):**
  ```powershell
  sqlcmd -S "SERVER_NAME" -U "USERNAME" -P "PASSWORD" -i database_setup_v3.sql
  sqlcmd -S "SERVER_NAME" -U "USERNAME" -P "PASSWORD" -i update_database_images_v2.sql
  sqlcmd -S "SERVER_NAME" -U "USERNAME" -P "PASSWORD" -i update_product_descriptions.sql
  ```

> [!TIP]
> Sau khi chạy thành công 3 script trên, hệ thống sẽ tự động khởi tạo thêm các bảng mở rộng như *SurveyQuestions, Surveys, LuckyWheelPrizes, LuckyWheelPlays* và nạp dữ liệu mặc định khi khởi động ứng dụng lần đầu nhờ cơ chế tự động Seed trong [Program.cs](file:///c:/Ecommerce-website/Program.cs).

### Bước 4: Restore Package NuGet & Build Dự án
Chạy lệnh sau để tải các package thư viện cần thiết cho dự án:
```bash
dotnet restore
dotnet build
```

### Bước 5: Chạy dự án
Bắt đầu chạy server local bằng lệnh:
```bash
dotnet run
```
Sau khi chạy thành công, giao diện bảng điều khiển của .NET sẽ hiển thị các cổng truy cập. Mở trình duyệt và truy cập vào địa chỉ:
* **`http://localhost:5000`** hoặc **`https://localhost:5001`** (hoặc cổng bất kỳ hiển thị trên màn hình console của bạn).

---

## 🔑 Tài Khoản Đăng Nhập Mặc Định

Sau khi chạy thành công cơ sở dữ liệu mẫu, bạn có thể đăng nhập bằng các tài khoản sau để trải nghiệm các tính năng:

*   **Tài khoản Quản trị viên (Admin):**
    *   **Email:** `admin@hutechstore.com`
    *   **Mật khẩu:** `Admin@123`
*   **Tài khoản Khách hàng (User):**
    *   Bạn có thể tự đăng ký một tài khoản mới trực tiếp từ giao diện trang web hoặc sử dụng tính năng Đăng nhập Google (nếu đã cấu hình).

---

## 📁 Cấu Trúc Dự Án Chính

```text
├── Controllers/         # Các Controller xử lý Logic chính (Auth, Cart, Order, Product, Admin,...)
├── Data/                # Chứa DbContext cấu hình các mối quan hệ bảng & Entity Framework Core
├── Models/              # Các Model biểu diễn dữ liệu của cơ sở dữ liệu
├── Services/            # Các Service xử lý logic nghiệp vụ phụ (Mail, Cart, Gemini AI,...)
├── ViewModels/          # Các ViewModel đóng gói dữ liệu truyền tải giữa Controller & View
├── Views/               # Thư mục chứa giao diện Razor (.cshtml) cho trang khách hàng & admin
├── wwwroot/             # Tài nguyên tĩnh: CSS, JS tự thiết kế, thư viện Client, Hình ảnh 3D, Hình ảnh sản phẩm
├── appsettings.json     # Cấu hình kết nối DB, API Key bên thứ ba (đã bỏ qua trong .gitignore)
├── database_setup_v3.sql # Script SQL khởi tạo DB HutechStore ban đầu
```

---

## 🛠️ Một số lỗi thường gặp & Cách khắc phục (Troubleshooting)

1.  **Lỗi kết nối cơ sở dữ liệu (`SqlException: Cannot open database ... requested by the login`):**
    *   Đảm bảo dịch vụ SQL Server đang chạy.
    *   Kiểm tra kỹ lại chuỗi kết nối trong `appsettings.json` xem tên Server đã chính xác chưa.
    *   Nếu dùng LocalDB, hãy chắc chắn đã bật LocalDB bằng lệnh `sqllocaldb start MSSQLLocalDB`.

2.  **Lỗi hiển thị sai Tiếng Việt (Font Encoding):**
    *   File script SQL được lưu ở định dạng UTF-8. Nếu bạn import bằng SSMS bị lỗi font hiển thị trên web, đừng lo lắng! Dự án tích hợp cơ chế tự động phát hiện và sửa lỗi mã hóa ký tự Tiếng Việt ngay tại [Program.cs](file:///c:/Ecommerce-website/Program.cs) khi khởi động ứng dụng lần đầu tiên.

3.  **Chatbot AI không hoạt động hoặc báo lỗi:**
    *   Kiểm tra xem bạn đã điền đúng `ApiKey` của Gemini trong `appsettings.json` chưa. Nếu không có API Key, Chatbot sẽ không thể phản hồi câu hỏi tư vấn sản phẩm.

---

## 📝 Bản quyền & Giấy phép
Dự án được phân phối dưới giấy phép **MIT License**. Bạn có thể tự do clone, chỉnh sửa và phát triển thêm cho mục đích học tập và làm việc.
