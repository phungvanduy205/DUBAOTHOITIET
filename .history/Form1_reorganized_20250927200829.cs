using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Web.WebView2.WinForms;

namespace THOITIET
{
    /// <summary>
    /// Form chính: xử lý sự kiện, gọi dịch vụ, cập nhật giao diện
    /// </summary>
    public partial class Form1 : Form
    {
        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;

        // Dữ liệu thời tiết từ API
        private OneCallResponse weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;

        // Kết nối DB lưu địa điểm
        private readonly string sqlConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=THOITIET;Trusted_Connection=True;TrustServerCertificate=True";
        private LocationRepository? locationRepo;

        // Danh sách địa điểm đã lưu (giữ để binding UI, nguồn lấy từ DB)
        private List<SavedLocation> savedLocations = new List<SavedLocation>();
        // private const string SAVED_LOCATIONS_FILE = "saved_locations.txt"; // Deprecated: dùng DB

        // Timer tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();

        // Bộ nhớ tạm dữ liệu để xuất CSV

        // Các fields mới cho tính năng nâng cao
        private PictureBox? backgroundPictureBox;
        private Chart? temperatureChart;
        private WebView2? windyView;
        private TabControl? tabChartMap;
        private const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
        private List<FavoriteLocation> favoriteLocations = new List<FavoriteLocation>();
        private int selectedDayIndex = 0; // Ngày được chọn trong dự báo 5 ngày

        // Throttle nền: lưu trạng thái lần trước
        private int? lastWeatherId = null;
        private bool? lastIsNight = null;

        // UI segmented runtime (không dùng nữa khi có UnitToggle designer)
        // Các biến btnC, btnF, donViSegment đã được xóa vì giờ dùng UnitToggle trong Designer
        
        // Lưu địa điểm
        private List<string> savedLocationNames = new List<string>();
        private int currentLocationIndex = 0;
        private string locationsFilePath = "saved_locations.json";

        public Form1()
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR START ===");
            InitializeComponent();
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();

            // Không tạo segmented runtime nữa (đã có UnitToggle trong Designer)
            
            // Đồng bộ hóa donViCelsius với unitToggle.IsCelsius
            donViCelsius = unitToggle.IsCelsius;
            
            // Đăng ký event UnitChanged để cập nhật hiển thị từ dữ liệu Kelvin
            unitToggle.UnitChanged += async (sender, isCelsius) => {
                donViCelsius = isCelsius;
                System.Diagnostics.Debug.WriteLine($"UnitToggle changed to: {(isCelsius ? "Celsius" : "Fahrenheit")}");
                if (weatherData != null)
                    await CapNhatThoiTiet();
            };
            
            // Bo tròn thanh tìm kiếm
            this.Load += (s, e) => {
                ApplyRoundedCorners(oTimKiemDiaDiem, 10);
                ApplyRoundedCorners(khung24Gio, 15);
                ApplyRoundedCorners(khung5Ngay, 15);
            };
            
            // Khởi tạo DB lưu địa điểm
            try
            {
                locationRepo = new LocationRepository(sqlConnectionString);
                locationRepo.EnsureCreated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureCreated DB error: {ex.Message}");
            }

            NapDiaDiemDaLuu();

            // Xóa panel gợi ý cũ nếu có
            var oldSuggestionPanel = Controls.Find("suggestionPanel", true).FirstOrDefault();
            if (oldSuggestionPanel != null)
            {
                Controls.Remove(oldSuggestionPanel);
                oldSuggestionPanel.Dispose();
            }

            // Tạo background động
            InitializeBackgroundPictureBox();
            
            // Set background mặc định ngay khi khởi động dựa trên thời gian hiện tại
            System.Diagnostics.Debug.WriteLine("Calling SetDefaultBackgroundOnStartup...");
            SetDefaultBackgroundOnStartup();

            // Tạo nội dung cho các panel chi tiết
            TaoNoiDungPanelChiTiet();

            // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
            _ = LoadInitialWeatherData();


            // Tạo file icon thật
            TaoFileIconThuc();

            // Không đặt địa điểm mặc định - để trống cho đến khi API load

            // Xóa gợi ý tìm kiếm
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR END ===");
        }

        #region ===== 1️⃣ NHẬP ĐỊA ĐIỂM, TÌM KIẾM, LƯU ĐỊA ĐIỂM, ĐỔI °C/°F =====

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD START ===");
            // Khởi tạo dữ liệu ban đầu
            CapNhatThoiGian();
            
            // Load danh sách địa điểm đã lưu từ DB
            NapDiaDiemDaLuu();
            
            // Tự động load thời tiết vị trí hiện tại khi khởi động
            LoadWeatherByIP();
            
            // Test background ngay lập tức
            System.Diagnostics.Debug.WriteLine("Calling TestBackground...");
            TestBackground();
            
            // Force set background ngay trong Form1_Load
            System.Diagnostics.Debug.WriteLine("Calling ForceSetBackgroundInLoad...");
            ForceSetBackgroundInLoad();
            
            // Đảm bảo "📍 Vị trí hiện tại" luôn có trong danh sách
            EnsureCurrentLocationInList();
            
            System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD END ===");
        }

        private async void NutTimKiem_Click(object? sender, EventArgs e)
        {
            var tuKhoa = oTimKiemDiaDiem.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                MessageBox.Show("Vui lòng nhập xã/phường, quận/huyện, tỉnh/thành để tìm kiếm.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await TimKiemDiaDiem(tuKhoa);
        }

        private void listBoxDiaDiemDaLuu_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuKienChonDiaDiemDaLuu();
        }
        #endregion

        #region ===== 2️⃣ GỌI API & THÔNG TIN MÔ TẢ =====
        #endregion

        #region ===== 3️⃣ THỜI TIẾT 24H & 5 NGÀY =====
        #endregion

        #region ===== 4️⃣ BIỂU ĐỒ =====
        #endregion

        #region ===== 5️⃣ BẢN ĐỒ =====

        private void LoadWindyMap(double lat, double lon)
        {
            EnsureWindyBrowser();
            if (windyView == null) return;

            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string embedUrl = $"https://embed.windy.com/embed2.html?key={WINDY_API_KEY}&lat={latStr}&lon={lonStr}&detailLat={latStr}&detailLon={lonStr}&zoom=7&overlay=temp&level=surface&menu=&message=true&marker=true&calendar=&pressure=true&type=map&location=coordinates&detail=true&metricWind=default&metricTemp=default";
            windyView.Source = new Uri(embedUrl);
        }
        #endregion

        #region ===== 6️⃣ BACKGROUND THAY ĐỔI THEO THỜI TIẾT =====

        private void InitializeBackgroundPictureBox()
        {
            // Không cần tạo PictureBox riêng biệt nữa
            // Background sẽ được set trực tiếp cho boCucChinh
            System.Diagnostics.Debug.WriteLine("Đã khởi tạo background system cho boCucChinh");
        }

        private void ApplyRoundedCorners(Control control, int radius)
        {
            try
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(control.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(control.Width - radius * 2, control.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, control.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseAllFigures();
                control.Region = new System.Drawing.Region(path);
            }
            catch
            {
                // Nếu không thể tạo region, bỏ qua
            }
        }

        private void CauHinhKhoiTao()
        {
            // Timer 1 giờ
            dongHoCapNhat.Interval = 60 * 60 * 1000;
            dongHoCapNhat.Tick += async (s, e) => { await CapNhatThoiTiet(); };

            // Timer cập nhật thời gian mỗi giây
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => CapNhatThoiGian();
            timer.Start();

            // Cập nhật thời gian lần đầu
            CapNhatThoiGian();
        }

        private void thanhTrenCung_Paint(object sender, PaintEventArgs e)
        {

        }

        private void anhNenDong_Click(object sender, EventArgs e)
        {

        }
        #endregion

        #region ===== CÁC METHOD KHÁC =====

        private void SetTransparentBackColor(Control control, Color color)
        {
            try
            {
                control.BackColor = color;
            }
            catch (ArgumentException)
            {
                // Nếu control không hỗ trợ trong suốt, dùng màu trắng mờ
                control.BackColor = Color.FromArgb(240, 240, 240);
            }
        }

        private void EnsureCurrentLocationInList()
        {
            System.Diagnostics.Debug.WriteLine("=== EnsureCurrentLocationInList START ===");
            System.Diagnostics.Debug.WriteLine($"ListBox items count: {listBoxDiaDiemDaLuu.Items.Count}");
            
            if (!listBoxDiaDiemDaLuu.Items.Contains("📍 Vị trí hiện tại"))
            {
                listBoxDiaDiemDaLuu.Items.Insert(0, "📍 Vị trí hiện tại");
                System.Diagnostics.Debug.WriteLine("✅ Added '📍 Vị trí hiện tại' to list");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⏭️ '📍 Vị trí hiện tại' already exists in list");
            }
            
            System.Diagnostics.Debug.WriteLine($"ListBox items count after: {listBoxDiaDiemDaLuu.Items.Count}");
            System.Diagnostics.Debug.WriteLine("=== EnsureCurrentLocationInList END ===");
        }

        private void CapNhatDanhSachDiaDiem()
        {
            listBoxDiaDiemDaLuu.Items.Clear();
            
            foreach (var location in savedLocationNames)
            {
                listBoxDiaDiemDaLuu.Items.Add(location);
            }
        }

        private void TaoNutDongForm()
        {
            var nutDong = new Button
            {
                Text = "✕",
                Size = new Size(30, 30),
                Location = new Point(this.Width - 35, 5),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(200, 255, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            nutDong.FlatAppearance.BorderSize = 0;
            nutDong.Click += (s, e) => this.Close();

            this.Controls.Add(nutDong);
            nutDong.BringToFront();
        }

        private async void CongTacDonVi_Click(object? sender, EventArgs e)
        {
            // Đảo ngược trạng thái đơn vị
            donViCelsius = !donViCelsius;

            // Cập nhật text của button
            CongTacDonVi.Text = donViCelsius ? "°C" : "°F";

            await CapNhatThoiTiet();
        }

        private void TaoDuLieuMau5Ngay()
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangNhieuNgay.Controls.Clear();
        }

        private void TaoDuLieuMau24Gio()
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangTheoGio.Controls.Clear();
        }

        private void EnsureWindyBrowser()
        {
            if (windyView != null) return;

            windyView = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = false
            };

            // Thêm vào tabMap khi đã khởi tạo từ Designer
            tabMap.Controls.Add(windyView);
            windyView.BringToFront();
        }

        private void ShowChart()
        {
            if (temperatureChart != null) temperatureChart.Visible = true;
            if (windyView != null) windyView.Visible = false;
        }

        private void ConvertDailyHighLowLabel(Label label, string newUnitSymbol)
        {
            // Dạng: "Cao nhất: 30°C\nThấp nhất: 24°C" (hỗ trợ cả º và chữ thường, dấu phẩy)
            var text = label.Text;
            var regex = new System.Text.RegularExpressions.Regex(
                @"Cao\s*nhất:\s*(-?\d+(?:[\.,]\d+)?)\s*[°º]\s*([cCfF]).*?Thấp\s*nhất:\s*(-?\d+(?:[\.,]\d+)?)\s*[°º]\s*([cCfF])",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            var m = regex.Match(text);
            if (!m.Success) return;

            var highText = m.Groups[1].Value.Replace(',', '.');
            double high = double.Parse(highText, System.Globalization.CultureInfo.InvariantCulture);
            bool highIsC = m.Groups[2].Value.Equals("c", StringComparison.OrdinalIgnoreCase);
            var lowText = m.Groups[3].Value.Replace(',', '.');
            double low = double.Parse(lowText, System.Globalization.CultureInfo.InvariantCulture);
            bool lowIsC = m.Groups[4].Value.Equals("c", StringComparison.OrdinalIgnoreCase);

            double highConv = ConvertTemperatureFromText(high.ToString(System.Globalization.CultureInfo.InvariantCulture), highIsC);
            double lowConv = ConvertTemperatureFromText(low.ToString(System.Globalization.CultureInfo.InvariantCulture), lowIsC);

            // Duy trì cùng định dạng 2 dòng
            label.Text = $"Cao nhất: {Math.Round(highConv)}{newUnitSymbol}\nThấp nhất: {Math.Round(lowConv)}{newUnitSymbol}";
        }

        private void CapNhatPanelChiTietMau(string nhietDo)
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
        }

        private void CapNhatThoiTietTheoNgay(string ngay, string nhietDo, string trangThai, string icon)
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangTheoGio.Controls.Clear();
            BangNhieuNgay.Controls.Clear();
        }
        #endregion

    }
}