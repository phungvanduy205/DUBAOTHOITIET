using System;
using System.Threading.Tasks;
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
        private const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
        private WebView2? windyView;

        public MapService()
        {
        }

        #region Map Management

        /// <summary>
        /// Hiển thị bản đồ
        /// </summary>
        public async void ShowMap(WebView2 mapControl)
        {
            try
            {
                await EnsureWindyBrowser(mapControl);
                if (mapControl != null)
                {
                    mapControl.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Ẩn bản đồ
        /// </summary>
        public void HideMap(WebView2 mapControl)
        {
            try
            {
                if (mapControl != null)
                {
                    mapControl.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ẩn bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Tải bản đồ Windy với tọa độ cụ thể
        /// </summary>
        public async void LoadWindyMap(double lat, double lon, WebView2 mapControl)
        {
            try
            {
                await EnsureWindyBrowser(mapControl);
                if (mapControl == null) return;

                // Tạo URL cho Windy.com
                var windyUrl = $"https://embed.windy.com/embed2.html?lat={lat}&lon={lon}&detailLat={lat}&detailLon={lon}&width=650&height=450&zoom=10&level=surface&overlay=wind&product=ecmwf&menu=&message=&marker=&calendar=now&pressure=&type=map&location=coordinates&detail=&metricWind=default&metricTemp=default&radarRange=-1";

                // Navigate đến URL
                await mapControl.EnsureCoreWebView2Async();
                mapControl.CoreWebView2.Navigate(windyUrl);

                System.Diagnostics.Debug.WriteLine($"Đã tải bản đồ Windy tại: {lat}, {lon}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải bản đồ Windy: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật marker theo vị trí
        /// </summary>
        public async void UpdateMarkerByLocation(double lat, double lon, WebView2 mapControl)
        {
            try
            {
                if (mapControl == null) return;

                await mapControl.EnsureCoreWebView2Async();
                
                // JavaScript để cập nhật marker
                var script = $@"
                    if (window.windy && window.windy.setView) {{
                        window.windy.setView([{lat}, {lon}], 10);
                    }}
                ";

                await mapControl.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"Đã cập nhật marker tại: {lat}, {lon}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật marker: {ex.Message}");
            }
        }

        #endregion

        #region Browser Initialization

        /// <summary>
        /// Đảm bảo WebView2 browser được khởi tạo
        /// </summary>
        private async Task EnsureWindyBrowser(WebView2 mapControl)
        {
            try
            {
                if (mapControl == null) return;

                // Kiểm tra xem CoreWebView2 đã được khởi tạo chưa
                if (mapControl.CoreWebView2 == null)
                {
                    // Khởi tạo CoreWebView2
                    await mapControl.EnsureCoreWebView2Async();
                }

                // Cấu hình WebView2
                if (mapControl.CoreWebView2 != null)
                {
                    // Bật JavaScript
                    mapControl.CoreWebView2.Settings.IsJavaScriptEnabled = true;
                    
                    // Bật WebGL
                    mapControl.CoreWebView2.Settings.AreWebGLEnabled = true;
                    
                    // Bật DOM Storage
                    mapControl.CoreWebView2.Settings.IsWebMessageEnabled = true;
                    
                    // Cấu hình User Agent
                    mapControl.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36";
                }

                windyView = mapControl;
                System.Diagnostics.Debug.WriteLine("WebView2 browser đã được khởi tạo thành công");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo WebView2: {ex.Message}");
            }
        }

        #endregion

        #region Map Configuration

        /// <summary>
        /// Cấu hình bản đồ
        /// </summary>
        public void ConfigureMap(WebView2 mapControl)
        {
            try
            {
                if (mapControl == null) return;

                // Cấu hình kích thước
                mapControl.Size = new System.Drawing.Size(650, 450);
                
                // Cấu hình vị trí
                mapControl.Location = new System.Drawing.Point(10, 10);
                
                // Cấu hình anchor
                mapControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                
                // Cấu hình border
                mapControl.BorderStyle = BorderStyle.FixedSingle;
                
                System.Diagnostics.Debug.WriteLine("Bản đồ đã được cấu hình");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cấu hình bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Thiết lập chế độ hiển thị bản đồ
        /// </summary>
        public void SetMapDisplayMode(WebView2 mapControl, MapDisplayMode mode)
        {
            try
            {
                if (mapControl == null) return;

                switch (mode)
                {
                    case MapDisplayMode.Wind:
                        LoadWindyMap(21.5941, 105.8432, mapControl); // Default to Thai Nguyen
                        break;
                    case MapDisplayMode.Temperature:
                        // Có thể thêm logic cho temperature map
                        break;
                    case MapDisplayMode.Precipitation:
                        // Có thể thêm logic cho precipitation map
                        break;
                    default:
                        LoadWindyMap(21.5941, 105.8432, mapControl);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thiết lập chế độ bản đồ: {ex.Message}");
            }
        }

        #endregion

        #region Map Events

        /// <summary>
        /// Xử lý sự kiện click trên bản đồ
        /// </summary>
        public void HandleMapClick(WebView2 mapControl, EventHandler<MapClickEventArgs> clickHandler)
        {
            try
            {
                if (mapControl == null) return;

                // Đăng ký sự kiện click
                mapControl.Click += (sender, e) =>
                {
                    // Có thể thêm logic xử lý click ở đây
                    System.Diagnostics.Debug.WriteLine("Bản đồ được click");
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý sự kiện click bản đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi vị trí trên bản đồ
        /// </summary>
        public void HandleMapLocationChange(WebView2 mapControl, EventHandler<MapLocationChangeEventArgs> locationChangeHandler)
        {
            try
            {
                if (mapControl == null) return;

                // Có thể thêm logic xử lý thay đổi vị trí ở đây
                System.Diagnostics.Debug.WriteLine("Vị trí bản đồ đã thay đổi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý sự kiện thay đổi vị trí: {ex.Message}");
            }
        }

        #endregion

        #region Map Utilities

        /// <summary>
        /// Lấy tọa độ hiện tại từ bản đồ
        /// </summary>
        public async Task<(double lat, double lon)> GetCurrentMapLocation(WebView2 mapControl)
        {
            try
            {
                if (mapControl == null || mapControl.CoreWebView2 == null)
                    return (0, 0);

                // JavaScript để lấy tọa độ hiện tại
                var script = @"
                    if (window.windy && window.windy.getView) {
                        var view = window.windy.getView();
                        return JSON.stringify({lat: view.lat, lon: view.lon});
                    }
                    return JSON.stringify({lat: 0, lon: 0});
                ";

                var result = await mapControl.CoreWebView2.ExecuteScriptAsync(script);
                
                // Parse kết quả
                if (!string.IsNullOrEmpty(result) && result != "null")
                {
                    var cleanResult = result.Trim('"').Replace("\\", "");
                    var parts = cleanResult.Split(',');
                    if (parts.Length >= 2)
                    {
                        if (double.TryParse(parts[0].Split(':')[1], out double lat) &&
                            double.TryParse(parts[1].Split(':')[1].Replace("}", ""), out double lon))
                        {
                            return (lat, lon);
                        }
                    }
                }

                return (0, 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy tọa độ bản đồ: {ex.Message}");
                return (0, 0);
            }
        }

        /// <summary>
        /// Thiết lập zoom level cho bản đồ
        /// </summary>
        public async void SetMapZoom(WebView2 mapControl, int zoomLevel)
        {
            try
            {
                if (mapControl == null || mapControl.CoreWebView2 == null) return;

                var script = $@"
                    if (window.windy && window.windy.setView) {{
                        var currentView = window.windy.getView();
                        window.windy.setView([currentView.lat, currentView.lon], {zoomLevel});
                    }}
                ";

                await mapControl.CoreWebView2.ExecuteScriptAsync(script);
                System.Diagnostics.Debug.WriteLine($"Đã thiết lập zoom level: {zoomLevel}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thiết lập zoom: {ex.Message}");
            }
        }

        #endregion
    }

    #region Enums and Event Args

    /// <summary>
    /// Chế độ hiển thị bản đồ
    /// </summary>
    public enum MapDisplayMode
    {
        Wind,
        Temperature,
        Precipitation,
        Pressure
    }

    /// <summary>
    /// Event args cho sự kiện click bản đồ
    /// </summary>
    public class MapClickEventArgs : EventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public System.Drawing.Point ClickPoint { get; set; }
    }

    /// <summary>
    /// Event args cho sự kiện thay đổi vị trí bản đồ
    /// </summary>
    public class MapLocationChangeEventArgs : EventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int ZoomLevel { get; set; }
    }

    #endregion
}
