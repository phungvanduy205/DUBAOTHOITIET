# Script PowerShell để cấu hình API Keys cho ứng dụng thời tiết
# Chạy script này với quyền Administrator

Write-Host "=== CẤU HÌNH API KEYS CHO ỨNG DỤNG THỜI TIẾT ===" -ForegroundColor Green
Write-Host ""

# Kiểm tra quyền Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ Vui lòng chạy PowerShell với quyền Administrator!" -ForegroundColor Red
    Write-Host "Nhấn chuột phải vào PowerShell và chọn 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

Write-Host "✅ Đang chạy với quyền Administrator" -ForegroundColor Green
Write-Host ""

# Hướng dẫn lấy API keys
Write-Host "📋 HƯỚNG DẪN LẤY API KEYS:" -ForegroundColor Cyan
Write-Host "1. OpenWeather: https://openweathermap.org/api" -ForegroundColor White
Write-Host "2. Google Geocoding: https://console.cloud.google.com/ (tùy chọn)" -ForegroundColor White
Write-Host "3. Geoapify: https://www.geoapify.com/ (tùy chọn)" -ForegroundColor White
Write-Host "4. Windy: https://www.windy.com/ (tùy chọn)" -ForegroundColor White
Write-Host ""

# Nhập API keys
Write-Host "🔑 NHẬP API KEYS:" -ForegroundColor Cyan

# OpenWeather API Key (bắt buộc)
$openweatherKey = Read-Host "OpenWeather API Key (bắt buộc)"
if ([string]::IsNullOrWhiteSpace($openweatherKey)) {
    Write-Host "❌ OpenWeather API Key không được để trống!" -ForegroundColor Red
    Read-Host "Nhấn Enter để thoát"
    exit 1
}

# Google Geocoding API Key (tùy chọn)
$geocodingKey = Read-Host "Google Geocoding API Key (tùy chọn, Enter để bỏ qua)"

# Geoapify API Key (tùy chọn)
$geoapifyKey = Read-Host "Geoapify API Key (tùy chọn, Enter để bỏ qua)"

# Windy API Key (tùy chọn)
$windyKey = Read-Host "Windy API Key (tùy chọn, Enter để bỏ qua)"

Write-Host ""
Write-Host "⚙️ ĐANG CẤU HÌNH ENVIRONMENT VARIABLES..." -ForegroundColor Yellow

try {
    # Cấu hình OpenWeather API Key
    [Environment]::SetEnvironmentVariable("OPENWEATHER_API_KEY", $openweatherKey, "User")
    Write-Host "✅ OPENWEATHER_API_KEY đã được cấu hình" -ForegroundColor Green

    # Cấu hình Google Geocoding API Key (nếu có)
    if (![string]::IsNullOrWhiteSpace($geocodingKey)) {
        [Environment]::SetEnvironmentVariable("GEOCODING_API_KEY", $geocodingKey, "User")
        Write-Host "✅ GEOCODING_API_KEY đã được cấu hình" -ForegroundColor Green
    }

    # Cấu hình Geoapify API Key (nếu có)
    if (![string]::IsNullOrWhiteSpace($geoapifyKey)) {
        [Environment]::SetEnvironmentVariable("GEOAPIFY_API_KEY", $geoapifyKey, "User")
        Write-Host "✅ GEOAPIFY_API_KEY đã được cấu hình" -ForegroundColor Green
    }

    # Cấu hình Windy API Key (nếu có)
    if (![string]::IsNullOrWhiteSpace($windyKey)) {
        [Environment]::SetEnvironmentVariable("WINDY_API_KEY", $windyKey, "User")
        Write-Host "✅ WINDY_API_KEY đã được cấu hình" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "🎉 CẤU HÌNH HOÀN TẤT!" -ForegroundColor Green
    Write-Host "Bây giờ bạn có thể chạy ứng dụng thời tiết." -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Lưu ý: Có thể cần khởi động lại ứng dụng để nhận diện Environment Variables mới." -ForegroundColor Yellow

} catch {
    Write-Host "❌ Lỗi khi cấu hình: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Read-Host "Nhấn Enter để thoát"
