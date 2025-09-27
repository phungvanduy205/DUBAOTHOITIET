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
        #region Fields

        // Service classes
        private readonly LocationService locationService = new LocationService();
        private readonly WeatherDisplayService weatherDisplayService = new WeatherDisplayService();
        private readonly BackgroundService backgroundService = new BackgroundService();

        // Weather data fields
        private bool donViCelsius = true;
        private ThoiTietHienTai? weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;
        private List<SavedLocation> savedLocations = new List<SavedLocation>();
        private DichVuThoiTiet dichVu = new DichVuThoiTiet();

        #endregion

        #region Constructor

        public Form1()
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR START ===");
            InitializeComponent();
            
            // Khởi tạo service classes
            InitializeServices();
            
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR END ===");
        }

        #endregion

        #region Service Initialization

        private void InitializeServices()
        {
            try
            {
                // Khởi tạo file lưu địa điểm nếu chưa có
                locationService.InitializeLocationsFile();
                
                // Khởi tạo background
                backgroundService.InitializeBackgroundPictureBox(boCucChinh);
                
                // Thiết lập nền mặc định khi khởi động
                backgroundService.SetDefaultBackgroundOnStartup(boCucChinh);
                
                // Tạo nội dung panel chi tiết
                weatherDisplayService.CreateDetailPanelContent(detailGridPanel);
                
                // Tạo file icon thực
                CreateResourcesDirectory();
                
                System.Diagnostics.Debug.WriteLine("Services initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo services: {ex.Message}");
            }
        }
        
        private void CreateResourcesDirectory()
        {
            try
            {
                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo thư mục Resources: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

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

        #region Weather Methods

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
                    if (weatherData != null)
                    {
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
                    }

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
                    if (weatherData != null)
                    {
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
                    }

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Lưu danh sách địa điểm vào file khi đóng ứng dụng
            locationService.SaveLocationsToFile();
        }

        #endregion

        #region Missing Event Handlers

        private void thanhTrenCung_Paint(object sender, PaintEventArgs e)
        {
            // Empty implementation for now
        }

        private void oTimKiemDiaDiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Empty implementation for now
        }

        private void listBoxDiaDiemDaLuu_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Empty implementation for now
        }

        private void nutXoaDiaDiem_Click(object sender, EventArgs e)
        {
            // Empty implementation for now
        }

        private void anhNenDong_Click(object sender, EventArgs e)
        {
            // Empty implementation for now
        }

        #endregion
    }
}
