# Hướng dẫn Refactoring Code

## Tổng quan
Code đã được tách thành các service riêng biệt để dễ bảo trì và mở rộng.

## Cấu trúc thư mục

```
THOITIET/
├── Services/
│   ├── WeatherService.cs      # Xử lý API thời tiết
│   ├── LocationService.cs     # Quản lý địa điểm
│   ├── ChartService.cs        # Biểu đồ nhiệt độ
│   ├── MapService.cs          # Bản đồ Windy
│   ├── UIService.cs           # Các chức năng UI
│   ├── BackgroundService.cs   # Background động
│   └── ApiConfig.cs           # Cấu hình API
├── Models/
│   └── WeatherModels.cs       # Các class model
├── Form1.cs                   # Form chính (cũ)
├── Form1Refactored.cs         # Form chính (mới)
└── REFACTORING_GUIDE.md       # File này
```

## Các Service

### 1. WeatherService.cs
**Chức năng:** Xử lý tất cả các API call liên quan đến thời tiết

**Các method chính:**
- `GetCurrentWeatherAsync(lat, lon)` - Lấy thời tiết hiện tại
- `GetWeatherDataAsync(lat, lon)` - Lấy dữ liệu thời tiết đầy đủ
- `GetCurrentLocationAsync()` - Lấy vị trí hiện tại từ IP
- `GetCoordinatesAsync(locationName)` - Lấy tọa độ từ tên địa điểm
- `ReverseGeocodeAsync(lat, lon)` - Reverse geocoding

**Cách sử dụng:**
```csharp
var weatherData = await WeatherService.GetCurrentWeatherAsync(21.0285, 105.8542);
var location = await WeatherService.GetCurrentLocationAsync();
```

### 2. LocationService.cs
**Chức năng:** Quản lý địa điểm đã lưu

**Các method chính:**
- `SaveLocation(name, lat, lon, savedLocations)` - Lưu địa điểm
- `LoadSavedLocations(savedLocations, listBox)` - Load địa điểm đã lưu
- `SaveFavoriteLocations(favoriteLocations)` - Lưu địa điểm yêu thích
- `LoadFavoriteLocations(favoriteLocations)` - Load địa điểm yêu thích
- `SaveLocationNames(savedLocationNames)` - Lưu danh sách tên địa điểm
- `LoadLocationNames(savedLocationNames)` - Load danh sách tên địa điểm

**Cách sử dụng:**
```csharp
LocationService.SaveLocation("Hanoi", 21.0285, 105.8542, savedLocations);
LocationService.LoadSavedLocations(savedLocations, listBoxDiaDiemDaLuu);
```

### 3. ChartService.cs
**Chức năng:** Xử lý biểu đồ nhiệt độ

**Các method chính:**
- `InitializeTemperatureChart(size)` - Khởi tạo biểu đồ
- `Show24hChartForDay(chart, daily, hourlyData, isCelsius)` - Hiển thị biểu đồ 24h
- `ExportChart(chart, fileName)` - Xuất biểu đồ ra file

**Cách sử dụng:**
```csharp
var chart = ChartService.InitializeTemperatureChart(new Size(400, 300));
ChartService.Show24hChartForDay(chart, dailyWeather, hourlyData, true);
```

### 4. MapService.cs
**Chức năng:** Xử lý bản đồ Windy

**Các method chính:**
- `ShowMapAsync(lat, lon, temperatureChart)` - Hiển thị bản đồ
- `ShowChart(temperatureChart)` - Hiển thị biểu đồ
- `UpdateMapLocation(lat, lon)` - Cập nhật vị trí bản đồ
- `EnsureWindyBrowser(parent)` - Đảm bảo WebView2 đã khởi tạo

**Cách sử dụng:**
```csharp
var mapService = new MapService();
await mapService.ShowMapAsync(21.0285, 105.8542, temperatureChart);
```

### 5. UIService.cs
**Chức năng:** Các chức năng UI helper

**Các method chính:**
- `CreateDetailPanel(parent, icon, title, value, location, size)` - Tạo panel chi tiết
- `ApplyRoundedCorners(control, radius)` - Bo tròn góc
- `CreateLocationContextMenu(locations, onSelected, onRemoved)` - Tạo context menu
- `ShowError(message, title)` - Hiển thị thông báo lỗi
- `ShowSuccess(message, title)` - Hiển thị thông báo thành công
- `SetLoadingState(control, isLoading)` - Set trạng thái loading

**Cách sử dụng:**
```csharp
var panel = UIService.CreateDetailPanel(parent, "🌡️", "Nhiệt độ", "25°C", new Point(0, 0), new Size(100, 60));
UIService.ShowSuccess("Đã lưu thành công!");
```

### 6. BackgroundService.cs
**Chức năng:** Xử lý background động theo thời tiết

**Các method chính:**
- `SetBackground(weatherMain, weatherId)` - Set background theo thời tiết
- `SetDefaultBackgroundOnStartup()` - Set background mặc định
- `TestBackground()` - Test background
- `ForceSetBackgroundInLoad()` - Force set background trong Form_Load

**Cách sử dụng:**
```csharp
var backgroundService = new BackgroundService(this);
backgroundService.SetBackground("clear", 800);
```

## Models

### WeatherModels.cs
Chứa tất cả các class model:
- `SavedLocation` - Địa điểm đã lưu
- `FavoriteLocation` - Địa điểm yêu thích
- `OneCallResponse` - Response từ One Call API
- `CurrentWeather`, `HourlyWeather`, `DailyWeather` - Các loại dữ liệu thời tiết
- `WeatherInfo`, `MainInfo`, `WindInfo` - Các thông tin chi tiết

## Cách migrate từ Form1.cs cũ

### Bước 1: Tạo instance các service
```csharp
private readonly WeatherService weatherService;
private readonly LocationService locationService;
// ... các service khác

public Form1()
{
    InitializeComponent();
    InitializeServices();
}

private void InitializeServices()
{
    weatherService = new WeatherService();
    locationService = new LocationService();
    // ... khởi tạo các service khác
}
```

### Bước 2: Thay thế các method cũ
```csharp
// Cũ
private async Task LoadWeatherByIP()
{
    // Code cũ...
}

// Mới
private async Task LoadWeatherByIP()
{
    var locationData = await WeatherService.GetCurrentLocationAsync();
    // Sử dụng service...
}
```

### Bước 3: Sử dụng UI helper
```csharp
// Cũ
var panel = new Panel();
// Tạo panel thủ công...

// Mới
var panel = UIService.CreateDetailPanel(parent, icon, title, value, location, size);
```

## Lợi ích của việc refactoring

1. **Tách biệt trách nhiệm:** Mỗi service có một nhiệm vụ cụ thể
2. **Dễ test:** Có thể test từng service riêng biệt
3. **Dễ bảo trì:** Code được tổ chức rõ ràng
4. **Tái sử dụng:** Các service có thể được sử dụng ở nhiều nơi
5. **Mở rộng:** Dễ dàng thêm tính năng mới

## Lưu ý khi sử dụng

1. **Async/Await:** Các method API đều là async, nhớ sử dụng await
2. **Exception handling:** Luôn wrap các service call trong try-catch
3. **Dispose:** Nhớ dispose các service khi không dùng nữa
4. **Thread safety:** Các service là static, an toàn với thread
5. **Configuration:** Cập nhật API key trong ApiConfig.cs

## Ví dụ hoàn chỉnh

Xem file `Form1Refactored.cs` để thấy cách sử dụng tất cả các service trong một form hoàn chỉnh.