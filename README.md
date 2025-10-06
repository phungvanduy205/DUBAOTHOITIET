# DUBAOTHOITIET
## Ứng dụng WinForms Thời Tiết (THOITIET)

Ứng dụng WinForms (.NET 8) hiển thị thời tiết hiện tại, dự báo theo giờ/ngày và bản đồ gió (Windy). Dữ liệu thời tiết sử dụng OpenWeather.

### Yêu cầu
- .NET SDK 8.0+
- Windows 10/11
- API keys: OpenWeather (bắt buộc), Geocoding/GEOAPIFY (tùy chọn), Windy (tùy chọn)

### Cấu hình API keys
1) Sao chép file `env.example` thành `.env` ở thư mục gốc dự án.
2) Điền API keys của bạn vào `.env`:

```
OPENWEATHER_API_KEY=your_api_key_here
GEOCODING_API_KEY=your_geocoding_key_here
GEOAPIFY_API_KEY=your_geoapify_key_here
WINDY_API_KEY=your_windy_key_here
```

Mã nguồn sẽ đọc `.env` qua lớp `CauHinhApi` trong `WeatherApiClasses.cs`.

### Chạy dự án
- Mở solution `THOITIET.sln` bằng Visual Studio 2022 hoặc dùng CLI:

```bash
dotnet restore
dotnet build
dotnet run --project THOITIET.csproj
```

### Đóng gói (Publish)
```bash
dotnet publish -c Release -r win-x64 --self-contained false
```
Artifact nằm trong `bin/Release/net8.0-windows/`.

### Thư mục quan trọng
- `Form1.cs`: UI chính
- `DichVuThoiTiet.cs`: gọi API OpenWeather (2.5, 3.0)
- `WeatherApiClasses.cs`: model/DTO và `CauHinhApi` đọc `.env`
- `WindyMapForm.cs`: tích hợp bản đồ Windy (nếu dùng)

### Gỡ bí mật trước khi up GitHub
- Đảm bảo KHÔNG commit file `.env`. File này đã được ignore trong `.gitignore`.
- Không đặt API key cứng trong mã nguồn. Hãy dùng biến môi trường như hướng dẫn trên.

### Giấy phép
Bạn có thể thêm giấy phép (MIT, Apache-2.0, v.v.) theo nhu cầu.

