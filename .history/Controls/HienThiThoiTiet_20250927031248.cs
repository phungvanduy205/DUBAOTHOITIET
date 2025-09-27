using System;
using System.Drawing;
using System.Windows.Forms;
using THOITIET.Models;

namespace THOITIET.Controls
{
    /// <summary>
    /// Control hiển thị thông tin thời tiết chính
    /// </summary>
    public partial class HienThiThoiTiet : UserControl
    {
        private Label lblDiaDiem;
        private Label lblNhietDo;
        private Label lblTrangThai;
        private Label lblNhietDoCamGiac;
        private Label lblTocDoGio;
        private Label lblTamNhin;
        private Label lblDoAm;
        private Label lblApSuat;
        private PictureBox picIconThoiTiet;
        private ChuyenDoiNhietDo chuyenDoiNhietDo;

        public HienThiThoiTiet()
        {
            KhoiTaoGiaoDien();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Size = new Size(400, 300);
            this.BackColor = Color.Transparent;

            // Địa điểm
            lblDiaDiem = new Label
            {
                Text = "Hà Nội, VN",
                Size = new Size(350, 30),
                Location = new Point(25, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Nhiệt độ chính
            lblNhietDo = new Label
            {
                Text = "25°C",
                Size = new Size(150, 60),
                Location = new Point(25, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Icon thời tiết
            picIconThoiTiet = new PictureBox
            {
                Size = new Size(80, 80),
                Location = new Point(200, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Trạng thái thời tiết
            lblTrangThai = new Label
            {
                Text = "Mây rải rác",
                Size = new Size(200, 25),
                Location = new Point(200, 140),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                BackColor = Color.Transparent
            };

            // Nhiệt độ cảm giác
            lblNhietDoCamGiac = new Label
            {
                Text = "Cảm giác: 27°C",
                Size = new Size(180, 20),
                Location = new Point(25, 130),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Tốc độ gió
            lblTocDoGio = new Label
            {
                Text = "Gió: 5 m/s",
                Size = new Size(100, 20),
                Location = new Point(25, 160),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Tầm nhìn
            lblTamNhin = new Label
            {
                Text = "Tầm nhìn: 10 km",
                Size = new Size(120, 20),
                Location = new Point(25, 180),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Độ ẩm
            lblDoAm = new Label
            {
                Text = "Độ ẩm: 65%",
                Size = new Size(100, 20),
                Location = new Point(25, 200),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Áp suất
            lblApSuat = new Label
            {
                Text = "Áp suất: 1013 hPa",
                Size = new Size(120, 20),
                Location = new Point(25, 220),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Control chuyển đổi nhiệt độ
            chuyenDoiNhietDo = new ChuyenDoiNhietDo
            {
                Location = new Point(300, 160)
            };

            this.Controls.AddRange(new Control[] {
                lblDiaDiem, lblNhietDo, lblTrangThai, lblNhietDoCamGiac,
                lblTocDoGio, lblTamNhin, lblDoAm, lblApSuat,
                picIconThoiTiet, chuyenDoiNhietDo
            });
        }

        /// <summary>
        /// Cập nhật hiển thị thời tiết
        /// </summary>
        public void CapNhatThoiTiet(OneCallResponse duLieu, string diaDiem, bool laCelsius = true)
        {
            if (duLieu?.Current == null) return;

            var current = duLieu.Current;
            var weather = current.Weather?.Length > 0 ? current.Weather[0] : null;

            // Cập nhật địa điểm
            lblDiaDiem.Text = diaDiem;

            // Cập nhật nhiệt độ
            var nhietDo = laCelsius ? 
                Math.Round(current.Temp, 1) : 
                Math.Round(current.Temp * 9.0 / 5.0 + 32, 1);
            lblNhietDo.Text = $"{nhietDo}°{(laCelsius ? "C" : "F")}";

            // Cập nhật trạng thái
            lblTrangThai.Text = weather?.Description ?? "Không xác định";

            // Cập nhật nhiệt độ cảm giác
            var nhietDoCamGiac = laCelsius ? 
                Math.Round(current.FeelsLike, 1) : 
                Math.Round(current.FeelsLike * 9.0 / 5.0 + 32, 1);
            lblNhietDoCamGiac.Text = $"Cảm giác: {nhietDoCamGiac}°{(laCelsius ? "C" : "F")}";

            // Cập nhật tốc độ gió
            var tocDoGio = laCelsius ? $"{current.WindSpeed:F1} m/s" : $"{current.WindSpeed * 2.237:F1} mph";
            lblTocDoGio.Text = $"Gió: {tocDoGio}";

            // Cập nhật tầm nhìn
            var tamNhin = laCelsius ? $"{current.Visibility / 1000.0:F1} km" : $"{current.Visibility * 0.000621371:F1} mi";
            lblTamNhin.Text = $"Tầm nhìn: {tamNhin}";

            // Cập nhật độ ẩm
            lblDoAm.Text = $"Độ ẩm: {current.Humidity}%";

            // Cập nhật áp suất
            var apSuat = laCelsius ? $"{current.Pressure} hPa" : $"{current.Pressure * 0.02953:F2} inHg";
            lblApSuat.Text = $"Áp suất: {apSuat}";

            // Cập nhật icon thời tiết
            CapNhatIconThoiTiet(weather?.Icon);
        }

        /// <summary>
        /// Cập nhật icon thời tiết
        /// </summary>
        private void CapNhatIconThoiTiet(string? iconCode)
        {
            if (string.IsNullOrEmpty(iconCode))
            {
                picIconThoiTiet.Image = null;
                return;
            }

            try
            {
                // Tạo icon từ code (có thể load từ file hoặc tạo bằng code)
                var icon = TaoIconThoiTiet(iconCode);
                picIconThoiTiet.Image = icon;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật icon: {ex.Message}");
                picIconThoiTiet.Image = null;
            }
        }

        /// <summary>
        /// Tạo icon thời tiết từ code
        /// </summary>
        private Image? TaoIconThoiTiet(string iconCode)
        {
            // Tạm thời trả về null, có thể implement sau
            // Hoặc load từ thư viện icon có sẵn
            return null;
        }

        /// <summary>
        /// Cập nhật đơn vị nhiệt độ
        /// </summary>
        public void CapNhatDonVi(bool laCelsius)
        {
            chuyenDoiNhietDo.LaCelsius = laCelsius;
        }

        /// <summary>
        /// Lấy control chuyển đổi nhiệt độ
        /// </summary>
        public ChuyenDoiNhietDo LayChuyenDoiNhietDo()
        {
            return chuyenDoiNhietDo;
        }

        /// <summary>
        /// Áp dụng style glassmorphism
        /// </summary>
        public void ApDungStyleGlassmorphism()
        {
            this.BackColor = Color.FromArgb(50, 255, 255, 255);
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 20, 20));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );
    }
}