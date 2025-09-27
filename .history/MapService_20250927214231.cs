using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý bản đồ thời tiết
    /// 5️⃣ Bản đồ
    /// </summary>
    public class MapService
    {
        private WebView2? windyView;
        private const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";

        public MapService()
        {
        }

        #region Map Initialization

        /// <summary>
        /// Khởi tạo bản đồ Windy
        /// </summary>
        public void InitializeWindyMap(Panel containerPanel)
        {
            try
            {
                if (containerPanel == null) return;

                // Xóa các control cũ
                containerPanel.Controls.Clear();

                // Tạo WebView2 cho Windy
                windyView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };

                containerPanel.Controls.Add(windyView);

                // Đăng ký sự kiện khi WebView2 sẵn sàng
                windyView.NavigationCompleted += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("Windy map loaded successfully");
                };

                // Tải bản đồ Windy
                LoadWindyMap();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo bản đồ Windy: {ex.Message}");
            }
        }

        /// <summary>
        /// Tải bản đồ Windy
        /// </summary>
        public void LoadWindyMap()
        {
            try
            {
                if (windyView == null) return;

                // URL của Windy với API key
                var windyUrl = $"https://embed.windy.com/embed2.html?lat=21.5941&lon=105.8432&detailLat=21.5941&detailLon=105.8432&width=650&height=450&zoom=10&level=surface&overlay=wind&product=ecmwf&menu=&message=&marker=&calendar=now&pressure=&type=map&location=coordinates&detail=&metricWind=default&metricTemp=default&radarRange=-1";

                windyView.Source = new Uri(windyUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải bản đồ Windy: {ex.Message}");
            }
        }

        #endregion

        #region Map Updates

        /// <summary>
        /// Cập nhật vị trí trên bản đồ
        /// </summary>
        public void UpdateMapLocation(double lat, double lon)
        {
            try
            {
                if (windyView == null) return;

                // Tạo URL mới với vị trí mới
                var windyUrl = $"https://embed.windy.com/embed2.html?lat={lat}&lon={lon}&detailLat={lat}&detailLon={lon}&width=650&height=450&zoom=10&level=surface&overlay=wind&product=ecmwf&menu=&message=&marker=&calendar=now&pressure=&type=map&location=coordinates&detail=&metricWind=default&metricTemp=default&radarRange=-1";

                windyView.Source = new Uri(windyUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật vị trí bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật overlay trên bản đồ
        /// </summary>
        public void UpdateMapOverlay(string overlay)
        {
            try
            {
                if (windyView == null) return;

                // Các overlay có sẵn: wind, temp, rain, clouds, pressure
                var windyUrl = $"https://embed.windy.com/embed2.html?lat=21.5941&lon=105.8432&detailLat=21.5941&detailLon=105.8432&width=650&height=450&zoom=10&level=surface&overlay={overlay}&product=ecmwf&menu=&message=&marker=&calendar=now&pressure=&type=map&location=coordinates&detail=&metricWind=default&metricTemp=default&radarRange=-1";

                windyView.Source = new Uri(windyUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật overlay bản đồ: {ex.Message}");
            }
        }

        #endregion

        #region Map Controls

        /// <summary>
        /// Hiển thị bản đồ
        /// </summary>
        public void ShowMap(Panel containerPanel)
        {
            try
            {
                if (containerPanel == null) return;

                // Đảm bảo bản đồ được khởi tạo
                if (windyView == null)
                {
                    InitializeWindyMap(containerPanel);
                }

                // Hiển thị bản đồ
                containerPanel.Visible = true;
                containerPanel.BringToFront();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Ẩn bản đồ
        /// </summary>
        public void HideMap(Panel containerPanel)
        {
            try
            {
                if (containerPanel == null) return;

                containerPanel.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ẩn bản đồ: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Lấy thông tin thời tiết từ bản đồ
        /// </summary>
        public Task<string> GetWeatherInfoFromMap(double lat, double lon)
        {
            try
            {
                if (windyView == null) return Task.FromResult("Không có dữ liệu");

                // Có thể thêm logic để lấy thông tin thời tiết từ Windy API
                // Hiện tại trả về thông tin cơ bản
                return Task.FromResult($"Vị trí: {lat:F4}, {lon:F4}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy thông tin thời tiết từ bản đồ: {ex.Message}");
                return Task.FromResult("Lỗi khi lấy dữ liệu");
            }
        }

        /// <summary>
        /// Cập nhật marker trên bản đồ
        /// </summary>
        public void UpdateMapMarker(double lat, double lon, string locationName)
        {
            try
            {
                if (windyView == null) return;

                // Cập nhật marker với tên địa điểm
                var windyUrl = $"https://embed.windy.com/embed2.html?lat={lat}&lon={lon}&detailLat={lat}&detailLon={lon}&width=650&height=450&zoom=10&level=surface&overlay=wind&product=ecmwf&menu=&message=&marker={lat},{lon}&calendar=now&pressure=&type=map&location=coordinates&detail={locationName}&metricWind=default&metricTemp=default&radarRange=-1";

                windyView.Source = new Uri(windyUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật marker bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa marker trên bản đồ
        /// </summary>
        public void ClearMapMarker()
        {
            try
            {
                if (windyView == null) return;

                // Tải lại bản đồ không có marker
                LoadWindyMap();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa marker bản đồ: {ex.Message}");
            }
        }

        #endregion
    }
}