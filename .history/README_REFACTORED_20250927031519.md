# Dự báo thời tiết - Phiên bản Refactored

## Cấu trúc dự án sau khi refactor (Đã dọn dẹp)

### 📁 Models/
Chứa các class model để nhận dữ liệu từ API:
- **LopDuLieuAPI.cs**: Các class model cho API OpenWeather (OneCall, Geocoding)

### 📁 Services/
Chứa các dịch vụ xử lý logic:
- **DichVuThoiTiet.cs**: Gọi API OpenWeather, viết lại gọn gàng
- **XuLyTimKiem.cs**: Xử lý nhập địa điểm và sự kiện nút tìm kiếm
- **QuanLyDiaDiem.cs**: Lưu, xóa, lấy danh sách địa điểm từ cơ sở dữ liệu
- **QuanLyThoiTiet.cs**: Lưu và truy xuất dữ liệu thời tiết từ cơ sở dữ liệu

### 📁 Controls/
Chứa các control tùy chỉnh:
- **ChuyenDoiNhietDo.cs**: Chuyển đổi °C/°F
- **HienThiThoiTiet.cs**: Hiển thị thông tin thời tiết chính
- **DuBaoTheoGioControl.cs**: Hiển thị dự báo 24h
- **DuBaoNhieuNgayControl.cs**: Hiển thị dự báo 5 ngày

### 📄 Form1.cs
Form chính chỉ xử lý sự kiện UI và gọi các service/control tương ứng:
- Không chứa logic xử lý trực tiếp
- Chỉ có các sự kiện UI (btnTimKiem_Click, oTimKiemDiaDiem_KeyPress...)
- Gọi sang các class/module tương ứng

## Luồng hoạt động

```
Form1.cs (UI Events)
    ↓
XuLyTimKiem (Xử lý tìm kiếm)
    ↓
DichVuThoiTiet (Gọi API)
    ↓
QuanLyDiaDiem + QuanLyThoiTiet (Lưu DB)
    ↓
HienThiThoiTiet + DuBaoTheoGioControl + DuBaoNhieuNgayControl (Hiển thị)
```

## Tính năng chính

### ✅ Đã hoàn thành
- [x] Tách code thành các module riêng biệt
- [x] Sử dụng tên tiếng Việt không dấu, dễ hiểu
- [x] Loại bỏ code không sử dụng
- [x] Hợp nhất logic trùng lặp
- [x] Code rõ ràng, gọn gàng, có chú thích
- [x] Lưu dữ liệu thời tiết vào database
- [x] Quản lý địa điểm đã lưu
- [x] Chuyển đổi đơn vị nhiệt độ
- [x] Dự báo 24h và 5 ngày
- [x] Dọn dẹp file cũ không liên quan

### 🔧 Cấu hình
- **Database**: SQL Server LocalDB
- **API**: OpenWeatherMap (One Call 3.0, Geocoding)
- **Framework**: .NET 8.0 Windows Forms

### 📊 Dữ liệu lưu trữ
- **WeatherSnapshots**: Lưu dữ liệu thời tiết với tên cột tiếng Việt
- **SavedLocations**: Lưu địa điểm đã tìm kiếm

## Cách sử dụng

1. **Tìm kiếm địa điểm**: Nhập tên địa điểm và nhấn Enter hoặc click "Tìm kiếm"
2. **Chuyển đổi đơn vị**: Click vào nút °C/°F để chuyển đổi
3. **Xem dự báo**: Scroll xuống để xem dự báo 24h và 5 ngày
4. **Chọn ngày**: Click vào card ngày để xem chi tiết

## Lợi ích sau refactor

- **Dễ bảo trì**: Code được tách thành các module riêng biệt
- **Dễ mở rộng**: Có thể thêm tính năng mới mà không ảnh hưởng code cũ
- **Dễ test**: Mỗi module có thể test độc lập
- **Code sạch**: Loại bỏ code thừa, tên biến rõ ràng
- **Hiệu suất**: Tối ưu hóa việc gọi API và lưu trữ dữ liệu