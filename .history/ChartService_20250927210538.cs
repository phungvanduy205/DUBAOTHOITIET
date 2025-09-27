using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý biểu đồ thời tiết
    /// 4️⃣ Biểu đồ
    /// </summary>
    public class ChartService
    {
        private bool isCelsius = true;

        public ChartService()
        {
        }

        #region Chart Management

        /// <summary>
        /// Hiển thị biểu đồ nhiệt độ 24 giờ
        /// </summary>
        public void ShowTemperatureChart24h(List<DuBaoTheoGioItem> hourlyList, Chart chartControl)
        {
            try
            {
                if (hourlyList == null || hourlyList.Count == 0 || chartControl == null)
                    return;

                // Xóa dữ liệu cũ
                chartControl.Series.Clear();
                chartControl.ChartAreas.Clear();

                // Tạo ChartArea
                var chartArea = new ChartArea("TemperatureArea")
                {
                    BackColor = Color.Transparent,
                    BorderColor = Color.White,
                    BorderWidth = 1
                };

                // Cấu hình trục X
                chartArea.AxisX.Title = "Thời gian (giờ)";
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
                chartArea.AxisX.Interval = 2; // Hiển thị mỗi 2 giờ

                // Cấu hình trục Y
                chartArea.AxisY.Title = isCelsius ? "Nhiệt độ (°C)" : "Nhiệt độ (°F)";
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);

                chartControl.ChartAreas.Add(chartArea);

                // Tạo series cho nhiệt độ
                var temperatureSeries = new Series("Nhiệt độ")
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.Orange,
                    BorderWidth = 3,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 6,
                    MarkerColor = Color.Orange
                };

                // Thêm dữ liệu
                var hoursToShow = hourlyList.Take(24).ToList();
                for (int i = 0; i < hoursToShow.Count; i++)
                {
                    var hour = hoursToShow[i];
                    var temp = isCelsius ? hour.NhietDo : (hour.NhietDo * 9.0 / 5.0 + 32);
                    var time = hour.ThoiGian.ToString("HH:mm");
                    
                    temperatureSeries.Points.AddXY(time, temp);
                }

                chartControl.Series.Add(temperatureSeries);

                // Cấu hình legend
                chartControl.Legends.Clear();
                var legend = new Legend("TemperatureLegend")
                {
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                chartControl.Legends.Add(legend);

                // Cấu hình title
                chartControl.Titles.Clear();
                var title = new Title("Biểu đồ nhiệt độ 24 giờ")
                {
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold)
                };
                chartControl.Titles.Add(title);

                // Hiển thị biểu đồ
                chartControl.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị biểu đồ 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị biểu đồ nhiệt độ 5 ngày
        /// </summary>
        public void ShowTemperatureChart5Days(List<DuBaoNgayItem> dailyList, Chart chartControl)
        {
            try
            {
                if (dailyList == null || dailyList.Length == 0 || chartControl == null)
                    return;

                // Xóa dữ liệu cũ
                chartControl.Series.Clear();
                chartControl.ChartAreas.Clear();

                // Tạo ChartArea
                var chartArea = new ChartArea("TemperatureArea")
                {
                    BackColor = Color.Transparent,
                    BorderColor = Color.White,
                    BorderWidth = 1
                };

                // Cấu hình trục X
                chartArea.AxisX.Title = "Ngày";
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);

                // Cấu hình trục Y
                chartArea.AxisY.Title = isCelsius ? "Nhiệt độ (°C)" : "Nhiệt độ (°F)";
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);

                chartControl.ChartAreas.Add(chartArea);

                // Tạo series cho nhiệt độ cao nhất
                var maxTempSeries = new Series("Nhiệt độ cao nhất")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.Red,
                    BorderWidth = 2
                };

                // Tạo series cho nhiệt độ thấp nhất
                var minTempSeries = new Series("Nhiệt độ thấp nhất")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.Blue,
                    BorderWidth = 2
                };

                // Thêm dữ liệu
                var daysToShow = dailyList.Take(5).ToArray();
                for (int i = 0; i < daysToShow.Length; i++)
                {
                    var day = daysToShow[i];
                    var maxTemp = isCelsius ? day.Temp.Max : (day.Temp.Max * 9.0 / 5.0 + 32);
                    var minTemp = isCelsius ? day.Temp.Min : (day.Temp.Min * 9.0 / 5.0 + 32);
                    var dayName = DateTimeOffset.FromUnixTimeSeconds(day.Dt).ToString("ddd");
                    
                    maxTempSeries.Points.AddXY(dayName, maxTemp);
                    minTempSeries.Points.AddXY(dayName, minTemp);
                }

                chartControl.Series.Add(maxTempSeries);
                chartControl.Series.Add(minTempSeries);

                // Cấu hình legend
                chartControl.Legends.Clear();
                var legend = new Legend("TemperatureLegend")
                {
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };
                chartControl.Legends.Add(legend);

                // Cấu hình title
                chartControl.Titles.Clear();
                var title = new Title("Biểu đồ nhiệt độ 5 ngày")
                {
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold)
                };
                chartControl.Titles.Add(title);

                // Hiển thị biểu đồ
                chartControl.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị biểu đồ 5 ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị biểu đồ
        /// </summary>
        public void ShowChart(Chart chartControl)
        {
            try
            {
                if (chartControl != null)
                {
                    chartControl.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị biểu đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Ẩn biểu đồ
        /// </summary>
        public void HideChart(Chart chartControl)
        {
            try
            {
                if (chartControl != null)
                {
                    chartControl.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ẩn biểu đồ: {ex.Message}");
            }
        }

        #endregion

        #region Chart Configuration

        /// <summary>
        /// Cấu hình biểu đồ cơ bản
        /// </summary>
        public void ConfigureChart(Chart chartControl)
        {
            try
            {
                if (chartControl == null) return;

                // Cấu hình màu nền
                chartControl.BackColor = Color.Transparent;

                // Cấu hình border
                chartControl.BorderlineColor = Color.White;
                chartControl.BorderlineWidth = 1;

                // Cấu hình padding
                chartControl.Padding = new Padding(10);

                // Cấu hình anti-aliasing
                chartControl.AntiAliasing = AntiAliasingStyles.All;
                chartControl.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cấu hình biểu đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật đơn vị nhiệt độ
        /// </summary>
        public void UpdateTemperatureUnit(bool isCelsius)
        {
            this.isCelsius = isCelsius;
        }

        #endregion

        #region Chart Data Processing

        /// <summary>
        /// Xử lý dữ liệu nhiệt độ cho biểu đồ 24h
        /// </summary>
        private (string[] labels, double[] values) ProcessHourlyData(HourlyWeather[] hourlyList)
        {
            try
            {
                var hoursToShow = hourlyList.Take(24).ToArray();
                var labels = new string[hoursToShow.Length];
                var values = new double[hoursToShow.Length];

                for (int i = 0; i < hoursToShow.Length; i++)
                {
                    var hour = hoursToShow[i];
                    labels[i] = DateTimeOffset.FromUnixTimeSeconds(hour.Dt).ToString("HH:mm");
                    values[i] = isCelsius ? hour.Temp : (hour.Temp * 9.0 / 5.0 + 32);
                }

                return (labels, values);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý dữ liệu hourly: {ex.Message}");
                return (new string[0], new double[0]);
            }
        }

        /// <summary>
        /// Xử lý dữ liệu nhiệt độ cho biểu đồ 5 ngày
        /// </summary>
        private (string[] labels, double[] maxValues, double[] minValues) ProcessDailyData(DailyWeather[] dailyList)
        {
            try
            {
                var daysToShow = dailyList.Take(5).ToArray();
                var labels = new string[daysToShow.Length];
                var maxValues = new double[daysToShow.Length];
                var minValues = new double[daysToShow.Length];

                for (int i = 0; i < daysToShow.Length; i++)
                {
                    var day = daysToShow[i];
                    labels[i] = DateTimeOffset.FromUnixTimeSeconds(day.Dt).ToString("ddd");
                    maxValues[i] = isCelsius ? day.Temp.Max : (day.Temp.Max * 9.0 / 5.0 + 32);
                    minValues[i] = isCelsius ? day.Temp.Min : (day.Temp.Min * 9.0 / 5.0 + 32);
                }

                return (labels, maxValues, minValues);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý dữ liệu daily: {ex.Message}");
                return (new string[0], new double[0], new double[0]);
            }
        }

        #endregion

        #region Chart Styling

        /// <summary>
        /// Áp dụng style cho biểu đồ
        /// </summary>
        public void ApplyChartStyle(Chart chartControl)
        {
            try
            {
                if (chartControl == null) return;

                // Style cho tất cả series
                foreach (Series series in chartControl.Series)
                {
                    series.BorderWidth = 2;
                    series.ShadowOffset = 2;
                }

                // Style cho tất cả chart areas
                foreach (ChartArea area in chartControl.ChartAreas)
                {
                    area.BackColor = Color.Transparent;
                    area.BorderColor = Color.White;
                    area.BorderWidth = 1;
                    
                    // Style cho grid
                    area.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
                    area.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi áp dụng style biểu đồ: {ex.Message}");
            }
        }

        #endregion
    }
}
