## Ứng dụng Thời tiết Windows Forms (THOITIET)

### Giới thiệu nhanh
Ứng dụng hiển thị thời tiết theo vị trí, kèm dự báo 24 giờ và 5 ngày. Mô tả trạng thái thời tiết được lấy đúng theo mô tả của OpenWeather và dịch sang tiếng Việt 1-1 (ví dụ: "overcast clouds" → "Nhiều mây"). Gợi ý hiển thị theo trạng thái thời tiết (không dựa theo nhiệt độ/độ ẩm/gió).

---

## 1) Yêu cầu hệ thống
- Windows 10/11
- .NET SDK 8.0 trở lên (khuyến nghị 8.0)
- Kết nối Internet
Kiểm tra .NET SDK:
```bash
dotnet --info
```

---

## 2) Lấy API Key
Ứng dụng dùng OpenWeather.

- Tạo tài khoản và lấy API key tại: `https://openweathermap.org/api`
- Bạn có thể cấu hình API key theo 1 trong 2 cách:
  1) Biến môi trường
     - `OPENWEATHER_API_KEY` (bắt buộc)
     - (Tùy chọn) `GEOAPIFY_API_KEY` nếu dùng geocoding Geoapify
     - (Tùy chọn) `GOOGLE_GEOCODING_API_KEY` nếu dùng geocoding Google
  2) File cấu hình `api_config.txt` đặt cạnh file `.sln`, mỗi dòng kiểu `KEY=VALUE`.

Ví dụ nội dung `api_config.txt`:
```text
OPENWEATHER_API_KEY=your_openweather_key_here
GOOGLE_GEOCODING_API_KEY=
GEOAPIFY_API_KEY=
```

Lưu ý: Ứng dụng gọi API với `lang=en` để lấy description tiếng Anh gốc, sau đó map sang tiếng Việt. Điều này đảm bảo không xuất hiện mô tả Việt hóa không mong muốn như "mây đen u ám".

---

## 3) Cấu trúc thư mục chính
- `Form1.cs` UI chính, binding dữ liệu và hiển thị
- `DichVuThoiTiet.cs` Gọi API (Weather/Forecast/One Call) và geocoding
- `WeatherApiClasses.cs` Kiểu dữ liệu, client One Call 3.0 (cũng dùng `lang=en`)
- `Controls/UnitToggle.cs` Công tắc đổi °C/°F
- `Properties/Resources.resx` Ảnh nền/icon
- `saved_locations.json` Danh sách địa điểm đã lưu

---

## 4) Build và chạy

### Bằng dòng lệnh
```powershell
cd THOITIET
dotnet build THOITIET.sln
dotnet run --project THOITIET.csproj
```

Nếu đang chạy app, hãy tắt app trước khi build lại để tránh lỗi “file đang được sử dụng”.

### Bằng Visual Studio
1. Mở `THOITIET.sln`
2. Chọn cấu hình `Debug | Any CPU`
3. Build và Run (F5)

---

## 5) Sử dụng nhanh trong ứng dụng
- Ô tìm kiếm: nhập địa danh (ví dụ: "Hà Nội, Vietnam") → Enter
- Công tắc `°C/°F`: đổi đơn vị nhiệt độ
- Khu vực thông tin chính hiển thị:
  - Nhiệt độ hiện tại lớn
  - Icon trạng thái thời tiết
  - Nhãn `nhanTrangThai`: mô tả tiếng Việt chuẩn + 2 gợi ý theo trạng thái
- Bảng 24 giờ: mỗi ô gồm giờ, nhiệt độ, icon nhỏ và mô tả rút gọn
- Dự báo 5 ngày: mô tả, cao/thấp và ghi chú mưa/gió

---

## 6) Quy tắc hiển thị mô tả & gợi ý
- API gọi với `lang=en` để lấy `weather.description` tiếng Anh gốc.
- Hàm `GetVietnameseWeatherDescription` ánh xạ 1-1 sang tiếng Việt (ví dụ "overcast clouds" → "Nhiều mây").
- Nhãn `nhanTrangThai` hiển thị: dòng 1 mô tả tiếng Việt; dòng 2 là 2 gợi ý hàng đầu.
- Gợi ý được tạo từ `GetWeatherSuggestions(string weatherDesc)` và chỉ dựa vào trạng thái (không dựa vào nhiệt độ/độ ẩm/gió).

---

## 7) Icon & bố cục
- Icon chính: lớn (khoảng 180x180)
- Icon bảng 24h và 5 ngày: nhỏ (khoảng 40x40)
- Không dùng icon trong biểu đồ
- `detailGridPanel`: lưới 2 cột × 3 hàng cho 5 panel (1 ô trống để cân đối)
- Các nhãn như "Áp suất khí quyển", "Tầm nhìn xa", "Cảm giác như" đã căn vị trí để không bị cắt chữ.

---

## 8) Khắc phục sự cố thường gặp
- Không cập nhật mô tả/mapping (vẫn thấy "mây đen u ám"):
  - Hãy đảm bảo gọi build lại sau khi đổi mã.
  - Kiểm tra URL API có `lang=en` trong `DichVuThoiTiet.cs` và `WeatherApiClasses.cs`.
  - Đảm bảo `GetVietnameseWeatherDescription` chứa cặp `{"overcast clouds", "Nhiều mây"}`.
- Lỗi build “file đang được sử dụng”:
  - Đóng app đang chạy rồi build lại.
- Không hiển thị gợi ý:
  - Ứng dụng đã bỏ banner cuộn và tích hợp gợi ý ngay dưới `nhanTrangThai`.
- Cảnh báo nullability: cảnh báo tại compile-time, không ảnh hưởng chạy; có thể tinh chỉnh thêm sau.

---

## 9) Tùy biến
- Thêm/bớt ánh xạ mô tả: chỉnh trong `GetVietnameseWeatherDescription` ở `Form1.cs`.
- Điều chỉnh gợi ý theo trạng thái: chỉnh trong `GetWeatherSuggestions`.
- Thay ảnh nền theo trạng thái/mã thời tiết: xem `SetBackground` trong `Form1.cs`.

---

## 10) Quy ước dịch mô tả (ví dụ)
- clear sky → Trời quang
- few clouds → Ít mây
- scattered clouds → Mây thưa
- broken clouds → Mây rải rác
- overcast clouds → Nhiều mây
- light/moderate/heavy rain → Mưa nhẹ/vừa/to
- very heavy rain → Mưa rất to
- extreme rain → Mưa cực to
- shower rain → Mưa rào
- light/heavy snow → Tuyết nhẹ/Tuyết to
- mist/fog → Sương mù/Sương mù dày
- thunderstorm variants → Bão (có mưa phùn nhẹ/vừa/to ...)

---

## 11) Đóng góp
PR/issue chào mừng. Vui lòng:
- Giữ quy tắc dịch 1-1 theo mô tả API gốc.
- Không bổ sung logic gợi ý dựa theo nhiệt độ/độ ẩm/gió nếu không có yêu cầu mới.

---

## 12) Giấy phép
Mã nguồn phục vụ học tập và mục đích cá nhân nội bộ.

