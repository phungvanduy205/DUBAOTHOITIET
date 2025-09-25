# Hướng dẫn sử dụng chức năng Lịch sử Thời tiết

## Tổng quan
Ứng dụng thời tiết đã được nâng cấp với chức năng lưu trữ lịch sử thời tiết vào SQL Server LocalDB.

## Cấu hình Database
- **SQL Server**: LocalDB `(localdb)\MSSQLLocalDB`
- **Database**: WeatherDB
- **Bảng**: WeatherHistory

## Các chức năng mới

### 1. Tab "Lịch sử thời tiết"
- Tab mới được thêm vào bên cạnh "Biểu đồ nhiệt độ" và "Bản đồ"
- Hiển thị danh sách tất cả dữ liệu thời tiết đã lưu

### 2. Các nút chức năng
- **"Lưu thời tiết hiện tại"**: Lưu dữ liệu thời tiết hiện tại vào database
- **"Xóa đã chọn"**: Xóa bản ghi đã chọn trong danh sách
- **"Xóa tất cả"**: Xóa toàn bộ dữ liệu lịch sử
- **"Làm mới"**: Tải lại dữ liệu từ database

### 3. Cấu trúc dữ liệu lưu trữ
Bảng `WeatherHistory` chứa các thông tin:
- **Id**: Khóa chính (tự động tăng)
- **Location**: Tên địa điểm
- **Latitude/Longitude**: Tọa độ địa điểm
- **Temperature**: Nhiệt độ
- **FeelsLike**: Nhiệt độ cảm giác
- **Humidity**: Độ ẩm (%)
- **Pressure**: Áp suất (hPa)
- **WindSpeed**: Tốc độ gió
- **WindDirection**: Hướng gió
- **Visibility**: Tầm nhìn (km)
- **WeatherDescription**: Mô tả thời tiết
- **WeatherIcon**: Icon thời tiết
- **Unit**: Đơn vị nhiệt độ (Celsius/Fahrenheit)
- **RecordedAt**: Thời gian lưu

## Cách sử dụng

### Lưu dữ liệu thời tiết
1. Tìm kiếm địa điểm và xem thông tin thời tiết
2. Chuyển sang tab "Lịch sử thời tiết"
3. Nhấn nút "Lưu thời tiết hiện tại"
4. Dữ liệu sẽ được lưu vào database

### Xem lịch sử
1. Chuyển sang tab "Lịch sử thời tiết"
2. Danh sách sẽ hiển thị tất cả dữ liệu đã lưu
3. Sử dụng nút "Làm mới" để cập nhật danh sách

### Quản lý dữ liệu
- **Xóa bản ghi cụ thể**: Chọn bản ghi và nhấn "Xóa đã chọn"
- **Xóa tất cả**: Nhấn "Xóa tất cả" (có xác nhận)

## Lưu ý kỹ thuật

### Yêu cầu hệ thống
- SQL Server LocalDB phải được cài đặt
- .NET 8.0 Runtime

### Cấu hình kết nối
Connection string được cấu hình trong `DatabaseHelper.cs`:
```csharp
"Server=(localdb)\\MSSQLLocalDB;Database=WeatherDB;Integrated Security=true;TrustServerCertificate=true;"
```

### Khởi tạo tự động
- Database và bảng sẽ được tạo tự động khi ứng dụng khởi động
- Không cần cấu hình thủ công

## Xử lý lỗi
- Nếu không kết nối được database, ứng dụng sẽ hiển thị thông báo cảnh báo
- Chức năng lưu trữ sẽ bị vô hiệu hóa nếu không có kết nối database
- Các chức năng khác của ứng dụng vẫn hoạt động bình thường

## Mở rộng
Có thể dễ dàng mở rộng thêm các chức năng:
- Xuất dữ liệu ra Excel/CSV
- Thống kê dữ liệu theo thời gian
- So sánh dữ liệu giữa các địa điểm
- Biểu đồ xu hướng nhiệt độ