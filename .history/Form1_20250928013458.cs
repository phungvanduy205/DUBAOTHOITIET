using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Web.WebView2.WinForms;

namespace THOITIET
{
    /// Form chính: xử lý sự kiện, gọi dịch vụ, cập nhật giao diện
    public partial class Form1 : Form
    {
        #region Fields và Properties
        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;
        // Dữ liệu thời tiết từ API
        private OneCallResponse weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;
        // File lưu địa điểm
        private readonly string locationsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_locations.txt");
        // Danh sách địa điểm đã lưu (giữ để liên kết giao diện, nguồn lấy từ file txt)
        private List<SavedLocation> savedLocations = new List<SavedLocation>();
        // Bộ đếm thời gian tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();
        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();
        // Các fields cho tính năng nâng cao
        private Chart? bieuDoNhietDo;
        private WebView2? banDoGio;
        private const string KHOABAN_DOGIO = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
        private List<FavoriteLocation> diaDiemYeuThich = new List<FavoriteLocation>();
        private int chiSoNgayDaChon = 0; // Ngày được chọn trong dự báo 5 ngày

        // Throttle nền: lưu trạng thái lần trước
        private int? thoiTietIdCu = null;
        private bool? banDemCu = null;

        #endregion
        #region Weather API và Data Loading
        private void HienThiThongTin(string name, OneCallResponse weather)
        {
            try
            {
                if (weather?.Current == null)
                {
                    return;
                }
                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";
                double nhietDoHienTai = donViCelsius
                    ? TemperatureConverter.ToCelsius(weather.Current.Temp)
                    : TemperatureConverter.ToFahrenheit(weather.Current.Temp);
                nhanNhietDoHienTai.Text = $"{Math.Round(nhietDoHienTai)}{kyHieuNhietDo}";
                var currentDescEn = weather.Current.Weather?[0]?.Description ?? "Không xác định";
                var currentDescVi = GetVietnameseWeatherDescription(currentDescEn);
                var currentSuggestions = GetWeatherSuggestions(currentDescEn);
                nhanTrangThai.Text = $"{currentDescVi}\n💡 {string.Join(" • ", currentSuggestions.Take(2))}";
                if (anhIconThoiTiet != null && weather.Current.Weather?.Length > 0)
                {
                    string iconCode = weather.Current.Weather[0].Icon ?? "01d";
                    anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                }
                CapNhatDiaDiem(name);
                CapNhatThoiGian();
                CapNhatPanelChiTietFromApi(weather.Current, kyHieuNhietDo);
                if (weather.Current.Weather?.Length > 0)
                {
                    var currentWeather = weather.Current.Weather[0];
                    SetBackground(currentWeather.Main ?? "Clear", currentWeather.Id);
                }
                else
                {
                    SetBackground("Clear", 800);
                }
                if (weather.Hourly != null && weather.Hourly.Length > 0)
                {
                    LoadDuBao24h(weather.Hourly, kyHieuNhietDo);
                }
                if (weather.Daily != null && weather.Daily.Length > 0)
                {
                    LoadForecast5Days(weather.Daily, kyHieuNhietDo);
                }
                BangTheoGio.Controls.Clear();
                BangNhieuNgay.Controls.Clear();
            }
            catch (Exception ex)
            {
            }
        }
        private async Task LoadWeatherByIP()
        {
            try
            {
                var locationData = await WeatherApiService.GetCurrentLocationAsync();
                if (locationData?.Results?.Length > 0)
                {
                    var result = locationData.Results[0];
                    string locationName = $"{result.Name}, {result.Country}";
                    oTimKiemDiaDiem.Text = locationName;
                    currentLocation = locationName;
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    CapNhatDiaDiem(locationName);
                    LuuDiaDiemSilent(locationName, result.Lat, result.Lon);
                    NapDiaDiemDaLuu();
                    var weatherData = await WeatherApiService.GetCurrentWeatherAsync(result.Lat, result.Lon);
                    if (weatherData != null)
                    {
                        this.weatherData = weatherData;
                        await CapNhatThoiTiet();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task LoadWeatherForDefaultLocation(string locationName)
        {
            try
            {
                var geocodingData = await WeatherApiService.GetCoordinatesAsync(locationName);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";
                    weatherData = await WeatherApiService.GetWeatherDataAsync(currentLat, currentLon);
                    if (weatherData != null)
                    {
                        HienThiThongTin(currentLocation, weatherData);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private async Task CapNhatThoiTiet()
        {
            if (weatherData == null) return;
            try
            {
                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";
                if (weatherData.Current != null)
                {
                    double nhietDoHienTai = donViCelsius
                        ? TemperatureConverter.ToCelsius(weatherData.Current.Temp)
                        : TemperatureConverter.ToFahrenheit(weatherData.Current.Temp);
                    nhanNhietDoHienTai.Text = $"{Math.Round(nhietDoHienTai)}{kyHieuNhietDo}";
                    var current = weatherData.Current;
                    var weatherDesc = GetVietnameseWeatherDescription(current.Weather?[0]?.Description ?? "Không xác định");
                    var suggestions = GetWeatherSuggestions(current.Weather?[0]?.Description ?? "");
                    nhanTrangThai.Text = $"{weatherDesc}\n💡 {string.Join(" • ", suggestions.Take(2))}";
                    if (anhIconThoiTiet != null && current.Weather?.Length > 0)
                    {
                        string iconCode = current.Weather[0].Icon ?? "01d";
                        anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                    }
                    CapNhatDiaDiem(currentLocation);
                    CapNhatThoiGian();
                    CapNhatPanelChiTietFromApi(current, kyHieuNhietDo);
                    SetBackground(current.Weather?[0]?.Main ?? "Clear", current.Weather?[0]?.Id ?? 800);
                }
                if (weatherData.Hourly != null && weatherData.Hourly.Length > 0)
                {
                    LoadDuBao24h(weatherData.Hourly, kyHieuNhietDo);
                }
                if (weatherData.Daily != null && weatherData.Daily.Length > 0)
                {
                    LoadForecast5Days(weatherData.Daily, kyHieuNhietDo);
                }
                BangTheoGio.Controls.Clear();
                BangNhieuNgay.Controls.Clear();
            }
            catch (Exception ex)
            {
            }
        }
        private async Task TimKiemDiaDiem(string diaDiem)
        {
            try
            {
                var geocodingData = await WeatherApiService.GetCoordinatesAsync(diaDiem);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";
                    try
                    {
                        weatherData = await WeatherApiService.GetWeatherDataAsync(currentLat, currentLon);
                        if (weatherData != null)
                        {
                            HienThiThongTin(currentLocation, weatherData);
                        }
                    }
                    catch (Exception apiEx)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region Constructor và Form Events
        public Form1()
        {
            InitializeComponent();
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();
            // Đồng bộ hóa donViCelsius với unitToggle.LaCelsius
            donViCelsius = unitToggle.LaCelsius;
            // Đăng ký event DonViThayDoi để cập nhật hiển thị từ dữ liệu Kelvin
            unitToggle.DonViThayDoi += async (sender, laCelsius) => {
                donViCelsius = laCelsius;
                if (weatherData != null)
                    await CapNhatThoiTiet();
            };
            // Bo tròn thanh tìm kiếm
            this.Load += (s, e) => {
                ApplyRoundedCorners(oTimKiemDiaDiem, 10);
                ApplyRoundedCorners(khung24Gio, 15);
                ApplyRoundedCorners(khung5Ngay, 15);
            };
            // Khởi tạo file lưu địa điểm nếu chưa có
            try
            {
                if (!File.Exists(locationsFilePath))
                {
                    File.WriteAllText(locationsFilePath, "");
                }
            }
            catch (Exception ex)
            {
            }
            NapDiaDiemDaLuu();
            // Xóa panel gợi ý cũ nếu có
            var oldSuggestionPanel = Controls.Find("suggestionPanel", true).FirstOrDefault();
            if (oldSuggestionPanel != null)
            {
                Controls.Remove(oldSuggestionPanel);
                oldSuggestionPanel.Dispose();
            }
            // Tạo background động
            InitializeBackgroundPictureBox();
            // Set background mặc định ngay khi khởi động dựa trên thời gian hiện tại
            SetDefaultBackgroundOnStartup();
            // Tạo nội dung cho các panel chi tiết
            TaoNoiDungPanelChiTiet();
            // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
            _ = LoadInitialWeatherData();
            // Tạo file icon thật
            TaoFileIconThuc();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CapNhatThoiGian();
            NapDiaDiemDaLuu();
            LoadWeatherByIP();
            TestBackground();
            ForceSetBackgroundInLoad();
        }

        #endregion
        #region Background và UI Setup
        private void ForceSetBackgroundInLoad()
        {
            try
            {
                if (boCucChinh == null)
                {
                    return;
                }
                int currentHour = DateTime.Now.Hour;
                bool isNight = currentHour >= 18 || currentHour < 6;
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                
                if (!Directory.Exists(resourcesPath))
                {
                    return;
                }

                Image backgroundImage;
                
                   if (isNight)
                   {
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                   }
                   else
                   {
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                   }

                // Force set background với nhiều cách
                boCucChinh.BackgroundImage = backgroundImage;
                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                boCucChinh.BackColor = Color.Transparent;
                
                // Force refresh
                boCucChinh.Invalidate();
                boCucChinh.Update();
                boCucChinh.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /// Test background để debug
        private void TestBackground()
        {
            try
            {
                if (boCucChinh == null)
                {
                    return;
                }
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                
                if (Directory.Exists(resourcesPath))
                {
                    var files = Directory.GetFiles(resourcesPath, "*.gif");
                    foreach (var file in files.Take(5))
                    {
                    }
                    
                    var testFile = Path.Combine(resourcesPath, "nen_ban_ngay.jpg");
                    if (File.Exists(testFile))
                    {
                        try
                        {
                            var testImage = Image.FromFile(testFile);
                            boCucChinh.BackgroundImage = testImage;
                            boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Khởi tạo background cho boCucChinh
        private void InitializeBackgroundPictureBox()
        {
        }

        /// Set background mặc định khi khởi động ứng dụng
        private void SetDefaultBackgroundOnStartup()
        {
            try
            {
                
                if (boCucChinh == null)
                {
                    return;
                }
                // Xác định ban đêm hay ban ngày dựa trên thời gian hiện tại
                int currentHour = DateTime.Now.Hour;
                bool isNight = currentHour >= 18 || currentHour < 6;
                   // Đường dẫn đến thư mục Resources trong bin/Debug
                   string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");                
                if (!Directory.Exists(resourcesPath))
                {
                    return;
                }
                // Liệt kê các file trong thư mục Resources
                var files = Directory.GetFiles(resourcesPath);
                Image backgroundImage;
                
                   if (isNight)
                   {
                       // Ban đêm - dùng nền ban đêm mặc định
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                   }
                   else
                   {
                       // Ban ngày - dùng nền ban ngày mặc định
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                   }

                // Set background cho boCucChinh
                boCucChinh.BackgroundImage = backgroundImage;
                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                boCucChinh.BackColor = Color.Transparent; // Đảm bảo BackColor là Transparent
            }
            catch (Exception ex)
            {
                // Fallback - dùng màu nền đơn giản
                if (boCucChinh != null)
                {
                    boCucChinh.BackgroundImage = null;
                    boCucChinh.BackColor = Color.Transparent;
                }
            }
        }
        /// Thiết lập nền theo thời gian và thời tiết
        private void SetBackground(string weatherMain = "Clear", int weatherId = 800)
        {
            try
            {
                
                if (boCucChinh == null)
                {
                    return;
                }

                Image backgroundImage;
                
                // Sử dụng thời gian từ API nếu có, nếu không thì dùng thời gian máy
                int currentHour;
                if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
                {
                    var apiTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt + weatherData.TimezoneOffset).DateTime;
                    currentHour = apiTime.Hour;
                }
                else
                {
                    currentHour = DateTime.Now.Hour;
                }
                
                bool isNight = currentHour >= 18 || currentHour < 6;

                // THROTTLE: nếu không thay đổi trạng thái ngày/đêm và mã thời tiết → bỏ qua
                if (thoiTietIdCu == weatherId && banDemCu == isNight)
                {
                    return;
                }
                thoiTietIdCu = weatherId;
                banDemCu = isNight;
                // Đường dẫn đến thư mục Resources trong bin/Debug
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                // Kiểm tra thư mục Resources có tồn tại không
                if (!Directory.Exists(resourcesPath))
                {
                    return;
                }                
                // (Optional) Có thể liệt kê file khi debug, nhưng tránh log quá nhiều gây giật
                // Chọn background dựa trên mã thời tiết từ OpenWeatherMap API
                if (weatherId >= 200 && weatherId <= 232)
                {
                    // Thunderstorm (dông, sấm chớp) => nen_giong_bao
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_giong_bao.jpg"));
                }
                else if (weatherId >= 300 && weatherId <= 321)
                {
                    // Drizzle (mưa phùn) => nen_mua_rao
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_mua_rao.jpg"));
                }
                else if (weatherId >= 500 && weatherId <= 531)
                {
                    // Rain (mưa) => nen_mua
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_mua.jpg"));
                }
                else if (weatherId >= 600 && weatherId <= 622)
                {
                    // Snow (tuyết) => nen_tuyet
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_tuyet.jpg"));
                }
                else if (weatherId >= 701 && weatherId <= 781)
                {
                    // Atmosphere (sương mù, bụi, khói…) => nen_suong_mu
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_suong_mu.jpg"));
                }
                else if (weatherId == 800)
                {
                    // Clear sky (trời quang/nắng)
                    if (isNight)
                    {
                        // Ban đêm: nền đêm yên tĩnh
                        var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                        backgroundImage = Image.FromFile(demPath);
                    }
                    else
                    {
                        // Ban ngày: trời quang
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                    }
                }
                else if (weatherId >= 801 && weatherId <= 804)
                {
                    // Clouds (mây) => nen_ban_ngay hoặc nen_ban_dem
                    if (isNight)
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                    }
                    else
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_ngay.jpg"));
                    }
                }
                else
                {
                    // Mặc định - dùng nền theo thời gian
                    if (isNight)
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                    }
                    else
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                    }
                }

                // Set background cho boCucChinh thay vì PictureBox riêng biệt
                if (boCucChinh != null)
                {
                    boCucChinh.BackgroundImage = backgroundImage;
                    boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                    boCucChinh.BackColor = Color.Transparent; // Đảm bảo BackColor là Transparent
                }
                // Cập nhật màu chữ theo thời gian
                CapNhatMauChuTheoThoiGian(isNight);
                // System.Diagnostics.Debug.WriteLine($"=== SetBackground hoàn thành thành công ===");
            }
            catch (Exception ex)
            {
                   // Fallback - tạo background gradient đơn giản cho boCucChinh
                   if (boCucChinh != null)
                   {
                       boCucChinh.BackgroundImage = null;
                       boCucChinh.BackColor = Color.Transparent;
                   }
            }
        }

        #endregion
        #region UI Updates và Styling
        /// Cập nhật màu chữ theo thời gian (ban đêm = trắng, ban ngày = đen)
        private void CapNhatMauChuTheoThoiGian(bool isNight)
        {
            try
            {
                Color textColor = isNight ? Color.White : Color.Black;

                // Cập nhật màu chữ cho các label chính
                nhanNhietDoHienTai.ForeColor = textColor;
                nhanTrangThai.ForeColor = textColor;
                nhanTenDiaDiem.ForeColor = textColor;
                // Cập nhật màu chữ cho các panel chi tiết
                CapNhatMauChuPanelChiTiet(textColor);
            }
            catch (Exception ex)
            {
            }
        }
        /// Cập nhật màu chữ cho các panel chi tiết
        private void CapNhatMauChuPanelChiTiet(Color textColor)
        {
            try
            {
                // Cập nhật panel chi tiết
                foreach (Control control in detailGridPanel.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }
                // Cập nhật dự báo 24 giờ
                foreach (Control control in BangTheoGio.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }
                // Cập nhật dự báo 5 ngày
                foreach (Control control in BangNhieuNgay.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region Weather API và Data Loading
        /// Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
        private async Task LoadInitialWeatherData()
        {
            try
            {
                // Khởi động không có dữ liệu gì, chỉ nạp danh sách địa điểm đã lưu
                NapDiaDiemDaLuu();

                // Hiển thị thông báo chào mừng
                nhanTenDiaDiem.Text = "Chào mừng đến với ứng dụng thời tiết";
                nhanThoiGian.Text = "Hãy tìm kiếm địa điểm để xem thông tin thời tiết";
                nhanNhietDoHienTai.Text = "--°C";
                nhanTrangThai.Text = "Chưa có dữ liệu";

                // Xóa các panel dự báo
                BangTheoGio.Controls.Clear();
                BangNhieuNgay.Controls.Clear();
                
                // Load thời tiết theo vị trí hiện tại (IP) để có tọa độ cho bản đồ
                await LoadWeatherByIP();
            }
            catch (Exception ex)
            {
            }
        }
        /// Cấu hình ban đầu cho form, timer, v.v.
        private void CauHinhKhoiTao()
        {
            // Timer 1 giờ
            dongHoCapNhat.Interval = 60 * 60 * 1000;
            dongHoCapNhat.Tick += async (s, e) => { await CapNhatThoiTiet(); };

            // Timer cập nhật thời gian mỗi giây
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => CapNhatThoiGian();
            timer.Start();
            // Cập nhật thời gian lần đầu
            CapNhatThoiGian();
        }
        /// Cập nhật thời gian hiện tại theo địa điểm
        private void CapNhatThoiGian()
        {
            try
            {
                DateTime now;
                // Nếu có dữ liệu thời tiết từ API, sử dụng thời gian từ API với múi giờ địa phương
                if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
                {
                    var utcTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt);
                    now = utcTime.AddSeconds(weatherData.TimezoneOffset).DateTime;
                }
                else
                {
                    // Fallback: sử dụng thời gian máy nếu chưa có dữ liệu API
                    now = DateTime.Now;
                }
                // Hiển thị thứ, ngày tháng năm
                var thu = GetThuVietNam(now.DayOfWeek);
                var ngayThang = now.ToString("dd/MM/yyyy");
                var gioPhut = now.ToString("HH:mm");
                // Cập nhật label địa điểm (nếu có) - chỉ khi chưa có dữ liệu
                if (nhanTenDiaDiem != null && string.IsNullOrEmpty(nhanTenDiaDiem.Text))
                {
                    nhanTenDiaDiem.Text = currentLocation;
                }
                // Cập nhật label thời gian (nếu có)
                if (nhanThoiGian != null)
                {
                    nhanThoiGian.Text = $"{thu}, {ngayThang} - {gioPhut}";
                }
                // Cập nhật background theo thời tiết hiện tại (nếu có dữ liệu)
                if (weatherData?.Current?.Weather?.Length > 0)
                {
                    var weather = weatherData.Current.Weather[0];
                    SetBackground(weather.Main ?? "Clear", weather.Id);
                }
                else
                {
                    // Fallback background cho boCucChinh - dùng nền ban ngày mặc định
                    if (boCucChinh != null)
                    {
                        try
                        {
                            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                            string fallbackFile = Path.Combine(resourcesPath, "nen_ban_ngay.jpg");
                            
                            if (File.Exists(fallbackFile))
                            {
                                boCucChinh.BackgroundImage = Image.FromFile(fallbackFile);
                                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                            }
                            else
                            {
                                // Nếu không có file, dùng màu nền đơn giản
                                boCucChinh.BackgroundImage = null;
                                boCucChinh.BackColor = Color.Transparent;
                            }
                        }
                        catch (Exception ex)
                        {
                            boCucChinh.BackgroundImage = null;
                            boCucChinh.BackColor = Color.Transparent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Cập nhật hiển thị địa điểm
        private void CapNhatDiaDiem(string diaDiem)
        {
            try
            {
                // Cập nhật label địa điểm hiện có
                if (nhanTenDiaDiem != null)
                {
                    nhanTenDiaDiem.Text = diaDiem;
                }
                // Cập nhật biến currentLocation
                currentLocation = diaDiem;
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region Location Management
        /// Lưu địa điểm vào file
        private void LuuDiaDiem(string name, double lat, double lon)
        {
            try
            {
                // Chuẩn hóa tên để so sánh không phân biệt dấu/hoa thường
                string NormalizeName(string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                    var formD = s.Normalize(NormalizationForm.FormD);
                    var sb = new StringBuilder(formD.Length);
                    foreach (var ch in formD)
                    {
                        var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                        if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                        {
                            sb.Append(ch);
                        }
                    }
                    return sb.ToString().Normalize(NormalizationForm.FormC).Trim().ToLowerInvariant();
                }
                bool CoordinatesEqual(double aLat, double aLon, double bLat, double bLon)
                {
                    const double epsilon = 0.2; // ~20km để gom city/province gần nhau
                    return Math.Abs(aLat - bLat) <= epsilon && Math.Abs(aLon - bLon) <= epsilon;
                }
                var normalizedNewName = NormalizeName(name);
                // Kiểm tra trùng theo tên đã chuẩn hóa hoặc theo toạ độ gần nhau
                if (savedLocations.Any(loc =>
                        NormalizeName(loc.Name) == normalizedNewName ||
                        CoordinatesEqual(loc.Lat, loc.Lon, lat, lon)))
                {
                    MessageBox.Show("Địa điểm này đã có trong danh sách!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return; // Đã tồn tại, không lưu trùng
                }
                // Thêm vào danh sách
                var newLocation = new SavedLocation(name, lat, lon);
                savedLocations.Add(newLocation);
                // Lưu vào file txt
                SaveLocationsToFile();
                // Cập nhật ListBox
                NapDiaDiemDaLuu();
            }
            catch (Exception ex)
            {
            }
        }
        /// Nạp danh sách địa điểm đã lưu từ file txt
        private void NapDiaDiemDaLuu()
        {
            try
                {
                    listBoxDiaDiemDaLuu.Items.Clear();
                savedLocations.Clear();
                
                LoadLocationsFromFile();
                
                foreach (var loc in savedLocations)
                {
                        listBoxDiaDiemDaLuu.Items.Add(loc.Name);
                    }
                }
            catch (Exception ex)
            {
            }
        }
        /// Lưu danh sách địa điểm vào file txt
        private void SaveLocationsToFile()
        {
            try
            {
                var lines = new List<string>();
                foreach (var loc in savedLocations)
                {
                    lines.Add($"{loc.Name}|{loc.Lat}|{loc.Lon}");
                }
                File.WriteAllLines(locationsFilePath, lines);
            }
            catch (Exception ex)
            {
            }
        }
        /// Đọc danh sách địa điểm từ file txt
        private void LoadLocationsFromFile()
        {
            try
            {
                if (!File.Exists(locationsFilePath))
                    return;

                var lines = File.ReadAllLines(locationsFilePath);
                savedLocations.Clear();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('|');
                    if (parts.Length == 3 && 
                        double.TryParse(parts[1], out double lat) && 
                        double.TryParse(parts[2], out double lon))
                    {
                        savedLocations.Add(new SavedLocation(parts[0], lat, lon));
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Sự kiện chọn địa điểm đã lưu
        private async void SuKienChonDiaDiemDaLuu()
        {
            try
            {
                var selectedLocationName = listBoxDiaDiemDaLuu.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedLocationName))
                    {
                        // Cập nhật ô tìm kiếm
                        oTimKiemDiaDiem.Text = selectedLocationName;
                        
                        // Tự động load thời tiết cho địa điểm đã lưu
                        await CapNhatThoiTiet();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Chuyển đổi thứ tiếng Anh sang tiếng Việt
        private string GetThuVietNam(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday: return "Chủ nhật";
                case DayOfWeek.Monday: return "Thứ hai";
                case DayOfWeek.Tuesday: return "Thứ ba";
                case DayOfWeek.Wednesday: return "Thứ tư";
                case DayOfWeek.Thursday: return "Thứ năm";
                case DayOfWeek.Friday: return "Thứ sáu";
                case DayOfWeek.Saturday: return "Thứ bảy";
                default: return "Thứ";
            }
        }

        /// Áp dụng style glassmorphism hiện đại cho giao diện
        private void ApDungStyleGlassmorphism()
        {
            try
            {
                // Cấu hình form để hỗ trợ trong suốt
                this.FormBorderStyle = FormBorderStyle.None;
                this.AllowTransparency = true;
                this.BackColor = Color.FromArgb(0, 0, 0, 0); // Nền hoàn toàn trong suốt
                // Thêm viền bo tròn cho form
                this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));
                // Thanh trên cùng - trong suốt mờ mờ
                thanhTrenCung.BackColor = Color.FromArgb(80, 255, 255, 255);
                // Panel chính - trong suốt để nền hiển thị
                boCucChinh.BackColor = Color.Transparent;
                khuVucTrai_HienTai.BackColor = Color.Transparent; // Trong suốt
                khuVucPhai_5Ngay.BackColor = Color.Transparent; // Trong suốt
                khuVucDuoi_24Gio.BackColor = Color.Transparent; // Trong suốt
                // GroupBox - trong suốt mờ mờ
                khung5Ngay.BackColor = Color.FromArgb(40, 255, 255, 255);
                khung24Gio.BackColor = Color.FromArgb(40, 255, 255, 255);
                // TextBox tìm kiếm - trong suốt với viền bo tròn
                SetTransparentBackColor(oTimKiemDiaDiem, Color.FromArgb(150, 255, 255, 255));
                oTimKiemDiaDiem.BorderStyle = BorderStyle.None;
                oTimKiemDiaDiem.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
                // Button tìm kiếm - trong suốt với viền bo tròn
                SetTransparentBackColor(NutTimKiem, Color.FromArgb(150, 255, 255, 255));
                NutTimKiem.FlatStyle = FlatStyle.Flat;
                NutTimKiem.FlatAppearance.BorderSize = 0;
                NutTimKiem.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                // Labels - màu trắng, font đẹp
                nhanTenDiaDiem.ForeColor = Color.White;
                nhanTenDiaDiem.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
                nhanNhietDoHienTai.ForeColor = Color.White;
                nhanNhietDoHienTai.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
                nhanTrangThai.ForeColor = Color.White;
                nhanTrangThai.Font = new Font("Segoe UI", 16F, FontStyle.Regular);
                // TabControl - hoàn toàn trong suốt
                tabDieuKhien.BackColor = Color.Transparent;
                tabChart.BackColor = Color.FromArgb(30, 50, 70, 90); // Nền xanh dương mờ
                // Thêm nút đóng form (vì đã bỏ border)
                TaoNutDongForm();
            }
            catch (Exception ex)
            {
            }
        }
        /// Helper method để set màu trong suốt an toàn
        private void SetTransparentBackColor(Control control, Color color)
        {
            try
            {
                control.BackColor = color;
            }
            catch (ArgumentException)
            {
                // Nếu control không hỗ trợ trong suốt, dùng màu trắng mờ
                control.BackColor = Color.FromArgb(240, 240, 240);
            }
        }
        /// Tạo viền bo tròn cho form
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        /// Áp dụng viền bo tròn cho control
        private void ApplyRoundedCorners(Control control, int radius)
        {
            try
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseAllFigures();
                control.Region = new System.Drawing.Region(path);
            }
            catch
            {
                // Nếu không thể tạo region, bỏ qua
            }
        }
        // (đã xoá phiên bản trùng lặp ShowChart/ShowMap)
        /// Lưu địa điểm tự động (không hiện thông báo)
        private void LuuDiaDiemSilent(string locationName, double lat, double lon)
        {
            try
            {
                // Chuẩn hóa tên để so sánh
                string NormalizeName(string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                    s = s.Replace(" ,", ",").Trim().Trim(',').Trim();
                    var formD = s.Normalize(NormalizationForm.FormD);
                    var sb = new System.Text.StringBuilder(formD.Length);
                    foreach (var ch in formD)
                    {
                        var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                        if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                        {
                            sb.Append(ch);
                        }
                    }
                    return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
                }

                bool CoordinatesEqual(double aLat, double aLon, double bLat, double bLon)
                {
                    const double epsilon = 0.2; // ~20km để gom city/province gần nhau
                    return Math.Abs(aLat - bLat) <= epsilon && Math.Abs(aLon - bLon) <= epsilon;
                }
                var cleanedName = locationName.Replace(" ,", ",").Trim().Trim(',').Trim();
                var normalizedName = NormalizeName(cleanedName);                
                // Nếu đã có địa điểm này thì bỏ qua (không thông báo)
                if (savedLocations.Any(loc =>
                        NormalizeName(loc.Name) == normalizedName ||
                        CoordinatesEqual(loc.Lat, loc.Lon, lat, lon)))
                {
                    return; // Bỏ qua im lặng
                }
                // Thêm vào danh sách và lưu vào file
                var newLocation = new SavedLocation(cleanedName, lat, lon);
                savedLocations.Add(newLocation);
                SaveLocationsToFile();               
                // Cập nhật danh sách
                NapDiaDiemDaLuu();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region Event Handlers
        /// Lưu địa điểm hiện tại
        private async void nutLuuDiaDiem_Click(object sender, EventArgs e)
        {
            var currentLocationText = oTimKiemDiaDiem.Text.Trim();
            if (string.IsNullOrEmpty(currentLocationText))
            {
                MessageBox.Show("Vui lòng nhập địa điểm trước khi lưu!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Chuẩn hóa tên để so sánh không phân biệt hoa/thường, dấu, dấu phẩy thừa
            string NormalizeName(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                s = s.Replace(" ,", ",").Trim().Trim(',').Trim();
                var formD = s.Normalize(NormalizationForm.FormD);
                var sb = new System.Text.StringBuilder(formD.Length);
                foreach (var ch in formD)
                {
                    var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(ch);
                    }
                }
                return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
            }
            var cleanedNameFinal = currentLocationText.Replace(" ,", ",").Trim().Trim(',').Trim();
            // Lấy toạ độ hiện tại nếu đã có từ lần tìm kiếm gần nhất; nếu chưa có, geocode nhanh
            double lat = currentLat;
            double lon = currentLon;
            try
            {
                if (lat == 0 && lon == 0)
                {
                    var geo = await WeatherApiService.GetCoordinatesAsync(cleanedNameFinal);
                    if (geo?.Results?.Length > 0)
                    {
                        lat = geo.Results[0].Lat;
                        lon = geo.Results[0].Lon;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            // Kiểm tra trùng lặp và lưu vào file
            var normalizedNew = NormalizeName(cleanedNameFinal);
            const double epsilon = 0.2; // ~20km
            
            bool CoordinatesEqual(double aLat, double aLon, double bLat, double bLon)
            {
                return Math.Abs(aLat - bLat) <= epsilon && Math.Abs(aLon - bLon) <= epsilon;
            }
            
            try
            {
                // Kiểm tra trùng lặp
                if (savedLocations.Any(loc =>
                        NormalizeName(loc.Name) == normalizedNew ||
                        CoordinatesEqual(loc.Lat, loc.Lon, lat, lon)))
                {
                    MessageBox.Show("Địa điểm này đã được lưu rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Thêm vào danh sách và lưu vào file
                var newLocation = new SavedLocation(cleanedNameFinal, lat, lon);
                savedLocations.Add(newLocation);
                SaveLocationsToFile();
                NapDiaDiemDaLuu();
                MessageBox.Show($"Đã lưu địa điểm: {cleanedNameFinal}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu địa điểm vào file.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// Chuyển đổi địa điểm - hiện dropdown để chọn
        private void nutChuyenDoiDiaDiem_Click(object sender, EventArgs e)
        {
            if (savedLocations.Count == 0) 
            {
                MessageBox.Show("Chưa có địa điểm nào được lưu. Hãy lưu địa điểm trước!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Tạo context menu để chọn địa điểm (lấy từ file)
            var contextMenu = new ContextMenuStrip();
            var locationsForMenu = savedLocations.Select(l => l.Name).ToList();
            foreach (var location in locationsForMenu)
            {
                // Tạo panel con chứa tên địa điểm và 2 nút
                var innerPanel = new Panel
                {
                    Width = 200,
                    Height = 30
                };
                // Label tên địa điểm (click để chọn)
                var locationLabel = new Label
                {
                    Text = location,
                    Location = new Point(5, 5),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };
                locationLabel.Click += async (s, args) => {
                    // Kiểm tra nếu là địa điểm IP
                    if (location == "📍 Vị trí hiện tại")
                    {
                        // Load thời tiết theo IP
                        await LoadWeatherByIP();
                    }
                    else
                    {
                        oTimKiemDiaDiem.Text = location;
                        currentLocation = location;
                        
                        // Cập nhật tên địa điểm hiển thị
                        CapNhatDiaDiem(location);
                        
                        await CapNhatThoiTiet();
                    }
                    contextMenu.Close();
                };
                // Nút xóa (✗) - chỉ hiện cho địa điểm khác (không phải vị trí hiện tại)
                Button deleteBtn = null;
                if (location != "📍 Vị trí hiện tại")
                {
                    deleteBtn = new Button
                    {
                        Text = "✗",
                        Location = new Point(160, 3),
                        Size = new Size(25, 24),
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        BackColor = Color.LightCoral,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    deleteBtn.Click += (s, args) => {
                        var result = MessageBox.Show($"Bạn có chắc muốn xóa địa điểm '{location}'?", "Xác nhận xóa", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                // Xóa khỏi danh sách và file
                                savedLocations.RemoveAll(loc => loc.Name == location);
                                SaveLocationsToFile();
                            }
                            catch (Exception ex)
                            {
                            }
                            NapDiaDiemDaLuu();
                            MessageBox.Show($"Đã xóa địa điểm: {location}", "Thành công", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            contextMenu.Close();
                        }
                    };
                }
                // Thêm các control vào panel
                innerPanel.Controls.Add(locationLabel);
                if (deleteBtn != null)
                {
                    innerPanel.Controls.Add(deleteBtn);
                }
                
                // Tạo ToolStripControlHost với panel
                var locationPanel = new ToolStripControlHost(innerPanel);
                contextMenu.Items.Add(locationPanel);
            }            
            // Hiện menu tại vị trí nút
            contextMenu.Show(nutChuyenDoiDiaDiem, new Point(0, nutChuyenDoiDiaDiem.Height));
        }
        /// Xóa địa điểm đã chọn khỏi danh sách
        private void nutXoaDiaDiem_Click(object sender, EventArgs e)
        {
            if (listBoxDiaDiemDaLuu.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn địa điểm cần xóa!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var selectedLocation = listBoxDiaDiemDaLuu.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedLocation)) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa địa điểm '{selectedLocation}'?", "Xác nhận xóa", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // Xóa khỏi danh sách và file
                savedLocations.RemoveAll(loc => loc.Name == selectedLocation);
                SaveLocationsToFile();
                NapDiaDiemDaLuu();
                MessageBox.Show($"Đã xóa địa điểm: {selectedLocation}", "Thành công", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        /// Chọn địa điểm mặc địnhh
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Lưu danh sách địa điểm vào file khi đóng ứng dụng
            SaveLocationsToFile();
        }
        /// Tạo nút đóng form
        private void TaoNutDongForm()
        {
            var nutDong = new Button
            {
                Text = "✕",
                Size = new Size(30, 30),
                Location = new Point(this.Width - 35, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(200, 255, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            nutDong.FlatAppearance.BorderSize = 0;
            nutDong.Click += (s, e) => this.Close();

            this.Controls.Add(nutDong);
            nutDong.BringToFront();
        }
        /// Sự kiện bấm nút Tìm kiếm: Geocoding để lấy lat/lon, sau đó cập nhật dữ liệu
        private async void NutTimKiem_Click(object? sender, EventArgs e)
        {
            var tuKhoa = oTimKiemDiaDiem.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                MessageBox.Show("Vui lòng nhập xã/phường, quận/huyện, tỉnh/thành để tìm kiếm.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await TimKiemDiaDiem(tuKhoa);
        }
        /// Đổi đơn vị °C ↔ °F và cập nhật lại dữ liệu
        private async void CongTacDonVi_Click(object? sender, EventArgs e)
        {
            // Đảo ngược trạng thái đơn vị
            donViCelsius = !donViCelsius;
            await CapNhatThoiTiet();
        }
        /// Cập nhật các panel chi tiết từ dữ liệu HourlyWeather
        private void CapNhatPanelChiTietFromHourlyApi(HourlyWeather hourly, string kyHieu)
        {
            try
            {
                // Cập nhật cảm giác như
                if (feelsLikePanel != null)
                {
                    var feelsLikeLabel = feelsLikePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (feelsLikeLabel != null)
                    {
                        feelsLikeLabel.Text = $"Cảm giác như\n{Math.Round(hourly.FeelsLike)}{kyHieu}";
                    }
                }
                // Cập nhật độ ẩm
                if (humidityPanel != null)
                {
                    var humidityLabel = humidityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (humidityLabel != null)
                    {
                        humidityLabel.Text = $"Độ ẩm\n{hourly.Humidity}%";
                    }
                }
                // Cập nhật gió
                if (windPanel != null)
                {
                    var windLabel = windPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (windLabel != null)
                    {
                        windLabel.Text = $"Gió\n{Math.Round(hourly.WindSpeed)} m/s";
                    }
                }
                // Cập nhật áp suất
                if (pressurePanel != null)
                {
                    var pressureLabel = pressurePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (pressureLabel != null)
                    {
                        pressureLabel.Text = $"Áp suất\n{hourly.Pressure} hPa";
                    }
                }
                // Cập nhật tầm nhìn
                if (visibilityPanel != null)
                {
                    var visibilityLabel = visibilityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (visibilityLabel != null)
                    {
                        visibilityLabel.Text = $"Tầm nhìn\n{hourly.Visibility / 1000} km";
                    }
                }
                // Cập nhật bình minh (không có trong HourlyWeather, giữ nguyên)
                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
            }
        }
        /// Cập nhật các panel chi tiết từ dữ liệu DailyWeather
        private void CapNhatPanelChiTietFromDailyApi(DailyWeather daily, string kyHieu)
        {
            try
            {
                // Cập nhật cảm giác như
                if (feelsLikePanel != null)
                {
                    var feelsLikeLabel = feelsLikePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (feelsLikeLabel != null)
                    {
                        feelsLikeLabel.Text = $"Cảm giác như\n{Math.Round(daily.FeelsLike.Day)}{kyHieu}";
                    }
                }
                // Cập nhật độ ẩm
                if (humidityPanel != null)
                {
                    var humidityLabel = humidityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (humidityLabel != null)
                    {
                        humidityLabel.Text = $"Độ ẩm\n{daily.Humidity}%";
                    }
                }
                // Cập nhật gió
                if (windPanel != null)
                {
                    var windLabel = windPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (windLabel != null)
                    {
                        windLabel.Text = $"Gió\n{Math.Round(daily.WindSpeed)} m/s";
                    }
                }
                // Cập nhật áp suất
                if (pressurePanel != null)
                {
                    var pressureLabel = pressurePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (pressureLabel != null)
                    {
                        pressureLabel.Text = $"Áp suất\n{daily.Pressure} hPa";
                    }
                }
                // Cập nhật tầm nhìn (không có trong DailyWeather, giữ nguyên)
                if (visibilityPanel != null)
                {
                    var visibilityLabel = visibilityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (visibilityLabel != null)
                    {
                        visibilityLabel.Text = $"Tầm nhìn\n-- km";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Cập nhật panel chi tiết từ dữ liệu API
        private void CapNhatPanelChiTietFromApi(CurrentWeather current, string kyHieu)
        {
            try
            {
                // Sử dụng TaoPanelChiTiet để cập nhật tất cả panel
                // Xử lý FeelsLike - nếu bằng 0 thì lấy từ Temp
                var feelsLikeValue = current.FeelsLike != 0 ? current.FeelsLike : current.Temp;
                var feelsLikeInUnit = donViCelsius ? TemperatureConverter.ToCelsius(feelsLikeValue)
                                                   : TemperatureConverter.ToFahrenheit(feelsLikeValue);
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác như", $"{Math.Round(feelsLikeInUnit)}{kyHieu}");

                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", $"{current.Humidity}%");
                // Xử lý Wind Speed - hiển thị chính xác
                string windText;
                if (current.WindSpeed == 0)
                {
                    // Chỉ hiển thị "Lặng gió" nếu thật sự là 0 (không phải do lỗi API)
                    windText = "Lặng gió";
                }
                else
                {
                    windText = $"{Math.Round(current.WindSpeed, 1)} m/s";
                }
                TaoPanelChiTiet(windPanel, "💨", "Tốc độ gió", windText);

                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất khí quyển", $"{current.Pressure} hPa");
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn xa", $"{current.Visibility / 1000.0:0.0} km");
            }
            catch (Exception ex)
            {
            }
        }
        /// Chuyển emoji thành icon thời tiết lớn (cho hiển thị chính)
        private Image GetWeatherIconFromEmoji(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 200); // Kích thước lớn 200x200px
        }
        /// Chuyển emoji thành icon thời tiết nhỏ (cho biểu đồ/cột)
        private Image GetWeatherIconFromEmojiSmall(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 24); // Kích thước rất nhỏ cho biểu đồ
        }
        /// Chuyển emoji thành icon thời tiết với kích thước tùy chỉnh
        private Image GetWeatherIconFromEmoji(string iconPath, int size)
        {
            // Load icon từ file PNG
            if (File.Exists(iconPath))
            {
                try
                {
                    var originalImage = Image.FromFile(iconPath);
                    // Resize về kích thước tùy chỉnh
                    var resizedImage = new Bitmap(size, size);
                    using (var g = Graphics.FromImage(resizedImage))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, 0, 0, size, size);
                    }
                    originalImage.Dispose();
                    return resizedImage;
                }
                catch (Exception ex)
                {
                }
            }
            // Fallback: tạo icon mặc định
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                using (var font = new Font("Segoe UI", size * 0.5f)) // Font size tỷ lệ với kích thước
                {
                    var brush = new SolidBrush(Color.Orange);
                    var rect = new RectangleF(0, 0, size, size);
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("☀", font, brush, rect, format);
                }
            }
            return bitmap;
        }
        /// Lấy đường dẫn đầy đủ của icon
        private string GetIconPath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
        }
        /// Lấy gợi ý theo thời tiết hiện tại
        private List<string> GetWeatherSuggestions(string weatherDesc)
        {
            var suggestions = new List<string>();
            var desc = weatherDesc?.ToLower() ?? "";
            // Gợi ý theo điều kiện thời tiết cụ thể
            if (desc.Contains("clear sky"))
            {
                suggestions.Add("☀️ Trời quang - Thích hợp cho picnic và dã ngoại");
                suggestions.Add("📸 Chụp ảnh ngoài trời");
            }
            else if (desc.Contains("few clouds"))
            {
                suggestions.Add("⛅ Ít mây - Thời tiết dễ chịu, thích hợp hoạt động ngoài trời");
            }
            else if (desc.Contains("scattered clouds"))
            {
                suggestions.Add("☁️ Mây thưa - Thời tiết mát mẻ, thích hợp đi bộ");
            }
            else if (desc.Contains("broken clouds"))
            {
                suggestions.Add("☁️ Mây rải rác - Thời tiết thay đổi, chuẩn bị sẵn áo mưa");
            }
            else if (desc.Contains("overcast clouds"))
            {
                suggestions.Add("🌫️ Nhiều mây - Có thể có mưa");
            }
            else if (desc.Contains("light rain"))
            {
                suggestions.Add("🌧️ Mưa nhẹ - Mang theo ô nhỏ");
            }
            else if (desc.Contains("moderate rain"))
            {
                suggestions.Add("☔ Mưa vừa - Mang theo ô hoặc áo mưa");
                suggestions.Add("🚗 Đường trơn trượt - Lái xe cẩn thận");
            }
            else if (desc.Contains("heavy rain"))
            {
                suggestions.Add("🌧️ Mưa to - Mang áo mưa và tránh ra ngoài");
                suggestions.Add("⚠️ Nguy hiểm - Tránh lái xe nếu không cần thiết");
            }
            else if (desc.Contains("very heavy rain"))
            {
                suggestions.Add("⛈️ Mưa rất to - Ở trong nhà, tránh ra ngoài");
                suggestions.Add("🚨 Cảnh báo - Có thể có lũ lụt");
            }
            else if (desc.Contains("extreme rain"))
            {
                suggestions.Add("🚨 Mưa cực to - Ở trong nhà an toàn");
                suggestions.Add("⚠️ Khẩn cấp - Tránh mọi hoạt động ngoài trời");
            }
            else if (desc.Contains("freezing rain"))
            {
                suggestions.Add("🧊 Mưa đá - Đường rất trơn, cẩn thận tuyệt đối");
            }
            else if (desc.Contains("shower rain"))
            {
                suggestions.Add("🌦️ Mưa rào - Mang theo ô, mưa có thể dừng nhanh");
            }
            else if (desc.Contains("light intensity shower rain"))
            {
                suggestions.Add("🌦️ Mưa rào nhẹ - Mang ô nhỏ phòng hờ");
            }
            else if (desc.Contains("heavy intensity shower rain"))
            {
                suggestions.Add("⛈️ Mưa rào to - Tránh ra ngoài khi mưa");
            }
            else if (desc.Contains("ragged shower rain"))
            {
                suggestions.Add("🌧️ Mưa rào không đều - Thời tiết thay đổi nhanh");
            }
            else if (desc.Contains("light snow"))
            {
                suggestions.Add("❄️ Tuyết nhẹ - Mặc ấm, đường có thể trơn");
            }
            else if (desc.Contains("snow"))
            {
                suggestions.Add("❄️ Tuyết - Mặc quần áo ấm và giày chống trượt");
            }
            else if (desc.Contains("heavy snow"))
            {
                suggestions.Add("🌨️ Tuyết to - Ở trong nhà, tránh ra ngoài");
                suggestions.Add("🏔️ Thời tiết tuyết - Thích hợp cho các hoạt động mùa đông");
            }
            else if (desc.Contains("sleet"))
            {
                suggestions.Add("🌨️ Mưa tuyết - Đường rất trơn, cẩn thận");
            }
            else if (desc.Contains("light shower sleet"))
            {
                suggestions.Add("🌨️ Mưa tuyết nhẹ - Mang giày chống trượt");
            }
            else if (desc.Contains("shower sleet"))
            {
                suggestions.Add("🌨️ Mưa tuyết - Cẩn thận khi di chuyển");
            }
            else if (desc.Contains("light rain and snow"))
            {
                suggestions.Add("🌨️ Mưa và tuyết nhẹ - Thời tiết lạnh ẩm");
            }
            else if (desc.Contains("rain and snow"))
            {
                suggestions.Add("🌨️ Mưa và tuyết - Mặc ấm và mang ô");
            }
            else if (desc.Contains("light shower snow"))
            {
                suggestions.Add("❄️ Tuyết rơi nhẹ - Thời tiết mát mẻ");
            }
            else if (desc.Contains("shower snow"))
            {
                suggestions.Add("❄️ Tuyết rơi - Thích hợp cho hoạt động mùa đông");
            }
            else if (desc.Contains("heavy shower snow"))
            {
                suggestions.Add("🌨️ Tuyết rơi to - Tránh ra ngoài");
            }
            else if (desc.Contains("mist"))
            {
                suggestions.Add("🌫️ Sương mù - Tầm nhìn hạn chế, lái xe cẩn thận");
            }
            else if (desc.Contains("smoke"))
            {
                suggestions.Add("💨 Khói - Tránh hít phải, đóng cửa sổ");
            }
            else if (desc.Contains("haze"))
            {
                suggestions.Add("🌫️ Sương mù nhẹ - Tầm nhìn giảm");
            }
            else if (desc.Contains("sand/dust whirls"))
            {
                suggestions.Add("🌪️ Cát/bụi xoáy - Tránh ra ngoài, đeo khẩu trang");
            }
            else if (desc.Contains("fog"))
            {
                suggestions.Add("🌫️ Sương mù dày - Tầm nhìn rất hạn chế");
                suggestions.Add("🚗 Lái xe cẩn thận - Bật đèn pha");
            }
            else if (desc.Contains("sand"))
            {
                suggestions.Add("🏜️ Cát - Đeo khẩu trang, tránh hít phải");
            }
            else if (desc.Contains("dust"))
            {
                suggestions.Add("💨 Bụi - Đeo khẩu trang, đóng cửa");
            }
            else if (desc.Contains("volcanic ash"))
            {
                suggestions.Add("🌋 Tro núi lửa - Ở trong nhà, đeo khẩu trang");
            }
            else if (desc.Contains("squalls"))
            {
                suggestions.Add("🌪️ Giông tố - Tránh ra ngoài, tìm nơi trú ẩn");
            }
            else if (desc.Contains("tornado"))
            {
                suggestions.Add("🌪️ Lốc xoáy - Tìm nơi trú ẩn an toàn ngay lập tức");
                suggestions.Add("🚨 Khẩn cấp - Ở trong nhà, tránh cửa sổ");
            }
            else if (desc.Contains("cold"))
            {
                suggestions.Add("🥶 Lạnh - Mặc quần áo ấm");
            }
            else if (desc.Contains("hot"))
            {
                suggestions.Add("🌡️ Nóng - Uống nhiều nước, tránh ánh nắng");
            }
            else if (desc.Contains("windy"))
            {
                suggestions.Add("💨 Có gió - Cẩn thận với các vật bay");
            }
            else if (desc.Contains("hail"))
            {
                suggestions.Add("🧊 Mưa đá - Tránh ra ngoài, có thể gây thương tích");
            }
            else if (desc.Contains("calm"))
            {
                suggestions.Add("🌬️ Lặng gió - Thời tiết yên tĩnh");
            }
            else if (desc.Contains("light breeze"))
            {
                suggestions.Add("🌬️ Gió nhẹ - Thời tiết dễ chịu");
            }
            else if (desc.Contains("gentle breeze"))
            {
                suggestions.Add("🌬️ Gió nhẹ - Thích hợp cho hoạt động ngoài trời");
            }
            else if (desc.Contains("moderate breeze"))
            {
                suggestions.Add("💨 Gió vừa - Thời tiết mát mẻ");
            }
            else if (desc.Contains("fresh breeze"))
            {
                suggestions.Add("💨 Gió mạnh - Cẩn thận với các vật nhẹ");
            }
            else if (desc.Contains("strong breeze"))
            {
                suggestions.Add("💨 Gió rất mạnh - Tránh các hoạt động ngoài trời");
            }
            else if (desc.Contains("high wind"))
            {
                suggestions.Add("🌪️ Gió cực mạnh - Ở trong nhà, cẩn thận");
            }
            else if (desc.Contains("gale"))
            {
                suggestions.Add("🌪️ Bão - Ở trong nhà an toàn");
            }
            else if (desc.Contains("severe gale"))
            {
                suggestions.Add("🌪️ Bão mạnh - Tránh mọi hoạt động ngoài trời");
            }
            else if (desc.Contains("storm"))
            {
                suggestions.Add("⛈️ Bão - Tìm nơi trú ẩn an toàn");
            }
            else if (desc.Contains("violent storm"))
            {
                suggestions.Add("🌪️ Bão dữ dội - Ở trong nhà, tránh cửa sổ");
            }
            else if (desc.Contains("hurricane"))
            {
                suggestions.Add("🌪️ Cuồng phong - Tìm nơi trú ẩn an toàn ngay lập tức");
                suggestions.Add("🚨 Khẩn cấp - Ở trong nhà, chuẩn bị đồ dự trữ");
            }
            return suggestions.Take(3).ToList(); // Chỉ lấy 3 gợi ý đầu tiên
        }
        /// Lấy icon thời tiết từ mã icon API
        private string GetWeatherIcon(string iconCode)
        {
            if (string.IsNullOrEmpty(iconCode)) return GetIconPath("troi_quang_ngay.png");

            return iconCode switch
            {
                // Nắng ban ngày/đêm
                "01d" => GetIconPath("troi_quang_ngay.png"), // sunny day
                "01n" => GetIconPath("troi_quang_dem.png"), // clear night
                // Ít mây
                "02d" => GetIconPath("it_may_ngay.png"), // few clouds day
                "02n" => GetIconPath("it_may_dem.png"), // few clouds night
                // Mây rải rác
                "03d" => GetIconPath("may_rac_rac_ngay.png"), // scattered clouds day
                "03n" => GetIconPath("may_rac_rac_dem.png"), // scattered clouds night
                // Mây dày
                "04d" => GetIconPath("may_day_ngay.png"), // broken clouds day
                "04n" => GetIconPath("may_day_dem.png"), // broken clouds night
                // Mưa rào
                "09d" => GetIconPath("mua_rao_ngay.png"), // shower rain day
                "09n" => GetIconPath("mua_rao_dem.png"), // shower rain night
                // Mưa
                "10d" => GetIconPath("mua_ngay.png"), // rain day
                "10n" => GetIconPath("mua_dem.png"), // rain night
                // Bão
                "11d" => GetIconPath("giong_bao_ngay.png"), // thunderstorm day
                "11n" => GetIconPath("giong_bao_dem.png"), // thunderstorm night
                // Tuyết
                "13d" => GetIconPath("tuyet_ngay.png"), // snow day
                "13n" => GetIconPath("tuyet_dem.png"), // snow night
                // Sương mù
                "50d" => GetIconPath("suong_mu_ngay.png"), // mist day
                "50n" => GetIconPath("suong_mu_dem.png"), // mist night
                _ => GetIconPath("troi_quang_ngay.png")
            };
        }
        /// Hiển thị danh sách 24 giờ vào FlowLayoutPanel BangTheoGio
        private void DoDuLieuBangTheoGio(List<DuBaoTheoGioItem> duLieu, string kyHieu)
        {
            BangTheoGio.SuspendLayout();
            BangTheoGio.Controls.Clear();

            if (duLieu == null || duLieu.Count == 0)
            {
                BangTheoGio.ResumeLayout();
                return;
            }

            foreach (var gio in duLieu)
            {
                var pnl = new Panel
                {
                    Width = 100,
                    Height = 140,
                    Margin = new Padding(8),
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                var nhanGio = new Label
                {
                    Text = gio.ThoiGian.ToString("HH:mm"),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 24
                };

                var pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Top,
                    Height = 72
                };
                pic.Image = ChonIconTheoIconCode(gio.IconCode) ?? ChonIconTheoMa(gio.MaThoiTiet);

                var nhanNhiet = new Label
                {
                    Text = $"{Math.Round(gio.NhietDo)}{kyHieu}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom,
                    Height = 28,
                    Font = new Font(Font, FontStyle.Bold)
                };
                pnl.Controls.Add(nhanNhiet);
                pnl.Controls.Add(pic);
                pnl.Controls.Add(nhanGio);
                BangTheoGio.Controls.Add(pnl);
            }
            BangTheoGio.ResumeLayout();
        }
        /// Hiển thị danh sách 5 ngày vào FlowLayoutPanel BangNhieuNgay
        private void DoDuLieuBangNhieuNgay(List<DuBaoNgayItem> duLieu, string kyHieu)
        {
            BangNhieuNgay.SuspendLayout();
            BangNhieuNgay.Controls.Clear();

            if (duLieu == null || duLieu.Count == 0)
            {
                // Hiển thị thông báo khi không có dữ liệu
                var lblKhongCoDuLieu = new Label
                {
                    Text = "Không có dữ liệu dự báo 5 ngày",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.Gray,
                    Font = new Font(Font, FontStyle.Italic)
                };
                BangNhieuNgay.Controls.Add(lblKhongCoDuLieu);
                BangNhieuNgay.ResumeLayout();
                return;
            }

            foreach (var ngay in duLieu)
            {
                var pnl = new Panel
                {
                    Width = BangNhieuNgay.ClientSize.Width - 24,
                    Height = 100,
                    Margin = new Padding(6),
                    BackColor = Color.FromArgb(200, 255, 255, 255), // Trắng bán trong suốt
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };
                // Tạo viền bo tròn
                pnl.Paint += (s, e) =>
                {
                    var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.FromArgb(100, 135, 206, 235), 2))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, rect, 8);
                    }
                };
                // Header với ngày và thứ
                var nhanNgay = new Label
                {
                    Text = $"{ngay.Ngay.ToString("dd/MM")} - {GetDayOfWeekVietnamese(ngay.Ngay.DayOfWeek)}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 28,
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(70, 130, 180),
                    Padding = new Padding(8, 4, 0, 0)
                };
                // Panel chứa icon và thông tin
                var khungDuoi = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8, 0, 8, 8)
                };
                // Icon thời tiết
                var pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 48,
                    Height = 48,
                    Dock = DockStyle.Left,
                    Margin = new Padding(0, 0, 8, 0)
                };
                pic.Image = ChonIconTheoIconCode(ngay.IconCode) ?? ChonIconTheoMa(ngay.MaThoiTiet);
                // Panel thông tin bên phải
                var khungThongTin = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };
                // Trạng thái thời tiết
                var nhanTrangThaiNho = new Label
                {
                    Text = ngay.TrangThaiMoTa ?? "Không xác định",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = new Font(Font.FontFamily, 9),
                    ForeColor = Color.FromArgb(100, 100, 100)
                };
                // Nhiệt độ cao/thấp
                var nhanNhiet = new Label
                {
                    Text = $"Cao: {Math.Round(ngay.NhietDoCao)}{kyHieu}  |  Thấp: {Math.Round(ngay.NhietDoThap)}{kyHieu}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 24,
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                // Thêm các control vào panel
                khungThongTin.Controls.Add(nhanNhiet);
                khungThongTin.Controls.Add(nhanTrangThaiNho);

                khungDuoi.Controls.Add(khungThongTin);
                khungDuoi.Controls.Add(pic);

                pnl.Controls.Add(khungDuoi);
                pnl.Controls.Add(nhanNgay);

                BangNhieuNgay.Controls.Add(pnl);
            }
            BangNhieuNgay.ResumeLayout();
        }
        // Helper method để lấy tên thứ bằng tiếng Việt
        private string GetDayOfWeekVietnamese(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }
        /// Chọn icon PNG theo mã thời tiết OpenWeather
        private Image? ChonIconTheoMa(int ma)
        {
            var thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            try
            {
                if (ma >= 200 && ma <= 232) return TaiAnh(Path.Combine(thuMuc, "icon_giong.png"));
                if ((ma >= 300 && ma <= 321) || (ma >= 500 && ma <= 531)) return TaiAnh(Path.Combine(thuMuc, "icon_mua.png"));
                if (ma >= 600 && ma <= 622) return TaiAnh(Path.Combine(thuMuc, "icon_tuyet.png"));
                if (ma == 800) return TaiAnh(Path.Combine(thuMuc, "icon_nang.png"));
                return TaiAnh(Path.Combine(thuMuc, "icon_may.png"));
            }
            catch { return null; }
        }
        // Chọn icon theo mã icon OpenWeather (phân biệt ngày/đêm: 01d/01n ... 50d/50n)
        private Image? ChonIconTheoIconCode(string iconCode)
        {
            if (string.IsNullOrWhiteSpace(iconCode))
            {
                // System.Diagnostics.Debug.WriteLine("IconCode rỗng");
                return null;
            }
            var code3 = iconCode.Length >= 3 ? iconCode.Substring(0, 3) : iconCode;
            var code2 = iconCode.Length >= 2 ? iconCode.Substring(0, 2) : iconCode;

            string goc = code2 switch
            {
                "01" => "troi_quang",
                "02" => "it_may",
                "03" => "may_rac_rac",
                "04" => "may_day",
                "09" => "mua_rao",
                "10" => "mua",
                "11" => "giong_bao",
                "13" => "tuyet",
                "50" => "suong_mu",
                _ => "may_day"
            };
            // Xác định hậu tố ngày/đêm
            string hauTo = code3.EndsWith("d", StringComparison.OrdinalIgnoreCase) ? "_ngay" :
                           (code3.EndsWith("n", StringComparison.OrdinalIgnoreCase) ? "_dem" : string.Empty);
            var tenUuTien = goc + hauTo;        // ví dụ: giong_bao_ngay.png
            var tenFallback = goc;               // ví dụ: giong_bao.png
            // 1) Thử lấy từ tài nguyên nhúng (Form1.resx) theo tên ưu tiên rồi fallback
            var tuNhung = TaiAnhTaiNguyen(tenUuTien) ?? TaiAnhTaiNguyen(tenFallback);
            if (tuNhung != null)
            {
                return tuNhung;
            }
            // 2) Lấy từ thư mục Resources cạnh .exe theo tên ưu tiên rồi fallback
            var thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var tuFile = TaiAnh(Path.Combine(thuMuc, tenUuTien + ".png"))
                        ?? TaiAnh(Path.Combine(thuMuc, tenFallback + ".png"));
            if (tuFile != null)
            {
                return tuFile;
            }
            else
            {
                return TaoIconTest(tenUuTien);
            }
        }
        /// Đổi nền động theo mã thời tiết cho toàn bộ giao diện
        private void HienThiIconVaNen(int ma, string iconCode)
        {
            anhIconThoiTiet.Image = ChonIconTheoIconCode(iconCode) ?? ChonIconTheoMa(ma);
            // Chọn nền GIF theo IconCode để khớp với icon
            var tenNen = ChonTenNenTheoIconCode(iconCode);
            if (string.IsNullOrEmpty(tenNen))
            {
                // Fallback theo mã thời tiết cũ nếu không có IconCode
                if (ma >= 200 && ma <= 232) tenNen = "nen_giong.gif";
                else if ((ma >= 300 && ma <= 321) || (ma >= 500 && ma <= 531)) tenNen = "nen_mua.jpg";
                else if (ma >= 600 && ma <= 622) tenNen = "nen_tuyet.jpg";
                else if (ma == 800) tenNen = "nen_troi_quang.jpg";
                else tenNen = "nen_mua.jpg";
            }
            // Thử nhiều đường dẫn khác nhau
            var duongDan = "";
            var thuMuc1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var thuMuc2 = Path.Combine(Environment.CurrentDirectory, "Resources");
            var thuMuc3 = Path.Combine(Application.StartupPath, "Resources");

            if (File.Exists(Path.Combine(thuMuc1, tenNen)))
                duongDan = Path.Combine(thuMuc1, tenNen);
            else if (File.Exists(Path.Combine(thuMuc2, tenNen)))
                duongDan = Path.Combine(thuMuc2, tenNen);
            else if (File.Exists(Path.Combine(thuMuc3, tenNen)))
                duongDan = Path.Combine(thuMuc3, tenNen);
            Image? nenHinh = null;
            if (!string.IsNullOrEmpty(duongDan) && File.Exists(duongDan))
            {
                try
                {
                    nenHinh = Image.FromFile(duongDan);
                }
                catch (Exception ex)
                {
                    nenHinh = TaoBackgroundTest(tenNen);
                }
            }
            else
            {
                nenHinh = TaoBackgroundTest(tenNen);
            }
            // Tạo nền toàn cục cho toàn bộ form
            TaoNenToanCuc(nenHinh);
        }
        /// Tạo nền toàn cục cho toàn bộ giao diện
        private void TaoNenToanCuc(Image? nenHinh)
        {
            if (nenHinh == null)
            {
                return;
            }
            try
            {
                // Xóa nền cũ nếu có
                var nenCu = this.Controls.OfType<PictureBox>().FirstOrDefault(p => p.Name == "NenToanCuc");
                if (nenCu != null)
                {
                    this.Controls.Remove(nenCu);
                    nenCu.Dispose();
                }
                // Tạo PictureBox nền toàn cục - TO NHẤT
                var nenToanCuc = new PictureBox
                {
                    Image = nenHinh,
                    SizeMode = PictureBoxSizeMode.Zoom, // Zoom để nền to nhất
                    Dock = DockStyle.Fill,
                    Location = new Point(0, 0),
                    Size = this.Size,
                    BackColor = Color.Transparent
                };
                // Thêm nền mới vào form
                nenToanCuc.Name = "NenToanCuc";
                this.Controls.Add(nenToanCuc);
                nenToanCuc.SendToBack(); // Đưa xuống dưới cùng
                // Đảm bảo tất cả controls hiển thị trên nền
                thanhTrenCung.BringToFront();
                boCucChinh.BringToFront();
                // Đảm bảo các panel chính hiển thị rõ ràng
                khuVucTrai_HienTai.BringToFront();
                khuVucPhai_5Ngay.BringToFront();
                khuVucDuoi_24Gio.BringToFront();
                // Refresh để đảm bảo hiển thị
                nenToanCuc.Refresh();
                this.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        // Chọn tên nền GIF theo IconCode để khớp với icon (1:1 mapping)
        private static string ChonTenNenTheoIconCode(string iconCode)
        {
            if (string.IsNullOrWhiteSpace(iconCode)) return "";
            var code2 = iconCode.Length >= 2 ? iconCode.Substring(0, 2) : iconCode;
            return code2 switch
            {
                "01" => "nen_troi_quang.jpg",        // trời quang
                "02" => "nen_it_may.gif",            // ít mây
                "03" => "nen_may_rac_rac.gif",       // mây rải rác
                "04" => "nen_may_day.gif",           // mây dày
                "09" => "nen_mua_rao.jpg",           // mưa rào
                "10" => "nen_mua.jpg",               // mưa
                "11" => "nen_giong_bao.jpg",         // giông bão
                "13" => "nen_tuyet.jpg",             // tuyết
                "50" => "nen_suong_mu.jpg",          // sương mù
                _ => "nen_may_day.gif"               // fallback
            };
        }
                private static Image? TaiAnh(string duongDan)
        {
            if (!File.Exists(duongDan))
            {
                return null;
            }

            try
            {
                using var fs = new FileStream(duongDan, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return Image.FromStream(fs);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // Thử tải ảnh nhúng từ tài nguyên Form1.resx theo tên (không phần mở rộng)
        private static Image? TaiAnhTaiNguyen(string ten)
        {
            try
            {
                // Lấy từ Form1.resx thông qua ComponentResourceManager
                var rm = new ComponentResourceManager(typeof(Form1));
                var obj = rm.GetObject(ten);
                return obj as Image;
            }
            catch { return null; }
        }
        /// Tạo icon đơn giản để test khi không có file
        private static Image TaoIconTest(string tenIcon, int size = 64)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            // Nền trong suốt
            g.Clear(Color.Transparent);
            // Vẽ icon đơn giản dựa trên tên
            var brush = new SolidBrush(Color.White);
            var pen = new Pen(Color.White, 2);

            if (tenIcon.Contains("troi_quang"))
            {
                // Mặt trời
                g.FillEllipse(brush, size / 4, size / 4, size / 2, size / 2);
                for (int i = 0; i < 8; i++)
                {
                    var angle = i * Math.PI / 4;
                    var x1 = size / 2 + (int)(size / 3 * Math.Cos(angle));
                    var y1 = size / 2 + (int)(size / 3 * Math.Sin(angle));
                    var x2 = size / 2 + (int)(size / 2.5 * Math.Cos(angle));
                    var y2 = size / 2 + (int)(size / 2.5 * Math.Sin(angle));
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            else if (tenIcon.Contains("mua"))
            {
                // Mưa
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(pen, size / 4 + i * size / 4, size / 3, size / 4 + i * size / 4, size * 2 / 3);
                }
            }
            else if (tenIcon.Contains("may"))
            {
                // Mây
                g.FillEllipse(brush, size / 6, size / 3, size / 3, size / 4);
                g.FillEllipse(brush, size / 3, size / 3, size / 3, size / 4);
                g.FillEllipse(brush, size / 2, size / 3, size / 3, size / 4);
            }
            else
            {
                // Icon mặc định - hình tròn
                g.FillEllipse(brush, size / 4, size / 4, size / 2, size / 2);
            }

            return bmp;
        }

        /// Tạo file icon PNG thật và lưu vào thư mục Resources
        private static void TaoFileIconThuc()
        {
            try
            {
                // Tạo icon trời quang
                var iconTroiQuang = TaoIconTest("troi_quang_ngay", 128);
                var duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "troi_quang_ngay.png");
                iconTroiQuang.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                // Tạo icon mưa
                var iconMua = TaoIconTest("mua", 128);
                duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mua.png");
                iconMua.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                // Tạo icon mây
                var iconMay = TaoIconTest("may_day", 128);
                duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "may_day.png");
                iconMay.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
            }
        }
        /// Event handler cho ListBox địa điểm đã lưu
        private void listBoxDiaDiemDaLuu_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuKienChonDiaDiemDaLuu();
        }
        /// Tạo background test khi không có file GIF - TO NHẤT VÀ THAY ĐỔI THEO THỜI TIẾT
        private static Image TaoBackgroundTest(string tenNen)
        {
            var bmp = new Bitmap(1920, 1080); // Kích thước TO NHẤT để phù hợp với mọi màn hình
            using var g = Graphics.FromImage(bmp);
            // Gradient background dựa trên loại thời tiết - THAY ĐỔI THEO THỜI TIẾT
            if (tenNen.Contains("troi_quang"))
            {
                // Nền gradient đẹp như glassmorphism (xanh lam và tím nhạt) - TRỜI QUANG
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 135, 206, 235), Color.FromArgb(255, 186, 85, 211));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("mua"))
            {
                // Nền gradient xám (mưa) - MƯA
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 105, 105, 105), Color.FromArgb(255, 47, 79, 79));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("may"))
            {
                // Nền gradient xám nhạt (mây) - MÂY
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 169, 169, 169), Color.FromArgb(255, 128, 128, 128));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("tuyet"))
            {
                // Nền gradient trắng (tuyết) - TUYẾT
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 240, 248, 255), Color.FromArgb(255, 176, 196, 222));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else
            {
                // Nền mặc định - gradient xanh dương đẹp
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 135, 206, 235), Color.FromArgb(255, 186, 85, 211));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }

            return bmp;
        }
        private static DateTime UnixToLocal(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().DateTime;
        }



        private void thanhTrenCung_Paint(object sender, PaintEventArgs e)
        {
        }
        private void anhNenDong_Click(object sender, EventArgs e)
        {
        }
        /// Xử lý khi người dùng nhấn phím trong ô tìm kiếm
        private void oTimKiemDiaDiem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.SelectedIndex >= 0 && listBoxGoiY.SelectedItem != null)
                    {
                        oTimKiemDiaDiem.Text = listBoxGoiY.SelectedItem.ToString();
                        listBoxGoiY.Visible = false;
                        _ = TimKiemDiaDiem(oTimKiemDiaDiem.Text.Trim());
                    }
                    else
                    {
                        _ = TimKiemDiaDiem(oTimKiemDiaDiem.Text.Trim());
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    listBoxGoiY.Visible = false;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.Items.Count > 0)
                    {
                        listBoxGoiY.Focus();
                        if (listBoxGoiY.SelectedIndex < listBoxGoiY.Items.Count - 1)
                            listBoxGoiY.SelectedIndex++;
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.Items.Count > 0)
                    {
                        listBoxGoiY.Focus();
                        if (listBoxGoiY.SelectedIndex > 0)
                            listBoxGoiY.SelectedIndex--;
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Xử lý khi người dùng chọn một gợi ý
        private void listBoxGoiY_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBoxGoiY.SelectedIndex >= 0 && listBoxGoiY.SelectedItem != null)
                {
                    oTimKiemDiaDiem.Text = listBoxGoiY.SelectedItem.ToString();
                    listBoxGoiY.Visible = false;
                    oTimKiemDiaDiem.Focus();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Xử lý sự kiện nhấn phím Enter trong ô tìm kiếm
        private async void oTimKiemDiaDiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                string diaDiem = oTimKiemDiaDiem.Text.Trim();
                if (!string.IsNullOrEmpty(diaDiem))
                {
                    await TimKiemDiaDiem(diaDiem);
                }
            }
        }
        /// Tìm kiếm địa điểm và lấy dữ liệu thời tiết
        private async Task TimKiemDiaDiem(string diaDiem)
        {
            try
            {
                // Lấy tọa độ từ tên địa điểm
                // Ưu tiên tìm ở Việt Nam, không phân biệt hoa/thường, có dấu hay không
                var geocodingData = await WeatherApiService.GetCoordinatesAsync(diaDiem);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";
                    // Lấy dữ liệu thời tiết
                    try
                    {
                        weatherData = await WeatherApiService.GetWeatherDataAsync(currentLat, currentLon);
                        if (weatherData != null)
                        {
                            // Hiển thị thông tin đầy đủ
                            HienThiThongTin(currentLocation, weatherData);
                            // Không tự động lưu địa điểm khi tìm kiếm
                            // Chỉ lưu khi người dùng bấm nút Lưu địa điểm
                        }
                        else
                        {
                        }
                    }
                    catch (Exception apiEx)
                    {
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Tìm kiếm gợi ý địa điểm dựa trên text nhập vào
        private List<string> TimKiemGoiYDiaDiem(string searchText)
        {
            // Để trống - chỉ tìm kiếm qua API
            return new List<string>();
        }
        /// Tạo nội dung cho các panel chi tiết thời tiết
        private void TaoNoiDungPanelChiTiet()
        {
            try
            {
                // Panel cảm giác thực tế
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác", "--");
                // Panel độ ẩm
                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", "--");
                // Panel gió
                TaoPanelChiTiet(windPanel, "💨", "Gió", "--");
                // Panel áp suất
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất", "--");
                // Panel tầm nhìn
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn", "--");
            }
            catch (Exception ex)
            {
            }
        }
        /// Tạo nội dung cho một panel chi tiết
        private void TaoPanelChiTiet(Panel panel, string icon, string title, string value)
        {
            try
            {
                panel.Controls.Clear();
                // Bo tròn viền cho panel
                ApplyRoundedCorners(panel, 15);
                // Label icon
                var iconLabel = new Label
                {
                    Text = icon,
                    Font = new Font("Segoe UI Emoji", 24F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(10, 10)
                };
                // Label value - căn giữa
                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 13F, FontStyle.Regular),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    Location = new Point(5, 50),
                    Size = new Size(panel.Width - 10, 25)
                };

                panel.Controls.Add(iconLabel);
                // Chỉ hiển thị giá trị thông tin, bỏ tiêu đề
                panel.Controls.Add(valueLabel);
                // Hàm nội bộ để căn giữa các thành phần theo chiều ngang và giữ khoảng cách dọc hợp lý
                // Căn hàng số liệu ngang với icon và giữ giữa theo chiều ngang
                void Reposition()
                {
                    // giữ icon cố định
                    var valueHeight = valueLabel.Height;
                    var targetY = iconLabel.Top + (iconLabel.Height - valueHeight) / 2;
                    if (targetY < 8) targetY = 8;
                    // Chừa khoảng cho icon bên trái để không che chữ
                    int minIconWidth = 32; // tối thiểu để chừa đủ chỗ cho emoji/icon
                    int iconRight = iconLabel.Left + Math.Max(iconLabel.Width, minIconWidth);
                    int leftPadding = iconRight + 16; // chừa rộng hơn để chắc chắn không chạm icon
                    if (leftPadding > panel.Width - 20) leftPadding = 10; // fallback nếu panel quá nhỏ
                    int rightPadding = 10;
                    int contentWidth = Math.Max(30, panel.Width - leftPadding - rightPadding);

                    valueLabel.Location = new Point(leftPadding, targetY);
                    valueLabel.Size = new Size(contentWidth, valueLabel.Height);
                }
                // Đảm bảo chữ nằm trên icon (tránh bị icon che)
                valueLabel.BringToFront();
                iconLabel.SendToBack();

                Reposition();
                panel.Resize += (s, e) => Reposition();
            }
            catch (Exception ex)
            {
            }
        }
        /// Cập nhật các panel chi tiết với dữ liệu thời tiết thực
        private void CapNhatPanelChiTiet(ThoiTietHienTai hienTai, string kyHieuNhietDo)
        {
            try
            {
                // Panel cảm giác thực tế
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác như", $"{Math.Round(hienTai.NhietDoCamGiac)}{kyHieuNhietDo}");
                // Panel độ ẩm
                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", $"{hienTai.DoAm}%");
                // Panel gió
                var donViGio = donViCelsius ? "m/s" : "mph";
                TaoPanelChiTiet(windPanel, "💨", "Tốc độ gió", $"{Math.Round(hienTai.TocDoGio)} {donViGio}");
                // Panel áp suất
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất khí quyển", $"{hienTai.ApSuat} hPa");
                // Panel tầm nhìn
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn xa", $"{hienTai.TamNhin / 1000.0:0.0} km");
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region Forecast và UI Components
        /// Tạo panel cho dự báo một ngày
        private Panel TaoPanelDuBaoNgay(string ngay, string nhietDo, string trangThai, string icon)
        {
            var panel = new Panel
            {
                BackColor = Color.FromArgb(120, 255, 255, 255),
                Size = new Size(400, 60),
                Margin = new Padding(5),
                Padding = new Padding(10)
            };
            // Label ngày
            var ngayLabel = new Label
            {
                Text = ngay,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(10, 5),
                Size = new Size(100, 25)
            };
            // Label icon
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(120, 5),
                Size = new Size(30, 30)
            };
            // Label nhiệt độ
            var nhietDoLabel = new Label
            {
                Text = nhietDo,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(160, 5),
                Size = new Size(60, 25)
            };
            // Label trạng thái
            var trangThaiLabel = new Label
            {
                Text = trangThai,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(10, 35),
                Size = new Size(200, 20)
            };
            panel.Controls.Add(ngayLabel);
            panel.Controls.Add(iconLabel);
            panel.Controls.Add(nhietDoLabel);
            panel.Controls.Add(trangThaiLabel);

            return panel;
        }
        /// Tạo card dự báo giờ với layout chuẩn
        private Panel TaoCardGio(HourlyWeather hour, string kyHieu)
        {
            try
            {
                var panel = new Panel
                {
                    Size = new Size(140, 200),
                    BackColor = Color.FromArgb(80, 128, 128, 128),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(6)
                };
                // Bo viền tròn
                ApplyRoundedCorners(panel, 15);
                // Tạo TableLayoutPanel với 4 hàng, 1 cột
                var tlpHourlyCard = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(5)
                };
                // Cấu hình RowStyles
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F)); // Hàng 0: Giờ
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F)); // Hàng 1: Nhiệt độ
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Hàng 2: Mô tả (AutoSize)
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Hàng 3: Icon
                // Hàng 0: Giờ (trên cùng)
                var time = UnixToLocal(hour.Dt);
                var lblHour = new Label
                {
                    Text = time.ToString("HH:mm"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                // Hàng 1: Nhiệt độ (to nhất)
                var tempInUnit = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                              : TemperatureConverter.ToFahrenheit(hour.Temp);
                var lblTemp = new Label
                {
                    Text = $"{Math.Round(tempInUnit)}{kyHieu}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 20F, FontStyle.Bold), // Font lớn nhất
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                // Hàng 2: Mô tả (tự xuống dòng)
                var lblDesc = new Label
                {
                    Text = GetVietnameseWeatherDescription(hour.Weather?[0]?.Description ?? "N/A"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    MaximumSize = new Size(130, 0), // Trừ padding
                    UseCompatibleTextRendering = true,
                    AutoEllipsis = false
                };
                // Hàng 3: Icon (dưới cùng)
                var picIcon = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
                // Load icon vào PictureBox
                try
                {
                    var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Weather?[0]?.Icon));
                    if (iconImage != null)
                    {
                        picIcon.Image = iconImage;
                    }
                }
                catch (Exception ex)
                {

                }

                // Thêm các control vào TableLayoutPanel
                tlpHourlyCard.Controls.Add(lblHour, 0, 0);
                tlpHourlyCard.Controls.Add(lblTemp, 0, 1);
                tlpHourlyCard.Controls.Add(lblDesc, 0, 2);
                tlpHourlyCard.Controls.Add(picIcon, 0, 3);
                // Thêm TableLayoutPanel vào panel chính
                panel.Controls.Add(tlpHourlyCard);
                // Thêm click handler
                panel.Click += (s, e) =>
                {
                    // Cập nhật thông tin chính với dữ liệu từ giờ được chọn
                    var tempDisp = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                                : TemperatureConverter.ToFahrenheit(hour.Temp);
                    nhanNhietDoHienTai.Text = $"{Math.Round(tempDisp)}{kyHieu}";
                    var weatherDesc = GetVietnameseWeatherDescription(hour.Weather?[0]?.Description ?? "N/A");
                var suggestions = GetWeatherSuggestions(hour.Weather?[0]?.Description ?? "");
                nhanTrangThai.Text = $"{weatherDesc}\n💡 {string.Join(" • ", suggestions.Take(2))}";

                    // Cập nhật icon thời tiết
                    if (anhIconThoiTiet != null)
                    {
                        anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Weather?[0]?.Icon));
                    }
                    // Cập nhật các panel chi tiết với dữ liệu từ giờ được chọn
                    CapNhatPanelChiTietFromHourlyApi(hour, kyHieu);

                    // Cập nhật background theo thời tiết
                    SetBackground(hour.Weather?[0]?.Main ?? "Clear", hour.Weather?[0]?.Id ?? 800);
                };

                return panel;
            }
            catch (Exception ex)
            {
                return new Panel();
            }
        }
        /// Load dự báo 24 giờ
        private void LoadDuBao24h(HourlyWeather[] hourlyList, string kyHieu)
        {
            try
            {
                if (BangTheoGio != null)
                {
                    BangTheoGio.Controls.Clear();
                    // Lấy 24 giờ đầu tiên
                    var data24h = hourlyList.Take(24).ToArray();
                    foreach (var hour in data24h)
                    {
                        var card = TaoCardGio(hour, kyHieu);
                        BangTheoGio.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Tạo card dự báo ngày với layout chuẩn
        private Panel TaoCardNgay(DailyWeather daily, string kyHieu)
        {
            try
            {
                var panel = new Panel
                {
                    Size = new Size(430, 70), // Tăng chiều cao để chứa label nhiệt độ lớn hơn
                    BackColor = Color.FromArgb(80, 128, 128, 128),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(3),
                    Padding = new Padding(8)
                };
                // Bo viền tròn
                ApplyRoundedCorners(panel, 15);
                // Layout mới: Icon bên trái, thông tin bên phải
                // 1. Icon thời tiết (bên trái)
                var picIcon = new PictureBox
                {
                    Location = new Point(8, 8),
                    Size = new Size(44, 44),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };
                // Load icon vào PictureBox
                try
                {
                    string iconCode = daily.Weather?[0]?.Icon ?? "01d";
                    string iconPath = Path.Combine(Application.StartupPath, "Resources", $"{iconCode}.png");
                    if (File.Exists(iconPath))
                    {
                        picIcon.Image = Image.FromFile(iconPath);
                    }
                    else
                    {
                        // Fallback: sử dụng emoji icon
                        var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                        if (iconImage != null)
                        {
                            picIcon.Image = iconImage;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                // 2. Ngày trong tuần + ngày/tháng (tiếng Việt)
                var date = UnixToLocal(daily.Dt);
                var lblDay = new Label
                {
                    Text = GetVietnameseDayName(daily.Dt),
                    Location = new Point(60, 8),
                    Size = new Size(150, 20), // Tăng chiều rộng để đủ chỗ cho "Hôm nay"
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true // Để text tự động điều chỉnh kích thước
                };
                // 3. Mô tả thời tiết (tiếng Việt)
                var lblDesc = new Label
                {
                    Text = GetVietnameseWeatherDescription(daily.Weather?[0]?.Description ?? "N/A"),
                    Location = new Point(60, 30),
                    Size = new Size(150, 20),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.LightGray,
                    BackColor = Color.Transparent,
                    AutoSize = true // Để text tự động điều chỉnh kích thước
                };
                // 4. Nhiệt độ cao/thấp (nổi bật)
                var tempMaxInUnit = donViCelsius ? TemperatureConverter.ToCelsius(daily.Temp.Max)
                                                 : TemperatureConverter.ToFahrenheit(daily.Temp.Max);
                var tempMinInUnit = donViCelsius ? TemperatureConverter.ToCelsius(daily.Temp.Min)
                                                 : TemperatureConverter.ToFahrenheit(daily.Temp.Min);
                var lblTemp = new Label
                {
                    Text = $"Cao nhất: {Math.Round(tempMaxInUnit)}°{kyHieu}\nThấp nhất: {Math.Round(tempMinInUnit)}°{kyHieu}",
                    Location = new Point(220, 8),
                    Size = new Size(120, 60), // Tăng chiều cao để hiển thị đầy đủ cả hai dòng
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold), // Giảm font size một chút
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                // 5. Thông tin mưa và gió
                var lblRainWind = new Label
                {
                    Text = GetRainWindInfo(daily),
                    Location = new Point(350, 8),
                    Size = new Size(70, 50),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.LightBlue,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                // Thêm các control vào panel
                panel.Controls.Add(picIcon);
                panel.Controls.Add(lblDay);
                panel.Controls.Add(lblDesc);
                panel.Controls.Add(lblTemp);
                panel.Controls.Add(lblRainWind);
                // Giữ màu chữ trắng như trước
                foreach (Control c in panel.Controls)
                {
                    if (c is Label lbl)
                    {
                        lbl.ForeColor = Color.White;
                    }
                }
                // Thêm click event cho tất cả control con để đảm bảo click được truyền lên panel cha
                // Sử dụng Tag để lưu reference đến panel cha
                picIcon.Tag = panel;
                lblDay.Tag = panel;
                lblDesc.Tag = panel;
                lblTemp.Tag = panel;
                lblRainWind.Tag = panel;
                return panel;
            }
            catch (Exception ex)
            {
                return new Panel();
            }
        }
        /// Load dự báo 5 ngày với click event
        private void LoadForecast5Days(DailyWeather[] dailyList, string kyHieu)
        {
            try
            {
                if (BangNhieuNgay != null)
                {
                    BangNhieuNgay.Controls.Clear();
                    // Lấy 5 ngày đầu tiên
                    var data5Ngay = dailyList.Take(5).ToArray();
                    for (int i = 0; i < data5Ngay.Length; i++)
                    {
                        var daily = data5Ngay[i];
                        var card = TaoCardNgay(daily, kyHieu);                   
                        // Thêm click event để chuyển sang biểu đồ 24h
                        int dayIndex = i; // Capture index để tránh closure issue
                        card.Click += (sender, e) => OnDayCardClicked(dayIndex, daily);                       
                        // Thêm click event cho tất cả control con
                        foreach (Control control in card.Controls)
                        {
                            control.Click += (s, e) => OnDayCardClicked(dayIndex, daily);
                            control.Cursor = Cursors.Hand;
                        }
                        // Thêm cursor pointer để hiển thị có thể click
                        card.Cursor = Cursors.Hand;                        
                        BangNhieuNgay.Controls.Add(card);
                    }
                    // Hiển thị mặc định biểu đồ 24h cho ngày đầu tiên và chọn tab Biểu đồ
                    if (data5Ngay.Length > 0)
                    {
                        Show24hChartForDay(data5Ngay[0]);
                        try { tabDieuKhien.SelectedTab = tabChart; } catch {}
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// Xử lý khi click vào card ngày
        private void OnDayCardClicked(int dayIndex, DailyWeather daily)
        {
            try
            {                
                chiSoNgayDaChon = dayIndex;
                // Cập nhật biểu đồ 24h cho ngày được chọn
                Show24hChartForDay(daily);
                // Highlight card được chọn (optional)
                HighlightSelectedDayCard(dayIndex);
            }
            catch (Exception ex)
            {
            }
        }
        /// Highlight card ngày được chọn
        private void HighlightSelectedDayCard(int dayIndex)
        {
            try
            {
                if (BangNhieuNgay?.Controls.Count > dayIndex)
                {
                    // Reset tất cả cards về màu bình thường
                    foreach (Control control in BangNhieuNgay.Controls)
                    {
                        if (control is Panel panel)
                        {
                            panel.BackColor = Color.FromArgb(80, 128, 128, 128);
                        }
                    }
                    // Highlight card được chọn
                    var selectedCard = BangNhieuNgay.Controls[dayIndex] as Panel;
                    if (selectedCard != null)
                    {
                        selectedCard.BackColor = Color.FromArgb(120, 255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// Hiển thị biểu đồ nhiệt độ 24h cho ngày được chọn
        private void Show24hChartForDay(DailyWeather daily)
        {
            try
            {
                if (weatherData?.Hourly == null || weatherData.Hourly.Length == 0)
                {
                    return;
                }
                // Khởi tạo Chart nếu chưa có
                if (bieuDoNhietDo == null)
                {
                    InitializeTemperatureChart();
                }
                // Lấy dữ liệu 24h cho ngày được chọn
                var dayStart = UnixToLocal(daily.Dt).Date;
                var dayEnd = dayStart.AddDays(1);
                // Thử filter theo ngày trước
                var hourlyData = weatherData.Hourly
                    .Where(h => 
                    {
                        var hourTime = UnixToLocal(h.Dt);
                        var isInRange = hourTime >= dayStart && hourTime < dayEnd;
                        return isInRange;
                    })
                    .Take(24)
                    .ToArray();
                // Nếu không đủ dữ liệu, sử dụng fallback
                if (hourlyData.Length < 12) // Ít hơn 12 giờ thì không đủ
                {
                    // Fallback: Lấy 24 giờ đầu tiên từ dữ liệu hourly
                    hourlyData = weatherData.Hourly.Take(24).ToArray();
                }
                // Xóa dữ liệu cũ
                bieuDoNhietDo.Series.Clear();
                // Tạo series cột
                var series = new Series("Nhiệt độ")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(200, 100, 200, 255),
                    BorderWidth = 1,
                    IsValueShownAsLabel = false
                };
                series["PointWidth"] = "0.6"; // Độ rộng cột
                // Thêm dữ liệu điểm
                foreach (var hour in hourlyData)
                {
                    var hourTime = UnixToLocal(hour.Dt);
                    var temperature = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                                  : TemperatureConverter.ToFahrenheit(hour.Temp);
                    
                    var pointIndex = series.Points.AddXY(hourTime.Hour, temperature);
                    var point = series.Points[pointIndex];
                    point.ToolTip = $"Giờ: {hourTime:HH:mm}\nNhiệt độ: {temperature:F1}°{(donViCelsius ? "C" : "F")}\nTrạng thái: {hour.Weather?[0]?.Description ?? "N/A"}";
                }

                bieuDoNhietDo.Series.Add(series);
                // Cấu hình trục X
                bieuDoNhietDo.ChartAreas[0].AxisX.Title = "Giờ";
                bieuDoNhietDo.ChartAreas[0].AxisX.TitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
                bieuDoNhietDo.ChartAreas[0].AxisX.TitleForeColor = Color.White;
                bieuDoNhietDo.ChartAreas[0].AxisX.Minimum = 0;
                bieuDoNhietDo.ChartAreas[0].AxisX.Maximum = 23;
                bieuDoNhietDo.ChartAreas[0].AxisX.Interval = 1; // hiện mỗi giờ: 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23
                bieuDoNhietDo.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Segoe UI", 7, FontStyle.Regular);
                bieuDoNhietDo.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                bieuDoNhietDo.ChartAreas[0].AxisX.LineColor = Color.FromArgb(200, 255, 255, 255);
                bieuDoNhietDo.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(100, 255, 255, 255);
                bieuDoNhietDo.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                // Cấu hình trục Y
                bieuDoNhietDo.ChartAreas[0].AxisY.Title = $"Nhiệt độ °{(donViCelsius ? "C" : "F")}";
                bieuDoNhietDo.ChartAreas[0].AxisY.TitleForeColor = Color.White;
                bieuDoNhietDo.ChartAreas[0].AxisY.TitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
                // Điều chỉnh trục Y theo dải °C/°F hợp lý
                if (donViCelsius)
                {
                    bieuDoNhietDo.ChartAreas[0].AxisY.Minimum = -10;
                    bieuDoNhietDo.ChartAreas[0].AxisY.Maximum = 50;
                    bieuDoNhietDo.ChartAreas[0].AxisY.Interval = 5;
                }
                else
                {
                    bieuDoNhietDo.ChartAreas[0].AxisY.Minimum = 10;  // ≈ 14°F ~ -10°C
                    bieuDoNhietDo.ChartAreas[0].AxisY.Maximum = 120; // ≈ 122°F ~ 50°C
                    bieuDoNhietDo.ChartAreas[0].AxisY.Interval = 10;
                }
                bieuDoNhietDo.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                bieuDoNhietDo.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                bieuDoNhietDo.ChartAreas[0].AxisY.LineColor = Color.FromArgb(200, 255, 255, 255);
                bieuDoNhietDo.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(100, 255, 255, 255);
                bieuDoNhietDo.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                bieuDoNhietDo.Titles[0].Font = new Font("Segoe UI", 16F, FontStyle.Regular);
                // Tự điều chỉnh dải trục Y theo dữ liệu, cộng trừ 3° đệm
                var allVals = series.Points.Select(p => p.YValues[0]).ToArray();
                if (allVals.Length > 0)
                {
                    double min = allVals.Min();
                    double max = allVals.Max();
                    bieuDoNhietDo.ChartAreas[0].AxisY.Minimum = Math.Floor(min - 3);
                    bieuDoNhietDo.ChartAreas[0].AxisY.Maximum = Math.Ceiling(max + 3);
                }
                // Cấu hình tiêu đề
                bieuDoNhietDo.Titles.Clear();
                bieuDoNhietDo.Titles.Add($"Biểu đồ nhiệt độ 24h - {GetVietnameseDayName(daily.Dt)}");
                bieuDoNhietDo.Titles[0].Font = new Font("Segoe UI", 16, FontStyle.Regular);
                bieuDoNhietDo.Titles[0].ForeColor = Color.White;
                // Cấu hình màu nền
                bieuDoNhietDo.BackColor = Color.FromArgb(40, 20, 40, 60);
                bieuDoNhietDo.ChartAreas[0].BackColor = Color.FromArgb(20, 30, 50, 70);
                bieuDoNhietDo.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                bieuDoNhietDo.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            }
            catch (Exception ex)
            {
            }
        }
        /// Khởi tạo Chart nhiệt độ
        private void CreateDailyComparisonChart(GroupBox parent)
        {
            try
            {
                var chart = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40),
                    AntiAliasing = AntiAliasingStyles.All
                };

                var chartArea = new ChartArea("ComparisonArea")
                {
                    BackColor = Color.FromArgb(20, 30, 40, 50),
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 1
                };
                // Cấu hình trục
                chartArea.AxisX.Interval = 1;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.Title = "Nhiệt độ (°C)";
                chartArea.AxisX.TitleFont = new Font("Segoe UI", 10F);
            
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.Title = "Ngày";
                chartArea.AxisY.TitleFont = new Font("Segoe UI", 10F);

                chart.ChartAreas.Add(chartArea);
                // Tạo series cho hôm nay và hôm qua
                var todaySeries = new Series("Hôm nay")
                {
                    ChartType = SeriesChartType.Bar,
                    Color = Color.FromArgb(255, 255, 159, 67),
                    BorderColor = Color.FromArgb(255, 255, 159, 67),
                    BorderWidth = 2
                };

                var yesterdaySeries = new Series("Hôm qua")
                {
                    ChartType = SeriesChartType.Bar,
                    Color = Color.FromArgb(255, 74, 144, 226),
                    BorderColor = Color.FromArgb(255, 74, 144, 226),
                    BorderWidth = 2
                };

                // Thêm dữ liệu mẫu
                todaySeries.Points.AddXY(25, "Tối thiểu");
                todaySeries.Points.AddXY(34, "Tối đa");
                
                yesterdaySeries.Points.AddXY(25, "Tối thiểu");
                yesterdaySeries.Points.AddXY(32, "Tối đa");

                chart.Series.Add(todaySeries);
                chart.Series.Add(yesterdaySeries);

                parent.Controls.Add(chart);
            }
            catch (Exception ex)
            {
            }
        }
        private void CreateRainProbabilityChart(GroupBox parent)
        {
            try
            {
                var chart = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40),
                    AntiAliasing = AntiAliasingStyles.All
                };

                var chartArea = new ChartArea("RainProbArea")
                {
                    BackColor = Color.Transparent, // Nền trong suốt
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 1
                };

                // Cấu hình trục
                chartArea.AxisX.Interval = 6; // Mỗi 6 giờ
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.Title = "Giờ";
                chartArea.AxisX.TitleFont = new Font("Segoe UI", 10F);
                chartArea.AxisX.Minimum = 0;
                chartArea.AxisX.Maximum = 24;
                
                chartArea.AxisY.Interval = 20; // Mỗi 20%
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.Title = "Tỉ lệ (%)";
                chartArea.AxisY.TitleFont = new Font("Segoe UI", 10F);
                chartArea.AxisY.Minimum = 0;
                chartArea.AxisY.Maximum = 100;

                // Cấu hình grid
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisX.MajorGrid.Enabled = true;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

                chart.ChartAreas.Add(chartArea);

                // Tạo series cho tỉ lệ mưa (SplineArea)
                var rainProbSeries = new Series("Tỉ lệ mưa")
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.FromArgb(120, Color.DeepSkyBlue), // Xanh nhạt có độ trong suốt
                    BorderColor = Color.DeepSkyBlue,
                    BorderWidth = 2
                };

                // Thêm dữ liệu mẫu (0% mưa trong ngày)
                for (int hour = 0; hour < 24; hour += 6)
                {
                    rainProbSeries.Points.AddXY(hour, 0); // 0% mưa
                }

                chart.Series.Add(rainProbSeries);

                // Thêm text hiển thị tỉ lệ mưa hôm nay
                var title = new Title("Khả năng có mưa vào hôm nay: 0%")
                {
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.White,
                    Docking = Docking.Top
                };
                chart.Titles.Add(title);

                parent.Controls.Add(chart);
            }
            catch (Exception ex)
            {
            }
        }
        private void CreateRainfallSummary(GroupBox parent)
        {
            try
            {
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(10)
                };

                // 24 giờ qua
                var past24hPanel = new Panel
                {
                    Location = new Point(10, 10),
                    Size = new Size(parent.Width - 40, 50),
                    BackColor = Color.FromArgb(60, 74, 144, 226),
                    Padding = new Padding(10)
                };

                var past24hIcon = new Label
                {
                    Text = "🌧️",
                    Font = new Font("Segoe UI Emoji", 20F),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var past24hLabel = new Label
                {
                    Text = "24 GIỜ QUA",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(50, 10),
                    AutoSize = true
                };

                var past24hValue = new Label
                {
                    Text = "Mưa 3 mm",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.White,
                    Location = new Point(50, 30),
                    AutoSize = true
                };

                past24hPanel.Controls.Add(past24hIcon);
                past24hPanel.Controls.Add(past24hLabel);
                past24hPanel.Controls.Add(past24hValue);

                // 24 giờ tới
                var next24hPanel = new Panel
                {
                    Location = new Point(10, 70),
                    Size = new Size(parent.Width - 40, 50),
                    BackColor = Color.FromArgb(60, 255, 159, 67),
                    Padding = new Padding(10)
                };

                var next24hIcon = new Label
                {
                    Text = "🌧️",
                    Font = new Font("Segoe UI Emoji", 20F),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var next24hLabel = new Label
                {
                    Text = "24 GIỜ TỚI",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(50, 10),
                    AutoSize = true
                };

                var next24hValue = new Label
                {
                    Text = "Mưa 4 mm",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.White,
                    Location = new Point(50, 30),
                    AutoSize = true
                };

                next24hPanel.Controls.Add(next24hIcon);
                next24hPanel.Controls.Add(next24hLabel);
                next24hPanel.Controls.Add(next24hValue);

                panel.Controls.Add(past24hPanel);
                panel.Controls.Add(next24hPanel);
                parent.Controls.Add(panel);
            }
            catch (Exception ex)
            {
            }
        }

        private void InitializeTemperatureChart()
        {
            try
            {
                // Dùng 2 TabPage có sẵn trên giao diện: tabChart, tabMap
                bieuDoNhietDo = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40), // Nền tối đẹp
                    AntiAliasing = AntiAliasingStyles.All,
                    TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                    Margin = new Padding(0)
                };
                // Tạo ChartArea với gradient nền vàng-cam
                var chartArea = new ChartArea("MainArea")
                {
                    BackColor = Color.Orange, // Màu cam
                    BackSecondaryColor = Color.Yellow, // Màu vàng
                    BackGradientStyle = GradientStyle.TopBottom, // Gradient từ trên xuống
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 2,
                    Position = new ElementPosition(0, 0, 100, 100),
                    InnerPlotPosition = new ElementPosition(12, 20, 82, 70)
                };
                // Cấu hình grid đẹp hơn
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisX.MajorGrid.Enabled = true;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

                // Cấu hình màu chữ
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;

                bieuDoNhietDo.ChartAreas.Add(chartArea);

                // Tạo layout scrollable như trong hình
                tabChart.Controls.Clear();
                
                // Panel chính có thể scroll
                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.FromArgb(30, 25, 35, 45)
                };
                
                // 1. Biểu đồ nhiệt độ chính (line chart)
                var tempChartGroup = new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Text = "Nhiệt độ 24 giờ",
                    BackColor = Color.FromArgb(40, 30, 40, 50),
                    Padding = new Padding(5)
                };
                
                // Chuyển biểu đồ về line chart
                bieuDoNhietDo.ChartAreas[0].BackColor = Color.FromArgb(20, 30, 40, 50);
                bieuDoNhietDo.BackColor = Color.FromArgb(30, 20, 30, 40);
                bieuDoNhietDo.Dock = DockStyle.Fill;
                tempChartGroup.Controls.Add(bieuDoNhietDo);
                
                
                mainPanel.Controls.Add(tempChartGroup);
                tabChart.Controls.Add(mainPanel);
                // Đảm bảo control bản đồ tồn tại và nằm trên tabMap
                EnsureWindyBrowser();
                if (banDoGio != null)
                {
                    banDoGio.Dock = DockStyle.Fill;
                    tabMap.Controls.Clear();
                    tabMap.Controls.Add(banDoGio);
                }

                // Nút export trên tab biểu đồ
                var btnExport = new Button
                {
                    Text = "Xuất biểu đồ",
                    Location = new Point(334, 182),
                    Size = new Size(124, 29),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                btnExport.Click += (s, e) => ExportChart();
                tabChart.Controls.Add(btnExport);

                // Chuyển đổi hiển thị khi đổi tab
                try
                {
                    tabDieuKhien.SelectedIndexChanged -= TabDieuKhien_SelectedIndexChanged;
                }
                catch { }
                tabDieuKhien.SelectedIndexChanged += TabDieuKhien_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
            }
        }

        private void EnsureWindyBrowser()
        {
            if (banDoGio != null) return;

            banDoGio = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
            // Thêm vào tabMap khi đã khởi tạo từ Designer
            tabMap.Controls.Add(banDoGio);
            banDoGio.BringToFront();
        }
        private void ShowChart()
        {
            if (bieuDoNhietDo != null) bieuDoNhietDo.Visible = true;
            if (banDoGio != null) banDoGio.Visible = false;
        }

        private async void ShowMap()
        {
            EnsureWindyBrowser();
            if (banDoGio == null) return;
            // Nếu chưa có tọa độ hiện tại, lấy từ vị trí hiện tại
            if (currentLat == 0 && currentLon == 0)
            {
                try
                {
                    var locationData = await WeatherApiService.GetCurrentLocationAsync();
                    if (locationData?.Results?.Length > 0)
                    {
                        var result = locationData.Results[0];
                        currentLat = result.Lat;
                        currentLon = result.Lon;
                    }
                }
                catch (Exception ex)
                {
                    // Fallback về tọa độ mặc định (Hà Nội)
                    currentLat = 21.0285;
                    currentLon = 105.8542;
                }
            }
            // Luôn nạp theo vị trí hiện tại
            LoadWindyMap(currentLat, currentLon);
            if (bieuDoNhietDo != null) bieuDoNhietDo.Visible = false;
            banDoGio.Visible = true;
        }
        private void TabDieuKhien_SelectedIndexChanged(object? sender, EventArgs e)
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
            catch { }
        }

        private void LoadWindyMap(double lat, double lon)
        {
            EnsureWindyBrowser();
            if (banDoGio == null) return;

            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string embedUrl = $"https://embed.windy.com/embed2.html?key={KHOABAN_DOGIO}&lat={latStr}&lon={lonStr}&detailLat={latStr}&detailLon={lonStr}&zoom=7&overlay=temp&level=surface&menu=&message=true&marker=true&calendar=&pressure=true&type=map&location=coordinates&detail=true&metricWind=default&metricTemp=default";
            banDoGio.Source = new Uri(embedUrl);
        }
        /// Xuất biểu đồ ra file hình ảnh
        private void ExportChart()
        {
            try
            {
                if (bieuDoNhietDo == null)
                {
                    MessageBox.Show("Không có biểu đồ để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                    saveDialog.Title = "Xuất biểu đồ nhiệt độ";
                    saveDialog.FileName = $"Biểu đồ nhiệt độ {DateTime.Now:yyyy-MM-dd HH-mm-ss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        bieuDoNhietDo.SaveImage(saveDialog.FileName, ChartImageFormat.Png);
                        MessageBox.Show($"Đã xuất biểu đồ thành công!\nFile: {saveDialog.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi xuất biểu đồ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
        #region Utility Methods
        /// Chuyển đổi Celsius sang Fahrenheit
        private double ConvertCelsiusToFahrenheit(double celsius)
        {
            return (celsius * 9.0 / 5.0) + 32.0;
        }
        /// Lấy nhiệt độ theo đơn vị hiện tại
        private double GetTemperatureInUnit(double celsius)
        {
            return donViCelsius ? celsius : ConvertCelsiusToFahrenheit(celsius);
        }
        /// Chuyển đổi nhiệt độ từ text hiện tại sang đơn vị mới
        private double ConvertTemperatureFromText(string tempText, bool isCurrentlyCelsius)
        {
            if (double.TryParse(tempText, out double temp))
            {
                if (isCurrentlyCelsius && !donViCelsius)
                {
                    // Đang là C, chuyển sang F
                    return ConvertCelsiusToFahrenheit(temp);
                }
                else if (!isCurrentlyCelsius && donViCelsius)
                {
                    // Đang là F, chuyển sang C
                    return ConvertFahrenheitToCelsius(temp);
                }
                else
                {
                    // Cùng đơn vị, không cần chuyển đổi
                    return temp;
                }
            }
            return temp;
        }
        /// Chuyển đổi Fahrenheit sang Celsius
        private double ConvertFahrenheitToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32.0) * 5.0 / 9.0;
        }
        /// Tạo panel cho dự báo một giờ
        #region Quản lý địa điểm yêu thích
        /// Lưu danh sách địa điểm yêu thích vào file JSON
        private void SaveLocations()
        {
            try
            {
                var json = JsonConvert.SerializeObject(diaDiemYeuThich, Formatting.Indented);
                File.WriteAllText("favorite_locations.json", json);
            }
            catch (Exception ex)
            {
            }
        }
        /// Tải danh sách địa điểm yêu thích từ file JSON
        private void LoadLocations()
        {
            try
            {
                if (File.Exists("favorite_locations.json"))
                {
                    var json = File.ReadAllText("favorite_locations.json");
                    diaDiemYeuThich = JsonConvert.DeserializeObject<List<FavoriteLocation>>(json) ?? new List<FavoriteLocation>();
                }
                else
                {
                    diaDiemYeuThich = new List<FavoriteLocation>();
                }
            }
            catch (Exception ex)
            {
                diaDiemYeuThich = new List<FavoriteLocation>();
            }
        }
        /// Thêm địa điểm hiện tại vào danh sách yêu thích
        private void AddCurrentLocationToFavorites()
        {
            try
            {
                if (weatherData?.Current == null || string.IsNullOrEmpty(currentLocation))
                {
                    MessageBox.Show("Không có dữ liệu địa điểm để thêm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Kiểm tra xem địa điểm đã tồn tại chưa
                var existingLocation = diaDiemYeuThich.FirstOrDefault(l => 
                    l.Name.Equals(currentLocation.Split(',')[0].Trim(), StringComparison.OrdinalIgnoreCase));

                if (existingLocation != null)
                {
                    MessageBox.Show("Địa điểm này đã có trong danh sách yêu thích!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Tạo địa điểm mới
                var newLocation = new FavoriteLocation
                {
                    Name = currentLocation.Split(',')[0].Trim(),
                    Country = currentLocation.Split(',').Length > 1 ? currentLocation.Split(',')[1].Trim() : "",
                    Latitude = weatherData.Lat,
                    Longitude = weatherData.Lon,
                    AddedDate = DateTime.Now
                };

                diaDiemYeuThich.Add(newLocation);
                SaveLocations();

                MessageBox.Show($"Đã thêm '{newLocation.Name}' vào danh sách yêu thích!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Cập nhật ComboBox nếu có
                UpdateFavoritesComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra khi thêm địa điểm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// Xóa địa điểm khỏi danh sách yêu thích
        private void RemoveSelectedLocation()
        {
            try
            {
                // Tìm địa điểm được chọn (có thể từ ComboBox hoặc cách khác)
                if (diaDiemYeuThich.Count == 0)
                {
                    MessageBox.Show("Danh sách yêu thích trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Hiển thị dialog chọn địa điểm để xóa
                var locationNames = diaDiemYeuThich.Select(l => $"{l.Name}, {l.Country}").ToArray();
                var selectedIndex = -1;
                
                // Tạo dialog đơn giản để chọn địa điểm
                using (var form = new Form())
                {
                    form.Text = "Chọn địa điểm để xóa";
                    form.Size = new Size(400, 300);
                    form.StartPosition = FormStartPosition.CenterParent;

                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        DataSource = locationNames
                    };

                    var buttonPanel = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 50
                    };

                    var btnOK = new Button
                    {
                        Text = "Xóa",
                        DialogResult = DialogResult.OK,
                        Location = new Point(200, 10),
                        Size = new Size(80, 30)
                    };

                    var btnCancel = new Button
                    {
                        Text = "Hủy",
                        DialogResult = DialogResult.Cancel,
                        Location = new Point(290, 10),
                        Size = new Size(80, 30)
                    };

                    buttonPanel.Controls.Add(btnOK);
                    buttonPanel.Controls.Add(btnCancel);
                    form.Controls.Add(listBox);
                    form.Controls.Add(buttonPanel);

                    if (form.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                    {
                        selectedIndex = listBox.SelectedIndex;
                    }
                }

                if (selectedIndex >= 0 && selectedIndex < diaDiemYeuThich.Count)
                {
                    var locationToRemove = diaDiemYeuThich[selectedIndex];
                    diaDiemYeuThich.RemoveAt(selectedIndex);
                    SaveLocations();

                    MessageBox.Show($"Đã xóa '{locationToRemove.Name}' khỏi danh sách yêu thích!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Cập nhật ComboBox nếu có
                    UpdateFavoritesComboBox();
                }
            }
            catch (Exception ex)
            {
                    MessageBox.Show("Có lỗi xảy ra khi xóa địa điểm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// Cập nhật ComboBox địa điểm yêu thích (nếu có)
        private void UpdateFavoritesComboBox()
        {
            try
            {
                // Tìm ComboBox địa điểm yêu thích trong form
                var comboBox = this.Controls.Find("comboFavorites", true).FirstOrDefault() as ComboBox;
                if (comboBox != null)
                {
                    comboBox.DataSource = null;
                    comboBox.DataSource = diaDiemYeuThich.Select(l => $"{l.Name}, {l.Country}").ToList();
                    
                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        /// Lấy tên ngày bằng tiếng Việt
        private string GetVietnameseDayName(long unixTime)
        {
            var date = UnixToLocal(unixTime);
        
            // Sử dụng thời gian từ API nếu có, nếu không thì dùng thời gian máy
            DateTime today;
            if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
            {
                var apiTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt + weatherData.TimezoneOffset).DateTime;
                today = apiTime.Date;
            }
            else
            {
                today = DateTime.Now.Date;
            }

            // So sánh ngày với độ chính xác cao hơn
            var targetDate = date.Date;
            
            if (targetDate == today)
            {
                return "Hôm nay";
            }
            else if (targetDate == today.AddDays(1))
            {
                return "Ngày mai";
            }
            else
            {
                string[] dayNames = { "Chủ nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
                string dayName = dayNames[(int)date.DayOfWeek];
                return $"{dayName} {date:dd/MM}";
            }
        }

        /// Chuyển đổi mô tả thời tiết sang tiếng Việt
        private string GetVietnameseWeatherDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return "N/A";

            var vietnameseMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "clear sky", "Trời quang" },
                { "few clouds", "Ít mây" },
                { "scattered clouds", "Mây thưa" },
                { "broken clouds", "Mây rải rác" },
                { "overcast clouds", "Nhiều mây" },
                { "heavy intensity rain", "Mưa to" },
                { "light rain", "Mưa nhẹ" },
                { "moderate rain", "Mưa vừa" },
                { "heavy rain", "Mưa to" },
                { "very heavy rain", "Mưa rất to" },
                { "extreme rain", "Mưa cực to" },
                { "freezing rain", "Mưa đá" },
                { "light intensity shower rain", "Mưa rào nhẹ" },
                { "shower rain", "Mưa rào" },
                { "heavy intensity shower rain", "Mưa rào to" },
                { "ragged shower rain", "Mưa rào không đều" },
                { "light snow", "Tuyết nhẹ" },
                { "snow", "Tuyết" },
                { "heavy snow", "Tuyết to" },
                { "sleet", "Mưa tuyết" },
                { "light shower sleet", "Mưa tuyết nhẹ" },
                { "shower sleet", "Mưa tuyết" },
                { "light rain and snow", "Mưa và tuyết nhẹ" },
                { "rain and snow", "Mưa và tuyết" },
                { "light shower snow", "Tuyết rơi nhẹ" },
                { "shower snow", "Tuyết rơi" },
                { "heavy shower snow", "Tuyết rơi to" },
                { "mist", "Sương mù" },
                { "smoke", "Khói" },
                { "haze", "Sương mù nhẹ" },
                { "sand/dust whirls", "Cát/bụi xoáy" },
                { "fog", "Sương mù dày" },
                { "sand", "Cát" },
                { "dust", "Bụi" },
                { "volcanic ash", "Tro núi lửa" },
                { "squalls", "Giông tố" },
                { "tornado", "Lốc xoáy" },
                { "cold", "Lạnh" },
                { "hot", "Nóng" },
                { "windy", "Có gió" },
                { "hail", "Mưa đá" },
                { "calm", "Lặng gió" },
                { "light breeze", "Gió nhẹ" },
                { "gentle breeze", "Gió nhẹ" },
                { "moderate breeze", "Gió vừa" },
                { "fresh breeze", "Gió mạnh" },
                { "strong breeze", "Gió rất mạnh" },
                { "high wind", "Gió cực mạnh" },
                { "gale", "Bão" },
                { "severe gale", "Bão mạnh" },
                { "storm", "Bão" },
                { "violent storm", "Bão dữ dội" },
                { "hurricane", "Cuồng phong" },
                // Thêm các trạng thái còn thiếu
                { "light intensity drizzle", "Mưa phùn nhẹ" },
                { "drizzle", "Mưa phùn" },
                { "heavy intensity drizzle", "Mưa phùn to" },
                { "light intensity drizzle rain", "Mưa phùn nhẹ" },
                { "drizzle rain", "Mưa phùn" },
                { "heavy intensity drizzle rain", "Mưa phùn to" },
                { "shower rain and drizzle", "Mưa rào và phùn" },
                { "heavy shower rain and drizzle", "Mưa rào và phùn to" },
                { "shower drizzle", "Mưa phùn rào" },
                { "light freezing drizzle", "Mưa phùn đóng băng nhẹ" },
                { "freezing drizzle", "Mưa phùn đóng băng" },
                { "snow grains", "Hạt tuyết" },
                { "thunderstorm with light rain", "Bão có mưa nhẹ" },
                { "thunderstorm with rain", "Bão có mưa" },
                { "thunderstorm with heavy rain", "Bão có mưa to" },
                { "light thunderstorm", "Bão nhẹ" },
                { "thunderstorm", "Bão" },
                { "heavy thunderstorm", "Bão to" },
                { "ragged thunderstorm", "Bão không đều" },
                { "thunderstorm with light drizzle", "Bão có mưa phùn nhẹ" },
                { "thunderstorm with drizzle", "Bão có mưa phùn" },
                { "thunderstorm with heavy drizzle", "Bão có mưa phùn to" }
            };

            return vietnameseMap.TryGetValue(description, out string? vietnamese) ? vietnamese : description;
        }
        /// Lấy thông tin mưa và gió
        private string GetRainWindInfo(DailyWeather daily)
        {
            var info = new List<string>();

            // Thông tin mưa - kiểm tra Rain object
            if (daily.Rain != null)
            {
                // Nếu có dữ liệu mưa, hiển thị thông tin cơ bản
                info.Add("Có mưa");
            }

            // Thông tin gió
            if (daily.WindSpeed > 0)
            {
                info.Add($"Gió: {Math.Round(daily.WindSpeed, 1)} m/s");
            }

            return string.Join("\n", info);
        }
        
    }
    // Extension method để vẽ hình chữ nhật bo tròn
    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rectangle, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rectangle.X, rectangle.Y, radius, radius, 180, 90);
            path.AddArc(rectangle.X + rectangle.Width - radius, rectangle.Y, radius, radius, 270, 90);
            path.AddArc(rectangle.X + rectangle.Width - radius, rectangle.Y + rectangle.Height - radius, radius, radius, 0, 90);
            path.AddArc(rectangle.X, rectangle.Y + rectangle.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            graphics.DrawPath(pen, path);
        }
        #endregion
    }
    #region Data Classes
    /// <summary>
    /// Class lưu trữ thông tin địa điểm đã lưu
    /// </summary>
    public class SavedLocation
    {
        public string Name { get; set; } = "";
        public double Lat { get; set; }
        public double Lon { get; set; }
        
        public SavedLocation(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }

    /// <summary>
    /// Class để quản lý địa điểm yêu thích
    /// </summary>
    public class FavoriteLocation
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }

    #endregion
}
