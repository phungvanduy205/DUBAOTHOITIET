# HƯỚNG DẪN CẤU HÌNH API KEYS

## Tại sao cần tách API keys ra khỏi code?
- **Bảo mật**: Tránh lộ API keys khi chia sẻ code
- **Linh hoạt**: Dễ dàng thay đổi API keys mà không cần sửa code
- **Môi trường**: Có thể dùng API keys khác nhau cho dev/prod

## ⭐ CÁCH 1: Sử dụng Environment Variables (KHUYẾN NGHỊ - ƯU TIÊN)

### ✅ Ưu điểm của Environment Variables:
- **Bảo mật cao nhất**: Không lưu trong file, không bị commit vào Git
- **Linh hoạt**: Dễ dàng thay đổi cho từng môi trường (dev/prod)
- **Tự động**: Không cần tạo file, chỉ cần set một lần
- **Chuẩn công nghiệp**: Được sử dụng rộng rãi trong các ứng dụng production

### Windows (PowerShell):
```powershell
# Mở PowerShell với quyền Administrator
[Environment]::SetEnvironmentVariable("OPENWEATHER_API_KEY", "your_api_key_here", "User")
[Environment]::SetEnvironmentVariable("GEOCODING_API_KEY", "your_geocoding_key_here", "User")
[Environment]::SetEnvironmentVariable("GEOAPIFY_API_KEY", "your_geoapify_key_here", "User")
[Environment]::SetEnvironmentVariable("WINDY_API_KEY", "your_windy_key_here", "User")
```

### Windows (Command Prompt):
```cmd
setx OPENWEATHER_API_KEY "your_api_key_here"
setx GEOCODING_API_KEY "your_geocoding_key_here"
setx GEOAPIFY_API_KEY "your_geoapify_key_here"
setx WINDY_API_KEY "your_windy_key_here"
```

### Linux/Mac:
```bash
export OPENWEATHER_API_KEY="your_api_key_here"
export GEOCODING_API_KEY="your_geocoding_key_here"
export GEOAPIFY_API_KEY="your_geoapify_key_here"
export WINDY_API_KEY="your_windy_key_here"
```

## Cách 2: Sử dụng file api_config.txt

Tạo file `api_config.txt` trong thư mục gốc của dự án:

```
OPENWEATHER_API_KEY=your_openweather_api_key_here
GEOCODING_API_KEY=your_geocoding_api_key_here
GEOAPIFY_API_KEY=your_geoapify_api_key_here
WINDY_API_KEY=your_windy_api_key_here
```

## Cách 3: Sử dụng appsettings.json (Nâng cao)

Tạo file `appsettings.json`:
```json
{
  "ApiKeys": {
    "OpenWeather": "your_openweather_api_key_here",
    "Geocoding": "your_geocoding_api_key_here",
    "Geoapify": "your_geoapify_api_key_here"
  }
}
```

## Lấy API Keys

### 1. OpenWeather API Key:
- Truy cập: https://openweathermap.org/api
- Đăng ký tài khoản miễn phí
- Lấy API key từ dashboard

### 2. Google Geocoding API Key (Tùy chọn):
- Truy cập: https://console.cloud.google.com/
- Bật Geocoding API
- Tạo API key

### 3. Geoapify API Key (Tùy chọn):
- Truy cập: https://www.geoapify.com/
- Đăng ký tài khoản miễn phí
- Lấy API key từ dashboard

### 4. Windy API Key (Tùy chọn):
- Truy cập: https://www.windy.com/
- Đăng ký tài khoản miễn phí
- Lấy API key từ dashboard

## Kiểm tra cấu hình

Sau khi cấu hình, chạy ứng dụng. Nếu cấu hình đúng, ứng dụng sẽ chạy bình thường. Nếu sai, sẽ hiện thông báo lỗi yêu cầu cấu hình API key.

## Lưu ý bảo mật

- **KHÔNG** commit API keys vào Git
- **KHÔNG** chia sẻ API keys công khai
- Thêm `api_config.txt` vào `.gitignore`
- Sử dụng environment variables cho production
