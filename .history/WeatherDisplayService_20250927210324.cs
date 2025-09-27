using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý hiển thị thông tin thời tiết
    /// 2️⃣ Gọi API & thông tin mô tả
    /// </summary>
    public class WeatherDisplayService
    {
        private readonly DichVuThoiTiet weatherService;
        private bool isCelsius = true;

        public WeatherDisplayService()
        {
            weatherService = new DichVuThoiTiet();
        }

        #region Weather Display

        /// <summary>
        /// Hiển thị thông tin thời tiết chính
        /// </summary>
        public void DisplayWeatherInfo(string locationName, ThoiTietHienTai weather, bool isCelsius, 
            Label lblLocation, Label lblTime, Label lblTemperature, Label lblStatus, 
            PictureBox picWeatherIcon, Panel detailPanel)
        {
            try
            {
                this.isCelsius = isCelsius;
                var temperatureUnit = isCelsius ? "°C" : "°F";
                var kyHieuNhietDo = isCelsius ? "°C" : "°F";

                // Cập nhật thông tin cơ bản
                if (lblLocation != null)
                    lblLocation.Text = locationName;

                if (lblTime != null)
                    lblTime.Text = DateTime.Now.ToString("HH:mm");

                if (weather != null)
                {
                    var temp = isCelsius ? weather.NhietDo : (weather.NhietDo * 9.0 / 5.0 + 32);
                    
                    if (lblTemperature != null)
                        lblTemperature.Text = $"{temp:F1}{temperatureUnit}";

                    if (lblStatus != null)
                        lblStatus.Text = weather.MoTa ?? "N/A";

                    // Cập nhật icon thời tiết
                    if (picWeatherIcon != null && !string.IsNullOrEmpty(weather.Icon))
                    {
                        picWeatherIcon.Image = GetWeatherIconFromEmoji(GetWeatherIcon(weather.Icon));
                    }
                }

                // Cập nhật panel chi tiết
                if (weather != null)
                {
                    UpdateDetailPanel(weather, kyHieuNhietDo, detailPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị thông tin thời tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin thời tiết khi đổi đơn vị
        /// </summary>
        public async Task UpdateWeatherDisplay(OneCallResponse weatherData, bool isCelsius,
            Label lblTemperature, Label lblStatus, PictureBox picWeatherIcon, Panel detailPanel)
        {
            try
            {
                if (weatherData == null) return;

                this.isCelsius = isCelsius;
                var kyHieuNhietDo = isCelsius ? "°C" : "°F";

                if (weatherData.Current != null)
                {
                    var current = weatherData.Current;
                    var temp = isCelsius ? current.Temp : (current.Temp * 9.0 / 5.0 + 32);
                    
                    if (lblTemperature != null)
                        lblTemperature.Text = $"{temp:F1}{(isCelsius ? "°C" : "°F")}";

                    if (lblStatus != null && current.Weather?.Length > 0)
                        lblStatus.Text = current.Weather[0].Description ?? "N/A";

                    // Cập nhật icon
                    if (picWeatherIcon != null && current.Weather?.Length > 0)
                    {
                        var iconCode = current.Weather[0].Icon;
                        if (!string.IsNullOrEmpty(iconCode))
                        {
                            picWeatherIcon.Image = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                        }
                    }

                    // Cập nhật panel chi tiết
                    UpdateDetailPanel(current, kyHieuNhietDo, detailPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật hiển thị thời tiết: {ex.Message}");
            }
        }

        #endregion

        #region Detail Panel Updates

        /// <summary>
        /// Cập nhật panel chi tiết từ dữ liệu API
        /// </summary>
        public void UpdateDetailPanelFromApi(ThoiTietHienTai current, string kyHieu, Panel detailPanel)
        {
            try
            {
                if (detailPanel == null) return;

                // Cập nhật cảm giác như
                var feelsLike = isCelsius ? current.FeelsLike : (current.FeelsLike * 9.0 / 5.0 + 32);
                UpdatePanelText(detailPanel, "feelsLikePanel", $"Cảm giác như: {feelsLike:F1}{kyHieu}");

                // Cập nhật độ ẩm
                UpdatePanelText(detailPanel, "humidityPanel", $"Độ ẩm: {current.Humidity}%");

                // Cập nhật tốc độ gió
                var windSpeed = isCelsius ? current.WindSpeed : (current.WindSpeed * 2.237); // m/s to mph
                var windUnit = isCelsius ? "m/s" : "mph";
                UpdatePanelText(detailPanel, "windPanel", $"Tốc độ gió: {windSpeed:F1} {windUnit}");

                // Cập nhật áp suất
                UpdatePanelText(detailPanel, "pressurePanel", $"Áp suất: {current.Pressure} hPa");

                // Cập nhật tầm nhìn
                var visibility = isCelsius ? current.Visibility / 1000.0 : (current.Visibility / 1000.0 * 0.621371); // km to miles
                var visibilityUnit = isCelsius ? "km" : "miles";
                UpdatePanelText(detailPanel, "visibilityPanel", $"Tầm nhìn: {visibility:F1} {visibilityUnit}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật panel chi tiết từ dữ liệu hourly
        /// </summary>
        public void UpdateDetailPanelFromHourlyApi(HourlyWeather hourly, string kyHieu, Panel detailPanel)
        {
            try
            {
                if (detailPanel == null) return;

                // Cập nhật cảm giác như
                var feelsLike = isCelsius ? hourly.FeelsLike : (hourly.FeelsLike * 9.0 / 5.0 + 32);
                UpdatePanelText(detailPanel, "feelsLikePanel", $"Cảm giác như: {feelsLike:F1}{kyHieu}");

                // Cập nhật độ ẩm
                UpdatePanelText(detailPanel, "humidityPanel", $"Độ ẩm: {hourly.Humidity}%");

                // Cập nhật tốc độ gió
                var windSpeed = isCelsius ? hourly.WindSpeed : (hourly.WindSpeed * 2.237);
                var windUnit = isCelsius ? "m/s" : "mph";
                UpdatePanelText(detailPanel, "windPanel", $"Tốc độ gió: {windSpeed:F1} {windUnit}");

                // Cập nhật áp suất
                UpdatePanelText(detailPanel, "pressurePanel", $"Áp suất: {hourly.Pressure} hPa");

                // Cập nhật tầm nhìn
                var visibility = isCelsius ? hourly.Visibility / 1000.0 : (hourly.Visibility / 1000.0 * 0.621371);
                var visibilityUnit = isCelsius ? "km" : "miles";
                UpdatePanelText(detailPanel, "visibilityPanel", $"Tầm nhìn: {visibility:F1} {visibilityUnit}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel hourly: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật panel chi tiết từ dữ liệu daily
        /// </summary>
        public void UpdateDetailPanelFromDailyApi(DailyWeather daily, string kyHieu, Panel detailPanel)
        {
            try
            {
                if (detailPanel == null) return;

                // Cập nhật cảm giác như (sử dụng temp.day)
                var feelsLike = isCelsius ? daily.Temp.Day : (daily.Temp.Day * 9.0 / 5.0 + 32);
                UpdatePanelText(detailPanel, "feelsLikePanel", $"Cảm giác như: {feelsLike:F1}{kyHieu}");

                // Cập nhật độ ẩm
                UpdatePanelText(detailPanel, "humidityPanel", $"Độ ẩm: {daily.Humidity}%");

                // Cập nhật tốc độ gió
                var windSpeed = isCelsius ? daily.WindSpeed : (daily.WindSpeed * 2.237);
                var windUnit = isCelsius ? "m/s" : "mph";
                UpdatePanelText(detailPanel, "windPanel", $"Tốc độ gió: {windSpeed:F1} {windUnit}");

                // Cập nhật áp suất
                UpdatePanelText(detailPanel, "pressurePanel", $"Áp suất: {daily.Pressure} hPa");

                // Cập nhật tầm nhìn (daily không có visibility, dùng giá trị mặc định)
                UpdatePanelText(detailPanel, "visibilityPanel", "Tầm nhìn: N/A");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel daily: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo nội dung ban đầu cho panel chi tiết
        /// </summary>
        public void CreateDetailPanelContent(Panel detailPanel)
        {
            try
            {
                if (detailPanel == null) return;

                // Tạo các panel con nếu chưa có
                CreatePanelIfNotExists(detailPanel, "feelsLikePanel", "Cảm giác như: --");
                CreatePanelIfNotExists(detailPanel, "humidityPanel", "Độ ẩm: --%");
                CreatePanelIfNotExists(detailPanel, "windPanel", "Tốc độ gió: --");
                CreatePanelIfNotExists(detailPanel, "pressurePanel", "Áp suất: -- hPa");
                CreatePanelIfNotExists(detailPanel, "visibilityPanel", "Tầm nhìn: --");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo nội dung panel chi tiết: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Cập nhật text của panel con
        /// </summary>
        private void UpdatePanelText(Panel parentPanel, string panelName, string text)
        {
            try
            {
                var panel = parentPanel.Controls.Find(panelName, true);
                if (panel.Length > 0 && panel[0] is Panel targetPanel)
                {
                    var label = targetPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (label != null)
                    {
                        label.Text = text;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật text panel {panelName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo panel con nếu chưa tồn tại
        /// </summary>
        private void CreatePanelIfNotExists(Panel parentPanel, string panelName, string text)
        {
            try
            {
                var existingPanel = parentPanel.Controls.Find(panelName, true);
                if (existingPanel.Length == 0)
                {
                    var newPanel = new Panel
                    {
                        Name = panelName,
                        Size = new Size(200, 30),
                        BackColor = Color.FromArgb(100, 255, 255, 255)
                    };

                    var label = new Label
                    {
                        Text = text,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = Color.White
                    };

                    newPanel.Controls.Add(label);
                    parentPanel.Controls.Add(newPanel);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo panel {panelName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy đường dẫn icon thời tiết
        /// </summary>
        private string GetWeatherIcon(string iconCode)
        {
            if (string.IsNullOrEmpty(iconCode)) return GetIconPath("troi_quang_ngay.png");

            var iconMap = new Dictionary<string, string>
            {
                ["01d"] = "troi_quang_ngay.png",
                ["01n"] = "troi_quang_dem.png",
                ["02d"] = "it_may_ngay.png",
                ["02n"] = "it_may_dem.png",
                ["03d"] = "may_rac_ngay.png",
                ["03n"] = "may_rac_dem.png",
                ["04d"] = "may_day_ngay.png",
                ["04n"] = "may_day_dem.png",
                ["09d"] = "mua_roi_ngay.png",
                ["09n"] = "mua_roi_dem.png",
                ["10d"] = "mua_ngay.png",
                ["10n"] = "mua_dem.png",
                ["11d"] = "giong_bao_ngay.png",
                ["11n"] = "giong_bao_dem.png",
                ["13d"] = "tuyet_ngay.png",
                ["13n"] = "tuyet_dem.png",
                ["50d"] = "suong_mu_ngay.png",
                ["50n"] = "suong_mu_dem.png"
            };

            return iconMap.TryGetValue(iconCode, out var iconPath) ? GetIconPath(iconPath) : GetIconPath("troi_quang_ngay.png");
        }

        /// <summary>
        /// Lấy đường dẫn đầy đủ của icon
        /// </summary>
        private string GetIconPath(string iconName)
        {
            var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            return Path.Combine(resourcesPath, iconName);
        }

        /// <summary>
        /// Tạo icon thời tiết từ emoji
        /// </summary>
        private Image GetWeatherIconFromEmoji(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 200);
        }

        /// <summary>
        /// Tạo icon thời tiết từ emoji với kích thước tùy chỉnh
        /// </summary>
        private Image GetWeatherIconFromEmoji(string iconPath, int size)
        {
            try
            {
                if (File.Exists(iconPath))
                {
                    var originalImage = Image.FromFile(iconPath);
                    return new Bitmap(originalImage, new Size(size, size));
                }
                else
                {
                    // Fallback: tạo icon mặc định
                    var bitmap = new Bitmap(size, size);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawString("☀", new Font("Segoe UI Emoji", size / 2), Brushes.Yellow, 
                            new PointF(size / 4, size / 4));
                    }
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo icon: {ex.Message}");
                return new Bitmap(size, size);
            }
        }

        #endregion
    }
}
