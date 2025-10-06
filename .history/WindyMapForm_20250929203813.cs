using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Form hiển thị bản đồ Windy phóng to trong cửa sổ riêng biệt
    /// </summary>
    public partial class WindyMapForm : Form
    {
        private WebView2? windyMap;
        private double latitude;
        private double longitude;
        private string windyApiKey;

        /// <summary>
        /// Khởi tạo form bản đồ Windy
        /// </summary>
        /// <param name="lat">Vĩ độ</param>
        /// <param name="lon">Kinh độ</param>
        /// <param name="apiKey">API key Windy</param>
        public WindyMapForm(double lat, double lon, string apiKey)
        {
            latitude = lat;
            longitude = lon;
            windyApiKey = apiKey;
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                InitializeWindyMap();
            }
        }

        private void BuildUi()
        {
            // Cấu hình form
            this.Text = "Bản đồ Gió Windy - Phóng to";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(800, 600);

            // Thêm menu bar
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Tệp");
            var refreshItem = new ToolStripMenuItem("Làm mới bản đồ", null, (s, e) => RefreshMap());
            var closeItem = new ToolStripMenuItem("Đóng", null, (s, e) => this.Close());

            fileMenu.DropDownItems.Add(refreshItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(closeItem);
            menuStrip.Items.Add(fileMenu);

            // Thêm menu View
            var viewMenu = new ToolStripMenuItem("Xem");
            var fullscreenItem = new ToolStripMenuItem("Toàn màn hình", null, (s, e) => ToggleFullscreen());
            var zoomInItem = new ToolStripMenuItem("Phóng to", null, (s, e) => ZoomIn());
            var zoomOutItem = new ToolStripMenuItem("Thu nhỏ", null, (s, e) => ZoomOut());

            viewMenu.DropDownItems.Add(fullscreenItem);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(zoomInItem);
            viewMenu.DropDownItems.Add(zoomOutItem);
            menuStrip.Items.Add(viewMenu);

            // Thêm menu Trợ giúp
            var helpMenu = new ToolStripMenuItem("Trợ giúp");
            var aboutItem = new ToolStripMenuItem("Giới thiệu", null, (s, e) => ShowAbout());
            helpMenu.DropDownItems.Add(aboutItem);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void InitializeWindyMap()
        {
            try
            {
                windyMap = new WebView2
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(0, 24), // Để tránh menu bar
                    Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 24),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };

                this.Controls.Add(windyMap);
                windyMap.BringToFront();
                
                // Load bản đồ Windy
                LoadWindyMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo bản đồ Windy: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadWindyMap()
        {
            if (windyMap == null) return;

            try
            {
                string latStr = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lonStr = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                
                // URL Windy với các tùy chọn nâng cao cho cửa sổ phóng to
                string embedUrl = $"https://embed.windy.com/embed2.html?" +
                    $"key={windyApiKey}&" +
                    $"lat={latStr}&lon={lonStr}&" +
                    $"detailLat={latStr}&detailLon={lonStr}&" +
                    $"zoom=8&" + // Zoom level cao hơn cho cửa sổ phóng to
                    $"overlay=temp&" +
                    $"level=surface&" +
                    $"menu=&" +
                    $"message=true&" +
                    $"marker=true&" +
                    $"calendar=&" +
                    $"pressure=true&" +
                    $"type=map&" +
                    $"location=coordinates&" +
                    $"detail=true&" +
                    $"metricWind=default&" +
                    $"metricTemp=default&" +
                    $"radarRange=-1"; // Hiển thị radar toàn cầu

                windyMap.Source = new Uri(embedUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải bản đồ Windy: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshMap()
        {
            LoadWindyMap();
        }

        private void ToggleFullscreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void ZoomIn()
        {
            // Windy map tự động xử lý zoom, chỉ cần refresh
            RefreshMap();
        }

        private void ZoomOut()
        {
            // Windy map tự động xử lý zoom, chỉ cần refresh
            RefreshMap();
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Bản đồ Gió Windy - Phiên bản phóng to\n\n" +
                "Cung cấp thông tin chi tiết về:\n" +
                "• Hướng và tốc độ gió\n" +
                "• Nhiệt độ\n" +
                "• Áp suất khí quyển\n" +
                "• Radar mưa\n\n" +
                "Dữ liệu được cung cấp bởi Windy.com",
                "Giới thiệu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // Điều chỉnh kích thước WebView2 khi form thay đổi kích thước
            if (windyMap != null)
            {
                windyMap.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 24);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Giải phóng tài nguyên WebView2
            if (windyMap != null)
            {
                windyMap.Dispose();
                windyMap = null;
            }
            base.OnFormClosing(e);
        }
    }
}
