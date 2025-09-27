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
    /// Cấu trúc được sắp xếp theo thứ tự:
    /// 1️⃣ Nhập địa điểm, tìm kiếm, lưu địa điểm, đổi °C/°F
    /// 2️⃣ Gọi API & thông tin mô tả
    /// 3️⃣ Thời tiết 24h & 5 ngày
    /// 4️⃣ Biểu đồ
    /// 5️⃣ Bản đồ
    /// 6️⃣ Background thay đổi theo thời tiết
    /// </summary>
    public partial class Form1 : Form
    {
        #region ===== FIELDS & PROPERTIES =====
        
        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;

        // Dữ liệu thời tiết từ API
        private OneCallResponse weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;

        // File lưu địa điểm
        private readonly string locationsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_locations.txt");

        // Danh sách địa điểm đã lưu (giữ để binding UI, nguồn lấy từ file txt)
        private List<SavedLocation> savedLocations = new List<SavedLocation>();

        // Timer tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();

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

        // Lưu địa điểm
        private int currentLocationIndex = 0;
        
        #endregion

        #region ===== CONSTRUCTOR & INITIALIZATION =====
        
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
            
            // Khởi tạo file lưu địa điểm nếu chưa có
            try
            {
                if (!File.Exists(locationsFilePath))
                {
                    File.WriteAllText(locationsFilePath, "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create locations file error: {ex.Message}");
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
        
        #endregion

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
            
            System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD END ===");
        }

        // TODO: Thêm tất cả các method liên quan đến nhóm 1 ở đây
        // - txtDiaDiem_KeyDown (nếu có nhấn Enter để tìm)
        // - btnTimKiem_Click
        // - btnLuu_Click (lưu địa điểm + thời tiết vào DB)
        // - btnChuyenDoiDonVi_Click (°C ↔ °F)
        // - Các hàm hỗ trợ kết nối DB (nếu chưa tách ra file khác)
        
        #endregion

        #region ===== 2️⃣ GỌI API & THÔNG TIN MÔ TẢ =====
        
        // TODO: Thêm tất cả các method liên quan đến nhóm 2 ở đây
        // - LayDuLieuThoiTiet() (gọi OpenWeather API)
        // - ParseKetQuaApi() (xử lý JSON)
        // - CapNhatThongTinChung() (hiển thị cảm giác như, tốc độ gió, tầm nhìn, độ ẩm, áp suất...)
        
        #endregion

        #region ===== 3️⃣ THỜI TIẾT 24H & 5 NGÀY =====
        
        // TODO: Thêm tất cả các method liên quan đến nhóm 3 ở đây
        // - HienThiDuBao24h()
        // - HienThiDuBao5Ngay()
        
        #endregion

        #region ===== 4️⃣ BIỂU ĐỒ =====
        
        // TODO: Thêm tất cả các method liên quan đến nhóm 4 ở đây
        // - VeBieuDoNhietDo24h()
        // - VeBieuDoNhietDo5Ngay()
        
        #endregion

        #region ===== 5️⃣ BẢN ĐỒ =====
        
        // TODO: Thêm tất cả các method liên quan đến nhóm 5 ở đây
        // - LoadBanDo() (hiển thị Google Maps / OpenStreetMap trong WebView2 hoặc control khác)
        // - CapNhatMarkerTheoViTri()
        
        #endregion

        #region ===== 6️⃣ BACKGROUND THAY ĐỔI THEO THỜI TIẾT =====
        
        // TODO: Thêm tất cả các method liên quan đến nhóm 6 ở đây
        // - CapNhatBackground() (đổi ảnh nền theo thời tiết: nắng, mưa, tuyết...)
        // - CapNhatIconDong() (icon động hoặc emoji tuỳ theo thời tiết)
        
        #endregion

        #region ===== HELPER METHODS =====
        
        // TODO: Thêm các helper methods chung ở đây
        
        #endregion

    }

    /// <summary>
    /// Class lưu trữ thông tin địa điểm đã lưu
    /// </summary>
    public class SavedLocation
    {
        public string Name { get; set; } = "";
        public double Lat { get; set; }
        public double Lon { get; set; }
        
        public SavedLocation(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }

    // Class để quản lý địa điểm yêu thích
    public class FavoriteLocation
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
