using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace THOITIET.Services
{
    /// <summary>
    /// Service xử lý bản đồ Windy
    /// </summary>
    public class MapService
    {
        private const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
        private WebView2 windyView;

        public MapService()
        {
            windyView = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
        }

        /// <summary>
        /// Đảm bảo WebView2 đã được khởi tạo
        /// </summary>
        public void EnsureWindyBrowser(Control parent)
        {
            if (windyView != null && windyView.Parent == null)
            {
                parent.Controls.Add(windyView);
                windyView.BringToFront();
            }
        }

        /// <summary>
        /// Hiển thị bản đồ Windy
        /// </summary>
        public async Task ShowMapAsync(double lat, double lon, Control temperatureChart)
        {
            try
            {
                // Nếu chưa có tọa độ, lấy từ vị trí hiện tại
                if (lat == 0 && lon == 0)
                {
                    var locationData = await WeatherService.GetCurrentLocationAsync();
                    if (locationData?.Results?.Length > 0)
                    {
                        var result = locationData.Results[0];
                        lat = result.Lat;
                        lon = result.Lon;
                        System.Diagnostics.Debug.WriteLine($"Lấy tọa độ hiện tại cho bản đồ: {lat}, {lon}");
                    }
                    else
                    {
                        // Fallback về tọa độ mặc định (Hà Nội)
                        lat = 21.0285;
                        lon = 105.8542;
                    }
                }

                // Load bản đồ Windy
                LoadWindyMap(lat, lon);

                // Ẩn biểu đồ nhiệt độ
                if (temperatureChart != null)
                    temperatureChart.Visible = false;

                // Hiển thị bản đồ
                windyView.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Ẩn bản đồ và hiển thị biểu đồ
        /// </summary>
        public void ShowChart(Control temperatureChart)
        {
            if (temperatureChart != null) 
                temperatureChart.Visible = true;
            if (windyView != null) 
                windyView.Visible = false;
        }

        /// <summary>
        /// Load bản đồ Windy với tọa độ cụ thể
        /// </summary>
        private void LoadWindyMap(double lat, double lon)
        {
            if (windyView == null) return;

            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            
            string embedUrl = $"https://embed.windy.com/embed2.html?key={WINDY_API_KEY}&lat={latStr}&lon={lonStr}&detailLat={latStr}&detailLon={lonStr}&zoom=7&overlay=temp&level=surface&menu=&message=true&marker=true&calendar=&pressure=true&type=map&location=coordinates&detail=true&metricWind=default&metricTemp=default";
            
            windyView.Source = new Uri(embedUrl);
        }

        /// <summary>
        /// Cập nhật vị trí bản đồ
        /// </summary>
        public void UpdateMapLocation(double lat, double lon)
        {
            LoadWindyMap(lat, lon);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            windyView?.Dispose();
        }
    }
}