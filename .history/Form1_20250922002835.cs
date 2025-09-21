
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

        public Form1()
        {
            InitializeComponent();
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();

            // Tạo background động
            TaoBackgroundDong();

            // Tạo nội dung cho các panel chi tiết
            TaoNoiDungPanelChiTiet();

            // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
            _ = LoadInitialWeatherData();

            // Tạo file icon thật
            TaoFileIconThuc();

            // Không đặt địa điểm mặc định - để trống cho đến khi API load

            // Xóa gợi ý tìm kiếm
        }


        /// <summary>
        /// Tạo background động cho form dựa trên thời tiết và thời gian
        /// </summary>
        private void TaoBackgroundDong(string weatherMain = "Clear")
        {
            try
            {
                Image backgroundImage;
                int currentHour = DateTime.Now.Hour;
                bool isNight = currentHour >= 18 || currentHour < 6;

                // Đường dẫn đến thư mục Resources
                string resourcesPath = Path.Combine(Application.StartupPath, "Resources");

                // Chọn background dựa trên thời tiết và thời gian
                switch (weatherMain.ToLower())
                {
                    case "snow":
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_tuyet.gif"));
                        break;
                    case "rain":
                    case "drizzle":
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_mua.gif"));
                        break;
                    case "thunderstorm":
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_giong_bao.gif"));
                        break;
                    case "mist":
                    case "fog":
                    case "haze":
                        backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_suong_mu.gif"));
                        break;
                    case "clouds":
                        if (isNight)
                            backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_may_day.gif"));
                        else
                            backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_may_day.gif"));
                        break;
                    case "clear":
                    default:
                        if (isNight)
                            backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_ban_dem.gif"));
                        else
                            backgroundImage = Image.FromFile(Path.Combine(resourcesPath, "nen_troi_quang.gif"));
                        break;
                }

                this.BackgroundImage = backgroundImage;
                this.BackgroundImageLayout = ImageLayout.Stretch;

                // Cập nhật màu chữ theo thời gian
                CapNhatMauChuTheoThoiGian(isNight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo background: {ex.Message}");
                // Fallback - tạo background gradient đơn giản
                this.BackgroundImage = null;
                this.BackColor = Color.FromArgb(135, 206, 250);
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
                // Lấy thời gian hiện tại theo múi giờ địa phương
                var now = DateTime.Now;
                
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
                    TaoBackgroundDong(weatherData.Current.Weather[0].Main ?? "Clear");
                }
                else
                {
                    // Fallback background
                    this.BackgroundImage = TaoBackgroundTest("troi_quang");
                    this.BackgroundImageLayout = ImageLayout.Stretch;
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
                
                MessageBox.Show($"Đang hiển thị thông tin:\nTên: {name}\nNhiệt độ: {weather.Current.Temp}{kyHieuNhietDo}\nTrạng thái: {weather.Current.Weather?[0]?.Description}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Cập nhật thông tin chính
                nhanNhietDoHienTai.Text = $"{Math.Round(weather.Current.Temp)}{kyHieuNhietDo}";
                nhanTrangThai.Text = weather.Current.Weather?[0]?.Description ?? "Không xác định";
                
                // Cập nhật địa điểm và thời gian
                CapNhatDiaDiem(name);
                CapNhatThoiGian();
                
                // Cập nhật các panel chi tiết
                CapNhatPanelChiTietFromApi(weather.Current, kyHieuNhietDo);
                
                // Cập nhật background theo thời tiết
                TaoBackgroundDong(weather.Current.Weather?[0]?.Main ?? "Clear");
                
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
                    TaoDuLieuMau5NgayFromApi(weather.Daily, kyHieuNhietDo);
                }
                else
                {
                    if (BangNhieuNgay != null)
                        BangNhieuNgay.Controls.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị thông tin: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (listBoxDiaDiemDaLuu.SelectedItem is SavedLocation selectedLocation)
                {
                    // Cập nhật tọa độ hiện tại
                    currentLat = selectedLocation.Lat;
                    currentLon = selectedLocation.Lon;
                    currentLocation = selectedLocation.Name;

                    // Lấy dữ liệu thời tiết hiện tại
                    var weatherData = await WeatherApiService.GetCurrentWeatherAsync(currentLat, currentLon);
                    if (weatherData != null)
                    {
                        HienThiThongTin(currentLocation, weatherData);
                        // Lưu địa điểm
                        LuuDiaDiem(currentLocation, currentLat, currentLon);
                    }
                    else
                    {
                        MessageBox.Show("API trả về null. Vui lòng kiểm tra API key hoặc kết nối mạng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                tabLichSu.BackColor = Color.Transparent;

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
                    nhanNhietDoHienTai.Text = $"{Math.Round(current.Temp)}{kyHieuNhietDo}";
                    nhanTrangThai.Text = current.Weather?[0]?.Description ?? "Không xác định";
                    
                    // Cập nhật địa điểm và thời gian
                    CapNhatDiaDiem(currentLocation);
                    CapNhatThoiGian();
                    
                    // Cập nhật các panel chi tiết
                    CapNhatPanelChiTietFromApi(current, kyHieuNhietDo);
                    
                    // Cập nhật background theo thời tiết
                    TaoBackgroundDong(current.Weather?[0]?.Main ?? "Clear");
                }

                // Cập nhật dự báo 24 giờ
                if (weatherData.Hourly != null && weatherData.Hourly.Length > 0)
                {
                    TaoDuLieuMau24GioFromApi(weatherData.Hourly, kyHieuNhietDo);
                }
                else
                {
                    // Để trống khi không có dữ liệu API
                    BangTheoGio.Controls.Clear();
                }

                // Cập nhật dự báo 5 ngày
                if (weatherData.Daily != null && weatherData.Daily.Length > 0)
                {
                    TaoDuLieuMau5NgayFromApi(weatherData.Daily, kyHieuNhietDo);
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
                if (sunrisePanel != null)
                {
                    var sunriseLabel = sunrisePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (sunriseLabel != null)
                    {
                        sunriseLabel.Text = $"Bình minh\n--:--";
                    }
                }
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
                if (sunrisePanel != null)
                {
                    var sunriseLabel = sunrisePanel.Controls.OfType<Label>().FirstOrDefault();
                    if (sunriseLabel != null)
                    {
                        sunriseLabel.Text = $"Bình minh\n--:--";
                    }
                }
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
                
                // Hiển thị debug trong MessageBox
                MessageBox.Show($"Debug API Data:\nFeelsLike: {current.FeelsLike}\nWindSpeed: {current.WindSpeed}\nHumidity: {current.Humidity}\nPressure: {current.Pressure}\nVisibility: {current.Visibility}\n\nAPI 3.0 Test - Nếu WindSpeed = 0, có thể do:\n1. API key không có quyền truy cập API 3.0\n2. Cần subscription riêng cho One Call 3.0\n3. Thử chuyển về API 2.5", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Sử dụng TaoPanelChiTiet để cập nhật tất cả panel
                // Xử lý FeelsLike - nếu bằng 0 thì lấy từ Temp
                var feelsLikeValue = current.FeelsLike != 0 ? current.FeelsLike : current.Temp;
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác", $"{Math.Round(feelsLikeValue)}{kyHieu}");
                
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
                TaoPanelChiTiet(windPanel, "💨", "Gió", windText);
                
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất", $"{current.Pressure} hPa");
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn", $"{current.Visibility / 1000.0:0.0} km");
                
                // Mọc/lặn - lấy từ dữ liệu daily nếu có
                if (weatherData?.Daily?.Length > 0)
                {
                    var daily = weatherData.Daily[0];
                    var sunrise = DateTimeOffset.FromUnixTimeSeconds(daily.Sunrise).ToString("HH:mm");
                    var sunset = DateTimeOffset.FromUnixTimeSeconds(daily.Sunset).ToString("HH:mm");
                    TaoPanelChiTiet(sunrisePanel, "🌅", "Mọc/Lặn", $"{sunrise}/{sunset}");
                }
                else
                {
                    TaoPanelChiTiet(sunrisePanel, "🌅", "Mọc/Lặn", "--:--/--:--");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật panel chi tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo dữ liệu 24 giờ từ API
        /// </summary>
        private void TaoDuLieuMau24GioFromApi(HourlyWeather[] hourlyData, string kyHieu)
        {
            try
            {
                // Xóa dữ liệu cũ
                if (BangTheoGio != null)
                {
                    BangTheoGio.Controls.Clear();
                }

                // Lấy 24 giờ đầu tiên
                var data24h = hourlyData.Take(24).ToArray();

                foreach (var item in data24h)
                {
                    var panelDuBao = new Panel
                    {
                        Width = 160, // Kích thước phù hợp với TableLayoutPanel
                        Height = 200, // MinimumHeight ~ 190-220
                        Margin = new Padding(6), // Margin như yêu cầu
                        BackColor = Color.FromArgb(120, 255, 255, 255),
                        BorderStyle = BorderStyle.None,
                        Padding = new Padding(8)
                    };

                    // Thêm bo góc
                    panelDuBao.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panelDuBao.Width, panelDuBao.Height, 15, 15));

                    // Thêm click handler để cập nhật thông tin chính
                    var time = UnixToLocal(item.Dt);
                    panelDuBao.Click += (s, e) => {
                        // Cập nhật thông tin chính với dữ liệu từ giờ được chọn
                        nhanNhietDoHienTai.Text = $"{Math.Round(item.Temp)}{kyHieu}";
                        nhanTrangThai.Text = item.Weather?[0]?.Description ?? "N/A";
                        
                        // Cập nhật icon thời tiết
                        if (anhIconThoiTiet != null)
                        {
                            anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(item.Weather?[0]?.Icon));
                        }
                        
                        // Cập nhật các panel chi tiết với dữ liệu từ giờ được chọn
                        CapNhatPanelChiTietFromHourlyApi(item, kyHieu);
                        
                        // Cập nhật background theo thời tiết
                        TaoBackgroundDong(item.Weather?[0]?.Main ?? "Clear");
                    };

                    // Tạo nội dung
                    TaoPanelDuBaoGioMoi(panelDuBao, time.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture), $"{Math.Round(item.Temp)}{kyHieu}", item.Weather?[0]?.Description ?? "N/A", GetWeatherIcon(item.Weather?[0]?.Icon));

                    if (BangTheoGio != null)
                    {
                        BangTheoGio.Controls.Add(panelDuBao);
                        System.Diagnostics.Debug.WriteLine($"Đã thêm panel 24h: {time.ToString("HH:mm")} - {Math.Round(item.Temp)}{kyHieu}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo dữ liệu 24h: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo dữ liệu 5 ngày từ API
        /// </summary>
        private void TaoDuLieuMau5NgayFromApi(DailyWeather[] dailyData, string kyHieu)
        {
            try
            {
                // Xóa dữ liệu cũ
                if (BangNhieuNgay != null)
                {
                    BangNhieuNgay.Controls.Clear();
                }

                // Thêm ô hôm nay trước
                if (dailyData.Length > 0)
                {
                    var homNay = dailyData[0];
                    var panelHomNay = new Panel
                    {
                        BackColor = Color.FromArgb(150, 255, 255, 255), // Sáng hơn để phân biệt
                        Size = new Size(450, 80),
                        Margin = new Padding(3),
                        Padding = new Padding(8),
                        BorderStyle = BorderStyle.None
                    };

                    // Thêm bo góc
                    panelHomNay.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panelHomNay.Width, panelHomNay.Height, 15, 15));

                    // Thêm click handler cho hôm nay
                    var dateHomNay = UnixToLocal(homNay.Dt);
                    var dayNameHomNay = "Hiện tại";
                    panelHomNay.Click += (s, e) => {
                        // Cập nhật thông tin chính với dữ liệu hôm nay
                        nhanNhietDoHienTai.Text = $"{Math.Round(homNay.Temp.Max)}{kyHieu}";
                        nhanTrangThai.Text = homNay.Weather?[0]?.Description ?? "N/A";
                        
                        // Cập nhật icon thời tiết
                        if (anhIconThoiTiet != null)
                        {
                            anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(homNay.Weather?[0]?.Icon));
                        }
                        
                        // Cập nhật các panel chi tiết với dữ liệu hôm nay
                        CapNhatPanelChiTietFromDailyApi(homNay, kyHieu);
                        
                        // Cập nhật background theo thời tiết
                        TaoBackgroundDong(homNay.Weather?[0]?.Main ?? "Clear");
                        
                        // Cập nhật thời gian hiển thị hôm nay
                        nhanThoiGian.Text = $"{dayNameHomNay}, {dateHomNay.ToString("dd/MM/yyyy")}";
                    };

                    // Tạo nội dung cho hôm nay
                    TaoPanelDuBaoNgayMoi(panelHomNay, dayNameHomNay, $"{Math.Round(homNay.Temp.Max)}{kyHieu}", homNay.Weather?[0]?.Description ?? "N/A", GetWeatherIcon(homNay.Weather?[0]?.Icon));

                    if (BangNhieuNgay != null)
                    {
                        BangNhieuNgay.Controls.Add(panelHomNay);
                    }
                }

                // Lấy 5 ngày tiếp theo
                var data5Ngay = dailyData.Skip(1).Take(5).ToArray();

                foreach (var item in data5Ngay)
                {
                    var panel = new Panel
                    {
                        BackColor = Color.FromArgb(120, 255, 255, 255),
                        Size = new Size(450, 80),
                        Margin = new Padding(3),
                        Padding = new Padding(8),
                        BorderStyle = BorderStyle.None
                    };

                    // Thêm bo góc
                    panel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panel.Width, panel.Height, 15, 15));

                    // Thêm click handler để cập nhật thông tin chính
                    var date = UnixToLocal(item.Dt);
                    var dayName = GetThuVietNam(date.DayOfWeek);
                    panel.Click += (s, e) => {
                        // Cập nhật thông tin chính với dữ liệu từ ngày được chọn
                        nhanNhietDoHienTai.Text = $"{Math.Round(item.Temp.Max)}{kyHieu}";
                        nhanTrangThai.Text = item.Weather?[0]?.Description ?? "N/A";
                        
                        // Cập nhật icon thời tiết
                        if (anhIconThoiTiet != null)
                        {
                            anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(item.Weather?[0]?.Icon));
                        }
                        
                        // Cập nhật các panel chi tiết với dữ liệu từ ngày được chọn
                        CapNhatPanelChiTietFromDailyApi(item, kyHieu);
                        
                        // Cập nhật background theo thời tiết
                        TaoBackgroundDong(item.Weather?[0]?.Main ?? "Clear");
                        
                        // Cập nhật thời gian hiển thị ngày được chọn
                        nhanThoiGian.Text = $"{dayName}, {date.ToString("dd/MM/yyyy")}";
                    };

                    // Tạo nội dung
                    TaoPanelDuBaoNgayMoi(panel, dayName, $"{Math.Round(item.Temp.Max)}{kyHieu}", item.Weather?[0]?.Description ?? "N/A", GetWeatherIcon(item.Weather?[0]?.Icon));

                    if (BangNhieuNgay != null)
                    {
                        BangNhieuNgay.Controls.Add(panel);
                        System.Diagnostics.Debug.WriteLine($"Đã thêm panel 5 ngày: {dayName} - {Math.Round(item.Temp.Max)}{kyHieu}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo dữ liệu 5 ngày: {ex.Message}");
            }
        }


        /// <summary>
        /// Chuyển emoji thành icon thời tiết
        /// </summary>
        private Image GetWeatherIconFromEmoji(string emoji)
        {
            // Tạo một bitmap đơn giản với emoji
            var bitmap = new Bitmap(64, 64);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                using (var font = new Font("Segoe UI Emoji", 32))
                {
                    var brush = new SolidBrush(Color.White);
                    var rect = new RectangleF(0, 0, 64, 64);
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(emoji, font, brush, rect, format);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Lấy icon thời tiết từ mã icon API
        /// </summary>
        private string GetWeatherIcon(string iconCode)
        {
            if (string.IsNullOrEmpty(iconCode)) return "☀️";
            
            return iconCode switch
            {
                "01d" or "01n" => "☀️", // clear sky
                "02d" or "02n" => "⛅", // few clouds
                "03d" or "03n" => "☁️", // scattered clouds
                "04d" or "04n" => "☁️", // broken clouds
                "09d" or "09n" => "🌧️", // shower rain
                "10d" or "10n" => "🌦️", // rain
                "11d" or "11n" => "⛈️", // thunderstorm
                "13d" or "13n" => "❄️", // snow
                "50d" or "50n" => "🌫️", // mist
                _ => "☀️"
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
                else if ((ma >= 300 && ma <= 321) || (ma >= 500 && ma <= 531)) tenNen = "nen_mua.gif";
                else if (ma >= 600 && ma <= 622) tenNen = "nen_tuyet.gif";
                else if (ma == 800) tenNen = "nen_troi_quang.gif";
                else tenNen = "nen_mua.gif";
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
                "01" => "nen_troi_quang.gif",        // trời quang
                "02" => "nen_it_may.gif",            // ít mây
                "03" => "nen_may_rac_rac.gif",       // mây rải rác
                "04" => "nen_may_day.gif",           // mây dày
                "09" => "nen_mua_rao.gif",           // mưa rào
                "10" => "nen_mua.gif",               // mưa
                "11" => "nen_giong_bao.gif",         // giông bão
                "13" => "nen_tuyet.gif",             // tuyết
                "50" => "nen_suong_mu.gif",          // sương mù
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
        /// Event handler cho nút lưu địa điểm
        /// </summary>
        private void nutLuuDiaDiem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(currentLocation) && currentLat != 0 && currentLon != 0)
                {
                    LuuDiaDiem(currentLocation, currentLat, currentLon);
                    MessageBox.Show($"Đã lưu địa điểm: {currentLocation}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Không có địa điểm để lưu. Vui lòng tìm kiếm địa điểm trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu địa điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                // Panel mặt trời mọc
                TaoPanelChiTiet(sunrisePanel, "🌅", "Mọc/Lặn", "--");
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

                // Label title
                var titleLabel = new Label
                {
                    Text = title,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(10, 35)
                };

                // Label value
                var valueLabel = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Location = new Point(10, 50)
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
                TaoPanelChiTiet(feelsLikePanel, "🌡️", "Cảm giác", $"{Math.Round(hienTai.NhietDoCamGiac)}{kyHieuNhietDo}");

                // Panel độ ẩm
                TaoPanelChiTiet(humidityPanel, "💧", "Độ ẩm", $"{hienTai.DoAm}%");

                // Panel gió
                var donViGio = donViCelsius ? "m/s" : "mph";
                TaoPanelChiTiet(windPanel, "💨", "Gió", $"{Math.Round(hienTai.TocDoGio)} {donViGio}");

                // Panel áp suất
                TaoPanelChiTiet(pressurePanel, "📊", "Áp suất", $"{hienTai.ApSuat} hPa");

                // Panel tầm nhìn
                TaoPanelChiTiet(visibilityPanel, "👁️", "Tầm nhìn", $"{hienTai.TamNhin / 1000.0:0.0} km");

                // Panel mặt trời mọc/lặn
                    var sunrise = UnixToLocal(hienTai.MatTroiMoc).ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    var sunset = UnixToLocal(hienTai.MatTroiLan).ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                TaoPanelChiTiet(sunrisePanel, "🌅", "Mọc/Lặn", $"{sunrise}/{sunset}");
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
                panel.Paint += (s, e) => {
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

                // Vẽ viền đậm
                panel.Paint += (s, e) => {
                    using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 2))
                    {
                        var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                };

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
                var lblTemp = new Label
                {
                    Text = $"{Math.Round(hour.Temp)}{kyHieu}",
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
                panel.Click += (s, e) => {
                    // Cập nhật thông tin chính với dữ liệu từ giờ được chọn
                    nhanNhietDoHienTai.Text = $"{Math.Round(hour.Temp)}{kyHieu}";
                    nhanTrangThai.Text = hour.Weather?[0]?.Description ?? "N/A";
                    
                    // Cập nhật icon thời tiết
                    if (anhIconThoiTiet != null)
                    {
                        anhIconThoiTiet.Image = GetWeatherIconFromEmoji(GetWeatherIcon(hour.Weather?[0]?.Icon));
                    }
                    
                    // Cập nhật các panel chi tiết với dữ liệu từ giờ được chọn
                    CapNhatPanelChiTietFromHourlyApi(hour, kyHieu);
                    
                    // Cập nhật background theo thời tiết
                    TaoBackgroundDong(hour.Weather?[0]?.Main ?? "Clear");
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
                    Size = new Size(140, 200),
                    BackColor = Color.FromArgb(80, 128, 128, 128),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(6)
                };

                // Vẽ viền đậm
                panel.Paint += (s, e) => {
                    using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 2))
                    {
                        var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                };

                // Tạo TableLayoutPanel với 4 hàng, 1 cột
                var tlpHourlyCard = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(5)
                };

                // Cấu hình RowStyles
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F)); // Hàng 0: Ngày
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F)); // Hàng 1: Nhiệt độ
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Hàng 2: Mô tả (AutoSize)
                tlpHourlyCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Hàng 3: Icon

                // Hàng 0: Ngày (trên cùng)
                var date = UnixToLocal(daily.Dt);
                var lblDay = new Label
                {
                    Text = date.ToString("dd/MM"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                // Hàng 1: Nhiệt độ min/max (to nhất)
                var lblTemp = new Label
                {
                    Text = $"{Math.Round(daily.Temp.Min)}°/{Math.Round(daily.Temp.Max)}°",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 18F, FontStyle.Bold), // Font lớn nhất
                    ForeColor = Color.White,
                    BackColor = Color.Transparent
                };

                // Hàng 2: Mô tả (tự xuống dòng)
                var lblDesc = new Label
                {
                    Text = daily.Weather?[0]?.Description ?? "N/A",
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
                    var iconImage = GetWeatherIconFromEmoji(GetWeatherIcon(daily.Weather?[0]?.Icon));
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
                tlpHourlyCard.Controls.Add(lblDay, 0, 0);
                tlpHourlyCard.Controls.Add(lblTemp, 0, 1);
                tlpHourlyCard.Controls.Add(lblDesc, 0, 2);
                tlpHourlyCard.Controls.Add(picIcon, 0, 3);

                // Thêm TableLayoutPanel vào panel chính
                panel.Controls.Add(tlpHourlyCard);

                return panel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo card ngày: {ex.Message}");
                return new Panel();
            }
        }

        /// <summary>
        /// Load dự báo 5 ngày
        /// </summary>
        private void LoadDuBao5Ngay(DailyWeather[] dailyList, string kyHieu)
        {
            try
            {
                if (BangNhieuNgay != null)
                {
                    BangNhieuNgay.Controls.Clear();
                    
                    // Lấy 5 ngày đầu tiên
                    var data5Ngay = dailyList.Take(5).ToArray();
                    
                    foreach (var daily in data5Ngay)
                    {
                        var card = TaoCardNgay(daily, kyHieu);
                        BangNhieuNgay.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load dự báo 5 ngày: {ex.Message}");
            }
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
                TaoBackgroundDong(weatherMain);

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
        
        public override string ToString()
        {
            return Name;
        }
    }
}
