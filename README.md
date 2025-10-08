# Hệ thống điều khiển và giám sát tập trung cho mạng LAN
Xây dựng một hệ thống Client – Server theo mô hình Remote Access Tool (RAT), cho phép quản trị và giám sát máy tính từ xa qua mạng LAN.
## Technologies Used 
- Ngôn ngữ: **C# (.NET 8)**
- Giao thức: **TCP Socket**
- Mô hình: **Client–Server**

## Install 

### 1. Yêu cầu môi trường
- Cài đặt [.NET SDK 8.0](https://dotnet.microsoft.com/download)  
- Hệ điều hành: Windows 10/11 hoặc Windows Server 2019+

### 2. Clone dự án
```bash
git clone https://github.com/bnguien/PBL4.git
cd PBL4
```
### 3. Build chương trình

Build toàn bộ solution:
```bash
dotnet build
```

Hoặc build riêng từng module:
```bash
cd src/Server
dotnet build
```
```bash
cd src/Client
dotnet build
```
## Use

1. Mở **terminal** tại thư mục `Server` và chạy:
   ```bash
   dotnet run
   ```
2. Mở **terminal** khác tại thư mục `Client` và chạy:
   ```bash
   dotnet run
   ```
## List of features

- **Kết nối & Quản lý**
  - Server lắng nghe trên cổng `5000` (có thể thay đổi cổng tùy ý), client tự động kết nối & duy trì kết nối.
  - Hiển thị trạng thái kết nối (Active/Idle).

- **Nhận diện Client**
  - Thu thập thông tin hệ thống, người dùng, IP, quyền tài khoản, quốc gia.

- **Điều khiển Desktop từ xa**
  - Truyền hình ảnh màn hình theo thời gian thực (có nén).
  - Hỗ trợ nhiều màn hình, điều khiển chuột & bàn phím.

- **Quản lý Tệp**
  - Duyệt ổ đĩa/thư mục, đổi tên, xóa, mở tệp.
  - Upload/Download file theo chunk, hiển thị tiến trình.

- **Quản lý Tiến trình**
  - Liệt kê process (PID, tên, cửa sổ), khởi chạy, kết thúc process.

- **Terminal từ xa**
  - Gửi lệnh, nhận output theo thời gian thực, quản lý phiên shell.

- **Thông tin Hệ thống**
  - CPU, RAM, GPU, ổ đĩa, OS, IP, MAC, uptime, antivirus, firewall.

- **Hộp thoại Thông báo**
  - Gửi thông báo với caption/nội dung/nút/icon, có chế độ preview & gửi hàng loạt.

- **Điều khiển Hệ thống**
  - Shutdown/Restart/Standby, yêu cầu reconnect, ngắt kết nối.

## Note
Project chỉ sử dụng cho mục đích **học tập và nghiên cứu**
