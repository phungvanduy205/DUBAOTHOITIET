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

        // Danh sách địa điểm đã lưu
        private List<SavedLocation> savedLocations = new List<SavedLocation>();
        private const string SAVED_LOCATIONS_FILE = "saved_locations.txt";

        // Kinh độ, vĩ độ hiện tại của địa điểm đã tìm
        private double? viDoHienTai;
        private double? kinhDoHienTai;

        // Timer tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();

        // Bộ nhớ tạm dữ liệu để xuất CSV
        private DataTable? bangLichSuBoNho;

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
            
            // Khởi tạo lưu địa điểm
            LoadSavedLocations();

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

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD START ===");
            // Khởi tạo dữ liệu ban đầu
            CapNhatThoiGian();
            
            // Load danh sách địa điểm đã lưu
            LoadSavedLocations();
            
            // Test background ngay lập tức
            System.Diagnostics.Debug.WriteLine("Calling TestBackground...");
            TestBackground();
            
            // Force set background ngay trong Form1_Load
            System.Diagnostics.Debug.WriteLine("Calling ForceSetBackgroundInLoad...");
            ForceSetBackgroundInLoad();
            System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD END ===");
        }
        
        /// <summary>
        /// Force set background trong Form1_Load để đảm bảo hiển thị
        /// </summary>
        private void ForceSetBackgroundInLoad()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ForceSetBackgroundInLoad ===");
                
                if (boCucChinh == null)
                {
                    System.Diagnostics.Debug.WriteLine("boCucChinh is NULL trong ForceSetBackgroundInLoad!");
                    return;
                }

                // Xác định ban đêm hay ban ngày
                int currentHour = DateTime.Now.Hour;
                bool isNight = currentHour >= 18 || currentHour < 6;
                
                System.Diagnostics.Debug.WriteLine($"ForceSetBackground: Thời gian {DateTime.Now:HH:mm}, IsNight: {isNight}");

                   // Đường dẫn đến thư mục Resources trong bin/Debug
                   string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"ForceSetBackground: Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }

                Image backgroundImage;
                
                   if (isNight)
                   {
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                       System.Diagnostics.Debug.WriteLine("ForceSetBackground: Chọn nền ban đêm");
                   }
                   else
                   {
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                       System.Diagnostics.Debug.WriteLine("ForceSetBackground: Chọn nền ban ngày");
                   }

                // Force set background với nhiều cách
                boCucChinh.BackgroundImage = backgroundImage;
                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                boCucChinh.BackColor = Color.Transparent;
                
                // Force refresh
                boCucChinh.Invalidate();
                boCucChinh.Update();
                boCucChinh.Refresh();
                
                System.Diagnostics.Debug.WriteLine($"ForceSetBackground: Đã force set background thành công");
                System.Diagnostics.Debug.WriteLine($"boCucChinh.BackgroundImage: {boCucChinh.BackgroundImage != null}");
                System.Diagnostics.Debug.WriteLine($"boCucChinh.Size: {boCucChinh.Size}");
                System.Diagnostics.Debug.WriteLine("=== End ForceSetBackgroundInLoad ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ForceSetBackgroundInLoad error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test background để debug
        /// </summary>
        private void TestBackground()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TEST BACKGROUND ===");
                
                // Kiểm tra boCucChinh
                if (boCucChinh == null)
                {
                    System.Diagnostics.Debug.WriteLine("boCucChinh is NULL!");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"boCucChinh tồn tại: {boCucChinh != null}");
                System.Diagnostics.Debug.WriteLine($"boCucChinh Size: {boCucChinh.Size}");
                System.Diagnostics.Debug.WriteLine($"boCucChinh Location: {boCucChinh.Location}");
                
                // Test load file trực tiếp
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                System.Diagnostics.Debug.WriteLine($"Resources path: {resourcesPath}");
                System.Diagnostics.Debug.WriteLine($"Directory exists: {Directory.Exists(resourcesPath)}");
                
                if (Directory.Exists(resourcesPath))
                {
                    var files = Directory.GetFiles(resourcesPath, "*.gif");
                    System.Diagnostics.Debug.WriteLine($"GIF files found: {files.Length}");
                    foreach (var file in files.Take(5))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {Path.GetFileName(file)}");
                    }
                    
                    // Test load một file cụ thể - nen_ban_ngay.jpg
                    var testFile = Path.Combine(resourcesPath, "nen_ban_ngay.jpg");
                    if (File.Exists(testFile))
                    {
                        System.Diagnostics.Debug.WriteLine($"Test file exists: {testFile}");
                        try
                        {
                            var testImage = Image.FromFile(testFile);
                            boCucChinh.BackgroundImage = testImage;
                            boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                            System.Diagnostics.Debug.WriteLine($"Test image loaded: {testImage.Width}x{testImage.Height}");
                            System.Diagnostics.Debug.WriteLine($"boCucChinh after test: Size={boCucChinh.Size}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading test image: {ex.Message}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Test file NOT exists: {testFile}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("=== END TEST BACKGROUND ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test background error: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi tạo background cho boCucChinh
        /// </summary>
        private void InitializeBackgroundPictureBox()
        {
            // Không cần tạo PictureBox riêng biệt nữa
            // Background sẽ được set trực tiếp cho boCucChinh
            System.Diagnostics.Debug.WriteLine("Đã khởi tạo background system cho boCucChinh");
        }

        /// <summary>
        /// Set background mặc định khi khởi động ứng dụng
        /// </summary>
        private void SetDefaultBackgroundOnStartup()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SetDefaultBackgroundOnStartup ===");
                
                if (boCucChinh == null)
                {
                    System.Diagnostics.Debug.WriteLine("boCucChinh is NULL trong SetDefaultBackgroundOnStartup!");
                    return;
                }

                // Xác định ban đêm hay ban ngày dựa trên thời gian hiện tại
                int currentHour = DateTime.Now.Hour;
                bool isNight = currentHour >= 18 || currentHour < 6;
                
                System.Diagnostics.Debug.WriteLine($"Thời gian hiện tại: {DateTime.Now:HH:mm}, IsNight: {isNight}");

                   // Đường dẫn đến thư mục Resources trong bin/Debug
                   string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                
                System.Diagnostics.Debug.WriteLine($"Resources path: {resourcesPath}");
                System.Diagnostics.Debug.WriteLine($"Directory exists: {Directory.Exists(resourcesPath)}");
                
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }
                
                // Liệt kê các file trong thư mục Resources
                var files = Directory.GetFiles(resourcesPath);
                System.Diagnostics.Debug.WriteLine($"Các file trong Resources: {string.Join(", ", files.Select(Path.GetFileName))}");

                Image backgroundImage;
                
                   if (isNight)
                   {
                       // Ban đêm - dùng nền ban đêm mặc định
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                       System.Diagnostics.Debug.WriteLine("SetDefaultBackground: Chọn nền ban đêm mặc định");
                   }
                   else
                   {
                       // Ban ngày - dùng nền ban ngày mặc định
                       backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                       System.Diagnostics.Debug.WriteLine("SetDefaultBackground: Chọn nền ban ngày mặc định");
                   }

                // Set background cho boCucChinh
                boCucChinh.BackgroundImage = backgroundImage;
                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                boCucChinh.BackColor = Color.Transparent; // Đảm bảo BackColor là Transparent
                
                System.Diagnostics.Debug.WriteLine($"SetDefaultBackground: Đã set background thành công cho boCucChinh");
                System.Diagnostics.Debug.WriteLine($"boCucChinh.BackgroundImage: {boCucChinh.BackgroundImage != null}");
                System.Diagnostics.Debug.WriteLine($"boCucChinh.BackgroundImageLayout: {boCucChinh.BackgroundImageLayout}");
                System.Diagnostics.Debug.WriteLine($"boCucChinh.BackColor: {boCucChinh.BackColor}");
                System.Diagnostics.Debug.WriteLine("=== End SetDefaultBackgroundOnStartup ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetDefaultBackgroundOnStartup error: {ex.Message}");
                // Fallback - dùng màu nền đơn giản
                if (boCucChinh != null)
                {
                    boCucChinh.BackgroundImage = null;
                    boCucChinh.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Thiết lập nền theo thời gian và thời tiết
        /// </summary>
        private void SetBackground(string weatherMain = "Clear", int weatherId = 800)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SetBackground được gọi với weatherMain: {weatherMain}, weatherId: {weatherId} ===");
                
                if (boCucChinh == null)
                {
                    System.Diagnostics.Debug.WriteLine("boCucChinh is NULL trong SetBackground!");
                    return;
                }

                Image backgroundImage;
                
                // Sử dụng thời gian từ API nếu có, nếu không thì dùng thời gian máy
                int currentHour;
                if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
                {
                    var apiTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt + weatherData.TimezoneOffset).DateTime;
                    currentHour = apiTime.Hour;
                }
                else
                {
                    currentHour = DateTime.Now.Hour;
                }
                
                bool isNight = currentHour >= 18 || currentHour < 6;

                // THROTTLE: nếu không thay đổi trạng thái ngày/đêm và mã thời tiết → bỏ qua
                if (lastWeatherId == weatherId && lastIsNight == isNight)
                {
                    System.Diagnostics.Debug.WriteLine("SetBackground: Bỏ qua vì không có thay đổi (throttle)");
                    return;
                }
                lastWeatherId = weatherId;
                lastIsNight = isNight;

                // Đường dẫn đến thư mục Resources trong bin/Debug
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                
                System.Diagnostics.Debug.WriteLine($"Resources path: {resourcesPath}");
                System.Diagnostics.Debug.WriteLine($"Weather main: '{weatherMain}', WeatherId: {weatherId}, IsNight: {isNight}");
                System.Diagnostics.Debug.WriteLine($"Current weather data: {weatherData?.Current?.Weather?[0]?.Main ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"Current weather ID: {(weatherData?.Current?.Weather?[0]?.Id ?? 0).ToString()}");
                System.Diagnostics.Debug.WriteLine($"WeatherId parameter: {weatherId}");
                
                // Kiểm tra thư mục Resources có tồn tại không
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }
                
                // (Optional) Có thể liệt kê file khi debug, nhưng tránh log quá nhiều gây giật

                // Chọn background dựa trên mã thời tiết từ OpenWeatherMap API
                if (weatherId >= 200 && weatherId <= 232)
                {
                    // Thunderstorm (dông, sấm chớp) => nen_giong_bao
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_giong_bao.jpg"));
                    System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_giong_bao.jpg (thunderstorm - {weatherId})");
                }
                else if (weatherId >= 300 && weatherId <= 321)
                {
                    // Drizzle (mưa phùn) => nen_mua_rao
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_mua_rao.jpg"));
                    System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_mua_rao.jpg (drizzle - {weatherId})");
                }
                else if (weatherId >= 500 && weatherId <= 531)
                {
                    // Rain (mưa) => nen_mua
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_mua.jpg"));
                    System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_mua.jpg (rain - {weatherId})");
                }
                else if (weatherId >= 600 && weatherId <= 622)
                {
                    // Snow (tuyết) => nen_tuyet
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_tuyet.jpg"));
                    System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_tuyet.jpg (snow - {weatherId})");
                }
                else if (weatherId >= 701 && weatherId <= 781)
                {
                    // Atmosphere (sương mù, bụi, khói…) => nen_suong_mu
                    backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_suong_mu.jpg"));
                    System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_suong_mu.jpg (atmosphere - {weatherId})");
                }
                else if (weatherId == 800)
                {
                    // Clear sky (trời quang/nắng)
                    if (isNight)
                    {
                        // Ban đêm: nền đêm yên tĩnh
                        var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                        backgroundImage = Image.FromFile(demPath);
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_ban_dem.png (clear night - {weatherId})");
                    }
                    else
                    {
                        // Ban ngày: trời nắng
                        var nangPath = Path.Combine(resourcesPath, "nen_troi_nang.jpg");
                        if (!File.Exists(nangPath))
                        {
                            // Fallback nếu thiếu file: dùng trời quang
                            nangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                        }
                        backgroundImage = Image.FromFile(nangPath);
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: {Path.GetFileName(nangPath)} (clear day/sunny - {weatherId})");
                    }
                }
                else if (weatherId >= 801 && weatherId <= 804)
                {
                    // Clouds (mây) => nen_ban_ngay hoặc nen_ban_dem
                    if (isNight)
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_ban_dem.png (clouds đêm - {weatherId})");
                    }
                    else
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_ngay.jpg"));
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_ban_ngay.jpg (clouds ngày - {weatherId})");
                    }
                }
                else
                {
                    // Mặc định - dùng nền theo thời gian
                    if (isNight)
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.png"));
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_ban_dem.png (mặc định đêm - {weatherId})");
                    }
                    else
                    {
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.jpg"));
                        System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_troi_quang.jpg (mặc định ngày - {weatherId})");
                    }
                }

                // Set background cho boCucChinh thay vì PictureBox riêng biệt
                if (boCucChinh != null)
                {
                    boCucChinh.BackgroundImage = backgroundImage;
                    boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                    boCucChinh.BackColor = Color.Transparent; // Đảm bảo BackColor là Transparent
                    System.Diagnostics.Debug.WriteLine($"Đã set background cho boCucChinh: {backgroundImage?.Width}x{backgroundImage?.Height}");
                    System.Diagnostics.Debug.WriteLine($"boCucChinh Size: {boCucChinh.Size}");
                    System.Diagnostics.Debug.WriteLine($"boCucChinh Location: {boCucChinh.Location}");
                    System.Diagnostics.Debug.WriteLine($"boCucChinh.BackgroundImage: {boCucChinh.BackgroundImage != null}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("boCucChinh is NULL!");
                }

                // Cập nhật màu chữ theo thời gian
                CapNhatMauChuTheoThoiGian(isNight);
                
                System.Diagnostics.Debug.WriteLine($"=== SetBackground hoàn thành thành công ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thiết lập nền: {ex.Message}");
                   // Fallback - tạo background gradient đơn giản cho boCucChinh
                   if (boCucChinh != null)
                   {
                       boCucChinh.BackgroundImage = null;
                       boCucChinh.BackColor = Color.Transparent;
                   }
            }
        }

        /// <summary>
        /// Cập nhật màu chữ theo thời gian (ban đêm = trắng, ban ngày = đen)
        /// </summary>
        private void CapNhatMauChuTheoThoiGian(bool isNight)
        {
            try
            {
                Color textColor = isNight ? Color.White : Color.Black;

                // Cập nhật màu chữ cho các label chính
                nhanNhietDoHienTai.ForeColor = textColor;
                nhanTrangThai.ForeColor = textColor;
                nhanTenDiaDiem.ForeColor = textColor;
                // nhanNgayGio.ForeColor = textColor; // Không tồn tại
                // nhanNhietDoCaoThap.ForeColor = textColor; // Không tồn tại

                // Cập nhật màu chữ cho các panel chi tiết
                CapNhatMauChuPanelChiTiet(textColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật màu chữ: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật màu chữ cho các panel chi tiết
        /// </summary>
        private void CapNhatMauChuPanelChiTiet(Color textColor)
        {
            try
            {
                // Cập nhật panel chi tiết
                foreach (Control control in detailGridPanel.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }

                // Cập nhật dự báo 24 giờ
                foreach (Control control in BangTheoGio.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }

                // Cập nhật dự báo 5 ngày
                foreach (Control control in BangNhieuNgay.Controls)
                {
                    if (control is Panel panel)
                    {
                        foreach (Control child in panel.Controls)
                        {
                            if (child is Label label)
                            {
                                label.ForeColor = textColor;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật màu chữ panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
        /// </summary>
        private async Task LoadInitialWeatherData()
        {
            try
            {
                // Khởi động không có dữ liệu gì, chỉ nạp danh sách địa điểm đã lưu
                NapDiaDiemDaLuu();

                // Hiển thị thông báo chào mừng
                nhanTenDiaDiem.Text = "Chào mừng đến với ứng dụng thời tiết";
                nhanThoiGian.Text = "Hãy tìm kiếm địa điểm để xem thông tin thời tiết";
                nhanNhietDoHienTai.Text = "--°C";
                nhanTrangThai.Text = "Chưa có dữ liệu";

                // Xóa các panel dự báo
                BangTheoGio.Controls.Clear();
                BangNhieuNgay.Controls.Clear();
                
                // Load thời tiết theo vị trí hiện tại (IP) để có tọa độ cho bản đồ
                await LoadWeatherByIP();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu ban đầu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cấu hình ban đầu cho form, timer, v.v.
        /// </summary>
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

        /// <summary>
        /// Cập nhật thời gian hiện tại theo địa điểm
        /// </summary>
        private void CapNhatThoiGian()
        {
            try
            {
                DateTime now;
                
                // Nếu có dữ liệu thời tiết từ API, sử dụng thời gian từ API với múi giờ địa phương
                if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
                {
                    // Sử dụng Unix timestamp từ API và chuyển đổi theo múi giờ địa phương
                    // TimezoneOffset là offset tính bằng giây từ UTC
                    var utcTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt);
                    now = utcTime.AddSeconds(weatherData.TimezoneOffset).DateTime;
                }
                else
                {
                    // Fallback: sử dụng thời gian máy nếu chưa có dữ liệu API
                    now = DateTime.Now;
                }

                // Hiển thị thứ, ngày tháng năm
                var thu = GetThuVietNam(now.DayOfWeek);
                var ngayThang = now.ToString("dd/MM/yyyy");
                var gioPhut = now.ToString("HH:mm");

                // Cập nhật label địa điểm (nếu có) - chỉ khi chưa có dữ liệu
                if (nhanTenDiaDiem != null && string.IsNullOrEmpty(nhanTenDiaDiem.Text))
                {
                    nhanTenDiaDiem.Text = currentLocation;
                }

                // Cập nhật label thời gian (nếu có)
                if (nhanThoiGian != null)
                {
                    nhanThoiGian.Text = $"{thu}, {ngayThang} - {gioPhut}";
                }

                // Cập nhật background theo thời tiết hiện tại (nếu có dữ liệu)
                if (weatherData?.Current?.Weather?.Length > 0)
                {
                    var weather = weatherData.Current.Weather[0];
                    System.Diagnostics.Debug.WriteLine($"=== API WEATHER DATA ===");
                    System.Diagnostics.Debug.WriteLine($"Weather Main: {weather.Main}");
                    System.Diagnostics.Debug.WriteLine($"Weather Description: {weather.Description}");
                    System.Diagnostics.Debug.WriteLine($"Weather ID: {weather.Id}");
                    System.Diagnostics.Debug.WriteLine($"Weather Icon: {weather.Icon}");
                    System.Diagnostics.Debug.WriteLine($"=== END API WEATHER DATA ===");
                    
                    SetBackground(weather.Main ?? "Clear", weather.Id);
                }
                else
                {
                    // Fallback background cho boCucChinh - dùng nền ban ngày mặc định
                    if (boCucChinh != null)
                    {
                        try
                        {
                            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                            string fallbackFile = Path.Combine(resourcesPath, "nen_ban_ngay.jpg");
                            
                            if (File.Exists(fallbackFile))
                            {
                                boCucChinh.BackgroundImage = Image.FromFile(fallbackFile);
                                boCucChinh.BackgroundImageLayout = ImageLayout.Stretch;
                                System.Diagnostics.Debug.WriteLine("Fallback: Đã load nen_ban_ngay.jpg");
                            }
                            else
                            {
                                // Nếu không có file, dùng màu nền đơn giản
                                boCucChinh.BackgroundImage = null;
                                boCucChinh.BackColor = Color.Transparent;
                                System.Diagnostics.Debug.WriteLine("Fallback: Không tìm thấy nen_ban_ngay.jpg, dùng màu nền");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Fallback background error: {ex.Message}");
                            boCucChinh.BackgroundImage = null;
                            boCucChinh.BackColor = Color.Transparent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật thời gian: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật hiển thị địa điểm
        /// </summary>
        private void CapNhatDiaDiem(string diaDiem)
        {
            try
            {
                // Cập nhật label địa điểm hiện có
                if (nhanTenDiaDiem != null)
                {
                    nhanTenDiaDiem.Text = diaDiem;
                }

                // Cập nhật biến currentLocation
                currentLocation = diaDiem;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị thông tin thời tiết đầy đủ
        /// </summary>
        private void HienThiThongTin(string name, OneCallResponse weather)
        {
            try
            {
                if (weather?.Current == null)
                {
                    MessageBox.Show("Dữ liệu thời tiết không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";

                System.Diagnostics.Debug.WriteLine($"Đang hiển thị thông tin: {name}, Nhiệt độ(K): {weather.Current.Temp}");

                // Cập nhật thông tin chính (API trả Kelvin)
                double nhietDoHienTai = donViCelsius
                    ? TemperatureConverter.ToCelsius(weather.Current.Temp)
                    : TemperatureConverter.ToFahrenheit(weather.Current.Temp);
                nhanNhietDoHienTai.Text = $"{Math.Round(nhietDoHienTai)}{kyHieuNhietDo}";
                nhanTrangThai.Text = weather.Current.Weather?[0]?.Description ?? "Không xác định";

                // Cập nhật icon thời tiết chính
                if (anhIconThoiTiet != null && weather.Current.Weather?.Length > 0)
                {
                    string iconCode = weather.Current.Weather[0].Icon ?? "01d";
                    anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                }

                // Cập nhật địa điểm và thời gian
                CapNhatDiaDiem(name);
                CapNhatThoiGian();

                // Cập nhật các panel chi tiết
                CapNhatPanelChiTietFromApi(weather.Current, kyHieuNhietDo);

                // Cập nhật background theo thời tiết
                if (weather.Current.Weather?.Length > 0)
                {
                    var currentWeather = weather.Current.Weather[0];
                    System.Diagnostics.Debug.WriteLine($"=== HienThiThongTin WEATHER DATA ===");
                    System.Diagnostics.Debug.WriteLine($"Weather Main: {currentWeather.Main}");
                    System.Diagnostics.Debug.WriteLine($"Weather Description: {currentWeather.Description}");
                    System.Diagnostics.Debug.WriteLine($"Weather ID: {currentWeather.Id}");
                    System.Diagnostics.Debug.WriteLine($"Weather Icon: {currentWeather.Icon}");
                    System.Diagnostics.Debug.WriteLine($"=== END HienThiThongTin WEATHER DATA ===");
                    
                    SetBackground(currentWeather.Main ?? "Clear", currentWeather.Id);
                }
                else
                {
                    SetBackground("Clear", 800);
                }

                // Cập nhật dự báo 24 giờ
                if (weather.Hourly != null && weather.Hourly.Length > 0)
                {
                    LoadDuBao24h(weather.Hourly, kyHieuNhietDo);
                }
                else
                {
                    if (BangTheoGio != null)
                        BangTheoGio.Controls.Clear();
                }

                // Cập nhật dự báo 5 ngày
                if (weather.Daily != null && weather.Daily.Length > 0)
                {
                    LoadForecast5Days(weather.Daily, kyHieuNhietDo);
                }
                else
                {
                    if (BangNhieuNgay != null)
                        BangNhieuNgay.Controls.Clear();
                }

                // Hiển thị gợi ý theo thời tiết
                HienThiGoiYThoiTiet(weather.Current);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị thông tin: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Hiển thị gợi ý theo thời tiết
        /// </summary>
        private void HienThiGoiYThoiTiet(CurrentWeather current)
        {
            try
            {
                if (current?.Weather?.Length == 0) return;

                var weatherDesc = current.Weather?[0]?.Description ?? "";
                var temperature = donViCelsius 
                    ? TemperatureConverter.ToCelsius(current.Temp)
                    : TemperatureConverter.ToFahrenheit(current.Temp);
                var humidity = current.Humidity;
                var windSpeed = current.WindSpeed;

                var suggestions = GetWeatherSuggestions(weatherDesc, temperature, humidity, windSpeed);

                // Tìm hoặc tạo panel gợi ý
                var suggestionPanel = Controls.Find("suggestionPanel", true).FirstOrDefault() as Panel;
                if (suggestionPanel == null)
                {
                    suggestionPanel = new Panel
                    {
                        Name = "suggestionPanel",
                        BackColor = Color.FromArgb(200, 255, 255, 255), // Nền trắng đậm hơn
                        Size = new Size(800, 40), // Panel rộng và cao hơn
                        Location = new Point(100, 50), // Vị trí cao, không bị che
                        BorderStyle = BorderStyle.None
                    };
                    ApplyRoundedCorners(suggestionPanel, 10);
                    Controls.Add(suggestionPanel);
                    suggestionPanel.BringToFront();
                    suggestionPanel.BringToFront(); // Gọi 2 lần để chắc chắn
                }

                suggestionPanel.Controls.Clear();

                // Tạo text chạy từ phải qua trái
                var scrollingText = string.Join(" • ", suggestions);
                scrollingText = $"💡 Gợi ý: {scrollingText} • ";

                var scrollingLabel = new Label
                {
                    Text = scrollingText,
                    Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(30, 30, 30),
                    Location = new Point(600, 8), // Bắt đầu từ bên phải panel
                    Size = new Size(1000, 20), // Kích thước lớn để chứa text dài
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false
                };
                suggestionPanel.Controls.Add(scrollingLabel);

                // Tạo Timer để chạy chữ
                var scrollTimer = new System.Windows.Forms.Timer
                {
                    Interval = 50, // Cập nhật mỗi 50ms
                    Enabled = true
                };

                scrollTimer.Tick += (sender, e) =>
                {
                    if (scrollingLabel.Location.X <= -scrollingLabel.Width)
                    {
                        scrollingLabel.Location = new Point(600, 8); // Reset về bên phải panel
                    }
                    else
                    {
                        scrollingLabel.Location = new Point(scrollingLabel.Location.X - 2, 8); // Di chuyển sang trái
                    }
                };

                // Lưu timer để có thể dừng sau này
                suggestionPanel.Tag = scrollTimer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hiển thị gợi ý: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu địa điểm vào file
        /// </summary>
        private void LuuDiaDiem(string name, double lat, double lon)
        {
            try
            {
                // Kiểm tra xem địa điểm đã tồn tại chưa
                if (savedLocations.Any(loc => loc.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return; // Đã tồn tại, không lưu trùng
                }

                // Thêm vào danh sách
                var newLocation = new SavedLocation(name, lat, lon);
                savedLocations.Add(newLocation);

                // Lưu vào file
                var lines = savedLocations.Select(loc => $"{loc.Name}|{loc.Lat}|{loc.Lon}");
                File.WriteAllLines(SAVED_LOCATIONS_FILE, lines);

                // Cập nhật ListBox
                NapDiaDiemDaLuu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Nạp danh sách địa điểm đã lưu từ file
        /// </summary>
        private void NapDiaDiemDaLuu()
        {
            try
            {
                if (!File.Exists(SAVED_LOCATIONS_FILE))
                {
                    listBoxDiaDiemDaLuu.Items.Clear();
                    return;
                }

                var lines = File.ReadAllLines(SAVED_LOCATIONS_FILE);
                savedLocations.Clear();
                listBoxDiaDiemDaLuu.Items.Clear();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length == 3 &&
                        double.TryParse(parts[1], out double lat) &&
                        double.TryParse(parts[2], out double lon))
                    {
                        var location = new SavedLocation(parts[0], lat, lon);
                        savedLocations.Add(location);
                        listBoxDiaDiemDaLuu.Items.Add(location);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp địa điểm đã lưu: {ex.Message}");
            }
        }

        /// <summary>
        /// Sự kiện chọn địa điểm đã lưu
        /// </summary>
        private async void SuKienChonDiaDiemDaLuu()
        {
            try
            {
                var selectedLocationName = listBoxDiaDiemDaLuu.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedLocationName))
                {
                    // Kiểm tra nếu là địa điểm IP
                    if (selectedLocationName == "📍 Vị trí hiện tại")
                    {
                        // Load thời tiết theo IP
                        await LoadWeatherByIP();
                    }
                    else
                    {
                        // Cập nhật ô tìm kiếm
                        oTimKiemDiaDiem.Text = selectedLocationName;
                        
                        // Tự động load thời tiết cho địa điểm đã lưu
                        await CapNhatThoiTiet();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn địa điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Chuyển đổi thứ tiếng Anh sang tiếng Việt
        /// </summary>
        private string GetThuVietNam(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday: return "Chủ nhật";
                case DayOfWeek.Monday: return "Thứ hai";
                case DayOfWeek.Tuesday: return "Thứ ba";
                case DayOfWeek.Wednesday: return "Thứ tư";
                case DayOfWeek.Thursday: return "Thứ năm";
                case DayOfWeek.Friday: return "Thứ sáu";
                case DayOfWeek.Saturday: return "Thứ bảy";
                default: return "Thứ";
            }
        }

        /// <summary>
        /// Áp dụng style glassmorphism hiện đại cho giao diện
        /// </summary>
        private void ApDungStyleGlassmorphism()
        {
            try
            {
                // Cấu hình form để hỗ trợ trong suốt
                this.FormBorderStyle = FormBorderStyle.None;
                this.AllowTransparency = true;
                this.BackColor = Color.FromArgb(0, 0, 0, 0); // Nền hoàn toàn trong suốt

                // Thêm viền bo tròn cho form
                this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));

                // Thanh trên cùng - trong suốt mờ mờ
                thanhTrenCung.BackColor = Color.FromArgb(80, 255, 255, 255);

                // Panel chính - trong suốt để nền hiển thị
                boCucChinh.BackColor = Color.Transparent;
                khuVucTrai_HienTai.BackColor = Color.Transparent; // Trong suốt
                khuVucPhai_5Ngay.BackColor = Color.Transparent; // Trong suốt
                khuVucDuoi_24Gio.BackColor = Color.Transparent; // Trong suốt

                // GroupBox - trong suốt mờ mờ
                khung5Ngay.BackColor = Color.FromArgb(40, 255, 255, 255);
                khung24Gio.BackColor = Color.FromArgb(40, 255, 255, 255);

                // TextBox tìm kiếm - trong suốt với viền bo tròn
                SetTransparentBackColor(oTimKiemDiaDiem, Color.FromArgb(150, 255, 255, 255));
                oTimKiemDiaDiem.BorderStyle = BorderStyle.None;
                oTimKiemDiaDiem.Font = new Font("Segoe UI", 12F, FontStyle.Regular);

                // Button tìm kiếm - trong suốt với viền bo tròn
                SetTransparentBackColor(NutTimKiem, Color.FromArgb(150, 255, 255, 255));
                NutTimKiem.FlatStyle = FlatStyle.Flat;
                NutTimKiem.FlatAppearance.BorderSize = 0;
                NutTimKiem.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

                // Button đơn vị - công tắc đẹp
                CongTacDonVi.BackColor = Color.FromArgb(100, 255, 255, 255);
                CongTacDonVi.ForeColor = Color.White;
                CongTacDonVi.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                CongTacDonVi.Text = donViCelsius ? "°C" : "°F";

                // Labels - màu trắng, font đẹp
                nhanTenDiaDiem.ForeColor = Color.White;
                nhanTenDiaDiem.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
                nhanNhietDoHienTai.ForeColor = Color.White;
                nhanNhietDoHienTai.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
                nhanTrangThai.ForeColor = Color.White;
                nhanTrangThai.Font = new Font("Segoe UI", 16F, FontStyle.Regular);

                // TabControl - hoàn toàn trong suốt
                tabDieuKhien.BackColor = Color.Transparent;
                tabChart.BackColor = Color.FromArgb(30, 50, 70, 90); // Nền xanh dương mờ

                // DataGridView - trong suốt mờ mờ
                BangLichSu.BackgroundColor = Color.FromArgb(40, 255, 255, 255);
                BangLichSu.ForeColor = Color.Black;

                // Thêm nút đóng form (vì đã bỏ border)
                TaoNutDongForm();
            }
            catch (Exception ex)
            {
                // Fallback: sử dụng màu không trong suốt
                System.Diagnostics.Debug.WriteLine($"Lỗi glassmorphism: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method để set màu trong suốt an toàn
        /// </summary>
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

        /// <summary>
        /// Tạo viền bo tròn cho form
        /// </summary>
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        /// <summary>
        /// Áp dụng viền bo tròn cho control
        /// </summary>
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

        /// <summary>
        /// Load danh sách địa điểm đã lưu
        /// </summary>
        private void LoadSavedLocations()
        {
            try
            {
                if (File.Exists(locationsFilePath))
                {
                    var json = File.ReadAllText(locationsFilePath);
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                    if (data?.locations != null)
                    {
                        savedLocationNames = data.locations.ToObject<List<string>>();
                    }
                }
                
                // Nếu chưa có địa điểm nào, thêm một số địa điểm mẫu
                if (savedLocationNames.Count == 0)
                {
                    savedLocationNames.Add("London");
                    savedLocationNames.Add("New York");
                    savedLocationNames.Add("Tokyo");
                }
                
                
                // Cập nhật danh sách trong ListBox
                CapNhatDanhSachDiaDiem();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dữ liệu thời tiết theo vị trí hiện tại (IP)
        /// </summary>
        private async Task LoadWeatherByIP()
        {
            try
            {
                // Lấy vị trí hiện tại theo IP
                var locationData = await WeatherApiService.GetCurrentLocationAsync();
                if (locationData?.Results?.Length > 0)
                {
                    var result = locationData.Results[0];
                    
                    // Cập nhật UI với tên địa điểm
                    string locationName = $"{result.Name}, {result.Country}";
                    oTimKiemDiaDiem.Text = locationName;
                    currentLocation = locationName;
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    CapNhatDiaDiem(locationName);
                    
                    // Thêm địa điểm IP vào danh sách nếu chưa có
                    string ipLocationKey = "📍 Vị trí hiện tại";
                    if (!savedLocationNames.Contains(ipLocationKey))
                    {
                        savedLocationNames.Insert(0, ipLocationKey); // Thêm vào đầu danh sách
                        SaveLocationList();
                        CapNhatDanhSachDiaDiem();
                    }
                    
                    // Lấy dữ liệu thời tiết (Kelvin)
                    var weatherData = await WeatherApiService.GetCurrentWeatherAsync(result.Lat, result.Lon);
                    if (weatherData != null)
                    {
                        this.weatherData = weatherData;
                        await CapNhatThoiTiet();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load thời tiết theo IP: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dữ liệu thời tiết cho địa điểm mặc định khi khởi động app
        /// </summary>
        private async Task LoadWeatherForDefaultLocation(string locationName)
        {
            try
            {
                // Lấy tọa độ từ tên địa điểm
                var geocodingData = await WeatherApiService.GetCoordinatesAsync(locationName);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";

                    // Lấy dữ liệu thời tiết
                    weatherData = await WeatherApiService.GetWeatherDataAsync(currentLat, currentLon);
                    if (weatherData != null)
                    {
                        // Hiển thị thông tin đầy đủ
                        HienThiThongTin(currentLocation, weatherData);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("API trả về null khi load địa điểm mặc định");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Không tìm thấy tọa độ cho địa điểm: {locationName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi load thời tiết cho địa điểm mặc định: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu danh sách địa điểm
        /// </summary>
        private void SaveLocationList()
        {
            try
            {
                var data = new
                {
                    locations = savedLocationNames
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(locationsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm: {ex.Message}");
            }
        }
        // (đã xoá phiên bản trùng lặp ShowChart/ShowMap)
        /// <summary>
        /// Lưu địa điểm hiện tại
        /// </summary>
        private void nutLuuDiaDiem_Click(object sender, EventArgs e)
        {
            var currentLocation = oTimKiemDiaDiem.Text.Trim();
            if (string.IsNullOrEmpty(currentLocation))
            {
                MessageBox.Show("Vui lòng nhập địa điểm trước khi lưu!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (savedLocationNames.Contains(currentLocation))
            {
                MessageBox.Show("Địa điểm này đã được lưu rồi!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            savedLocationNames.Add(currentLocation);
            SaveLocationList();
            CapNhatDanhSachDiaDiem();
            
            MessageBox.Show($"Đã lưu địa điểm: {currentLocation}", "Thành công", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Chuyển đổi địa điểm - hiện dropdown để chọn
        /// </summary>
        private void nutChuyenDoiDiaDiem_Click(object sender, EventArgs e)
        {
            if (savedLocationNames.Count == 0) 
            {
                MessageBox.Show("Chưa có địa điểm nào được lưu. Hãy lưu địa điểm trước!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Tạo context menu để chọn địa điểm
            var contextMenu = new ContextMenuStrip();
            
            foreach (var location in savedLocationNames)
            {
                // Tạo panel con chứa tên địa điểm và 2 nút
                var innerPanel = new Panel
                {
                    Width = 200,
                    Height = 30
                };
                
                // Label tên địa điểm (click để chọn)
                var locationLabel = new Label
                {
                    Text = location,
                    Location = new Point(5, 5),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };
                locationLabel.Click += async (s, args) => {
                    // Kiểm tra nếu là địa điểm IP
                    if (location == "📍 Vị trí hiện tại")
                    {
                        // Load thời tiết theo IP
                        await LoadWeatherByIP();
                    }
                    else
                    {
                        oTimKiemDiaDiem.Text = location;
                        currentLocation = location;
                        currentLocationIndex = savedLocationNames.IndexOf(location);
                        
                        // Cập nhật tên địa điểm hiển thị
                        CapNhatDiaDiem(location);
                        
                        await CapNhatThoiTiet();
                    }
                    SaveLocationList();
                    contextMenu.Close();
                };
                
                // Nút xóa (✗) - chỉ hiện cho địa điểm khác (không phải vị trí hiện tại)
                Button deleteBtn = null;
                if (location != "📍 Vị trí hiện tại")
                {
                    deleteBtn = new Button
                    {
                        Text = "✗",
                        Location = new Point(160, 3),
                        Size = new Size(25, 24),
                        Font = new Font("Arial", 10, FontStyle.Bold),
                        BackColor = Color.LightCoral,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    deleteBtn.Click += (s, args) => {
                        var result = MessageBox.Show($"Bạn có chắc muốn xóa địa điểm '{location}'?", "Xác nhận xóa", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            savedLocationNames.Remove(location);
                            SaveLocationList();
                            CapNhatDanhSachDiaDiem();
                            MessageBox.Show($"Đã xóa địa điểm: {location}", "Thành công", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            contextMenu.Close();
                        }
                    };
                }
                
                // Thêm các control vào panel
                innerPanel.Controls.Add(locationLabel);
                if (deleteBtn != null)
                {
                    innerPanel.Controls.Add(deleteBtn);
                }
                
                // Tạo ToolStripControlHost với panel
                var locationPanel = new ToolStripControlHost(innerPanel);
                contextMenu.Items.Add(locationPanel);
            }
            
            // Hiện menu tại vị trí nút
            contextMenu.Show(nutChuyenDoiDiaDiem, new Point(0, nutChuyenDoiDiaDiem.Height));
        }

        /// <summary>
        /// Xóa địa điểm đã chọn khỏi danh sách
        /// </summary>
        private void nutXoaDiaDiem_Click(object sender, EventArgs e)
        {
            if (listBoxDiaDiemDaLuu.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn địa điểm cần xóa!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedLocation = listBoxDiaDiemDaLuu.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedLocation)) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa địa điểm '{selectedLocation}'?", "Xác nhận xóa", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                savedLocationNames.Remove(selectedLocation);
                SaveLocationList();
                CapNhatDanhSachDiaDiem();
                MessageBox.Show($"Đã xóa địa điểm: {selectedLocation}", "Thành công", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Chọn địa điểm mặc định
        /// </summary>

        /// <summary>
        /// Cập nhật danh sách địa điểm trong ListBox
        /// </summary>
        private void CapNhatDanhSachDiaDiem()
        {
            listBoxDiaDiemDaLuu.Items.Clear();
            foreach (var location in savedLocationNames)
            {
                listBoxDiaDiemDaLuu.Items.Add(location);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Lưu địa điểm khi đóng ứng dụng
            SaveLocationList();
        }

        // Method TaoSegmentDonViChiF() đã được xóa vì giờ dùng UnitToggle trong Designer

        /// <summary>
        /// Tạo nút đóng form
        /// </summary>
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

        /// <summary>
        /// Sự kiện bấm nút Tìm kiếm: Geocoding để lấy lat/lon, sau đó cập nhật dữ liệu
        /// </summary>
        private async void NutTimKiem_Click(object? sender, EventArgs e)
        {
            var tuKhoa = oTimKiemDiaDiem.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                MessageBox.Show("Vui lòng nhập xã/phường, quận/huyện, tỉnh/thành để tìm kiếm.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"Nút tìm kiếm được nhấn: {tuKhoa}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await TimKiemDiaDiem(tuKhoa);
        }

        /// <summary>
        /// Đổi đơn vị °C ↔ °F và cập nhật lại dữ liệu
        /// </summary>
        private async void CongTacDonVi_Click(object? sender, EventArgs e)
        {
            // Đảo ngược trạng thái đơn vị
            donViCelsius = !donViCelsius;

            // Cập nhật text của button
            CongTacDonVi.Text = donViCelsius ? "°C" : "°F";

            await CapNhatThoiTiet();
        }

        /// <summary>
        /// Gọi API → hiển thị thời tiết hiện tại, dự báo 24h, dự báo 5 ngày; cập nhật nền/biểu tượng
        /// </summary>
        private async Task CapNhatThoiTiet()
        {
            if (weatherData == null) return;

            try
            {
                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";

                // Cập nhật thông tin hiện tại
                if (weatherData.Current != null)
                {
                    var current = weatherData.Current;
                    var currentDisplay = donViCelsius ? TemperatureConverter.ToCelsius(current.Temp)
                                                      : TemperatureConverter.ToFahrenheit(current.Temp);
                    nhanNhietDoHienTai.Text = $"{Math.Round(currentDisplay)}{kyHieuNhietDo}";
                    nhanTrangThai.Text = current.Weather?[0]?.Description ?? "Không xác định";

                    // Cập nhật icon thời tiết chính
                    if (anhIconThoiTiet != null && current.Weather?.Length > 0)
                    {
                        string iconCode = current.Weather[0].Icon ?? "01d";
                        anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                    }

                    // Cập nhật địa điểm và thời gian
                    CapNhatDiaDiem(currentLocation);
                    CapNhatThoiGian();

                    // Cập nhật các panel chi tiết
                    CapNhatPanelChiTietFromApi(current, kyHieuNhietDo);

                    // Cập nhật background theo thời tiết
                    SetBackground(current.Weather?[0]?.Main ?? "Clear", current.Weather?[0]?.Id ?? 800);
                }

                // Cập nhật dự báo 24 giờ
                if (weatherData.Hourly != null && weatherData.Hourly.Length > 0)
                {
                    LoadDuBao24h(weatherData.Hourly, kyHieuNhietDo);
                }
                else
                {
                    // Để trống khi không có dữ liệu API
                    BangTheoGio.Controls.Clear();
                }

                // Cập nhật dự báo 5 ngày
                if (weatherData.Daily != null && weatherData.Daily.Length > 0)
                {
                    LoadForecast5Days(weatherData.Daily, kyHieuNhietDo);
                }
                else
                {
                    // Để trống khi không có dữ liệu API
                    BangNhieuNgay.Controls.Clear();
                }
            }
            catch (Exception ex)
            {
                // Để trống khi có lỗi
                BangTheoGio.Controls.Clear();
                BangNhieuNgay.Controls.Clear();
                MessageBox.Show("Có lỗi khi cập nhật thời tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cập nhật các panel chi tiết từ dữ liệu HourlyWeather
        /// </summary>
        private void CapNhatPanelChiTietFromHourlyApi(HourlyWeather hourly, string kyHieu)
        {
            try
            {
                // Cập nhật cảm giác như
                if (feelsLikePanel != null)
                {
                    var feelsLikeLabel = feelsLikePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (feelsLikeLabel != null)
                    {
                        feelsLikeLabel.Text = $"Cảm giác như\n{Math.Round(hourly.FeelsLike)}{kyHieu}";
                    }
                }

                // Cập nhật độ ẩm
                if (humidityPanel != null)
                {
                    var humidityLabel = humidityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (humidityLabel != null)
                    {
                        humidityLabel.Text = $"Độ ẩm\n{hourly.Humidity}%";
                    }
                }

                // Cập nhật gió
                if (windPanel != null)
                {
                    var windLabel = windPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (windLabel != null)
                    {
                        windLabel.Text = $"Gió\n{Math.Round(hourly.WindSpeed)} m/s";
                    }
                }

                // Cập nhật áp suất
                if (pressurePanel != null)
                {
                    var pressureLabel = pressurePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (pressureLabel != null)
                    {
                        pressureLabel.Text = $"Áp suất\n{hourly.Pressure} hPa";
                    }
                }

                // Cập nhật tầm nhìn
                if (visibilityPanel != null)
                {
                    var visibilityLabel = visibilityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (visibilityLabel != null)
                    {
                        visibilityLabel.Text = $"Tầm nhìn\n{hourly.Visibility / 1000} km";
                    }
                }

                // Cập nhật bình minh (không có trong HourlyWeather, giữ nguyên)
                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết từ HourlyWeather: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật các panel chi tiết từ dữ liệu DailyWeather
        /// </summary>
        private void CapNhatPanelChiTietFromDailyApi(DailyWeather daily, string kyHieu)
        {
            try
            {
                // Cập nhật cảm giác như
                if (feelsLikePanel != null)
                {
                    var feelsLikeLabel = feelsLikePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (feelsLikeLabel != null)
                    {
                        feelsLikeLabel.Text = $"Cảm giác như\n{Math.Round(daily.FeelsLike.Day)}{kyHieu}";
                    }
                }

                // Cập nhật độ ẩm
                if (humidityPanel != null)
                {
                    var humidityLabel = humidityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (humidityLabel != null)
                    {
                        humidityLabel.Text = $"Độ ẩm\n{daily.Humidity}%";
                    }
                }

                // Cập nhật gió
                if (windPanel != null)
                {
                    var windLabel = windPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (windLabel != null)
                    {
                        windLabel.Text = $"Gió\n{Math.Round(daily.WindSpeed)} m/s";
                    }
                }

                // Cập nhật áp suất
                if (pressurePanel != null)
                {
                    var pressureLabel = pressurePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (pressureLabel != null)
                    {
                        pressureLabel.Text = $"Áp suất\n{daily.Pressure} hPa";
                    }
                }

                // Cập nhật tầm nhìn (không có trong DailyWeather, giữ nguyên)
                if (visibilityPanel != null)
                {
                    var visibilityLabel = visibilityPanel.Controls.OfType<Label>().FirstOrDefault();
                    if (visibilityLabel != null)
                    {
                        visibilityLabel.Text = $"Tầm nhìn\n-- km";
                    }
                }

                // Cập nhật bình minh (không có trong DailyWeather, giữ nguyên)
                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết từ DailyWeather: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật panel chi tiết từ dữ liệu API
        /// </summary>
        private void CapNhatPanelChiTietFromApi(CurrentWeather current, string kyHieu)
        {
            try
            {
                // Debug: Kiểm tra tất cả giá trị
                System.Diagnostics.Debug.WriteLine($"=== DEBUG API DATA ===");
                System.Diagnostics.Debug.WriteLine($"FeelsLike: {current.FeelsLike}");
                System.Diagnostics.Debug.WriteLine($"Humidity: {current.Humidity}");
                System.Diagnostics.Debug.WriteLine($"WindSpeed: {current.WindSpeed}");
                System.Diagnostics.Debug.WriteLine($"Pressure: {current.Pressure}");
                System.Diagnostics.Debug.WriteLine($"Visibility: {current.Visibility}");
                System.Diagnostics.Debug.WriteLine($"=======================");

                // Gỡ popup debug để tránh làm phiền người dùng

                // Sử dụng TaoPanelChiTiet để cập nhật tất cả panel
                // Xử lý FeelsLike - nếu bằng 0 thì lấy từ Temp
                var feelsLikeValue = current.FeelsLike != 0 ? current.FeelsLike : current.Temp;
                var feelsLikeInUnit = donViCelsius ? TemperatureConverter.ToCelsius(feelsLikeValue)
                                                   : TemperatureConverter.ToFahrenheit(feelsLikeValue);
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác như", $"{Math.Round(feelsLikeInUnit)}{kyHieu}");

                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", $"{current.Humidity}%");

                // Xử lý Wind Speed - hiển thị chính xác
                string windText;
                if (current.WindSpeed == 0)
                {
                    // Chỉ hiển thị "Lặng gió" nếu thật sự là 0 (không phải do lỗi API)
                    windText = "Lặng gió";
                }
                else
                {
                    windText = $"{Math.Round(current.WindSpeed, 1)} m/s";
                }
                TaoPanelChiTiet(windPanel, "💨", "Tốc độ gió", windText);

                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất khí quyển", $"{current.Pressure} hPa");
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn xa", $"{current.Visibility / 1000.0:0.0} km");

                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo dữ liệu 24 giờ từ API
        /// </summary>



        /// <summary>
        /// Chuyển emoji thành icon thời tiết lớn (cho hiển thị chính)
        /// </summary>
        private Image GetWeatherIconFromEmoji(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 200); // Kích thước lớn 200x200px
        }

        /// <summary>
        /// Chuyển emoji thành icon thời tiết nhỏ (cho biểu đồ/cột)
        /// </summary>
        private Image GetWeatherIconFromEmojiSmall(string iconPath)
        {
            return GetWeatherIconFromEmoji(iconPath, 24); // Kích thước rất nhỏ cho biểu đồ
        }

        /// <summary>
        /// Chuyển emoji thành icon thời tiết với kích thước tùy chỉnh
        /// </summary>
        private Image GetWeatherIconFromEmoji(string iconPath, int size)
        {
            // Load icon từ file PNG
            if (File.Exists(iconPath))
            {
                try
                {
                    var originalImage = Image.FromFile(iconPath);
                    // Resize về kích thước tùy chỉnh
                    var resizedImage = new Bitmap(size, size);
                    using (var g = Graphics.FromImage(resizedImage))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, 0, 0, size, size);
                    }
                    originalImage.Dispose();
                    return resizedImage;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi load icon: {ex.Message}");
                }
            }
            
            // Fallback: tạo icon mặc định
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                using (var font = new Font("Segoe UI", size * 0.5f)) // Font size tỷ lệ với kích thước
                {
                    var brush = new SolidBrush(Color.Orange);
                    var rect = new RectangleF(0, 0, size, size);
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("☀", font, brush, rect, format);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Lấy đường dẫn đầy đủ của icon
        /// </summary>
        private string GetIconPath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
        }

        /// <summary>
        /// Lấy gợi ý theo thời tiết hiện tại
        /// </summary>
        private List<string> GetWeatherSuggestions(string weatherDesc, double temperature, double humidity, double windSpeed)
        {
            var suggestions = new List<string>();
            var desc = weatherDesc?.ToLower() ?? "";
            var tempC = temperature;

            // Gợi ý theo nhiệt độ
            if (tempC < 10)
            {
                suggestions.Add("🌡️ Nhiệt độ thấp - Mặc áo ấm và giữ ấm cơ thể");
                suggestions.Add("☕ Thời tiết lạnh - Thưởng thức đồ uống nóng");
            }
            else if (tempC > 30)
            {
                suggestions.Add("🌞 Nhiệt độ cao - Uống nhiều nước và tránh ánh nắng trực tiếp");
                suggestions.Add("🏖️ Thời tiết nóng - Tìm nơi có bóng mát");
            }
            else if (tempC >= 20 && tempC <= 28)
            {
                suggestions.Add("🌤️ Thời tiết dễ chịu - Thích hợp cho hoạt động ngoài trời");
            }

            // Gợi ý theo điều kiện thời tiết
            if (desc.Contains("rain") || desc.Contains("mưa"))
            {
                suggestions.Add("☔ Có mưa - Mang theo ô hoặc áo mưa");
                suggestions.Add("🚗 Đường trơn trượt - Lái xe cẩn thận");
            }
            else if (desc.Contains("snow") || desc.Contains("tuyết"))
            {
                suggestions.Add("❄️ Có tuyết - Mặc quần áo ấm và giày chống trượt");
                suggestions.Add("🏔️ Thời tiết tuyết - Thích hợp cho các hoạt động mùa đông");
            }
            else if (desc.Contains("clear") || desc.Contains("quang"))
            {
                suggestions.Add("☀️ Trời quang - Thích hợp cho picnic và dã ngoại");
                suggestions.Add("📸 Thời tiết đẹp - Chụp ảnh ngoài trời");
            }
            else if (desc.Contains("cloud") || desc.Contains("mây"))
            {
                suggestions.Add("☁️ Nhiều mây - Thời tiết mát mẻ, thích hợp đi bộ");
            }

            // Gợi ý theo độ ẩm
            if (humidity > 80)
            {
                suggestions.Add("💧 Độ ẩm cao - Có thể cảm thấy oi bức");
            }
            else if (humidity < 30)
            {
                suggestions.Add("🌵 Độ ẩm thấp - Da có thể bị khô, nên dưỡng ẩm");
            }

            // Gợi ý theo tốc độ gió
            if (windSpeed > 10)
            {
                suggestions.Add("💨 Gió mạnh - Cẩn thận với các vật bay");
            }
            else if (windSpeed < 2)
            {
                suggestions.Add("🌬️ Ít gió - Không khí có thể tù đọng");
            }

            // Gợi ý chung
            suggestions.Add("📱 Theo dõi thời tiết thường xuyên để cập nhật");
            suggestions.Add("🌍 Bảo vệ môi trường - Hành động vì khí hậu");

            return suggestions.Take(3).ToList(); // Chỉ lấy 3 gợi ý đầu tiên
        }

        /// <summary>
        /// Lấy icon thời tiết từ mã icon API
        /// </summary>
        private string GetWeatherIcon(string iconCode)
        {
            if (string.IsNullOrEmpty(iconCode)) return GetIconPath("troi_quang_ngay.png");

            return iconCode switch
            {
                // Nắng ban ngày/đêm
                "01d" => GetIconPath("troi_quang_ngay.png"), // sunny day
                "01n" => GetIconPath("troi_quang_dem.png"), // clear night
                // Ít mây
                "02d" => GetIconPath("it_may_ngay.png"), // few clouds day
                "02n" => GetIconPath("it_may_dem.png"), // few clouds night
                // Mây rải rác
                "03d" => GetIconPath("may_rac_rac_ngay.png"), // scattered clouds day
                "03n" => GetIconPath("may_rac_rac_dem.png"), // scattered clouds night
                // Mây dày
                "04d" => GetIconPath("may_day_ngay.png"), // broken clouds day
                "04n" => GetIconPath("may_day_dem.png"), // broken clouds night
                // Mưa rào
                "09d" => GetIconPath("mua_rao_ngay.png"), // shower rain day
                "09n" => GetIconPath("mua_rao_dem.png"), // shower rain night
                // Mưa
                "10d" => GetIconPath("mua_ngay.png"), // rain day
                "10n" => GetIconPath("mua_dem.png"), // rain night
                // Bão
                "11d" => GetIconPath("giong_bao_ngay.png"), // thunderstorm day
                "11n" => GetIconPath("giong_bao_dem.png"), // thunderstorm night
                // Tuyết
                "13d" => GetIconPath("tuyet_ngay.png"), // snow day
                "13n" => GetIconPath("tuyet_dem.png"), // snow night
                // Sương mù
                "50d" => GetIconPath("suong_mu_ngay.png"), // mist day
                "50n" => GetIconPath("suong_mu_dem.png"), // mist night
                _ => GetIconPath("troi_quang_ngay.png")
            };
        }

        /// <summary>
        /// Hiển thị danh sách 24 giờ vào FlowLayoutPanel BangTheoGio
        /// </summary>
        private void DoDuLieuBangTheoGio(List<DuBaoTheoGioItem> duLieu, string kyHieu)
        {
            BangTheoGio.SuspendLayout();
            BangTheoGio.Controls.Clear();

            if (duLieu == null || duLieu.Count == 0)
            {
                BangTheoGio.ResumeLayout();
                return;
            }

            foreach (var gio in duLieu)
            {
                var pnl = new Panel
                {
                    Width = 100,
                    Height = 140,
                    Margin = new Padding(8),
                    BackColor = Color.FromArgb(245, 245, 245)
                };

                var nhanGio = new Label
                {
                    Text = gio.ThoiGian.ToString("HH:mm"),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 24
                };

                var pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Top,
                    Height = 72
                };
                pic.Image = ChonIconTheoIconCode(gio.IconCode) ?? ChonIconTheoMa(gio.MaThoiTiet);

                var nhanNhiet = new Label
                {
                    Text = $"{Math.Round(gio.NhietDo)}{kyHieu}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Bottom,
                    Height = 28,
                    Font = new Font(Font, FontStyle.Bold)
                };

                pnl.Controls.Add(nhanNhiet);
                pnl.Controls.Add(pic);
                pnl.Controls.Add(nhanGio);

                BangTheoGio.Controls.Add(pnl);
            }

            BangTheoGio.ResumeLayout();
        }

        /// <summary>
        /// Hiển thị danh sách 5 ngày vào FlowLayoutPanel BangNhieuNgay
        /// </summary>
        private void DoDuLieuBangNhieuNgay(List<DuBaoNgayItem> duLieu, string kyHieu)
        {
            BangNhieuNgay.SuspendLayout();
            BangNhieuNgay.Controls.Clear();

            if (duLieu == null || duLieu.Count == 0)
            {
                // Hiển thị thông báo khi không có dữ liệu
                var lblKhongCoDuLieu = new Label
                {
                    Text = "Không có dữ liệu dự báo 5 ngày",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.Gray,
                    Font = new Font(Font, FontStyle.Italic)
                };
                BangNhieuNgay.Controls.Add(lblKhongCoDuLieu);
                BangNhieuNgay.ResumeLayout();
                return;
            }

            foreach (var ngay in duLieu)
            {
                var pnl = new Panel
                {
                    Width = BangNhieuNgay.ClientSize.Width - 24,
                    Height = 100,
                    Margin = new Padding(6),
                    BackColor = Color.FromArgb(200, 255, 255, 255), // Trắng bán trong suốt
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                // Tạo viền bo tròn
                pnl.Paint += (s, e) =>
                {
                    var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.FromArgb(100, 135, 206, 235), 2))
                    {
                        e.Graphics.DrawRoundedRectangle(pen, rect, 8);
                    }
                };

                // Header với ngày và thứ
                var nhanNgay = new Label
                {
                    Text = $"{ngay.Ngay.ToString("dd/MM")} - {GetDayOfWeekVietnamese(ngay.Ngay.DayOfWeek)}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 28,
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(70, 130, 180),
                    Padding = new Padding(8, 4, 0, 0)
                };

                // Panel chứa icon và thông tin
                var khungDuoi = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8, 0, 8, 8)
                };

                // Icon thời tiết
                var pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 48,
                    Height = 48,
                    Dock = DockStyle.Left,
                    Margin = new Padding(0, 0, 8, 0)
                };
                pic.Image = ChonIconTheoIconCode(ngay.IconCode) ?? ChonIconTheoMa(ngay.MaThoiTiet);

                // Panel thông tin bên phải
                var khungThongTin = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };

                // Trạng thái thời tiết
                var nhanTrangThaiNho = new Label
                {
                    Text = ngay.TrangThaiMoTa ?? "Không xác định",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = new Font(Font.FontFamily, 9),
                    ForeColor = Color.FromArgb(100, 100, 100)
                };

                // Nhiệt độ cao/thấp
                var nhanNhiet = new Label
                {
                    Text = $"Cao: {Math.Round(ngay.NhietDoCao)}{kyHieu}  |  Thấp: {Math.Round(ngay.NhietDoThap)}{kyHieu}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Top,
                    Height = 24,
                    Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };

                // Thêm các control vào panel
                khungThongTin.Controls.Add(nhanNhiet);
                khungThongTin.Controls.Add(nhanTrangThaiNho);

                khungDuoi.Controls.Add(khungThongTin);
                khungDuoi.Controls.Add(pic);

                pnl.Controls.Add(khungDuoi);
                pnl.Controls.Add(nhanNgay);

                BangNhieuNgay.Controls.Add(pnl);
            }

            BangNhieuNgay.ResumeLayout();
        }

        // Helper method để lấy tên thứ bằng tiếng Việt
        private string GetDayOfWeekVietnamese(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => ""
            };
        }

        /// <summary>
        /// Hiển thị dữ liệu lịch sử (DataGridView) và lưu DataTable để xuất
        /// </summary>
        private void HienThiBangLichSu(List<LichSuNgayItem> duLieu, string kyHieu)
        {
            System.Diagnostics.Debug.WriteLine($"Hiển thị lịch sử: {duLieu?.Count ?? 0} items");

            var dt = new DataTable();
            dt.Columns.Add("Ngày");
            dt.Columns.Add("Nhiệt độ TB (" + kyHieu + ")");
            dt.Columns.Add("Cao (" + kyHieu + ")");
            dt.Columns.Add("Thấp (" + kyHieu + ")");
            dt.Columns.Add("Độ ẩm (%)");
            dt.Columns.Add("Trạng thái");

            if (duLieu != null && duLieu.Count > 0)
            {
                foreach (var r in duLieu.OrderByDescending(x => x.Ngay))
                {
                    dt.Rows.Add(
                        r.Ngay.ToString("dd/MM/yyyy"),
                        Math.Round(r.NhietDoTrungBinh),
                        Math.Round(r.NhietDoCao),
                        Math.Round(r.NhietDoThap),
                        r.DoAmTrungBinh,
                        r.TrangThaiMoTa
                    );
                }
                System.Diagnostics.Debug.WriteLine($"Đã thêm {dt.Rows.Count} dòng vào DataTable");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Không có dữ liệu lịch sử để hiển thị");
            }

            bangLichSuBoNho = dt;
            BangLichSu.DataSource = dt;
            System.Diagnostics.Debug.WriteLine($"DataGridView có {BangLichSu.Rows.Count} dòng");
        }

        /// <summary>
        /// Xuất lịch sử ra CSV
        /// </summary>
        private void NutXuatLichSu_Click(object? sender, EventArgs e)
        {
            if (bangLichSuBoNho == null || bangLichSuBoNho.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "lich_su_thoi_tiet.csv"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var csv = ChuyenDataTableSangCsv(bangLichSuBoNho);
                    File.WriteAllText(dlg.FileName, csv, Encoding.UTF8);
                    MessageBox.Show("Xuất CSV thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Chuyển DataTable sang chuỗi CSV (UTF-8)
        /// </summary>
        private static string ChuyenDataTableSangCsv(DataTable dt)
        {
            var sb = new StringBuilder();
            var tenCot = dt.Columns.Cast<DataColumn>().Select(c => BaoCSV(c.ColumnName));
            sb.AppendLine(string.Join(",", tenCot));
            foreach (DataRow row in dt.Rows)
            {
                var o = row.ItemArray.Select(v => BaoCSV(v?.ToString() ?? string.Empty));
                sb.AppendLine(string.Join(",", o));
            }
            return sb.ToString();

            static string BaoCSV(string input)
            {
                if (input.Contains("\"") || input.Contains(",") || input.Contains("\n"))
                {
                    return "\"" + input.Replace("\"", "\"\"") + "\"";
                }
                return input;
            }
        }

        /// <summary>
        /// Chọn icon PNG theo mã thời tiết OpenWeather
        /// </summary>
        private Image? ChonIconTheoMa(int ma)
        {
            var thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            try
            {
                if (ma >= 200 && ma <= 232) return TaiAnh(Path.Combine(thuMuc, "icon_giong.png"));
                if ((ma >= 300 && ma <= 321) || (ma >= 500 && ma <= 531)) return TaiAnh(Path.Combine(thuMuc, "icon_mua.png"));
                if (ma >= 600 && ma <= 622) return TaiAnh(Path.Combine(thuMuc, "icon_tuyet.png"));
                if (ma == 800) return TaiAnh(Path.Combine(thuMuc, "icon_nang.png"));
                return TaiAnh(Path.Combine(thuMuc, "icon_may.png"));
            }
            catch { return null; }
        }

        // Chọn icon theo mã icon OpenWeather (phân biệt ngày/đêm: 01d/01n ... 50d/50n)
        private Image? ChonIconTheoIconCode(string iconCode)
        {
            if (string.IsNullOrWhiteSpace(iconCode))
            {
                System.Diagnostics.Debug.WriteLine("IconCode rỗng");
                return null;
            }

            var code3 = iconCode.Length >= 3 ? iconCode.Substring(0, 3) : iconCode;
            var code2 = iconCode.Length >= 2 ? iconCode.Substring(0, 2) : iconCode;

            string goc = code2 switch
            {
                "01" => "troi_quang",
                "02" => "it_may",
                "03" => "may_rac_rac",
                "04" => "may_day",
                "09" => "mua_rao",
                "10" => "mua",
                "11" => "giong_bao",
                "13" => "tuyet",
                "50" => "suong_mu",
                _ => "may_day"
            };

            // Xác định hậu tố ngày/đêm
            string hauTo = code3.EndsWith("d", StringComparison.OrdinalIgnoreCase) ? "_ngay" :
                           (code3.EndsWith("n", StringComparison.OrdinalIgnoreCase) ? "_dem" : string.Empty);

            var tenUuTien = goc + hauTo;        // ví dụ: giong_bao_ngay.png
            var tenFallback = goc;               // ví dụ: giong_bao.png

            System.Diagnostics.Debug.WriteLine($"Tìm icon: {iconCode} -> {tenUuTien} hoặc {tenFallback}");

            // 1) Thử lấy từ tài nguyên nhúng (Form1.resx) theo tên ưu tiên rồi fallback
            var tuNhung = TaiAnhTaiNguyen(tenUuTien) ?? TaiAnhTaiNguyen(tenFallback);
            if (tuNhung != null)
            {
                System.Diagnostics.Debug.WriteLine($"Tìm thấy icon từ tài nguyên: {tenUuTien}");
                return tuNhung;
            }

            // 2) Lấy từ thư mục Resources cạnh .exe theo tên ưu tiên rồi fallback
            var thuMuc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var tuFile = TaiAnh(Path.Combine(thuMuc, tenUuTien + ".png"))
                        ?? TaiAnh(Path.Combine(thuMuc, tenFallback + ".png"));

            if (tuFile != null)
            {
                System.Diagnostics.Debug.WriteLine($"Tìm thấy icon từ file: {tenUuTien}.png");
                return tuFile;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Không tìm thấy icon: {tenUuTien}.png hoặc {tenFallback}.png");
                // Tạo icon test để hiển thị
                System.Diagnostics.Debug.WriteLine($"Tạo icon test: {tenUuTien}");
                return TaoIconTest(tenUuTien);
            }
        }

        /// <summary>
        /// Đổi nền động theo mã thời tiết cho toàn bộ giao diện
        /// </summary>
        private void HienThiIconVaNen(int ma, string iconCode)
        {
            System.Diagnostics.Debug.WriteLine($"Hiển thị icon và nền: ma={ma}, iconCode={iconCode}");

            anhIconThoiTiet.Image = ChonIconTheoIconCode(iconCode) ?? ChonIconTheoMa(ma);

            // Chọn nền GIF theo IconCode để khớp với icon
            var tenNen = ChonTenNenTheoIconCode(iconCode);
            if (string.IsNullOrEmpty(tenNen))
            {
                // Fallback theo mã thời tiết cũ nếu không có IconCode
                if (ma >= 200 && ma <= 232) tenNen = "nen_giong.gif";
                else if ((ma >= 300 && ma <= 321) || (ma >= 500 && ma <= 531)) tenNen = "nen_mua.jpg";
                else if (ma >= 600 && ma <= 622) tenNen = "nen_tuyet.jpg";
                else if (ma == 800) tenNen = "nen_troi_quang.jpg";
                else tenNen = "nen_mua.jpg";
            }

            System.Diagnostics.Debug.WriteLine($"Tìm nền: {tenNen}");

            // Thử nhiều đường dẫn khác nhau
            var duongDan = "";
            var thuMuc1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            var thuMuc2 = Path.Combine(Environment.CurrentDirectory, "Resources");
            var thuMuc3 = Path.Combine(Application.StartupPath, "Resources");

            if (File.Exists(Path.Combine(thuMuc1, tenNen)))
                duongDan = Path.Combine(thuMuc1, tenNen);
            else if (File.Exists(Path.Combine(thuMuc2, tenNen)))
                duongDan = Path.Combine(thuMuc2, tenNen);
            else if (File.Exists(Path.Combine(thuMuc3, tenNen)))
                duongDan = Path.Combine(thuMuc3, tenNen);

            System.Diagnostics.Debug.WriteLine($"Đường dẫn nền: {duongDan}");
            System.Diagnostics.Debug.WriteLine($"File tồn tại: {File.Exists(duongDan)}");

            Image? nenHinh = null;
            if (!string.IsNullOrEmpty(duongDan) && File.Exists(duongDan))
            {
                try
                {
                    // Tải ảnh GIF động
                    nenHinh = Image.FromFile(duongDan);
                    System.Diagnostics.Debug.WriteLine($"Đã tải nền thành công: {tenNen}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi tải nền: {ex.Message}");
                    nenHinh = TaoBackgroundTest(tenNen);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Không tìm thấy file nền: {tenNen}");
                nenHinh = TaoBackgroundTest(tenNen);
            }

            // Tạo nền toàn cục cho toàn bộ form
            TaoNenToanCuc(nenHinh);
        }

        /// <summary>
        /// Tạo nền toàn cục cho toàn bộ giao diện
        /// </summary>
        private void TaoNenToanCuc(Image? nenHinh)
        {
            if (nenHinh == null)
            {
                System.Diagnostics.Debug.WriteLine("NenHinh is null, không thể tạo nền");
                return;
            }

            try
            {
                // Xóa nền cũ nếu có
                var nenCu = this.Controls.OfType<PictureBox>().FirstOrDefault(p => p.Name == "NenToanCuc");
                if (nenCu != null)
                {
                    this.Controls.Remove(nenCu);
                    nenCu.Dispose();
                }

                // Tạo PictureBox nền toàn cục - TO NHẤT
                var nenToanCuc = new PictureBox
                {
                    Image = nenHinh,
                    SizeMode = PictureBoxSizeMode.Zoom, // Zoom để nền to nhất
                    Dock = DockStyle.Fill,
                    Location = new Point(0, 0),
                    Size = this.Size,
                    BackColor = Color.Transparent
                };

                // Thêm nền mới vào form
                nenToanCuc.Name = "NenToanCuc";
                this.Controls.Add(nenToanCuc);
                nenToanCuc.SendToBack(); // Đưa xuống dưới cùng

                // Đảm bảo tất cả controls hiển thị trên nền
                thanhTrenCung.BringToFront();
                boCucChinh.BringToFront();

                // Đảm bảo các panel chính hiển thị rõ ràng
                khuVucTrai_HienTai.BringToFront();
                khuVucPhai_5Ngay.BringToFront();
                khuVucDuoi_24Gio.BringToFront();

                // Refresh để đảm bảo hiển thị
                nenToanCuc.Refresh();
                this.Refresh();

                System.Diagnostics.Debug.WriteLine($"Đã tạo nền toàn cục thành công - Kích thước: {nenHinh.Width}x{nenHinh.Height}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo nền toàn cục: {ex.Message}");
            }
        }

        // Chọn tên nền GIF theo IconCode để khớp với icon (1:1 mapping)
        private static string ChonTenNenTheoIconCode(string iconCode)
        {
            if (string.IsNullOrWhiteSpace(iconCode)) return "";
            var code2 = iconCode.Length >= 2 ? iconCode.Substring(0, 2) : iconCode;
            return code2 switch
            {
                "01" => "nen_troi_quang.jpg",        // trời quang
                "02" => "nen_it_may.gif",            // ít mây
                "03" => "nen_may_rac_rac.gif",       // mây rải rác
                "04" => "nen_may_day.gif",           // mây dày
                "09" => "nen_mua_rao.jpg",           // mưa rào
                "10" => "nen_mua.jpg",               // mưa
                "11" => "nen_giong_bao.jpg",         // giông bão
                "13" => "nen_tuyet.jpg",             // tuyết
                "50" => "nen_suong_mu.jpg",          // sương mù
                _ => "nen_may_day.gif"               // fallback
            };
        }

        private static Image? TaiAnh(string duongDan)
        {
            if (!File.Exists(duongDan))
            {
                System.Diagnostics.Debug.WriteLine($"File không tồn tại: {duongDan}");
                return null;
            }

            try
            {
                using var fs = new FileStream(duongDan, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                return Image.FromStream(fs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải file {duongDan}: {ex.Message}");
                return null;
            }
        }

        // Thử tải ảnh nhúng từ tài nguyên Form1.resx theo tên (không phần mở rộng)
        private static Image? TaiAnhTaiNguyen(string ten)
        {
            try
            {
                // Lấy từ Form1.resx thông qua ComponentResourceManager
                var rm = new ComponentResourceManager(typeof(Form1));
                var obj = rm.GetObject(ten);
                return obj as Image;
            }
            catch { return null; }
        }

        /// <summary>
        /// Tạo icon đơn giản để test khi không có file
        /// </summary>
        private static Image TaoIconTest(string tenIcon, int size = 64)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);

            // Nền trong suốt
            g.Clear(Color.Transparent);

            // Vẽ icon đơn giản dựa trên tên
            var brush = new SolidBrush(Color.White);
            var pen = new Pen(Color.White, 2);

            if (tenIcon.Contains("troi_quang"))
            {
                // Mặt trời
                g.FillEllipse(brush, size / 4, size / 4, size / 2, size / 2);
                for (int i = 0; i < 8; i++)
                {
                    var angle = i * Math.PI / 4;
                    var x1 = size / 2 + (int)(size / 3 * Math.Cos(angle));
                    var y1 = size / 2 + (int)(size / 3 * Math.Sin(angle));
                    var x2 = size / 2 + (int)(size / 2.5 * Math.Cos(angle));
                    var y2 = size / 2 + (int)(size / 2.5 * Math.Sin(angle));
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            else if (tenIcon.Contains("mua"))
            {
                // Mưa
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(pen, size / 4 + i * size / 4, size / 3, size / 4 + i * size / 4, size * 2 / 3);
                }
            }
            else if (tenIcon.Contains("may"))
            {
                // Mây
                g.FillEllipse(brush, size / 6, size / 3, size / 3, size / 4);
                g.FillEllipse(brush, size / 3, size / 3, size / 3, size / 4);
                g.FillEllipse(brush, size / 2, size / 3, size / 3, size / 4);
            }
            else
            {
                // Icon mặc định - hình tròn
                g.FillEllipse(brush, size / 4, size / 4, size / 2, size / 2);
            }

            return bmp;
        }

        /// <summary>
        /// Tạo file icon PNG thật và lưu vào thư mục Resources
        /// </summary>
        private static void TaoFileIconThuc()
        {
            try
            {
                // Tạo icon trời quang
                var iconTroiQuang = TaoIconTest("troi_quang_ngay", 128);
                var duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "troi_quang_ngay.png");
                iconTroiQuang.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                System.Diagnostics.Debug.WriteLine($"Đã tạo file icon: {duongDan}");

                // Tạo icon mưa
                var iconMua = TaoIconTest("mua", 128);
                duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mua.png");
                iconMua.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                System.Diagnostics.Debug.WriteLine($"Đã tạo file icon: {duongDan}");

                // Tạo icon mây
                var iconMay = TaoIconTest("may_day", 128);
                duongDan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "may_day.png");
                iconMay.Save(duongDan, System.Drawing.Imaging.ImageFormat.Png);
                System.Diagnostics.Debug.WriteLine($"Đã tạo file icon: {duongDan}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo file icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler cho ListBox địa điểm đã lưu
        /// </summary>
        private void listBoxDiaDiemDaLuu_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuKienChonDiaDiemDaLuu();
        }


        /// <summary>
        /// Tạo background test khi không có file GIF - TO NHẤT VÀ THAY ĐỔI THEO THỜI TIẾT
        /// </summary>
        private static Image TaoBackgroundTest(string tenNen)
        {
            var bmp = new Bitmap(1920, 1080); // Kích thước TO NHẤT để phù hợp với mọi màn hình
            using var g = Graphics.FromImage(bmp);

            // Gradient background dựa trên loại thời tiết - THAY ĐỔI THEO THỜI TIẾT
            if (tenNen.Contains("troi_quang"))
            {
                // Nền gradient đẹp như glassmorphism (xanh lam và tím nhạt) - TRỜI QUANG
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 135, 206, 235), Color.FromArgb(255, 186, 85, 211));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("mua"))
            {
                // Nền gradient xám (mưa) - MƯA
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 105, 105, 105), Color.FromArgb(255, 47, 79, 79));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("may"))
            {
                // Nền gradient xám nhạt (mây) - MÂY
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 169, 169, 169), Color.FromArgb(255, 128, 128, 128));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else if (tenNen.Contains("tuyet"))
            {
                // Nền gradient trắng (tuyết) - TUYẾT
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 240, 248, 255), Color.FromArgb(255, 176, 196, 222));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }
            else
            {
                // Nền mặc định - gradient xanh dương đẹp
                var brush = new LinearGradientBrush(new Point(0, 0), new Point(1920, 1080),
                    Color.FromArgb(255, 135, 206, 235), Color.FromArgb(255, 186, 85, 211));
                g.FillRectangle(brush, 0, 0, 1920, 1080);
            }

            return bmp;
        }

        private static DateTime UnixToLocal(long unix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().DateTime;
        }



        private void thanhTrenCung_Paint(object sender, PaintEventArgs e)
        {

        }

        private void anhNenDong_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Xử lý khi người dùng nhập text vào ô tìm kiếm
        /// </summary>

        /// <summary>
        /// Xử lý khi người dùng nhấn phím trong ô tìm kiếm
        /// </summary>
        private void oTimKiemDiaDiem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.SelectedIndex >= 0 && listBoxGoiY.SelectedItem != null)
                    {
                        oTimKiemDiaDiem.Text = listBoxGoiY.SelectedItem.ToString();
                        listBoxGoiY.Visible = false;
                        _ = TimKiemDiaDiem(oTimKiemDiaDiem.Text.Trim());
                    }
                    else
                    {
                        _ = TimKiemDiaDiem(oTimKiemDiaDiem.Text.Trim());
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    listBoxGoiY.Visible = false;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.Items.Count > 0)
                    {
                        listBoxGoiY.Focus();
                        if (listBoxGoiY.SelectedIndex < listBoxGoiY.Items.Count - 1)
                            listBoxGoiY.SelectedIndex++;
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    if (listBoxGoiY.Visible && listBoxGoiY.Items.Count > 0)
                    {
                        listBoxGoiY.Focus();
                        if (listBoxGoiY.SelectedIndex > 0)
                            listBoxGoiY.SelectedIndex--;
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xử lý phím: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý khi người dùng chọn một gợi ý
        /// </summary>
        private void listBoxGoiY_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBoxGoiY.SelectedIndex >= 0 && listBoxGoiY.SelectedItem != null)
                {
                    oTimKiemDiaDiem.Text = listBoxGoiY.SelectedItem.ToString();
                    listBoxGoiY.Visible = false;
                    oTimKiemDiaDiem.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi chọn gợi ý: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn phím Enter trong ô tìm kiếm
        /// </summary>
        private async void oTimKiemDiaDiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                string diaDiem = oTimKiemDiaDiem.Text.Trim();
                if (!string.IsNullOrEmpty(diaDiem))
                {
                    await TimKiemDiaDiem(diaDiem);
                }
            }
        }

        /// <summary>
        /// Tìm kiếm địa điểm và lấy dữ liệu thời tiết
        /// </summary>
        private async Task TimKiemDiaDiem(string diaDiem)
        {
            try
            {
                MessageBox.Show($"Bắt đầu tìm kiếm địa điểm: {diaDiem}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Lấy tọa độ từ tên địa điểm
                var geocodingData = await WeatherApiService.GetCoordinatesAsync(diaDiem);
                MessageBox.Show($"Kết quả geocoding: {(geocodingData?.Results?.Length > 0 ? "Thành công" : "Thất bại")}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (geocodingData?.Results?.Length > 0)
                {
                    var result = geocodingData.Results[0];
                    currentLat = result.Lat;
                    currentLon = result.Lon;
                    currentLocation = $"{result.Name}, {result.Country}";

                    // Lấy dữ liệu thời tiết
                    try
                    {
                        weatherData = await WeatherApiService.GetWeatherDataAsync(currentLat, currentLon);
                        if (weatherData != null)
                        {
                            // Hiển thị thông tin đầy đủ
                            HienThiThongTin(currentLocation, weatherData);

                            // Lưu địa điểm vào file
                            LuuDiaDiem(currentLocation, currentLat, currentLon);
                        }
                        else
                        {
                            MessageBox.Show("API trả về null. Vui lòng kiểm tra API key hoặc kết nối mạng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception apiEx)
                    {
                        MessageBox.Show($"Lỗi khi gọi API thời tiết: {apiEx.Message}", "Lỗi API", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy địa điểm. Vui lòng thử lại với tên địa điểm khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tìm kiếm gợi ý địa điểm dựa trên text nhập vào
        /// </summary>
        private List<string> TimKiemGoiYDiaDiem(string searchText)
        {
            // Để trống - chỉ tìm kiếm qua API
            return new List<string>();
        }

        /// <summary>
        /// Tạo nội dung cho các panel chi tiết thời tiết
        /// </summary>
        private void TaoNoiDungPanelChiTiet()
        {
            try
            {
                // Panel cảm giác thực tế
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác", "--");

                // Panel độ ẩm
                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", "--");

                // Panel gió
                TaoPanelChiTiet(windPanel, "💨", "Gió", "--");

                // Panel áp suất
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất", "--");

                // Panel tầm nhìn
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn", "--");

                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo nội dung cho một panel chi tiết
        /// </summary>
        private void TaoPanelChiTiet(Panel panel, string icon, string title, string value)
        {
            try
            {
                panel.Controls.Clear();
                
                // Bo tròn viền cho panel
                ApplyRoundedCorners(panel, 15);

                // Label icon
                var iconLabel = new Label
                {
                    Text = icon,
                    Font = new Font("Segoe UI Emoji", 20F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(10, 10)
                };

                // Label title - căn giữa và hiển thị đầy đủ
                var titleLabel = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular), // Giữ font size 10 như ban đầu
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(5, 45), // Dịch chữ xuống từ 35 xuống 45 để tránh bị che bởi icon
                    Size = new Size(panel.Width - 10, 18), // Giữ kích thước ban đầu
                    AutoEllipsis = false, // Tắt AutoEllipsis để hiển thị đầy đủ
                    AutoSize = false // Đảm bảo không tự động resize
                };

                // Label value - căn giữa
                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 13F, FontStyle.Regular),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(5, 60), // Dịch chữ xuống từ 50 xuống 60 để tránh chồng lên title
                    Size = new Size(panel.Width - 10, 25)
                };

                panel.Controls.Add(iconLabel);
                panel.Controls.Add(titleLabel);
                panel.Controls.Add(valueLabel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo panel {panel.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật các panel chi tiết với dữ liệu thời tiết thực
        /// </summary>
        private void CapNhatPanelChiTiet(ThoiTietHienTai hienTai, string kyHieuNhietDo)
        {
            try
            {
                // Panel cảm giác thực tế
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác như", $"{Math.Round(hienTai.NhietDoCamGiac)}{kyHieuNhietDo}");

                // Panel độ ẩm
                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", $"{hienTai.DoAm}%");

                // Panel gió
                var donViGio = donViCelsius ? "m/s" : "mph";
                TaoPanelChiTiet(windPanel, "💨", "Tốc độ gió", $"{Math.Round(hienTai.TocDoGio)} {donViGio}");

                // Panel áp suất
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất khí quyển", $"{hienTai.ApSuat} hPa");

                // Panel tầm nhìn
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn xa", $"{hienTai.TamNhin / 1000.0:0.0} km");

                // Đã xóa sunrisePanel
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo dữ liệu mẫu cho dự báo 5 ngày
        /// </summary>
        private void TaoDuLieuMau5Ngay()
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangNhieuNgay.Controls.Clear();
        }

        /// <summary>
        /// Tạo panel cho dự báo một ngày (phiên bản mới giống panel chi tiết)
        /// </summary>

        private void TaoPanelDuBaoNgayMoi(Panel panel, string ngay, string nhietDo, string trangThai, string icon)
        {
            try
            {
                panel.Controls.Clear();
                panel.Padding = new Padding(8);

                // Tạo nền xám nhạt cho từng panel riêng biệt với viền đậm như panel chi tiết
                panel.BackColor = Color.FromArgb(80, 128, 128, 128);
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.Paint += (s, e) =>
                {
                    // Vẽ viền đậm như panel chi tiết
                    using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 2))
                    {
                        var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                };

                // Icon thời tiết bên trái
                var iconLabel = new Label
                {
                    Text = icon,
                    Font = new Font("Segoe UI Emoji", 24F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(8, 8),
                    Size = new Size(40, 40)
                };

                // Ngày/thứ ở giữa trên
                var ngayLabel = new Label
                {
                    Text = ngay,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(55, 8),
                    Size = new Size(panel.Width - 120, 25)
                };

                // Mô tả thời tiết ở giữa dưới
                var trangThaiLabel = new Label
                {
                    Text = trangThai,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(55, 35),
                    Size = new Size(panel.Width - 120, 20)
                };

                // Nhiệt độ to ở bên phải
                var nhietDoLabel = new Label
                {
                    Text = nhietDo,
                    Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(panel.Width - 60, 8),
                    Size = new Size(50, 40)
                };

                panel.Controls.Add(iconLabel);
                panel.Controls.Add(ngayLabel);
                panel.Controls.Add(trangThaiLabel);
                panel.Controls.Add(nhietDoLabel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo panel dự báo ngày mới: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo panel cho dự báo một ngày
        /// </summary>
        private Panel TaoPanelDuBaoNgay(string ngay, string nhietDo, string trangThai, string icon)
        {
            var panel = new Panel
            {
                BackColor = Color.FromArgb(120, 255, 255, 255),
                Size = new Size(400, 60),
                Margin = new Padding(5),
                Padding = new Padding(10)
            };

            // Label ngày
            var ngayLabel = new Label
            {
                Text = ngay,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(10, 5),
                Size = new Size(100, 25)
            };

            // Label icon
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(120, 5),
                Size = new Size(30, 30)
            };

            // Label nhiệt độ
            var nhietDoLabel = new Label
            {
                Text = nhietDo,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(160, 5),
                Size = new Size(60, 25)
            };

            // Label trạng thái
            var trangThaiLabel = new Label
            {
                Text = trangThai,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(10, 35),
                Size = new Size(200, 20)
            };

            panel.Controls.Add(ngayLabel);
            panel.Controls.Add(iconLabel);
            panel.Controls.Add(nhietDoLabel);
            panel.Controls.Add(trangThaiLabel);

            return panel;
        }

        /// <summary>
        /// Tạo dữ liệu mẫu cho dự báo 24 giờ
        /// </summary>
        private void TaoDuLieuMau24Gio()
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangTheoGio.Controls.Clear();
        }

        /// <summary>
        /// Tạo panel cho dự báo một giờ (phiên bản mới giống panel chi tiết)
        /// </summary>
        /// <summary>
        /// Tạo card dự báo giờ với layout chuẩn
        /// </summary>
        private Panel TaoCardGio(HourlyWeather hour, string kyHieu)
        {
            try
            {
                var panel = new Panel
                {
                    Size = new Size(140, 200),
                    BackColor = Color.FromArgb(80, 128, 128, 128),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(6)
                };

                // Bo viền tròn
                ApplyRoundedCorners(panel, 15);

                // Tạo TableLayoutPanel với 4 hàng, 1 cột
                var tlpHourlyCard = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(5)
                };

                // Cấu hình RowStyles
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F)); // Hàng 0: Giờ
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F)); // Hàng 1: Nhiệt độ
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Hàng 2: Mô tả (AutoSize)
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Hàng 3: Icon

                // Hàng 0: Giờ (trên cùng)
                var time = UnixToLocal(hour.Dt);
                var lblHour = new Label
                {
                    Text = time.ToString("HH:mm"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                // Hàng 1: Nhiệt độ (to nhất)
                var tempInUnit = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                              : TemperatureConverter.ToFahrenheit(hour.Temp);
                var lblTemp = new Label
                {
                    Text = $"{Math.Round(tempInUnit)}{kyHieu}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 20F, FontStyle.Bold), // Font lớn nhất
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                // Hàng 2: Mô tả (tự xuống dòng)
                var lblDesc = new Label
                {
                    Text = hour.Weather?[0]?.Description ?? "N/A",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    MaximumSize = new Size(130, 0), // Trừ padding
                    UseCompatibleTextRendering = true,
                    AutoEllipsis = false
                };

                // Hàng 3: Icon (dưới cùng)
                var picIcon = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };

                // Load icon vào PictureBox
                try
                {
                    var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Weather?[0]?.Icon));
                    if (iconImage != null)
                    {
                        picIcon.Image = iconImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi load icon: {ex.Message}");
                }

                // Thêm các control vào TableLayoutPanel
                tlpHourlyCard.Controls.Add(lblHour, 0, 0);
                tlpHourlyCard.Controls.Add(lblTemp, 0, 1);
                tlpHourlyCard.Controls.Add(lblDesc, 0, 2);
                tlpHourlyCard.Controls.Add(picIcon, 0, 3);

                // Thêm TableLayoutPanel vào panel chính
                panel.Controls.Add(tlpHourlyCard);

                // Thêm click handler
                panel.Click += (s, e) =>
                {
                    // Cập nhật thông tin chính với dữ liệu từ giờ được chọn
                    var tempDisp = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                                : TemperatureConverter.ToFahrenheit(hour.Temp);
                    nhanNhietDoHienTai.Text = $"{Math.Round(tempDisp)}{kyHieu}";
                    nhanTrangThai.Text = hour.Weather?[0]?.Description ?? "N/A";

                    // Cập nhật icon thời tiết
                    if (anhIconThoiTiet != null)
                    {
                        anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Weather?[0]?.Icon));
                    }

                    // Cập nhật các panel chi tiết với dữ liệu từ giờ được chọn
                    CapNhatPanelChiTietFromHourlyApi(hour, kyHieu);

                    // Cập nhật background theo thời tiết
                    SetBackground(hour.Weather?[0]?.Main ?? "Clear", hour.Weather?[0]?.Id ?? 800);
                };

                return panel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card giờ: {ex.Message}");
                return new Panel();
            }
        }

        /// <summary>
        /// Load dự báo 24 giờ
        /// </summary>
        private void LoadDuBao24h(HourlyWeather[] hourlyList, string kyHieu)
        {
            try
            {
                if (BangTheoGio != null)
                {
                    BangTheoGio.Controls.Clear();

                    // Lấy 24 giờ đầu tiên
                    var data24h = hourlyList.Take(24).ToArray();

                    foreach (var hour in data24h)
                    {
                        var card = TaoCardGio(hour, kyHieu);
                        BangTheoGio.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dự báo 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo card dự báo ngày với layout chuẩn
        /// </summary>
        private Panel TaoCardNgay(DailyWeather daily, string kyHieu)
        {
            try
            {
                var panel = new Panel
                {
                    Size = new Size(430, 70), // Tăng chiều cao để chứa label nhiệt độ lớn hơn
                    BackColor = Color.FromArgb(80, 128, 128, 128),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(3),
                    Padding = new Padding(8)
                };

                // Bo viền tròn
                ApplyRoundedCorners(panel, 15);

                // Layout mới: Icon bên trái, thông tin bên phải

                // 1. Icon thời tiết (bên trái)
                var picIcon = new PictureBox
                {
                    Location = new Point(8, 8),
                    Size = new Size(44, 44),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };

                // Load icon vào PictureBox
                try
                {
                    string iconCode = daily.Weather?[0]?.Icon ?? "01d";
                    string iconPath = Path.Combine(Application.StartupPath, "Resources", $"{iconCode}.png");
                    if (File.Exists(iconPath))
                    {
                        picIcon.Image = Image.FromFile(iconPath);
                    }
                    else
                    {
                        // Fallback: sử dụng emoji icon
                        var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(iconCode));
                        if (iconImage != null)
                        {
                            picIcon.Image = iconImage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi load icon: {ex.Message}");
                }

                // 2. Ngày trong tuần + ngày/tháng (tiếng Việt)
                var date = UnixToLocal(daily.Dt);
                var lblDay = new Label
                {
                    Text = GetVietnameseDayName(daily.Dt),
                    Location = new Point(60, 8),
                    Size = new Size(150, 20), // Tăng chiều rộng để đủ chỗ cho "Hôm nay"
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true // Để text tự động điều chỉnh kích thước
                };

                // 3. Mô tả thời tiết (tiếng Việt)
                var lblDesc = new Label
                {
                    Text = GetVietnameseWeatherDescription(daily.Weather?[0]?.Description ?? "N/A"),
                    Location = new Point(60, 30),
                    Size = new Size(150, 20),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.LightGray,
                    BackColor = Color.Transparent,
                    AutoSize = true // Để text tự động điều chỉnh kích thước
                };

                // 4. Nhiệt độ cao/thấp (nổi bật)
                var tempMaxInUnit = donViCelsius ? TemperatureConverter.ToCelsius(daily.Temp.Max)
                                                 : TemperatureConverter.ToFahrenheit(daily.Temp.Max);
                var tempMinInUnit = donViCelsius ? TemperatureConverter.ToCelsius(daily.Temp.Min)
                                                 : TemperatureConverter.ToFahrenheit(daily.Temp.Min);
                var lblTemp = new Label
                {
                    Text = $"Cao nhất: {Math.Round(tempMaxInUnit)}°{kyHieu}\nThấp nhất: {Math.Round(tempMinInUnit)}°{kyHieu}",
                    Location = new Point(220, 8),
                    Size = new Size(120, 60), // Tăng chiều cao để hiển thị đầy đủ cả hai dòng
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold), // Giảm font size một chút
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // 5. Thông tin mưa và gió
                var lblRainWind = new Label
                {
                    Text = GetRainWindInfo(daily),
                    Location = new Point(350, 8),
                    Size = new Size(70, 50),
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.LightBlue,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Thêm các control vào panel
                panel.Controls.Add(picIcon);
                panel.Controls.Add(lblDay);
                panel.Controls.Add(lblDesc);
                panel.Controls.Add(lblTemp);
                panel.Controls.Add(lblRainWind);

                // Thêm click event cho tất cả control con để đảm bảo click được truyền lên panel cha
                // Sử dụng Tag để lưu reference đến panel cha
                picIcon.Tag = panel;
                lblDay.Tag = panel;
                lblDesc.Tag = panel;
                lblTemp.Tag = panel;
                lblRainWind.Tag = panel;

                return panel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card ngày: {ex.Message}");
                return new Panel();
            }
        }

        /// <summary>
        /// Load dự báo 5 ngày với click event
        /// </summary>
        private void LoadForecast5Days(DailyWeather[] dailyList, string kyHieu)
        {
            try
            {
                if (BangNhieuNgay != null)
                {
                    BangNhieuNgay.Controls.Clear();

                    // Lấy 5 ngày đầu tiên
                    var data5Ngay = dailyList.Take(5).ToArray();

                    for (int i = 0; i < data5Ngay.Length; i++)
                    {
                        var daily = data5Ngay[i];
                        var card = TaoCardNgay(daily, kyHieu);
                        
                        // Thêm click event để chuyển sang biểu đồ 24h
                        int dayIndex = i; // Capture index để tránh closure issue
                        card.Click += (sender, e) => OnDayCardClicked(dayIndex, daily);
                        
                        // Thêm click event cho tất cả control con
                        foreach (Control control in card.Controls)
                        {
                            control.Click += (s, e) => OnDayCardClicked(dayIndex, daily);
                            control.Cursor = Cursors.Hand;
                        }
                        
                        // Thêm cursor pointer để hiển thị có thể click
                        card.Cursor = Cursors.Hand;
                        
                        System.Diagnostics.Debug.WriteLine($"Tạo card cho ngày {i}: {GetVietnameseDayName(daily.Dt)}");
                        
                        BangNhieuNgay.Controls.Add(card);
                    }

                    // Hiển thị mặc định biểu đồ 24h cho ngày đầu tiên và chọn tab Biểu đồ
                    if (data5Ngay.Length > 0)
                    {
                        Show24hChartForDay(data5Ngay[0]);
                        try { tabDieuKhien.SelectedTab = tabChart; } catch {}
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dự báo 5 ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Xử lý khi click vào card ngày
        /// </summary>
        private void OnDayCardClicked(int dayIndex, DailyWeather daily)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Click vào card ngày {dayIndex}: {GetVietnameseDayName(daily.Dt)}");
                
                selectedDayIndex = dayIndex;
                
                // Cập nhật biểu đồ 24h cho ngày được chọn
                Show24hChartForDay(daily);
                
                // Highlight card được chọn (optional)
                HighlightSelectedDayCard(dayIndex);
                
                System.Diagnostics.Debug.WriteLine($"Đã cập nhật biểu đồ cho ngày {dayIndex}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi click card ngày: {ex.Message}");
            }
        }

        /// <summary>
        /// Highlight card ngày được chọn
        /// </summary>
        private void HighlightSelectedDayCard(int dayIndex)
        {
            try
            {
                if (BangNhieuNgay?.Controls.Count > dayIndex)
                {
                    // Reset tất cả cards về màu bình thường
                    foreach (Control control in BangNhieuNgay.Controls)
                    {
                        if (control is Panel panel)
                        {
                            panel.BackColor = Color.FromArgb(80, 128, 128, 128);
                        }
                    }
                    
                    // Highlight card được chọn
                    var selectedCard = BangNhieuNgay.Controls[dayIndex] as Panel;
                    if (selectedCard != null)
                    {
                        selectedCard.BackColor = Color.FromArgb(120, 255, 255, 255);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi highlight card: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị biểu đồ nhiệt độ 24h cho ngày được chọn
        /// </summary>
        private void Show24hChartForDay(DailyWeather daily)
        {
            try
            {
                if (weatherData?.Hourly == null || weatherData.Hourly.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Không có dữ liệu hourly để tạo biểu đồ");
                    return;
                }

                // Khởi tạo Chart nếu chưa có
                if (temperatureChart == null)
                {
                    InitializeTemperatureChart();
                }

                // Lấy dữ liệu 24h cho ngày được chọn
                var dayStart = UnixToLocal(daily.Dt).Date;
                var dayEnd = dayStart.AddDays(1);
                
                System.Diagnostics.Debug.WriteLine($"Tìm dữ liệu hourly cho ngày: {dayStart:yyyy-MM-dd} đến {dayEnd:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"Tổng số hourly data: {weatherData.Hourly.Length}");
                
                // Thử filter theo ngày trước
                var hourlyData = weatherData.Hourly
                    .Where(h => 
                    {
                        var hourTime = UnixToLocal(h.Dt);
                        var isInRange = hourTime >= dayStart && hourTime < dayEnd;
                        return isInRange;
                    })
                    .Take(24)
                    .ToArray();

                System.Diagnostics.Debug.WriteLine($"Tìm thấy {hourlyData.Length} điểm dữ liệu hourly sau filter");

                // Nếu không đủ dữ liệu, sử dụng fallback
                if (hourlyData.Length < 12) // Ít hơn 12 giờ thì không đủ
                {
                    System.Diagnostics.Debug.WriteLine($"Không đủ dữ liệu hourly cho ngày {dayStart:yyyy-MM-dd}, sử dụng fallback");
                    
                    // Fallback: Lấy 24 giờ đầu tiên từ dữ liệu hourly
                    hourlyData = weatherData.Hourly.Take(24).ToArray();
                    System.Diagnostics.Debug.WriteLine($"Sử dụng fallback: {hourlyData.Length} điểm dữ liệu");
                }

                // Xóa dữ liệu cũ
                temperatureChart.Series.Clear();

                // Tạo series cột
                var series = new Series("Nhiệt độ")
                {
                    ChartType = SeriesChartType.Column,
                    Color = Color.FromArgb(200, 100, 200, 255),
                    BorderWidth = 1,
                    IsValueShownAsLabel = false
                };
                series["PointWidth"] = "0.6"; // Độ rộng cột

                // Thêm dữ liệu điểm
                foreach (var hour in hourlyData)
                {
                    var hourTime = UnixToLocal(hour.Dt);
                    var temperature = donViCelsius ? TemperatureConverter.ToCelsius(hour.Temp)
                                                  : TemperatureConverter.ToFahrenheit(hour.Temp);
                    
                    var pointIndex = series.Points.AddXY(hourTime.Hour, temperature);
                    var point = series.Points[pointIndex];
                    point.ToolTip = $"Giờ: {hourTime:HH:mm}\nNhiệt độ: {temperature:F1}°{(donViCelsius ? "C" : "F")}\nTrạng thái: {hour.Weather?[0]?.Description ?? "N/A"}";

                    // Không hiển thị icon trong biểu đồ
                }

                temperatureChart.Series.Add(series);

                // Bỏ đánh dấu chữ T và C

                // Cấu hình trục X
                temperatureChart.ChartAreas[0].AxisX.Title = "Giờ";
                temperatureChart.ChartAreas[0].AxisX.TitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
                temperatureChart.ChartAreas[0].AxisX.TitleForeColor = Color.White;
                temperatureChart.ChartAreas[0].AxisX.Minimum = 0;
                temperatureChart.ChartAreas[0].AxisX.Maximum = 23;
                temperatureChart.ChartAreas[0].AxisX.Interval = 1; // hiện mỗi giờ: 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23
                temperatureChart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Segoe UI", 7, FontStyle.Regular);
                temperatureChart.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                temperatureChart.ChartAreas[0].AxisX.LineColor = Color.FromArgb(200, 255, 255, 255);
                temperatureChart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(100, 255, 255, 255);
                temperatureChart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                // Cấu hình trục Y
                temperatureChart.ChartAreas[0].AxisY.Title = $"Nhiệt độ °{(donViCelsius ? "C" : "F")}";
                temperatureChart.ChartAreas[0].AxisY.TitleForeColor = Color.White;
                temperatureChart.ChartAreas[0].AxisY.TitleFont = new Font("Segoe UI", 12, FontStyle.Regular);
                // Điều chỉnh trục Y theo dải °C/°F hợp lý
                if (donViCelsius)
                {
                    temperatureChart.ChartAreas[0].AxisY.Minimum = -10;
                    temperatureChart.ChartAreas[0].AxisY.Maximum = 50;
                    temperatureChart.ChartAreas[0].AxisY.Interval = 5;
                }
                else
                {
                    temperatureChart.ChartAreas[0].AxisY.Minimum = 10;  // ≈ 14°F ~ -10°C
                    temperatureChart.ChartAreas[0].AxisY.Maximum = 120; // ≈ 122°F ~ 50°C
                    temperatureChart.ChartAreas[0].AxisY.Interval = 10;
                }
                temperatureChart.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                temperatureChart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                temperatureChart.ChartAreas[0].AxisY.LineColor = Color.FromArgb(200, 255, 255, 255);
                temperatureChart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(100, 255, 255, 255);
                temperatureChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                temperatureChart.Titles[0].Font = new Font("Segoe UI", 16F, FontStyle.Regular);

                // Tự điều chỉnh dải trục Y theo dữ liệu, cộng trừ 3° đệm
                var allVals = series.Points.Select(p => p.YValues[0]).ToArray();
                if (allVals.Length > 0)
                {
                    double min = allVals.Min();
                    double max = allVals.Max();
                    temperatureChart.ChartAreas[0].AxisY.Minimum = Math.Floor(min - 3);
                    temperatureChart.ChartAreas[0].AxisY.Maximum = Math.Ceiling(max + 3);
                }

                // Cấu hình tiêu đề
                temperatureChart.Titles.Clear();
                temperatureChart.Titles.Add($"Biểu đồ nhiệt độ 24h - {GetVietnameseDayName(daily.Dt)}");
                temperatureChart.Titles[0].Font = new Font("Segoe UI", 16, FontStyle.Regular);
                temperatureChart.Titles[0].ForeColor = Color.White;

                // Cấu hình màu nền
                temperatureChart.BackColor = Color.FromArgb(40, 20, 40, 60);
                temperatureChart.ChartAreas[0].BackColor = Color.FromArgb(20, 30, 50, 70);
                temperatureChart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                temperatureChart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                System.Diagnostics.Debug.WriteLine($"Đã tạo biểu đồ với {hourlyData.Length} điểm dữ liệu");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo biểu đồ: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi tạo Chart nhiệt độ
        /// </summary>
        private void CreateDailyComparisonChart(GroupBox parent)
        {
            try
            {
                var chart = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40),
                    AntiAliasing = AntiAliasingStyles.All
                };

                var chartArea = new ChartArea("ComparisonArea")
                {
                    BackColor = Color.FromArgb(20, 30, 40, 50),
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 1
                };

                // Cấu hình trục
                chartArea.AxisX.Interval = 1;
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.Title = "Nhiệt độ (°C)";
                chartArea.AxisX.TitleFont = new Font("Segoe UI", 10F);
                
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.Title = "Ngày";
                chartArea.AxisY.TitleFont = new Font("Segoe UI", 10F);

                chart.ChartAreas.Add(chartArea);

                // Tạo series cho hôm nay và hôm qua
                var todaySeries = new Series("Hôm nay")
                {
                    ChartType = SeriesChartType.Bar,
                    Color = Color.FromArgb(255, 255, 159, 67),
                    BorderColor = Color.FromArgb(255, 255, 159, 67),
                    BorderWidth = 2
                };

                var yesterdaySeries = new Series("Hôm qua")
                {
                    ChartType = SeriesChartType.Bar,
                    Color = Color.FromArgb(255, 74, 144, 226),
                    BorderColor = Color.FromArgb(255, 74, 144, 226),
                    BorderWidth = 2
                };

                // Thêm dữ liệu mẫu
                todaySeries.Points.AddXY(25, "Tối thiểu");
                todaySeries.Points.AddXY(34, "Tối đa");
                
                yesterdaySeries.Points.AddXY(25, "Tối thiểu");
                yesterdaySeries.Points.AddXY(32, "Tối đa");

                chart.Series.Add(todaySeries);
                chart.Series.Add(yesterdaySeries);

                parent.Controls.Add(chart);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo biểu đồ so sánh: {ex.Message}");
            }
        }

        private void CreateRainProbabilityChart(GroupBox parent)
        {
            try
            {
                var chart = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40),
                    AntiAliasing = AntiAliasingStyles.All
                };

                var chartArea = new ChartArea("RainProbArea")
                {
                    BackColor = Color.Transparent, // Nền trong suốt
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 1
                };

                // Cấu hình trục
                chartArea.AxisX.Interval = 6; // Mỗi 6 giờ
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisX.Title = "Giờ";
                chartArea.AxisX.TitleFont = new Font("Segoe UI", 10F);
                chartArea.AxisX.Minimum = 0;
                chartArea.AxisX.Maximum = 24;
                
                chartArea.AxisY.Interval = 20; // Mỗi 20%
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;
                chartArea.AxisY.Title = "Tỉ lệ (%)";
                chartArea.AxisY.TitleFont = new Font("Segoe UI", 10F);
                chartArea.AxisY.Minimum = 0;
                chartArea.AxisY.Maximum = 100;

                // Cấu hình grid
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisX.MajorGrid.Enabled = true;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

                chart.ChartAreas.Add(chartArea);

                // Tạo series cho tỉ lệ mưa (SplineArea)
                var rainProbSeries = new Series("Tỉ lệ mưa")
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.FromArgb(120, Color.DeepSkyBlue), // Xanh nhạt có độ trong suốt
                    BorderColor = Color.DeepSkyBlue,
                    BorderWidth = 2
                };

                // Thêm dữ liệu mẫu (0% mưa trong ngày)
                for (int hour = 0; hour < 24; hour += 6)
                {
                    rainProbSeries.Points.AddXY(hour, 0); // 0% mưa
                }

                chart.Series.Add(rainProbSeries);

                // Thêm text hiển thị tỉ lệ mưa hôm nay
                var title = new Title("Khả năng có mưa vào hôm nay: 0%")
                {
                    Font = new Font("Segoe UI", 11F),
                    ForeColor = Color.White,
                    Docking = Docking.Top
                };
                chart.Titles.Add(title);

                parent.Controls.Add(chart);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo biểu đồ tỉ lệ mưa: {ex.Message}");
            }
        }

        private void CreateRainfallSummary(GroupBox parent)
        {
            try
            {
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(10)
                };

                // 24 giờ qua
                var past24hPanel = new Panel
                {
                    Location = new Point(10, 10),
                    Size = new Size(parent.Width - 40, 50),
                    BackColor = Color.FromArgb(60, 74, 144, 226),
                    Padding = new Padding(10)
                };

                var past24hIcon = new Label
                {
                    Text = "🌧️",
                    Font = new Font("Segoe UI Emoji", 20F),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var past24hLabel = new Label
                {
                    Text = "24 GIỜ QUA",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(50, 10),
                    AutoSize = true
                };

                var past24hValue = new Label
                {
                    Text = "Mưa 3 mm",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.White,
                    Location = new Point(50, 30),
                    AutoSize = true
                };

                past24hPanel.Controls.Add(past24hIcon);
                past24hPanel.Controls.Add(past24hLabel);
                past24hPanel.Controls.Add(past24hValue);

                // 24 giờ tới
                var next24hPanel = new Panel
                {
                    Location = new Point(10, 70),
                    Size = new Size(parent.Width - 40, 50),
                    BackColor = Color.FromArgb(60, 255, 159, 67),
                    Padding = new Padding(10)
                };

                var next24hIcon = new Label
                {
                    Text = "🌧️",
                    Font = new Font("Segoe UI Emoji", 20F),
                    ForeColor = Color.White,
                    Location = new Point(10, 10),
                    AutoSize = true
                };

                var next24hLabel = new Label
                {
                    Text = "24 GIỜ TỚI",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(50, 10),
                    AutoSize = true
                };

                var next24hValue = new Label
                {
                    Text = "Mưa 4 mm",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.White,
                    Location = new Point(50, 30),
                    AutoSize = true
                };

                next24hPanel.Controls.Add(next24hIcon);
                next24hPanel.Controls.Add(next24hLabel);
                next24hPanel.Controls.Add(next24hValue);

                panel.Controls.Add(past24hPanel);
                panel.Controls.Add(next24hPanel);
                parent.Controls.Add(panel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo tổng lượng mưa: {ex.Message}");
            }
        }

        private void InitializeTemperatureChart()
        {
            try
            {
                // Dùng 2 TabPage có sẵn trên giao diện: tabChart, tabMap

                temperatureChart = new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(30, 20, 30, 40), // Nền tối đẹp
                    AntiAliasing = AntiAliasingStyles.All,
                    TextAntiAliasingQuality = TextAntiAliasingQuality.High,
                    Margin = new Padding(0)
                };

                // Tạo ChartArea với gradient nền vàng-cam
                var chartArea = new ChartArea("MainArea")
                {
                    BackColor = Color.Orange, // Màu cam
                    BackSecondaryColor = Color.Yellow, // Màu vàng
                    BackGradientStyle = GradientStyle.TopBottom, // Gradient từ trên xuống
                    BorderColor = Color.FromArgb(100, 255, 255, 255),
                    BorderWidth = 2,
                    Position = new ElementPosition(0, 0, 100, 100),
                    InnerPlotPosition = new ElementPosition(12, 20, 82, 70)
                };

                // Cấu hình grid đẹp hơn
                chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(60, 255, 255, 255);
                chartArea.AxisX.MajorGrid.Enabled = true;
                chartArea.AxisY.MajorGrid.Enabled = true;
                chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

                // Cấu hình màu chữ
                chartArea.AxisX.LabelStyle.ForeColor = Color.White;
                chartArea.AxisY.LabelStyle.ForeColor = Color.White;
                chartArea.AxisX.TitleForeColor = Color.White;
                chartArea.AxisY.TitleForeColor = Color.White;

                temperatureChart.ChartAreas.Add(chartArea);

                // Tạo layout scrollable như trong hình
                tabChart.Controls.Clear();
                
                // Panel chính có thể scroll
                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.FromArgb(30, 25, 35, 45)
                };
                
                // 1. Biểu đồ nhiệt độ chính (line chart)
                var tempChartGroup = new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Text = "Nhiệt độ 24 giờ",
                    BackColor = Color.FromArgb(40, 30, 40, 50),
                    Padding = new Padding(5)
                };
                
                // Chuyển biểu đồ về line chart
                temperatureChart.ChartAreas[0].BackColor = Color.FromArgb(20, 30, 40, 50);
                temperatureChart.BackColor = Color.FromArgb(30, 20, 30, 40);
                temperatureChart.Dock = DockStyle.Fill;
                tempChartGroup.Controls.Add(temperatureChart);
                
                
                mainPanel.Controls.Add(tempChartGroup);
                tabChart.Controls.Add(mainPanel);

                // Đảm bảo control bản đồ tồn tại và nằm trên tabMap
                EnsureWindyBrowser();
                if (windyView != null)
                {
                    windyView.Dock = DockStyle.Fill;
                    tabMap.Controls.Clear();
                    tabMap.Controls.Add(windyView);
                }

                // Nút export trên tab biểu đồ
                var btnExport = new Button
                {
                    Text = "Xuất biểu đồ",
                    Location = new Point(334, 182),
                    Size = new Size(124, 29),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                btnExport.Click += (s, e) => ExportChart();
                tabChart.Controls.Add(btnExport);

                // Chuyển đổi hiển thị khi đổi tab
                try
                {
                    tabDieuKhien.SelectedIndexChanged -= TabDieuKhien_SelectedIndexChanged;
                }
                catch { }
                tabDieuKhien.SelectedIndexChanged += TabDieuKhien_SelectedIndexChanged;

                System.Diagnostics.Debug.WriteLine("Đã khởi tạo Chart nhiệt độ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo Chart: {ex.Message}");
            }
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

        private async void ShowMap()
        {
            EnsureWindyBrowser();
            if (windyView == null) return;
            
            // Nếu chưa có tọa độ hiện tại, lấy từ vị trí hiện tại
            if (currentLat == 0 && currentLon == 0)
            {
                try
                {
                    var locationData = await WeatherApiService.GetCurrentLocationAsync();
                    if (locationData?.Results?.Length > 0)
                    {
                        var result = locationData.Results[0];
                        currentLat = result.Lat;
                        currentLon = result.Lon;
                        System.Diagnostics.Debug.WriteLine($"Lấy tọa độ hiện tại cho bản đồ: {currentLat}, {currentLon}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi lấy vị trí hiện tại cho bản đồ: {ex.Message}");
                    // Fallback về tọa độ mặc định (Hà Nội)
                    currentLat = 21.0285;
                    currentLon = 105.8542;
                }
            }
            
            // Luôn nạp theo vị trí hiện tại
            LoadWindyMap(currentLat, currentLon);
            if (temperatureChart != null) temperatureChart.Visible = false;
            windyView.Visible = true;
        }

        private void TabDieuKhien_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (tabDieuKhien.SelectedTab == tabMap)
                {
                    ShowMap();
                }
                else if (tabDieuKhien.SelectedTab == tabChart)
                {
                    ShowChart();
                }
            }
            catch { }
        }

        private void LoadWindyMap(double lat, double lon)
        {
            EnsureWindyBrowser();
            if (windyView == null) return;

            string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string embedUrl = $"https://embed.windy.com/embed2.html?key={WINDY_API_KEY}&lat={latStr}&lon={lonStr}&detailLat={latStr}&detailLon={lonStr}&zoom=7&overlay=temp&level=surface&menu=&message=true&marker=true&calendar=&pressure=true&type=map&location=coordinates&detail=true&metricWind=default&metricTemp=default";
            windyView.Source = new Uri(embedUrl);
        }

        /// <summary>
        /// Xuất biểu đồ ra file hình ảnh
        /// </summary>
        private void ExportChart()
        {
            try
            {
                if (temperatureChart == null)
                {
                    MessageBox.Show("Không có biểu đồ để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                    saveDialog.Title = "Xuất biểu đồ nhiệt độ";
                    saveDialog.FileName = $"Biểu đồ nhiệt độ {DateTime.Now:yyyy-MM-dd HH-mm-ss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        temperatureChart.SaveImage(saveDialog.FileName, ChartImageFormat.Png);
                        MessageBox.Show($"Đã xuất biểu đồ thành công!\nFile: {saveDialog.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xuất biểu đồ: {ex.Message}");
                MessageBox.Show("Có lỗi xảy ra khi xuất biểu đồ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Chuyển đổi Celsius sang Fahrenheit
        /// </summary>
        private double ConvertCelsiusToFahrenheit(double celsius)
        {
            return (celsius * 9.0 / 5.0) + 32.0;
        }

        /// <summary>
        /// Lấy nhiệt độ theo đơn vị hiện tại
        /// </summary>
        private double GetTemperatureInUnit(double celsius)
        {
            return donViCelsius ? celsius : ConvertCelsiusToFahrenheit(celsius);
        }

        /// <summary>
        /// Chuyển đổi nhiệt độ từ text hiện tại sang đơn vị mới
        /// </summary>
        private double ConvertTemperatureFromText(string tempText, bool isCurrentlyCelsius)
        {
            if (double.TryParse(tempText, out double temp))
            {
                if (isCurrentlyCelsius && !donViCelsius)
                {
                    // Đang là C, chuyển sang F
                    return ConvertCelsiusToFahrenheit(temp);
                }
                else if (!isCurrentlyCelsius && donViCelsius)
                {
                    // Đang là F, chuyển sang C
                    return ConvertFahrenheitToCelsius(temp);
                }
                else
                {
                    // Cùng đơn vị, không cần chuyển đổi
                    return temp;
                }
            }
            return temp;
        }

        /// <summary>
        /// Chuyển đổi Fahrenheit sang Celsius
        /// </summary>
        private double ConvertFahrenheitToCelsius(double fahrenheit)
        {
            return (fahrenheit - 32.0) * 5.0 / 9.0;
        }

        /// <summary>
        /// Cập nhật UI khi chuyển đổi đơn vị nhiệt độ - tự tính toán từ text hiện tại
        /// </summary>
        private async Task CapNhatUIKhiChuyenDoiDonVi()
        {
            try
            {
                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";
                System.Diagnostics.Debug.WriteLine($"Bắt đầu cập nhật UI với đơn vị: {kyHieuNhietDo}");

                // Cập nhật nhiệt độ chính từ text hiện tại
                if (nhanNhietDoHienTai != null && nhanNhietDoHienTai.Text.Contains("°"))
                {
                    var isCurrentlyCelsius = nhanNhietDoHienTai.Text.Contains("°C");
                    var currentTempText = nhanNhietDoHienTai.Text.Replace("°C", "").Replace("°F", "");
                    var convertedTemp = ConvertTemperatureFromText(currentTempText, isCurrentlyCelsius);
                    nhanNhietDoHienTai.Text = $"{Math.Round(convertedTemp)}{kyHieuNhietDo}";
                    System.Diagnostics.Debug.WriteLine($"Cập nhật nhiệt độ chính: {convertedTemp}{kyHieuNhietDo}");
                }

                // Cập nhật panel chi tiết từ text hiện tại
                if (feelsLikePanel != null)
                {
                    var feelsLikeValueLabel = feelsLikePanel.Controls
                        .OfType<Label>()
                        .FirstOrDefault(l => l.Text.Contains("°") || l.Text.Contains("º"));
                    if (feelsLikeValueLabel != null)
                    {
                        bool isCurrentlyCelsius = feelsLikeValueLabel.Text.IndexOf("°C", StringComparison.OrdinalIgnoreCase) >= 0
                                                 || feelsLikeValueLabel.Text.IndexOf("ºC", StringComparison.OrdinalIgnoreCase) >= 0;
                        var tempText = feelsLikeValueLabel.Text
                            .Replace("°C", "").Replace("ºC", "")
                            .Replace("°F", "").Replace("ºF", "");
                        var convertedTemp = ConvertTemperatureFromText(tempText, isCurrentlyCelsius);
                        feelsLikeValueLabel.Text = $"{Math.Round(convertedTemp)}{kyHieuNhietDo}";
                    }
                }

                // Cập nhật dự báo 24h (duyệt đệ quy các Label bên trong card)
                if (BangTheoGio != null && BangTheoGio.Controls.Count > 0)
                {
                    foreach (Control card in BangTheoGio.Controls)
                    {
                        if (card is Panel panel)
                        {
                            foreach (var label in GetAllChildLabels(panel))
                            {
                                if (!label.Text.Contains("°")) continue;
                                var updated = TryConvertSimpleTemperatureLabel(label, kyHieuNhietDo);
                                if (!updated)
                                {
                                    // Không phải dạng đơn giản, bỏ qua
                                }
                            }
                        }
                    }
                }

                // Cập nhật dự báo 5 ngày (xử lý label chứa 2 giá trị Cao/Thấp)
                if (BangNhieuNgay != null && BangNhieuNgay.Controls.Count > 0)
                {
                    foreach (Control card in BangNhieuNgay.Controls)
                    {
                        if (card is Panel panel)
                        {
                            var tempLabel = panel.Controls.OfType<Label>()
                                                         .FirstOrDefault(l => l.Text.Contains("Cao nhất") && l.Text.Contains("Thấp nhất"));
                            if (tempLabel != null)
                            {
                                ConvertDailyHighLowLabel(tempLabel, kyHieuNhietDo);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Hoàn thành cập nhật UI với đơn vị: {kyHieuNhietDo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật UI: {ex.Message}");
                MessageBox.Show($"Lỗi cập nhật đơn vị: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static IEnumerable<Label> GetAllChildLabels(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is Label lbl)
                {
                    yield return lbl;
                }
                foreach (var nested in GetAllChildLabels(child))
                {
                    yield return nested;
                }
            }
        }

        private bool TryConvertSimpleTemperatureLabel(Label label, string newUnitSymbol)
        {
            // Dạng đơn giản: "23°C" hoặc "73°F" (hỗ trợ cả "º" và chữ thường)
            var text = label.Text.Trim();
            // Nếu chỉ là ký hiệu đơn vị, đổi trực tiếp
            var unitOnly = text.Replace(" ", "");
            if (string.Equals(unitOnly, "°C", StringComparison.OrdinalIgnoreCase) || string.Equals(unitOnly, "ºC", StringComparison.OrdinalIgnoreCase) || unitOnly.Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                label.Text = newUnitSymbol;
                return true;
            }
            if (string.Equals(unitOnly, "°F", StringComparison.OrdinalIgnoreCase) || string.Equals(unitOnly, "ºF", StringComparison.OrdinalIgnoreCase) || unitOnly.Equals("F", StringComparison.OrdinalIgnoreCase))
            {
                label.Text = newUnitSymbol;
                return true;
            }

            var match = System.Text.RegularExpressions.Regex.Match(text, @"^\s*(-?\d+(?:[\.,]\d+)?)\s*[°º]\s*([cCfF])\s*$");
            if (!match.Success) return false;

            var numText = match.Groups[1].Value.Replace(',', '.');
            double value = double.Parse(numText, System.Globalization.CultureInfo.InvariantCulture);
            bool isCurrentlyCelsius = match.Groups[2].Value.Equals("c", StringComparison.OrdinalIgnoreCase);
            double converted = ConvertTemperatureFromText(value.ToString(System.Globalization.CultureInfo.InvariantCulture), isCurrentlyCelsius);
            label.Text = $"{Math.Round(converted)}{newUnitSymbol}";
            return true;
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

        /// <summary>
        /// Tạo panel cho dự báo một giờ
        /// </summary>
        private void TaoPanelDuBaoGio(Panel panel, string icon, string gio, string nhietDo, string trangThai)
        {
            try
            {
                panel.Controls.Clear();
                panel.Padding = new Padding(5);

                // Label giờ
                var gioLabel = new Label
                {
                    Text = gio,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(5, 5)
                };

                // Label icon
                var iconLabel = new Label
                {
                    Text = icon,
                    Font = new Font("Segoe UI", 20F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(5, gioLabel.Bottom + 2)
                };

                // Label nhiệt độ
                var nhietDoLabel = new Label
                {
                    Text = nhietDo,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(5, iconLabel.Bottom + 2)
                };

                // Label trạng thái
                var trangThaiLabel = new Label
                {
                    Text = trangThai,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(5, nhietDoLabel.Bottom + 2)
                };

                panel.Controls.Add(gioLabel);
                panel.Controls.Add(iconLabel);
                panel.Controls.Add(nhietDoLabel);
                panel.Controls.Add(trangThaiLabel);

                // Căn giữa các control trong panel
                gioLabel.Left = (panel.Width - gioLabel.Width) / 2;
                iconLabel.Left = (panel.Width - iconLabel.Width) / 2;
                nhietDoLabel.Left = (panel.Width - nhietDoLabel.Width) / 2;
                trangThaiLabel.Left = (panel.Width - trangThaiLabel.Width) / 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo panel dự báo giờ: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thời tiết theo giờ được chọn
        /// </summary>
        private void CapNhatThoiTietTheoGio(string gio, string nhietDo, string trangThai, string icon)
        {
            try
            {
                // Cập nhật thông tin chính
                nhanNhietDoHienTai.Text = nhietDo;
                nhanTrangThai.Text = trangThai;

                // Cập nhật icon
                anhIconThoiTiet.Image = null; // Xóa icon cũ
                // Có thể thêm logic để load icon mới

                // Cập nhật background theo thời tiết
                string weatherMain = trangThai.ToLower().Contains("mưa") ? "rain" :
                                   trangThai.ToLower().Contains("nắng") ? "clear" :
                                   trangThai.ToLower().Contains("mây") ? "clouds" : "clear";
                SetBackground(weatherMain);

                // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật thời tiết theo giờ: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật các panel chi tiết với dữ liệu mẫu
        /// </summary>
        private void CapNhatPanelChiTietMau(string nhietDo)
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
        }

        /// <summary>
        /// Cập nhật thời tiết theo ngày được chọn
        /// </summary>
        private void CapNhatThoiTietTheoNgay(string ngay, string nhietDo, string trangThai, string icon)
        {
            // Để trống - chỉ hiển thị khi có dữ liệu thật từ API
            BangTheoGio.Controls.Clear();
            BangNhieuNgay.Controls.Clear();
        }

        #region Quản lý địa điểm yêu thích

        /// <summary>
        /// Lưu danh sách địa điểm yêu thích vào file JSON
        /// </summary>
        private void SaveLocations()
        {
            try
            {
                var json = JsonConvert.SerializeObject(favoriteLocations, Formatting.Indented);
                File.WriteAllText("favorite_locations.json", json);
                System.Diagnostics.Debug.WriteLine($"Đã lưu {favoriteLocations.Count} địa điểm yêu thích");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm yêu thích: {ex.Message}");
            }
        }

        /// <summary>
        /// Tải danh sách địa điểm yêu thích từ file JSON
        /// </summary>
        private void LoadLocations()
        {
            try
            {
                if (File.Exists("favorite_locations.json"))
                {
                    var json = File.ReadAllText("favorite_locations.json");
                    favoriteLocations = JsonConvert.DeserializeObject<List<FavoriteLocation>>(json) ?? new List<FavoriteLocation>();
                    
                    
                    System.Diagnostics.Debug.WriteLine($"Đã tải {favoriteLocations.Count} địa điểm yêu thích");
                }
                else
                {
                    favoriteLocations = new List<FavoriteLocation>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải địa điểm yêu thích: {ex.Message}");
                favoriteLocations = new List<FavoriteLocation>();
            }
        }

        /// <summary>
        /// Thêm địa điểm hiện tại vào danh sách yêu thích
        /// </summary>
        private void AddCurrentLocationToFavorites()
        {
            try
            {
                if (weatherData?.Current == null || string.IsNullOrEmpty(currentLocation))
                {
                    MessageBox.Show("Không có dữ liệu địa điểm để thêm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra xem địa điểm đã tồn tại chưa
                var existingLocation = favoriteLocations.FirstOrDefault(l => 
                    l.Name.Equals(currentLocation.Split(',')[0].Trim(), StringComparison.OrdinalIgnoreCase));

                if (existingLocation != null)
                {
                    MessageBox.Show("Địa điểm này đã có trong danh sách yêu thích!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Tạo địa điểm mới
                var newLocation = new FavoriteLocation
                {
                    Name = currentLocation.Split(',')[0].Trim(),
                    Country = currentLocation.Split(',').Length > 1 ? currentLocation.Split(',')[1].Trim() : "",
                    Latitude = weatherData.Lat,
                    Longitude = weatherData.Lon,
                    AddedDate = DateTime.Now
                };

                favoriteLocations.Add(newLocation);
                SaveLocations();

                MessageBox.Show($"Đã thêm '{newLocation.Name}' vào danh sách yêu thích!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Cập nhật ComboBox nếu có
                UpdateFavoritesComboBox();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi thêm địa điểm yêu thích: {ex.Message}");
                MessageBox.Show("Có lỗi xảy ra khi thêm địa điểm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Xóa địa điểm khỏi danh sách yêu thích
        /// </summary>
        private void RemoveSelectedLocation()
        {
            try
            {
                // Tìm địa điểm được chọn (có thể từ ComboBox hoặc cách khác)
                if (favoriteLocations.Count == 0)
                {
                    MessageBox.Show("Danh sách yêu thích trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Hiển thị dialog chọn địa điểm để xóa
                var locationNames = favoriteLocations.Select(l => $"{l.Name}, {l.Country}").ToArray();
                var selectedIndex = -1;
                
                // Tạo dialog đơn giản để chọn địa điểm
                using (var form = new Form())
                {
                    form.Text = "Chọn địa điểm để xóa";
                    form.Size = new Size(400, 300);
                    form.StartPosition = FormStartPosition.CenterParent;

                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        DataSource = locationNames
                    };

                    var buttonPanel = new Panel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 50
                    };

                    var btnOK = new Button
                    {
                        Text = "Xóa",
                        DialogResult = DialogResult.OK,
                        Location = new Point(200, 10),
                        Size = new Size(80, 30)
                    };

                    var btnCancel = new Button
                    {
                        Text = "Hủy",
                        DialogResult = DialogResult.Cancel,
                        Location = new Point(290, 10),
                        Size = new Size(80, 30)
                    };

                    buttonPanel.Controls.Add(btnOK);
                    buttonPanel.Controls.Add(btnCancel);
                    form.Controls.Add(listBox);
                    form.Controls.Add(buttonPanel);

                    if (form.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                    {
                        selectedIndex = listBox.SelectedIndex;
                    }
                }

                if (selectedIndex >= 0 && selectedIndex < favoriteLocations.Count)
                {
                    var locationToRemove = favoriteLocations[selectedIndex];
                    favoriteLocations.RemoveAt(selectedIndex);
                    SaveLocations();

                    MessageBox.Show($"Đã xóa '{locationToRemove.Name}' khỏi danh sách yêu thích!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Cập nhật ComboBox nếu có
                    UpdateFavoritesComboBox();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa địa điểm yêu thích: {ex.Message}");
                MessageBox.Show("Có lỗi xảy ra khi xóa địa điểm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Cập nhật ComboBox địa điểm yêu thích (nếu có)
        /// </summary>
        private void UpdateFavoritesComboBox()
        {
            try
            {
                // Tìm ComboBox địa điểm yêu thích trong form
                var comboBox = this.Controls.Find("comboFavorites", true).FirstOrDefault() as ComboBox;
                if (comboBox != null)
                {
                    comboBox.DataSource = null;
                    comboBox.DataSource = favoriteLocations.Select(l => $"{l.Name}, {l.Country}").ToList();
                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật ComboBox: {ex.Message}");
            }
        }



        #endregion

        /// <summary>
        /// Lấy tên ngày bằng tiếng Việt
        /// </summary>
        private string GetVietnameseDayName(long unixTime)
        {
            var date = UnixToLocal(unixTime);
            
            // Sử dụng thời gian từ API nếu có, nếu không thì dùng thời gian máy
            DateTime today;
            if (weatherData?.Current != null && weatherData.TimezoneOffset != 0)
            {
                var apiTime = DateTimeOffset.FromUnixTimeSeconds(weatherData.Current.Dt + weatherData.TimezoneOffset).DateTime;
                today = apiTime.Date;
            }
            else
            {
                today = DateTime.Now.Date;
            }

            // So sánh ngày với độ chính xác cao hơn
            var targetDate = date.Date;
            
            if (targetDate == today)
            {
                return "Hôm nay";
            }
            else if (targetDate == today.AddDays(1))
            {
                return "Ngày mai";
            }
            else
            {
                string[] dayNames = { "Chủ nhật", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };
                string dayName = dayNames[(int)date.DayOfWeek];
                return $"{dayName} {date:dd/MM}";
            }
        }

        /// <summary>
        /// Chuyển đổi mô tả thời tiết sang tiếng Việt
        /// </summary>
        private string GetVietnameseWeatherDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return "N/A";

            var vietnameseMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "clear sky", "Trời quang" },
                { "few clouds", "Ít mây" },
                { "scattered clouds", "Mây thưa" },
                { "broken clouds", "Mây rải rác" },
                { "overcast clouds", "Nhiều mây" },
                { "light rain", "Mưa nhẹ" },
                { "moderate rain", "Mưa vừa" },
                { "heavy rain", "Mưa to" },
                { "very heavy rain", "Mưa rất to" },
                { "extreme rain", "Mưa cực to" },
                { "freezing rain", "Mưa đá" },
                { "light intensity shower rain", "Mưa rào nhẹ" },
                { "shower rain", "Mưa rào" },
                { "heavy intensity shower rain", "Mưa rào to" },
                { "ragged shower rain", "Mưa rào không đều" },
                { "light snow", "Tuyết nhẹ" },
                { "snow", "Tuyết" },
                { "heavy snow", "Tuyết to" },
                { "sleet", "Mưa tuyết" },
                { "light shower sleet", "Mưa tuyết nhẹ" },
                { "shower sleet", "Mưa tuyết" },
                { "light rain and snow", "Mưa và tuyết nhẹ" },
                { "rain and snow", "Mưa và tuyết" },
                { "light shower snow", "Tuyết rơi nhẹ" },
                { "shower snow", "Tuyết rơi" },
                { "heavy shower snow", "Tuyết rơi to" },
                { "mist", "Sương mù" },
                { "smoke", "Khói" },
                { "haze", "Sương mù nhẹ" },
                { "sand/dust whirls", "Cát/bụi xoáy" },
                { "fog", "Sương mù dày" },
                { "sand", "Cát" },
                { "dust", "Bụi" },
                { "volcanic ash", "Tro núi lửa" },
                { "squalls", "Giông tố" },
                { "tornado", "Lốc xoáy" },
                { "cold", "Lạnh" },
                { "hot", "Nóng" },
                { "windy", "Có gió" },
                { "hail", "Mưa đá" },
                { "calm", "Lặng gió" },
                { "light breeze", "Gió nhẹ" },
                { "gentle breeze", "Gió nhẹ" },
                { "moderate breeze", "Gió vừa" },
                { "fresh breeze", "Gió mạnh" },
                { "strong breeze", "Gió rất mạnh" },
                { "high wind", "Gió cực mạnh" },
                { "gale", "Bão" },
                { "severe gale", "Bão mạnh" },
                { "storm", "Bão" },
                { "violent storm", "Bão dữ dội" },
                { "hurricane", "Cuồng phong" }
            };

            return vietnameseMap.TryGetValue(description, out string? vietnamese) ? vietnamese : description;
        }

        /// <summary>
        /// Lấy thông tin mưa và gió
        /// </summary>
        private string GetRainWindInfo(DailyWeather daily)
        {
            var info = new List<string>();

            // Thông tin mưa - kiểm tra Rain object
            if (daily.Rain != null)
            {
                // Nếu có dữ liệu mưa, hiển thị thông tin cơ bản
                info.Add("Có mưa");
            }

            // Thông tin gió
            if (daily.WindSpeed > 0)
            {
                info.Add($"Gió: {Math.Round(daily.WindSpeed, 1)} m/s");
            }

            return string.Join("\n", info);
        }

        

        
    }

    // Extension method để vẽ hình chữ nhật bo tròn
    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rectangle, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rectangle.X, rectangle.Y, radius, radius, 180, 90);
            path.AddArc(rectangle.X + rectangle.Width - radius, rectangle.Y, radius, radius, 270, 90);
            path.AddArc(rectangle.X + rectangle.Width - radius, rectangle.Y + rectangle.Height - radius, radius, radius, 0, 90);
            path.AddArc(rectangle.X, rectangle.Y + rectangle.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            graphics.DrawPath(pen, path);
        }
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
