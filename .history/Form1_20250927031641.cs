using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using THOITIET.Models;
using THOITIET.Services;
using THOITIET.Controls;

namespace THOITIET
{
    /// <summary>
    /// Form chính: chỉ xử lý sự kiện UI và gọi các service/control tương ứng
    /// </summary>
    public partial class Form1 : Form
    {
        // Services
        private readonly XuLyTimKiem xuLyTimKiem;
        private readonly QuanLyDiaDiem quanLyDiaDiem;
        private readonly QuanLyThoiTiet quanLyThoiTiet;

        // Controls
        private HienThiThoiTiet hienThiThoiTiet;
        private DuBaoTheoGioControl duBaoTheoGioControl;
        private DuBaoNhieuNgayControl duBaoNhieuNgayControl;

        // Dữ liệu hiện tại
        private OneCallResponse? duLieuThoiTiet;
        private string diaDiemHienTai = "";
        private double viDoHienTai = 0;
        private double kinhDoHienTai = 0;

        // Timer tự động cập nhật
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        public Form1()
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR START ===");
            InitializeComponent();
            
            // Khởi tạo services
            xuLyTimKiem = new XuLyTimKiem();
            quanLyDiaDiem = new QuanLyDiaDiem();
            quanLyThoiTiet = new QuanLyThoiTiet();

            // Khởi tạo controls
            KhoiTaoControls();

            // Cấu hình form
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();

            // Đăng ký sự kiện
            DangKySuKien();

            // Tải dữ liệu ban đầu
            _ = TaiDuLieuBanDau();

            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR END ===");
        }

        /// <summary>
        /// Khởi tạo các controls
        /// </summary>
        private void KhoiTaoControls()
        {
            // Control hiển thị thời tiết chính
            hienThiThoiTiet = new HienThiThoiTiet
            {
                Location = new Point(50, 100),
                Size = new Size(400, 300)
            };

            // Control dự báo theo giờ
            duBaoTheoGioControl = new DuBaoTheoGioControl
            {
                Location = new Point(50, 420),
                Size = new Size(800, 120)
            };

            // Control dự báo 5 ngày
            duBaoNhieuNgayControl = new DuBaoNhieuNgayControl
            {
                Location = new Point(50, 560),
                Size = new Size(800, 150)
            };

            // Thêm controls vào form
            this.Controls.Add(hienThiThoiTiet);
            this.Controls.Add(duBaoTheoGioControl);
            this.Controls.Add(duBaoNhieuNgayControl);
        }

        /// <summary>
        /// Đăng ký các sự kiện
        /// </summary>
        private void DangKySuKien()
        {
            // Sự kiện chuyển đổi đơn vị nhiệt độ
            hienThiThoiTiet.LayChuyenDoiNhietDo().DonViThayDoi += async (sender, laCelsius) =>
            {
                await CapNhatDonViHienThi(laCelsius);
            };

            // Sự kiện chọn ngày trong dự báo 5 ngày
            duBaoNhieuNgayControl.NgayDuocChon += (sender, ngayIndex) =>
            {
                XuLyChonNgay(ngayIndex);
            };

            // Timer cập nhật tự động
            dongHoCapNhat.Interval = 3600000; // 1 giờ
            dongHoCapNhat.Tick += async (sender, e) => await CapNhatThoiTietTuDong();

            // Bo tròn các control
            this.Load += (s, e) =>
            {
                ApplyRoundedCorners(oTimKiemDiaDiem, 10);
                ApplyRoundedCorners(khung24Gio, 15);
                ApplyRoundedCorners(khung5Ngay, 15);
            };
        }

        /// <summary>
        /// Tải dữ liệu ban đầu
        /// </summary>
        private async Task TaiDuLieuBanDau()
        {
            try
            {
                // Thử lấy địa điểm hiện tại
                var ketQua = await xuLyTimKiem.LayDiaDiemHienTai();
                if (ketQua.ThanhCong && ketQua.DuLieuThoiTiet != null)
                {
                    CapNhatDuLieuThoiTiet(ketQua.DuLieuThoiTiet, ketQua.ViTri?.TenDayDu ?? "Vị trí hiện tại");
                }
                else
                {
                    // Fallback: Hà Nội
                    var ketQuaHanoi = await xuLyTimKiem.TimKiemDiaDiem("Hà Nội");
                    if (ketQuaHanoi.ThanhCong && ketQuaHanoi.DuLieuThoiTiet != null)
                    {
                        CapNhatDuLieuThoiTiet(ketQuaHanoi.DuLieuThoiTiet, ketQuaHanoi.ViTri?.TenDayDu ?? "Hà Nội");
                    }
                }

                // Bắt đầu timer cập nhật
                dongHoCapNhat.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải dữ liệu ban đầu: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu thời tiết
        /// </summary>
        private void CapNhatDuLieuThoiTiet(OneCallResponse duLieu, string diaDiem)
        {
            duLieuThoiTiet = duLieu;
            diaDiemHienTai = diaDiem;

            if (duLieu?.Current != null)
            {
                viDoHienTai = duLieu.Lat;
                kinhDoHienTai = duLieu.Lon;
            }

            // Cập nhật các controls
            var laCelsius = hienThiThoiTiet.LayChuyenDoiNhietDo().LaCelsius;
            hienThiThoiTiet.CapNhatThoiTiet(duLieu, diaDiem, laCelsius);
            duBaoTheoGioControl.CapNhatDuBaoTheoGio(duLieu, laCelsius);
            duBaoNhieuNgayControl.CapNhatDuBao5Ngay(duLieu, laCelsius);
        }

        /// <summary>
        /// Cập nhật đơn vị hiển thị
        /// </summary>
        private async Task CapNhatDonViHienThi(bool laCelsius)
        {
            if (duLieuThoiTiet != null)
            {
                hienThiThoiTiet.CapNhatThoiTiet(duLieuThoiTiet, diaDiemHienTai, laCelsius);
                duBaoTheoGioControl.LaCelsius = laCelsius;
                duBaoNhieuNgayControl.LaCelsius = laCelsius;
            }
        }

        /// <summary>
        /// Xử lý chọn ngày trong dự báo 5 ngày
        /// </summary>
        private void XuLyChonNgay(int ngayIndex)
        {
            // Có thể thêm logic xử lý khi chọn ngày
            System.Diagnostics.Debug.WriteLine($"Đã chọn ngày thứ {ngayIndex}");
        }

        /// <summary>
        /// Cập nhật thời tiết tự động
        /// </summary>
        private async Task CapNhatThoiTietTuDong()
        {
            if (!string.IsNullOrEmpty(diaDiemHienTai))
            {
                var ketQua = await xuLyTimKiem.TimKiemDiaDiem(diaDiemHienTai);
                if (ketQua.ThanhCong && ketQua.DuLieuThoiTiet != null)
                {
                    CapNhatDuLieuThoiTiet(ketQua.DuLieuThoiTiet, ketQua.ViTri?.TenDayDu ?? diaDiemHienTai);
                }
            }
        }

        #region Sự kiện UI

        /// <summary>
        /// Sự kiện click nút tìm kiếm
        /// </summary>
        private async void btnTimKiem_Click(object sender, EventArgs e)
        {
            var tenDiaDiem = oTimKiemDiaDiem.Text.Trim();
            if (string.IsNullOrEmpty(tenDiaDiem))
            {
                MessageBox.Show("Vui lòng nhập tên địa điểm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Hiển thị loading
                btnTimKiem.Enabled = false;
                btnTimKiem.Text = "Đang tìm...";

                var ketQua = await xuLyTimKiem.TimKiemDiaDiem(tenDiaDiem);
                
                if (ketQua.ThanhCong && ketQua.DuLieuThoiTiet != null)
                {
                    CapNhatDuLieuThoiTiet(ketQua.DuLieuThoiTiet, ketQua.ViTri?.TenDayDu ?? tenDiaDiem);
                }
                else
                {
                    MessageBox.Show(ketQua.ThongBaoLoi, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTimKiem.Enabled = true;
                btnTimKiem.Text = "Tìm kiếm";
            }
        }

        /// <summary>
        /// Sự kiện nhấn Enter trong ô tìm kiếm
        /// </summary>
        private async void oTimKiemDiaDiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                await btnTimKiem_Click(sender, e);
            }
        }

        /// <summary>
        /// Sự kiện thay đổi text trong ô tìm kiếm (gợi ý)
        /// </summary>
        private async void oTimKiemDiaDiem_TextChanged(object sender, EventArgs e)
        {
            var text = oTimKiemDiaDiem.Text.Trim();
            if (text.Length >= 2)
            {
                // Có thể thêm logic gợi ý ở đây
                var goiY = await xuLyTimKiem.LayGoiYDiaDiem(text);
                // Hiển thị gợi ý nếu cần
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Cấu hình khởi tạo
        /// </summary>
        private void CauHinhKhoiTao()
        {
            this.Text = "Dự báo thời tiết";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(135, 206, 235); // Sky blue
        }

        /// <summary>
        /// Áp dụng style glassmorphism
        /// </summary>
        private void ApDungStyleGlassmorphism()
        {
            // Áp dụng style cho các controls
            hienThiThoiTiet?.ApDungStyleGlassmorphism();
            duBaoTheoGioControl?.ApDungStyleGlassmorphism();
            duBaoNhieuNgayControl?.ApDungStyleGlassmorphism();
        }

        /// <summary>
        /// Làm tròn góc cho control
        /// </summary>
        private void ApplyRoundedCorners(Control control, int radius)
        {
            if (control != null)
            {
                control.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius));
            }
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

        #endregion

    }
}