# 📱 HutechStore - Advanced Smart E-Commerce Platform

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?logo=.net&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)
[![Three.js](https://img.shields.io/badge/Three.js-Interactive%203D-000000?logo=three.js&logoColor=white)](https://threejs.org/)

Một nền tảng thương mại điện tử thông minh chuyên biệt cho điện thoại thông minh (iPhone & Samsung), được xây dựng trên nền tảng **ASP.NET Core MVC** và **SQL Server**. Dự án tích hợp các công nghệ hiện đại mang lại trải nghiệm người dùng tối ưu.

---

## ✨ Tính Năng Nổi Bật

*   🎨 **Interactive 3D Simulation**: Trực quan hóa sản phẩm bằng Three.js cho phép xoay, đổi màu và xem chi tiết sản phẩm 3D trực quan.
*   🤖 **AI Chatbot Consultant**: Trợ lý ảo tư vấn cấu hình điện thoại và gợi ý mua sắm thông minh kết hợp mô hình Gemini và n8n workflow.
*   📊 **Realtime Dashboard**: Trang quản trị theo dõi doanh số, lượt truy cập thời gian thực sử dụng SignalR.
*   🎮 **Gamification**: Tích hợp các minigame hấp dẫn như Vòng Quay May Mắn (Lucky Wheel) nhận coupon giảm giá và khảo sát (Survey) tích điểm.

---

## 🛠️ Công Nghệ Sử Dụng

*   **Backend**: ASP.NET Core MVC, Entity Framework Core (SQL Server Provider).
*   **Database**: Microsoft SQL Server (2019 / 2022 / LocalDB).
*   **Frontend**: HTML5, CSS3, JavaScript, Bootstrap, Three.js, SignalR.

---

## 🗄️ Hướng Dẫn Cấu Hình Cơ Sở Dữ Liệu (Database Setup)

Để dự án chạy ổn định và có đầy đủ dữ liệu mẫu (sản phẩm, hình ảnh màu chi tiết, mô tả storytelling biên tập chuyên nghiệp), người mới clone/pull dự án **PHẢI** thực hiện thiết lập cơ sở dữ liệu theo các bước dưới đây.

### 📋 Danh Sách & Thứ Tự Chạy Script SQL

Bạn cần chạy 3 file script SQL trong thư mục gốc theo đúng thứ tự sau:

| Thứ Tự | Tên File Script | Mục Đích | Tác Vụ |
| :---: | :--- | :--- | :--- |
| **1️⃣** | **[database_setup_v3.sql](file:///c:/Ecommerce-website/database_setup_v3.sql)** | Khởi tạo database `HutechStore`, tạo cấu trúc bảng chính và dữ liệu Seed cơ bản. | **Khởi tạo & Tạo Bảng** |
| **2️⃣** | **[update_database_images_v2.sql](file:///c:/Ecommerce-website/update_database_images_v2.sql)** | Đồng bộ hóa toàn bộ ảnh đại diện sản phẩm, bảng màu sắc và tạo đầy đủ các biến thể sản phẩm (Dung lượng, RAM, Giá riêng biệt cho từng màu). | **Nạp Hình Ảnh & Biến Thể** |
| **3️⃣** | **[update_product_descriptions.sql](file:///c:/Ecommerce-website/update_product_descriptions.sql)** | Cập nhật nội dung mô tả sản phẩm bằng các bài viết storytelling chất lượng cao (theo chuẩn Apple Newsroom & Samsung Galaxy). | **Nạp Mô Tả Chi Tiết** |

> [!WARNING]
> **Không chạy file `update_database_images.sql` cũ.** 
> Hãy sử dụng phiên bản cải tiến **`update_database_images_v2.sql`** để tránh các lỗi xung đột khóa ngoại (Foreign Key) và đảm bảo các biến thể lưu trữ được đồng bộ chính xác.

---

### 💻 Hướng Dẫn Chi Tiết Các Bước Thực Hiện

#### Bước 1: Tạo file cấu hình `appsettings.json`
1. Tại thư mục gốc của dự án, sao chép file cấu hình mẫu:
   * **Windows (PowerShell):** `cp appsettings.Example.json appsettings.json`
   * **MacOS / Linux:** `cp appsettings.Example.json appsettings.json`
2. Mở file `appsettings.json` và cấu hình chuỗi kết nối SQL Server của bạn tại mục `ConnectionStrings:DefaultConnection`.
   * **Nếu sử dụng LocalDB (Mặc định đi kèm Visual Studio):**
     ```json
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HutechStore;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
     ```
   * **Nếu sử dụng SQL Server Standalone hoặc Docker (Cần chỉnh sửa Server, User, Password):**
     ```json
     "DefaultConnection": "Server=YOUR_SERVER;Database=HutechStore;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true;TrustServerCertificate=True;"
     ```

#### Bước 2: Chạy các SQL Script vào SQL Server

##### 👉 Cách 1: Sử dụng công cụ đồ họa (SSMS / Azure Data Studio) - Khuyên Dùng
1. Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**.
2. Kết nối vào máy chủ SQL Server của bạn.
3. Nhấp đúp chuột mở lần lượt từng file script dưới đây từ thư mục dự án và nhấn nút **Execute (hoặc phím F5)** để chạy:
   * Chạy **`database_setup_v3.sql`** trước để tạo Database và cấu trúc bảng.
   * Chạy **`update_database_images_v2.sql`** tiếp theo để nạp ảnh/biến thể.
   * Chạy **`update_product_descriptions.sql`** cuối cùng để hoàn tất mô tả.

##### 👉 Cách 2: Sử dụng Command Line (`sqlcmd`) để chạy nhanh
Nếu máy tính của bạn đã cài đặt công cụ dòng lệnh `sqlcmd`, bạn có thể mở Terminal/PowerShell tại thư mục dự án và chạy nhanh các lệnh sau:

*   **Chạy với SQL Server LocalDB:**
    ```powershell
    sqlcmd -S "(localdb)\MSSQLLocalDB" -i database_setup_v3.sql
    sqlcmd -S "(localdb)\MSSQLLocalDB" -i update_database_images_v2.sql
    sqlcmd -S "(localdb)\MSSQLLocalDB" -i update_product_descriptions.sql
    ```

*   **Chạy với SQL Server Instance khác (Sử dụng Windows Authentication):**
    ```powershell
    sqlcmd -S "." -i database_setup_v3.sql
    sqlcmd -S "." -i update_database_images_v2.sql
    sqlcmd -S "." -i update_product_descriptions.sql
    ```

*   **Chạy với SQL Server Instance khác (Sử dụng SQL Server Authentication):**
    ```powershell
    sqlcmd -S "<Ten_Server>" -U "<Tai_Khoan>" -P "<Mat_Khau>" -i database_setup_v3.sql
    sqlcmd -S "<Ten_Server>" -U "<Tai_Khoan>" -P "<Mat_Khau>" -i update_database_images_v2.sql
    sqlcmd -S "<Ten_Server>" -U "<Tai_Khoan>" -P "<Mat_Khau>" -i update_product_descriptions.sql
    ```

> [!TIP]
> Sau khi bạn chạy thành công 3 script trên, hệ thống sẽ tự động khởi tạo thêm các bảng mở rộng như *SurveyQuestions, Surveys, LuckyWheelPrizes, LuckyWheelPlays* và nạp dữ liệu câu hỏi mặc định khi bạn khởi động ứng dụng lần đầu tiên nhờ cơ chế tự động Seed trong [Program.cs](file:///c:/Ecommerce-website/Program.cs).

---

## 🚀 Khởi Chạy Ứng Dụng (Run Project)

Sau khi hoàn tất cấu hình cơ sở dữ liệu ở trên:

1. Restore các package NuGet cần thiết:
   ```bash
   dotnet restore
   ```
2. Chạy ứng dụng:
   ```bash
   dotnet run
   ```
3. Mở trình duyệt và truy cập: **`http://localhost:5000`** (hoặc cổng hiển thị trên console).

---

## 🔑 Tài Khoản Đăng Nhập Mặc Định

Để kiểm tra các tính năng của người dùng và quản trị viên, bạn có thể đăng nhập bằng các tài khoản mẫu sau:

*   **Tài khoản Admin (Quản trị viên):**
    *   **Email:** `admin@hutechstore.com`
    *   **Mật khẩu:** `Admin@123`
*   **Tài khoản User (Khách hàng):**
    *   Bạn có thể tự tạo tài khoản mới dễ dàng thông qua chức năng Đăng ký trên website hoặc dùng tài khoản Google.

