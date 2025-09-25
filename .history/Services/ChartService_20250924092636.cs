using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace THOITIET.Services
{
    /// <summary>
    /// Service xử lý biểu đồ nhiệt độ
    /// </summary>
    public class ChartService
    {
        /// <summary>
        /// Khởi tạo biểu đồ nhiệt độ
        /// </summary>
        public static Chart InitializeTemperatureChart(Size size)
        {
            var chart = new Chart
            {
                Size = size,
                BackColor = Color.FromArgb(240, 248, 255),
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };

            // Tạo ChartArea
            var chartArea = new ChartArea("MainArea")
            {
                BackColor = Color.FromArgb(255, 255, 255),
                BackSecondaryColor = Color.FromArgb(240, 248, 255),
                BackGradientStyle = GradientStyle.TopBottom,
                BorderColor = Color.FromArgb(200, 200, 200),
                BorderWidth = 1,
                Position = new ElementPosition(5, 10, 90, 85),
                InnerPlotPosition = new ElementPosition(8, 5, 87, 90)
            };

            // Cấu hình trục X
            chartArea.AxisX.Title = "Giờ";
            chartArea.AxisX.TitleFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 7F);
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(70, 70, 70);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisX.LineColor = Color.FromArgb(180, 180, 180);
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 23;

            // Cấu hình trục Y
            chartArea.AxisY.Title = "Nhiệt độ";
            chartArea.AxisY.TitleFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(70, 70, 70);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisY.LineColor = Color.FromArgb(180, 180, 180);

            chart.ChartAreas.Add(chartArea);

            // Tạo Legend
            var legend = new Legend("MainLegend")
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Center,
                ForeColor = Color.FromArgb(60, 60, 60),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8F),
                Position = new ElementPosition(5, 2, 90, 8)
            };
            chart.Legends.Add(legend);

            return chart;
        }

        /// <summary>
        /// Hiển thị biểu đồ nhiệt độ cho ngày được chọn
        /// </summary>
        public static void Show24hChartForDay(Chart chart, DailyWeather daily, HourlyWeather[] hourlyData, bool isCelsius)
        {
            try
            {
                // Lọc dữ liệu theo ngày được chọn
                var targetDate = UnixToLocal(daily.Dt).Date;
                var hourlyForDay = hourlyData
                    .Where(h => UnixToLocal(h.Dt).Date == targetDate)
                    .OrderBy(h => h.Dt)
                    .ToArray();

                if (hourlyForDay.Length == 0) return;

                // Xóa series cũ
                chart.Series.Clear();

                // Tạo series nhiệt độ thực tế
                var tempSeries = new Series("Nhiệt độ thực tế")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(70, 130, 180),
                    BorderWidth = 1,
                    BorderColor = Color.FromArgb(50, 100, 150),
                    IsValueShownAsLabel = true,
                    LabelFormat = "0°",
                    LabelForeColor = Color.FromArgb(50, 50, 50),
                    LabelFont = new Font("Segoe UI", 7F),
                    PointWidth = 0.6
                };

                // Tạo series cảm giác như
                var feelsLikeSeries = new Series("Cảm giác như")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(255, 140, 0),
                    BorderWidth = 1,
                    BorderColor = Color.FromArgb(200, 100, 0),
                    IsValueShownAsLabel = true,
                    LabelFormat = "0°",
                    LabelForeColor = Color.FromArgb(50, 50, 50),
                    LabelFont = new Font("Segoe UI", 7F),
                    PointWidth = 0.6
                };

                // Thêm dữ liệu
                foreach (var hour in hourlyForDay)
                {
                    var hourTime = UnixToLocal(hour.Dt);
                    var hourValue = hourTime.Hour;
                    
                    var temp = isCelsius ? hour.Temp : (hour.Temp * 9.0 / 5.0 + 32);
                    var feelsLike = isCelsius ? hour.FeelsLike : (hour.FeelsLike * 9.0 / 5.0 + 32);

                    tempSeries.Points.AddXY(hourValue, temp);
                    feelsLikeSeries.Points.AddXY(hourValue, feelsLike);
                }

                // Thêm series vào chart
                chart.Series.Add(tempSeries);
                chart.Series.Add(feelsLikeSeries);

                // Cập nhật tiêu đề
                chart.Titles.Clear();
                var title = new Title($"Biểu đồ nhiệt độ 24h - {targetDate:dd/MM/yyyy}")
                {
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                chart.Titles.Add(title);

                // Cập nhật trục Y
                var chartArea = chart.ChartAreas[0];
                var allTemps = hourlyForDay.SelectMany(h => new[] { h.Temp, h.FeelsLike });
                var minTemp = allTemps.Min();
                var maxTemp = allTemps.Max();
                var padding = (maxTemp - minTemp) * 0.1;

                chartArea.AxisY.Minimum = Math.Max(0, minTemp - padding);
                chartArea.AxisY.Maximum = maxTemp + padding;
                chartArea.AxisY.Interval = Math.Max(1, (maxTemp - minTemp) / 8);

                // Cập nhật đơn vị
                var unit = isCelsius ? "°C" : "°F";
                chartArea.AxisY.Title = $"Nhiệt độ ({unit})";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị biểu đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Chuyển đổi Unix timestamp sang DateTime local
        /// </summary>
        private static DateTime UnixToLocal(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToLocalTime();
        }

        /// <summary>
        /// Xuất biểu đồ ra file hình ảnh
        /// </summary>
        public static void ExportChart(Chart chart, string fileName = "temperature_chart.png")
        {
            try
            {
                chart.SaveImage(fileName, ChartImageFormat.Png);
                System.Windows.Forms.MessageBox.Show($"Đã xuất biểu đồ ra file: {fileName}", "Thành công", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi xuất biểu đồ: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}