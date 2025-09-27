using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý hiển thị dự báo thời tiết
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
        /// Hiển thị dự báo thời tiết 24 giờ
        /// </summary>
        public void Display24HourForecast(List<DuBaoTheoGioItem> hourlyList, string kyHieu, Panel containerPanel)
        {
            try
            {
                if (hourlyList == null || hourlyList.Count == 0 || containerPanel == null)
                    return;

                // Xóa nội dung cũ
                containerPanel.Controls.Clear();

                // Lấy 24 giờ đầu tiên
                var hoursToShow = hourlyList.Take(24).ToList();

                for (int i = 0; i < hoursToShow.Count; i++)
                {
                    var hour = hoursToShow[i];
                    var hourCard = CreateHourCard(hour, kyHieu, i);
                    if (hourCard != null)
                    {
                        containerPanel.Controls.Add(hourCard);
                    }
                }

                // Sắp xếp các card theo thứ tự ngang
                ArrangeCardsHorizontally(containerPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị dự báo 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo card hiển thị thông tin thời tiết theo giờ
        /// </summary>
        private Panel CreateHourCard(DuBaoTheoGioItem hour, string kyHieu, int index)
        {
            try
            {
                var card = new Panel
                {
                    Size = new Size(120, 150),
                    BackColor = Color.FromArgb(100, 255, 255, 255),
                    Margin = new Padding(5),
                    Tag = index
                };

                // Thời gian
                var timeLabel = new Label
                {
                    Text = hour.ThoiGian.ToString("HH:mm"),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 25
                };
                card.Controls.Add(timeLabel);

                // Icon thời tiết
                var iconPictureBox = new PictureBox
                {
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(40, 30),
                    BackColor = Color.Transparent
                };

                if (!string.IsNullOrEmpty(hour.Icon))
                {
                    var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Icon));
                    if (iconImage != null)
                    {
                        iconPictureBox.Image = iconImage;
                    }
                }

                card.Controls.Add(iconPictureBox);

                // Nhiệt độ
                var temp = isCelsius ? hour.NhietDo : (hour.NhietDo * 9.0 / 5.0 + 32);
                var tempLabel = new Label
                {
                    Text = $"{temp:F1}{kyHieu}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 80),
                    Size = new Size(120, 25)
                };
                card.Controls.Add(tempLabel);

                // Mô tả thời tiết
                var descLabel = new Label
                {
                    Text = hour.MoTa ?? "N/A",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.LightGray,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 110),
                    Size = new Size(120, 30),
                    AutoEllipsis = true
                };
                card.Controls.Add(descLabel);

                // Đăng ký sự kiện click
                card.Click += (s, e) => OnHourCardClick(hour, kyHieu);

                return card;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card giờ: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region 5 Day Forecast

        /// <summary>
        /// Hiển thị dự báo thời tiết 5 ngày
        /// </summary>
        public void Display5DayForecast(List<DuBaoNgayItem> dailyList, string kyHieu, Panel containerPanel)
        {
            try
            {
                if (dailyList == null || dailyList.Count == 0 || containerPanel == null)
                    return;

                // Xóa nội dung cũ
                containerPanel.Controls.Clear();

                // Lấy 5 ngày đầu tiên
                var daysToShow = dailyList.Take(5).ToList();

                for (int i = 0; i < daysToShow.Count; i++)
                {
                    var day = daysToShow[i];
                    var dayCard = CreateDayCard(day, kyHieu, i);
                    if (dayCard != null)
                    {
                        containerPanel.Controls.Add(dayCard);
                    }
                }

                // Sắp xếp các card theo thứ tự dọc
                ArrangeCardsVertically(containerPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị dự báo 5 ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo card hiển thị thông tin thời tiết theo ngày
        /// </summary>
        private Panel CreateDayCard(DuBaoNgayItem daily, string kyHieu, int index)
        {
            try
            {
                var card = new Panel
                {
                    Size = new Size(300, 80),
                    BackColor = Color.FromArgb(100, 255, 255, 255),
                    Margin = new Padding(5),
                    Tag = index
                };

                // Ngày trong tuần
                var dayLabel = new Label
                {
                    Text = daily.ThoiGian.ToString("dddd"),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    Size = new Size(100, 25)
                };
                card.Controls.Add(dayLabel);

                // Icon thời tiết
                var iconPictureBox = new PictureBox
                {
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(120, 20),
                    BackColor = Color.Transparent
                };

                if (!string.IsNullOrEmpty(daily.Icon))
                {
                    var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(daily.Icon));
                    if (iconImage != null)
                    {
                        iconPictureBox.Image = iconImage;
                    }
                }

                card.Controls.Add(iconPictureBox);

                // Nhiệt độ cao nhất
                var maxTemp = isCelsius ? daily.NhietDoCao : (daily.NhietDoCao * 9.0 / 5.0 + 32);
                var maxTempLabel = new Label
                {
                    Text = $"{maxTemp:F1}{kyHieu}",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(180, 10),
                    Size = new Size(50, 25)
                };
                card.Controls.Add(maxTempLabel);

                // Nhiệt độ thấp nhất
                var minTemp = isCelsius ? daily.NhietDoThap : (daily.NhietDoThap * 9.0 / 5.0 + 32);
                var minTempLabel = new Label
                {
                    Text = $"{minTemp:F1}{kyHieu}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.LightGray,
                    Location = new Point(180, 35),
                    Size = new Size(50, 25)
                };
                card.Controls.Add(minTempLabel);

                // Mô tả thời tiết
                var descLabel = new Label
                {
                    Text = daily.MoTa ?? "N/A",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.LightGray,
                    Location = new Point(10, 45),
                    Size = new Size(100, 25),
                    AutoEllipsis = true
                };
                card.Controls.Add(descLabel);

                // Đăng ký sự kiện click
                card.Click += (s, e) => OnDayCardClick(daily, kyHieu, index);

                return card;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card ngày: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Xử lý sự kiện click vào card giờ
        /// </summary>
        private void OnHourCardClick(DuBaoTheoGioItem hour, string kyHieu)
        {
            try
            {
                // Cập nhật thông tin chi tiết khi click vào giờ
                System.Diagnostics.Debug.WriteLine($"Clicked hour: {hour.ThoiGian:HH:mm}");
                
                // Có thể thêm logic cập nhật panel chi tiết ở đây
                // Ví dụ: UpdateDetailPanelFromHourlyApi(hour, kyHieu, detailPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý click card giờ: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý sự kiện click vào card ngày
        /// </summary>
        private void OnDayCardClick(DuBaoNgayItem daily, string kyHieu, int dayIndex)
        {
            try
            {
                // Cập nhật thông tin chi tiết khi click vào ngày
                System.Diagnostics.Debug.WriteLine($"Clicked day: {DateTimeOffset.FromUnixTimeSeconds(daily.Dt):dddd}");
                
                // Có thể thêm logic cập nhật panel chi tiết ở đây
                // Ví dụ: UpdateDetailPanelFromDailyApi(daily, kyHieu, detailPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý click card ngày: {ex.Message}");
            }
        }

        #endregion

        #region Layout Management

        /// <summary>
        /// Sắp xếp các card theo chiều ngang
        /// </summary>
        private void ArrangeCardsHorizontally(Panel containerPanel)
        {
            try
            {
                var cards = containerPanel.Controls.OfType<Panel>().OrderBy(p => (int)p.Tag).ToArray();
                var x = 10;
                var y = 10;

                foreach (var card in cards)
                {
                    card.Location = new Point(x, y);
                    x += card.Width + 10;
                }

                // Cập nhật kích thước container nếu cần
                if (cards.Length > 0)
                {
                    var totalWidth = cards.Length * (cards[0].Width + 10) + 10;
                    containerPanel.AutoScrollMinSize = new Size(totalWidth, containerPanel.Height);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi sắp xếp card ngang: {ex.Message}");
            }
        }

        /// <summary>
        /// Sắp xếp các card theo chiều dọc
        /// </summary>
        private void ArrangeCardsVertically(Panel containerPanel)
        {
            try
            {
                var cards = containerPanel.Controls.OfType<Panel>().OrderBy(p => (int)p.Tag).ToArray();
                var x = 10;
                var y = 10;

                foreach (var card in cards)
                {
                    card.Location = new Point(x, y);
                    y += card.Height + 10;
                }

                // Cập nhật kích thước container nếu cần
                if (cards.Length > 0)
                {
                    var totalHeight = cards.Length * (cards[0].Height + 10) + 10;
                    containerPanel.AutoScrollMinSize = new Size(containerPanel.Width, totalHeight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi sắp xếp card dọc: {ex.Message}");
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

        /// <summary>
        /// Tạo icon thời tiết từ emoji
        /// </summary>
        private Image GetWeatherIconFromEmoji(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 40);
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
