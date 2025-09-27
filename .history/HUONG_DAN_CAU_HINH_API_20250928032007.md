# HƯỚNG DẪN CẤU HÌNH API KEYS

Ứng dụng thời tiết cần các API keys để hoạt động. **CHỈ SỬ DỤNG FILE .env**

## Tại sao chỉ sử dụng file .env?
- **Đơn giản**: Chỉ cần tạo 1 file, không cần cấu hình phức tạp
- **Bảo mật**: Được bảo vệ bởi .gitignore, không bị commit vào Git
- **Linh hoạt**: Dễ dàng thay đổi cho từng môi trường
- **Dễ hiểu**: Người dùng mới có thể sử dụng ngay

## ⭐ CÁCH DUY NHẤT: Sử dụng file .env

### Bước 1: Tạo file .env
```bash
# Copy file mẫu
copy env.example .env
```

### Bước 2: Điền API keys vào file .env
Mở file `.env` và điền API keys thực:

```
OPENWEATHER_API_KEY=e3758b5bafed0fc3b4fa2cf4434f1dc1
GEOCODING_API_KEY=your_geocoding_key_here
GEOAPIFY_API_KEY=30009cf7650b4e6aaad866fd961c2e4d
WINDY_API_KEY=NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP
```
### Bước 3: Chạy ứng dụng
```bash
dotnet run
# hoặc
.\bin\Debug\net8.0-windows\THOITIET.exe
```

## Lấy API Keys

### 1. OpenWeather API Key (Bắt buộc):
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

## Cấu trúc file .env

```
# API Keys cho ứng dụng thời tiết
# Thay thế các giá trị bên dưới bằng API keys thực của bạn

# OpenWeather API Key (Bắt buộc)
OPENWEATHER_API_KEY=your_openweather_api_key_here

# Google Geocoding API Key (Tùy chọn)
GEOCODING_API_KEY=your_google_geocoding_key_here

# Geoapify API Key (Tùy chọn)  
GEOAPIFY_API_KEY=your_geoapify_key_here

# Windy API Key (Tùy chọn)
WINDY_API_KEY=your_windy_key_here
```

## Kiểm tra cấu hình

Sau khi cấu hình, chạy ứng dụng. Nếu cấu hình đúng, ứng dụng sẽ chạy bình thường. Nếu sai, sẽ hiện thông báo lỗi yêu cầu cấu hình API key.

## Lưu ý bảo mật

- ✅ File `.env` được bảo vệ bởi `.gitignore`
- ✅ Không commit file `.env` vào Git
- ✅ Chỉ sử dụng cho môi trường development
- ❌ Không chia sẻ API keys công khai

## Troubleshooting

### Lỗi "Không tìm thấy cấu hình API Keys":
1. Kiểm tra file `.env` có tồn tại không
2. Kiểm tra nội dung file `.env` có đúng format không
3. Kiểm tra API keys có hợp lệ không

### Lỗi "API key không hợp lệ":
1. Kiểm tra API key có đúng không
2. Kiểm tra API key có hết hạn không
3. Kiểm tra API key có đủ quyền không