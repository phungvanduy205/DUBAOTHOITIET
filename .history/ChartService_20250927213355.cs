using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace THOITIET
{
    public class ChartService
    {
        private bool isCelsius = true;

        public ChartService()
        {
        }

        public void ShowTemperatureChart24h(List<DuBaoTheoGioItem> hourlyData, Chart chart, bool isCelsius)
        {
            try
            {
                this.isCelsius = isCelsius;
                
                if (chart == null || hourlyData == null || hourlyData.Count == 0)
                    return;

                chart.Series.Clear();
                chart.ChartAreas.Clear();

                var chartArea = new ChartArea("TemperatureArea")
                {
                    BackColor = Color.Transparent,
                    BorderColor = Color.White,
                    BorderWidth = 1
                };

                chartArea.AxisX.Title = "Thời gian";
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
                chartArea.AxisX.Interval = 1;

                chartArea.AxisY.Title = isCelsius ? "Nhiệt độ (°C)" : "Nhiệt độ (°F)";
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);

                chart.ChartAreas.Add(chartArea);

                var temperatureSeries = new Series("Nhiệt độ")
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.Orange,
                    BorderWidth = 3,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 6,
                    MarkerColor = Color.Orange
                };

                foreach (var hourData in hourlyData.Take(24))
                {
                    var temp = isCelsius ? hourData.NhietDo : (hourData.NhietDo * 9.0 / 5.0 + 32);
                    temperatureSeries.Points.AddXY(hourData.ThoiGian.ToString("HH:mm"), temp);
                }

                chart.Series.Add(temperatureSeries);

                chart.BackColor = Color.Transparent;
                chart.Titles.Clear();
                chart.Titles.Add(new Title("Nhiệt độ 24 giờ", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.White));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo biểu đồ nhiệt độ 24h: {ex.Message}");
            }
        }

        public void ShowTemperatureChart5Days(List<DuBaoNgayItem> dailyData, Chart chart, bool isCelsius)
        {
            try
            {
                this.isCelsius = isCelsius;
                
                if (chart == null || dailyData == null || dailyData.Count == 0)
                    return;

                chart.Series.Clear();
                chart.ChartAreas.Clear();

                var chartArea = new ChartArea("TemperatureArea")
                {
                    BackColor = Color.Transparent,
                    BorderColor = Color.White,
                    BorderWidth = 1
                };

                chartArea.AxisX.Title = "Ngày";
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);
                chartArea.AxisX.Interval = 1;

                chartArea.AxisY.Title = isCelsius ? "Nhiệt độ (°C)" : "Nhiệt độ (°F)";
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(50, 255, 255, 255);

                chart.ChartAreas.Add(chartArea);

                var highTempSeries = new Series("Nhiệt độ cao")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.Red,
                    BorderWidth = 2
                };

                var lowTempSeries = new Series("Nhiệt độ thấp")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.Blue,
                    BorderWidth = 2
                };

                foreach (var dayData in dailyData.Take(5))
                {
                    var highTemp = isCelsius ? dayData.NhietDoCao : (dayData.NhietDoCao * 9.0 / 5.0 + 32);
                    var lowTemp = isCelsius ? dayData.NhietDoThap : (dayData.NhietDoThap * 9.0 / 5.0 + 32);
                    
                    highTempSeries.Points.AddXY(dayData.Ngay.ToString("dd/MM"), highTemp);
                    lowTempSeries.Points.AddXY(dayData.Ngay.ToString("dd/MM"), lowTemp);
                }

                chart.Series.Add(highTempSeries);
                chart.Series.Add(lowTempSeries);

                chart.BackColor = Color.Transparent;
                chart.Titles.Clear();
                chart.Titles.Add(new Title("Nhiệt độ 5 ngày", Docking.Top, new Font("Segoe UI", 12, FontStyle.Bold), Color.White));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo biểu đồ nhiệt độ 5 ngày: {ex.Message}");
            }
        }
    }
}