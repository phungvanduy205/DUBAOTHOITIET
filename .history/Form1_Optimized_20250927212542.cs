using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THOITIET
{
    public partial class Form1 : Form
    {
        #region Fields & Properties

        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;

        // Dữ liệu thời tiết từ API
        private ThoiTietHienTai? weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;

        // Danh sách địa điểm đã lưu
        private List<SavedLocation> savedLocations = new List<SavedLocation>();

        // Timer tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();

        // Service classes
        private readonly LocationService locationService = new LocationService();
        private readonly WeatherDisplayService weatherDisplayService = new WeatherDisplayService();
        private readonly BackgroundService backgroundService = new BackgroundService();

        #endregion

        #region Constructor & Initialization

        public Form1()
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR START ===");
            InitializeComponent();
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();

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
            locationService.InitializeLocationsFile();
            
            // Nạp danh sách địa điểm đã lưu
            NapDiaDiemDaLuu();
            
            // Khởi tạo background
            backgroundService.InitializeBackgroundPictureBox(boCucChinh);
            
            // Thiết lập nền mặc định khi khởi động
            backgroundService.SetDefaultBackgroundOnStartup(boCucChinh);
            
            // Tạo nội dung panel chi tiết
            weatherDisplayService.CreateDetailPanelContent(detailGridPanel);
            
            // Tạo file icon thực
            TaoFileIconThuc();
            
            // Cập nhật thời gian
            CapNhatThoiGian();
            
            // Nạp danh sách địa điểm đã lưu
            NapDiaDiemDaLuu();
            
            // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
            _ = LoadWeatherByIP();
            
            // Test background
            TestBackground();
            
            // Force set background trong load
            System.Diagnostics.Debug.WriteLine("Calling ForceSetBackgroundInLoad...");
            backgroundService.ForceSetBackgroundInLoad(boCucChinh);
            
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR END ===");
        }

        #endregion

        #region 1️⃣ NHẬP ĐỊA ĐIỂM, TÌM KIẾM, LƯU ĐỊA ĐIỂM, ĐỔI °C/°F

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD START ===");
                
                // Cập nhật thời gian
                CapNhatThoiGian();
                
                // Nạp danh sách địa điểm đã lưu
                NapDiaDiemDaLuu();
                
                // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
                await LoadWeatherByIP();
                
                System.Diagnostics.Debug.WriteLine("=== FORM1_LOAD END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi Form1_Load: {ex.Message}");
            }
        }

        private async void oTimKiemDiaDiem_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    await TimKiemDiaDiem();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi oTimKiemDiaDiem_KeyDown: {ex.Message}");
            }
        }

        private async void NutTimKiem_Click(object? sender, EventArgs e)
        {
            try
            {
                await TimKiemDiaDiem();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi NutTimKiem_Click: {ex.Message}");
            }
        }

        private async Task TimKiemDiaDiem()
        {
            try
            {
                var tuKhoa = oTimKiemDiaDiem.Text?.Trim();
                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    MessageBox.Show("Vui lòng nhập tên địa điểm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = await locationService.SearchLocationAsync(tuKhoa);
                if (result.success)
                {
                    currentLocation = result.location;
                    currentLat = result.lat;
                    currentLon = result.lon;
                    weatherData = result.weatherData;

                    // Hiển thị thông tin thời tiết
                    weatherDisplayService.DisplayWeatherInfo(
                        currentLocation, 
                        weatherData, 
                        donViCelsius,
                        nhanTenDiaDiem, 
                        nhanThoiGian, 
                        nhanNhietDoHienTai, 
                        nhanTrangThai, 
                        anhIconThoiTiet, 
                        detailGridPanel
                    );

                    // Cập nhật background theo thời tiết
                    if (weatherData != null)
                    {
                        backgroundService.SetBackground(weatherData.TrangThaiMoTa ?? "Clear", weatherData.MaThoiTiet, boCucChinh);
                    }

                    // Lưu địa điểm tự động
                    locationService.SaveLocationSilent(currentLocation, currentLat, currentLon);
                    NapDiaDiemDaLuu();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi TimKiemDiaDiem: {ex.Message}");
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void nutLuuDiaDiem_Click(object sender, EventArgs e)
        {
            try
            {
                var currentLocationText = oTimKiemDiaDiem.Text.Trim();
                if (string.IsNullOrEmpty(currentLocationText))
                {
                    MessageBox.Show("Vui lòng nhập tên địa điểm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (currentLat == 0 && currentLon == 0)
                {
                    MessageBox.Show("Không có dữ liệu vị trí để lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                locationService.SaveLocation(currentLocationText, currentLat, currentLon);
                NapDiaDiemDaLuu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nutLuuDiaDiem_Click: {ex.Message}");
                MessageBox.Show($"Lỗi khi lưu địa điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void nutChuyenDoiDiaDiem_Click(object sender, EventArgs e)
        {
            try
            {
                if (savedLocations.Count == 0) 
                {
                    MessageBox.Show("Chưa có địa điểm nào được lưu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var locationNames = savedLocations.Select(l => l.Name).ToList();
                
                using (var form = new Form())
                {
                    form.Text = "Chọn địa điểm";
                    form.Size = new Size(400, 300);
                    form.StartPosition = FormStartPosition.CenterParent;
                    
                    var listBox = new ListBox
                    {
                        Dock = DockStyle.Fill,
                        DataSource = locationNames
                    };
                    
                    form.Controls.Add(listBox);
                    
                    if (form.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                    {
                        var selectedLocation = savedLocations[listBox.SelectedIndex];
                        currentLocation = selectedLocation.Name;
                        currentLat = selectedLocation.Lat;
                        currentLon = selectedLocation.Lon;
                        
                        // Tải dữ liệu thời tiết cho địa điểm đã chọn
                        _ = LoadWeatherForLocation(selectedLocation.Name, selectedLocation.Lat, selectedLocation.Lon);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nutChuyenDoiDiaDiem_Click: {ex.Message}");
                MessageBox.Show($"Lỗi khi chuyển đổi địa điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 2️⃣ GỌI API & THÔNG TIN MÔ TẢ

        private async Task LoadWeatherByIP()
        {
            try
            {
                var result = await locationService.GetCurrentLocationAsync();
                if (result.success)
                {
                    currentLocation = result.location;
                    currentLat = result.lat;
                    currentLon = result.lon;
                    weatherData = result.weatherData;

                    // Hiển thị thông tin thời tiết
                    weatherDisplayService.DisplayWeatherInfo(
                        currentLocation, 
                        weatherData, 
                        donViCelsius,
                        nhanTenDiaDiem, 
                        nhanThoiGian, 
                        nhanNhietDoHienTai, 
                        nhanTrangThai, 
                        anhIconThoiTiet, 
                        detailGridPanel
                    );

                    // Cập nhật background theo thời tiết
                    if (weatherData != null)
                    {
                        backgroundService.SetBackground(weatherData.TrangThaiMoTa ?? "Clear", weatherData.MaThoiTiet, boCucChinh);
                    }

                    // Lưu địa điểm tự động
                    locationService.SaveLocationSilent(currentLocation, currentLat, currentLon);
                    NapDiaDiemDaLuu();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadWeatherByIP: {ex.Message}");
            }
        }

        private async Task LoadWeatherForLocation(string locationName, double lat, double lon)
        {
            try
            {
                var weatherData = await dichVu.LayThoiTietHienTai(lat, lon);
                if (weatherData != null)
                {
                    this.weatherData = weatherData;

                    // Hiển thị thông tin thời tiết
                    weatherDisplayService.DisplayWeatherInfo(
                        locationName, 
                        weatherData, 
                        donViCelsius,
                        nhanTenDiaDiem, 
                        nhanThoiGian, 
                        nhanNhietDoHienTai, 
                        nhanTrangThai, 
                        anhIconThoiTiet, 
                        detailGridPanel
                    );

                    // Cập nhật background theo thời tiết
                    backgroundService.SetBackground(weatherData.TrangThaiMoTa ?? "Clear", weatherData.MaThoiTiet, boCucChinh);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi LoadWeatherForLocation: {ex.Message}");
            }
        }

        private async Task CapNhatThoiTiet()
        {
            try
            {
                if (weatherData == null) return;

                await weatherDisplayService.UpdateWeatherDisplay(
                    weatherData, 
                    donViCelsius,
                    nhanNhietDoHienTai, 
                    nhanTrangThai, 
                    anhIconThoiTiet, 
                    detailGridPanel
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi CapNhatThoiTiet: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private void CauHinhKhoiTao()
        {
            try
            {
                // Cấu hình timer tự động cập nhật mỗi 1 giờ
                dongHoCapNhat.Interval = 3600000; // 1 giờ = 3600000 ms
                dongHoCapNhat.Tick += async (s, e) => { await CapNhatThoiTiet(); };
                dongHoCapNhat.Start();

                // Timer cập nhật thời gian mỗi giây
                var dongHoThoiGian = new System.Windows.Forms.Timer();
                dongHoThoiGian.Interval = 1000; // 1 giây
                dongHoThoiGian.Tick += (s, e) => CapNhatThoiGian();
                dongHoThoiGian.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi CauHinhKhoiTao: {ex.Message}");
            }
        }

        private void CapNhatThoiGian()
        {
            try
            {
                if (nhanThoiGian != null)
                {
                    nhanThoiGian.Text = DateTime.Now.ToString("HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi CapNhatThoiGian: {ex.Message}");
            }
        }

        private void NapDiaDiemDaLuu()
        {
            try
            {
                listBoxDiaDiemDaLuu.Items.Clear();
                savedLocations = locationService.GetSavedLocations();

                foreach (var loc in savedLocations)
                {
                    listBoxDiaDiemDaLuu.Items.Add(loc.Name);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp địa điểm đã lưu: {ex.Message}");
            }
        }

        private void ApDungStyleGlassmorphism()
        {
            try
            {
                // Áp dụng style glassmorphism cho các control chính
                ApplyRoundedCorners(thanhTrenCung, 15);
                ApplyRoundedCorners(boCucChinh, 20);
                ApplyRoundedCorners(khuVucTrai_HienTai, 15);
                ApplyRoundedCorners(khuVucPhai_5Ngay, 15);
                ApplyRoundedCorners(khuVucDuoi_24Gio, 15);
                
                // Áp dụng style cho các khung con
                ApplyRoundedCorners(khung5Ngay, 10);
                ApplyRoundedCorners(khung24Gio, 10);
                
                // Áp dụng style cho các control tìm kiếm
                ApplyRoundedCorners(oTimKiemDiaDiem, 8);
                ApplyRoundedCorners(NutTimKiem, 8);
                ApplyRoundedCorners(CongTacDonVi, 8);
                
                // Áp dụng style cho các label chính
                ApplyRoundedCorners(nhanTenDiaDiem, 5);
                ApplyRoundedCorners(nhanNhietDoHienTai, 5);
                ApplyRoundedCorners(nhanTrangThai, 5);
                
                // Áp dụng style cho tab control
                ApplyRoundedCorners(tabDieuKhien, 10);
                ApplyRoundedCorners(tabChart, 10);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ApDungStyleGlassmorphism: {ex.Message}");
            }
        }

        private void ApplyRoundedCorners(Control control, int radius)
        {
            try
            {
                if (control == null) return;

                control.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ApplyRoundedCorners: {ex.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void TaoFileIconThuc()
        {
            try
            {
                // Tạo thư mục Resources nếu chưa có
                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi TaoFileIconThuc: {ex.Message}");
            }
        }

        private void TestBackground()
        {
            try
            {
                // Test background với các loại thời tiết khác nhau
                System.Diagnostics.Debug.WriteLine("Testing background...");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestBackground error: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Lưu danh sách địa điểm vào file khi đóng ứng dụng
            locationService.SaveLocationsToFile();
        }

        #endregion
    }
}
