using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý dự báo thời tiết 24h và 5 ngày
    /// 3️⃣ Thời tiết 24h & 5 ngày
    /// </summary>
    public class ForecastService
    {
        private bool isCelsius = true;

        public ForecastService()
        {
        }

        #region 24 Hour Forecast

        /// <summary>
        /// Hiển thị dự báo 24 giờ
        /// </summary>
        public void Display24HourForecast(List<DuBaoTheoGioItem> hourlyData, Panel containerPanel, bool isCelsius)
        {
            try
            {
                this.isCelsius = isCelsius;
                
                if (containerPanel == null || hourlyData == null || hourlyData.Count == 0)
                    return;

                // Xóa các control cũ
                containerPanel.Controls.Clear();

                // Tạo scroll panel nếu cần
                var scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                var flowLayout = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                // Tạo card cho mỗi giờ
                foreach (var hourData in hourlyData.Take(24))
                {
                    var hourCard = CreateHourCard(hourData);
                    flowLayout.Controls.Add(hourCard);
                }

                scrollPanel.Controls.Add(flowLayout);
                containerPanel.Controls.Add(scrollPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị dự báo 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo card cho một giờ
        /// </summary>
        private Panel CreateHourCard(DuBaoTheoGioItem hourData)
        {
            try
            {
                var card = new Panel
                {
                    Size = new Size(80, 120),
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(100, 255, 255, 255),
                    BorderStyle = BorderStyle.None
                };

                // Thời gian
                var timeLabel = new Label
                {
                    Text = hourData.ThoiGian.ToString("HH:mm"),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 25
                };

                // Icon thời tiết
                var iconPictureBox = new PictureBox
                {
                    Size = new Size(40, 40),
                    Location = new Point(20, 30),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };

                // Nhiệt độ
                var temp = isCelsius ? hourData.NhietDo : (hourData.NhietDo * 9.0 / 5.0 + 32);
                var tempLabel = new Label
                {
                    Text = $"{temp:F0}°",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 75),
                    Size = new Size(80, 20)
                };

                // Mô tả (nếu có)
                var descLabel = new Label
                {
                    Text = "N/A", // DuBaoTheoGioItem không có MoTa
                    Font = new Font("Segoe UI", 7),
                    ForeColor = Color.LightGray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 95),
                    Size = new Size(80, 20)
                };

                card.Controls.AddRange(new Control[] { timeLabel, iconPictureBox, tempLabel, descLabel });

                // Thêm sự kiện click
                card.Click += (s, e) => OnHourCardClick(hourData);

                return card;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card giờ: {ex.Message}");
                return new Panel();
            }
        }

        /// <summary>
        /// Xử lý khi click vào card giờ
        /// </summary>
        private void OnHourCardClick(DuBaoTheoGioItem hourData)
        {
            try
            {
                // Có thể thêm logic xử lý khi click vào card giờ
                System.Diagnostics.Debug.WriteLine($"Clicked on hour: {hourData.ThoiGian:HH:mm}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý click card giờ: {ex.Message}");
            }
        }

        #endregion

        #region 5 Day Forecast

        /// <summary>
        /// Hiển thị dự báo 5 ngày
        /// </summary>
        public void Display5DayForecast(List<DuBaoNgayItem> dailyData, Panel containerPanel, bool isCelsius)
        {
            try
            {
                this.isCelsius = isCelsius;
                
                if (containerPanel == null || dailyData == null || dailyData.Count == 0)
                    return;

                // Xóa các control cũ
                containerPanel.Controls.Clear();

                // Tạo scroll panel nếu cần
                var scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                var flowLayout = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                // Tạo card cho mỗi ngày
                foreach (var dayData in dailyData.Take(5))
                {
                    var dayCard = CreateDayCard(dayData);
                    flowLayout.Controls.Add(dayCard);
                }

                scrollPanel.Controls.Add(flowLayout);
                containerPanel.Controls.Add(scrollPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị dự báo 5 ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo card cho một ngày
        /// </summary>
        private Panel CreateDayCard(DuBaoNgayItem dayData)
        {
            try
            {
                var card = new Panel
                {
                    Size = new Size(300, 80),
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(100, 255, 255, 255),
                    BorderStyle = BorderStyle.None
                };

                // Ngày trong tuần
                var dayLabel = new Label
                {
                    Text = dayData.Ngay.ToString("dddd"),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    Size = new Size(100, 25)
                };

                // Icon thời tiết
                var iconPictureBox = new PictureBox
                {
                    Size = new Size(40, 40),
                    Location = new Point(120, 20),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };

                // Nhiệt độ cao/thấp
                var highTemp = isCelsius ? dayData.NhietDoCao : (dayData.NhietDoCao * 9.0 / 5.0 + 32);
                var lowTemp = isCelsius ? dayData.NhietDoThap : (dayData.NhietDoThap * 9.0 / 5.0 + 32);
                var tempLabel = new Label
                {
                    Text = $"{highTemp:F0}° / {lowTemp:F0}°",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(180, 10),
                    Size = new Size(100, 25)
                };

                // Mô tả thời tiết
                var descLabel = new Label
                {
                    Text = dayData.TrangThaiMoTa ?? "N/A",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.LightGray,
                    Location = new Point(180, 35),
                    Size = new Size(100, 25)
                };

                card.Controls.AddRange(new Control[] { dayLabel, iconPictureBox, tempLabel, descLabel });

                // Thêm sự kiện click
                card.Click += (s, e) => OnDayCardClick(dayData);

                return card;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card ngày: {ex.Message}");
                return new Panel();
            }
        }

        /// <summary>
        /// Xử lý khi click vào card ngày
        /// </summary>
        private void OnDayCardClick(DuBaoNgayItem dayData)
        {
            try
            {
                // Có thể thêm logic xử lý khi click vào card ngày
                System.Diagnostics.Debug.WriteLine($"Clicked on day: {dayData.Ngay:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý click card ngày: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

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

        #endregion
    }
}