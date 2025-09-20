
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

namespace THOITIET
{
    /// <summary>
    /// Form chính: xử lý sự kiện, gọi dịch vụ, cập nhật giao diện
    /// </summary>
    public partial class Form1 : Form
    {
        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;

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

            // Test background ngay khi form load
            TestBackground();

            // Tạo file icon thật
            TaoFileIconThuc();
        }

        /// <summary>
        /// Test background ngay khi form load
        /// </summary>
        private void TestBackground()
        {
            System.Diagnostics.Debug.WriteLine("Test background khi form load");

            // Tạo background gradient đẹp
            anhNenDong.Image = TaoBackgroundTest("nen_troi_quang");
            anhNenDong.Visible = true;
            anhNenDong.SendToBack();
            anhNenDong.Refresh();

            // Force refresh toàn bộ panel
            khuVucTrai_HienTai.Refresh();
            this.Refresh();

            System.Diagnostics.Debug.WriteLine($"Background test size: {anhNenDong.Image?.Size}");
            System.Diagnostics.Debug.WriteLine($"Panel size: {khuVucTrai_HienTai.Size}");
        }

        /// <summary>
        /// Cấu hình ban đầu cho form, timer, v.v.
        /// </summary>
        private void CauHinhKhoiTao()
        {
            // Timer 1 giờ
            dongHoCapNhat.Interval = 60 * 60 * 1000;
            dongHoCapNhat.Tick += async (s, e) => { await CapNhatThoiTiet(); };
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

                // CheckBox đơn vị - trong suốt
                try
                {
                    CongTacDonVi.BackColor = Color.Transparent;
                }
                catch
                {
                    CongTacDonVi.BackColor = Color.FromArgb(200, 255, 255, 255); // Fallback
                }
                CongTacDonVi.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

                // Labels - màu trắng, font đẹp
                nhanTenDiaDiem.ForeColor = Color.White;
                nhanTenDiaDiem.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
                nhanNhietDoHienTai.ForeColor = Color.White;
                nhanNhietDoHienTai.Font = new Font("Segoe UI", 48F, FontStyle.Bold);
                nhanTrangThai.ForeColor = Color.White;
                nhanTrangThai.Font = new Font("Segoe UI", 16F, FontStyle.Regular);
                nhanThongTinPhu.ForeColor = Color.White;
                nhanThongTinPhu.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

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

            try
            {
                var viTris = await dichVu.TimDiaDiem(tuKhoa);
                var dauTien = viTris.FirstOrDefault();
                if (dauTien == null)
                {
                    MessageBox.Show("Không tìm thấy địa điểm. Vui lòng kiểm tra lại.", "Không có kết quả",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                viDoHienTai = dauTien.ViDo;
                kinhDoHienTai = dauTien.KinhDo;
                nhanTenDiaDiem.Text = dauTien.TenDayDu;

                await CapNhatThoiTiet();

                if (!dongHoCapNhat.Enabled) dongHoCapNhat.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi tìm địa điểm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Đổi đơn vị °C ↔ °F và cập nhật lại dữ liệu
        /// </summary>
        private async void CongTacDonVi_CheckedChanged(object? sender, EventArgs e)
        {
            // Checked = bật chuyển sang °F
            donViCelsius = !CongTacDonVi.Checked;
            await CapNhatThoiTiet();
        }

        /// <summary>
        /// Gọi API → hiển thị thời tiết hiện tại, dự báo 24h, dự báo 5 ngày; cập nhật nền/biểu tượng
        /// </summary>
        private async Task CapNhatThoiTiet()
        {
            if (viDoHienTai == null || kinhDoHienTai == null) return;

            try
            {
                var donViApi = donViCelsius ? "metric" : "imperial";
                var kyHieuNhietDo = donViCelsius ? "°C" : "°F";

                // Hiện tại
                var hienTai = await dichVu.LayThoiTietHienTai(viDoHienTai.Value, kinhDoHienTai.Value, donViApi);
                nhanNhietDoHienTai.Text = $"{Math.Round(hienTai.NhietDo)}{kyHieuNhietDo}";
                nhanTrangThai.Text = hienTai.TrangThaiMoTa ?? string.Empty;
                nhanThongTinPhu.Text =
                    $"Cảm giác thực tế: {Math.Round(hienTai.NhietDoCamGiac)}{kyHieuNhietDo}\n" +
                    $"Độ ẩm: {hienTai.DoAm}%   Gió: {Math.Round(hienTai.TocDoGio)} {(donViCelsius ? "m/s" : "mph")}\n" +
                    $"Áp suất: {hienTai.ApSuat} hPa   Tầm nhìn: {hienTai.TamNhin / 1000.0:0.0} km\n" +
                    $"Mặt trời mọc: {UnixToLocal(hienTai.MatTroiMoc)}   Mặt trời lặn: {UnixToLocal(hienTai.MatTroiLan)}";

                // Icon + nền
                HienThiIconVaNen(hienTai.MaThoiTiet, hienTai.IconCode);

                // 24 giờ
                var duBaoGio = await dichVu.LayDuBaoTheoGio(viDoHienTai.Value, kinhDoHienTai.Value, donViApi, 24);
                System.Diagnostics.Debug.WriteLine($"Dự báo 24h: {duBaoGio?.Count ?? 0} items");
                DoDuLieuBangTheoGio(duBaoGio, kyHieuNhietDo);

                // 5 ngày
                var duBaoNgay = await dichVu.LayDuBao5Ngay(viDoHienTai.Value, kinhDoHienTai.Value, donViApi);
                System.Diagnostics.Debug.WriteLine($"Dự báo 5 ngày: {duBaoNgay?.Count ?? 0} items");
                DoDuLieuBangNhieuNgay(duBaoNgay, kyHieuNhietDo);

                // Lịch sử
                var lichSu = await dichVu.LayLichSu30Ngay(viDoHienTai.Value, kinhDoHienTai.Value, donViApi);
                System.Diagnostics.Debug.WriteLine($"Lịch sử 30 ngày: {lichSu?.Count ?? 0} items");
                HienThiBangLichSu(lichSu, kyHieuNhietDo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi cập nhật thời tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private static string UnixToLocal(long unix)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().DateTime;
            return dt.ToString("HH:mm");
        }

        private void BangNhieuNgay_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BangTheoGio_Paint(object sender, PaintEventArgs e)
        {

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
}
