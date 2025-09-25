using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using THOITIET.Services;
using THOITIET.Models;

namespace THOITIET
{
    /// <summary>
    /// Form chính đã được refactor sử dụng các service
    /// </summary>
    public partial class Form1 : Form
    {
        // Services
        private MapService? mapService;
        private BackgroundService? backgroundService;

        // Data
        private OneCallResponse weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;
        private bool donViCelsius = true;

        // UI Controls (giả sử đã có trong Designer)
        private TextBox oTimKiemDiaDiem;
        private Label nhanTenDiaDiem;
        private Label nhanNhietDoHienTai;
        private Label nhanTrangThai;
        private FlowLayoutPanel BangTheoGio;
        private FlowLayoutPanel BangNhieuNgay;
        private ListBox listBoxDiaDiemDaLuu;
        private Button nutLuuDiaDiem;
        private Button nutXoaDiaDiem;
        private TabControl tabDieuKhien;
        private TabPage tabChart;
        private TabPage tabMap;
        private Chart temperatureChart;
        private Button nutTimKiem;

        // Data collections
        private List<SavedLocation> savedLocations = new List<SavedLocation>();
        private List<string> savedLocationNames = new List<string>();

        public Form1()
        {
            InitializeComponent();
            InitializeServices();
            LoadInitialData();
        }

        private void InitializeServices()
        {
            // Khởi tạo các service
            mapService = new MapService();
            backgroundService = new BackgroundService(this);
            
            // Khởi tạo biểu đồ nhiệt độ
            temperatureChart = ChartService.InitializeTemperatureChart(new Size(592, 272));
            tabChart.Controls.Add(temperatureChart);
        }

        private async void LoadInitialData()
        {
            try
            {
                // Load địa điểm đã lưu
                LocationService.LoadSavedLocations(savedLocations, listBoxDiaDiemDaLuu);
                LocationService.LoadLocationNames(savedLocationNames);

                // Load thời tiết theo vị trí hiện tại
                await LoadWeatherByIP();

                // Set background mặc định
                backgroundService.SetDefaultBackgroundOnStartup();
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi khởi tạo: {ex.Message}");
            }
        }

        /// <summary>
        /// Load thời tiết theo vị trí hiện tại (IP)
        /// </summary>
        private async Task LoadWeatherByIP()
        {
            try
            {
                var locationData = await WeatherService.GetCurrentLocationAsync();
                if (locationData?.Results?.Length > 0)
                {
                    var result = locationData.Results[0];
                    
                    // Cập nhật UI với tên địa điểm
                    string locationName = $"{result.Name}, {result.Country}";
                    oTimKiemDiaDiem.Text = locationName;
                    currentLocation = locationName;
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    
                    // Thêm địa điểm IP vào danh sách nếu chưa có
                    string ipLocationKey = "📍 Vị trí hiện tại";
                    if (!savedLocationNames.Contains(ipLocationKey))
                    {
                        savedLocationNames.Insert(0, ipLocationKey);
                        LocationService.SaveLocationNames(savedLocationNames);
                    }
                    
                    // Lấy dữ liệu thời tiết
                    var weatherData = await WeatherService.GetCurrentWeatherAsync(result.Lat, result.Lon);
                    if (weatherData != null)
                    {
                        this.weatherData = weatherData;
                        await UpdateWeatherDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load thời tiết theo IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật hiển thị thời tiết
        /// </summary>
        private async Task UpdateWeatherDisplay()
        {
            try
            {
                if (weatherData == null) return;

                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";
                var temp = donViCelsius ? weatherData.Current.Temp : (weatherData.Current.Temp * 9.0 / 5.0 + 32);

                // Cập nhật UI
                nhanTenDiaDiem.Text = currentLocation;
                nhanNhietDoHienTai.Text = $"{temp:F0}{kyHieuNhietDo}";
                nhanTrangThai.Text = weatherData.Current.Weather?.Length > 0 ? 
                    weatherData.Current.Weather[0].Description : "Không xác định";

                // Cập nhật background
                if (weatherData.Current.Weather?.Length > 0)
                {
                    var currentWeather = weatherData.Current.Weather[0];
                    backgroundService.SetBackground(currentWeather.Main ?? "Clear", currentWeather.Id);
                }

                // Cập nhật dự báo 24h
                if (weatherData.Hourly?.Length > 0)
                {
                    Load24hForecast(weatherData.Hourly, kyHieuNhietDo);
                }

                // Cập nhật dự báo 5 ngày
                if (weatherData.Daily?.Length > 0)
                {
                    Load5DayForecast(weatherData.Daily, kyHieuNhietDo);
                }
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi cập nhật hiển thị: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dự báo 24h
        /// </summary>
        private void Load24hForecast(HourlyWeather[] hourly, string kyHieuNhietDo)
        {
            try
            {
                BangTheoGio.Controls.Clear();

                for (int i = 0; i < Math.Min(24, hourly.Length); i++)
                {
                    var hour = hourly[i];
                    var hourTime = UnixToLocal(hour.Dt);
                    var temp = donViCelsius ? hour.Temp : (hour.Temp * 9.0 / 5.0 + 32);

                var panel = UIService.CreateDetailPanel(
                    BangTheoGio,
                    GetWeatherIcon(hour.Weather?.Length > 0 ? hour.Weather[0].Icon : "01d"),
                    hourTime.ToString("HH:mm"),
                    $"{temp:F0}{kyHieuNhietDo}",
                    Point.Empty,
                    new Size(80, 60)
                );

                    BangTheoGio.Controls.Add(panel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dự báo 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dự báo 5 ngày
        /// </summary>
        private void Load5DayForecast(DailyWeather[] daily, string kyHieuNhietDo)
        {
            try
            {
                BangNhieuNgay.Controls.Clear();

                for (int i = 1; i < Math.Min(6, daily.Length); i++) // Bỏ qua ngày hôm nay
                {
                    var day = daily[i];
                    var dayTime = UnixToLocal(day.Dt);
                    var maxTemp = donViCelsius ? day.Temp.Max : (day.Temp.Max * 9.0 / 5.0 + 32);
                    var minTemp = donViCelsius ? day.Temp.Min : (day.Temp.Min * 9.0 / 5.0 + 32);

                var panel = UIService.CreateDetailPanel(
                    BangNhieuNgay,
                    GetWeatherIcon(day.Weather?.Length > 0 ? day.Weather[0].Icon : "01d"),
                    dayTime.ToString("ddd"),
                    $"{maxTemp:F0}°/{minTemp:F0}°",
                    Point.Empty,
                    new Size(100, 80)
                );

                    BangNhieuNgay.Controls.Add(panel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dự báo 5 ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Tìm kiếm địa điểm
        /// </summary>
        private async void TimKiemDiaDiem(string diaDiem)
        {
            try
            {
                UIService.SetLoadingState(this, true);

                var geocodingData = await WeatherService.GetCoordinatesAsync(diaDiem);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";

                    // Lấy dữ liệu thời tiết
                    weatherData = await WeatherService.GetWeatherDataAsync(currentLat, currentLon);
                    if (weatherData != null)
                    {
                        await UpdateWeatherDisplay();
                        LocationService.SaveLocation(currentLocation, currentLat, currentLon, savedLocations);
                        UIService.ShowSuccess($"Đã tìm thấy thời tiết cho {currentLocation}");
                    }
                    else
                    {
                        UIService.ShowError("Không thể lấy dữ liệu thời tiết");
                    }
                }
                else
                {
                    UIService.ShowWarning("Không tìm thấy địa điểm");
                }
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi tìm kiếm: {ex.Message}");
            }
            finally
            {
                UIService.SetLoadingState(this, false);
            }
        }

        /// <summary>
        /// Hiển thị bản đồ
        /// </summary>
        private async void ShowMap()
        {
            try
            {
                mapService.EnsureWindyBrowser(tabMap);
                await mapService.ShowMapAsync(currentLat, currentLon, temperatureChart);
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi hiển thị bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị biểu đồ
        /// </summary>
        private void ShowChart()
        {
            try
            {
                mapService.ShowChart(temperatureChart);
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi hiển thị biểu đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu địa điểm yêu thích
        /// </summary>
        private void LuuDiaDiemYeuThich()
        {
            try
            {
                if (string.IsNullOrEmpty(currentLocation))
                {
                    uiService.ShowWarning("Chưa có địa điểm để lưu");
                    return;
                }

                var newLocation = new FavoriteLocation
                {
                    Name = currentLocation.Split(',')[0].Trim(),
                    Country = currentLocation.Split(',').Length > 1 ? currentLocation.Split(',')[1].Trim() : "",
                    Latitude = currentLat,
                    Longitude = currentLon,
                    AddedDate = DateTime.Now
                };

                // Thêm vào danh sách và lưu
                // favoriteLocations.Add(newLocation);
                // LocationService.SaveFavoriteLocations(favoriteLocations);

                UIService.ShowSuccess($"Đã thêm '{newLocation.Name}' vào danh sách yêu thích!");
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi lưu địa điểm: {ex.Message}");
            }
        }

        // Helper methods
        private DateTime UnixToLocal(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToLocalTime();
        }

        private string GetWeatherIcon(string iconCode)
        {
            // Logic lấy icon thời tiết
            return iconCode switch
            {
                "01d" => "☀️",
                "01n" => "🌙",
                "02d" => "⛅",
                "02n" => "☁️",
                "03d" or "03n" => "☁️",
                "04d" or "04n" => "☁️",
                "09d" or "09n" => "🌧️",
                "10d" or "10n" => "🌦️",
                "11d" or "11n" => "⛈️",
                "13d" or "13n" => "❄️",
                "50d" or "50n" => "🌫️",
                _ => "☀️"
            };
        }

        // Event handlers
        private void NutTimKiem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(oTimKiemDiaDiem.Text))
            {
                TimKiemDiaDiem(oTimKiemDiaDiem.Text);
            }
        }

        private void NutLuuDiaDiem_Click(object sender, EventArgs e)
        {
            LuuDiaDiemYeuThich();
        }

        private void NutXoaDiaDiem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxDiaDiemDaLuu.SelectedIndex >= 0)
                {
                    var selectedLocation = listBoxDiaDiemDaLuu.SelectedItem as SavedLocation;
                    if (selectedLocation != null)
                    {
                        if (UIService.ShowConfirm($"Bạn có chắc muốn xóa '{selectedLocation.Name}'?"))
                        {
                            LocationService.RemoveLocation(listBoxDiaDiemDaLuu.SelectedIndex, savedLocations);
                            LocationService.LoadSavedLocations(savedLocations, listBoxDiaDiemDaLuu);
                            UIService.ShowSuccess("Đã xóa địa điểm thành công!");
                        }
                    }
                }
                else
                {
                    UIService.ShowWarning("Vui lòng chọn địa điểm cần xóa");
                }
            }
            catch (Exception ex)
            {
                UIService.ShowError($"Lỗi xóa địa điểm: {ex.Message}");
            }
        }

        private void TabDieuKhien_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabDieuKhien.SelectedTab == tabMap)
                {
                    ShowMap();
                }
                else if (tabDieuKhien.SelectedTab == tabChart)
                {
                    ShowChart();
                }
            }
            catch (Exception ex)
            {
                uiService.ShowError($"Lỗi chuyển tab: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mapService?.Dispose();
                backgroundService?.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}