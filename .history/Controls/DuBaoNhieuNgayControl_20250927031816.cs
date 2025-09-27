using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using THOITIET.Models;

namespace THOITIET.Controls
{
    /// <summary>
    /// Control hiển thị dự báo thời tiết 5 ngày
    /// </summary>
    public partial class DuBaoNhieuNgayControl : UserControl
    {
        private FlowLayoutPanel flowPanel;
        private bool _laCelsius = true;
        private int _ngayDuocChon = -1;

        public bool LaCelsius
        {
            get => _laCelsius;
            set
            {
                if (_laCelsius != value)
                {
                    _laCelsius = value;
                    CapNhatDonViHienThi();
                }
            }
        }

        public event EventHandler<int>? NgayDuocChon;

        public DuBaoNhieuNgayControl()
        {
            KhoiTaoGiaoDien();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Size = new Size(800, 150);
            this.BackColor = Color.Transparent;

            // FlowLayoutPanel để chứa các card ngày
            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 5, 10, 5)
            };

            this.Controls.Add(flowPanel);
        }

        /// <summary>
        /// Cập nhật dữ liệu dự báo 5 ngày
        /// </summary>
        public void CapNhatDuBao5Ngay(OneCallResponse duLieu, bool laCelsius = true)
        {
            if (duLieu?.Daily == null) return;

            _laCelsius = laCelsius;
            flowPanel.Controls.Clear();
            _ngayDuocChon = -1;

            var soNgay = Math.Min(5, duLieu.Daily.Length);
            for (int i = 0; i < soNgay; i++)
            {
                var daily = duLieu.Daily[i];
                var card = TaoCardNgay(daily, i);
                flowPanel.Controls.Add(card);
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu dự báo 5 ngày từ danh sách
        /// </summary>
        public void CapNhatDuBao5Ngay(List<DuBaoNgayItem> danhSachNgay, bool laCelsius = true)
        {
            _laCelsius = laCelsius;
            flowPanel.Controls.Clear();
            _ngayDuocChon = -1;

            for (int i = 0; i < danhSachNgay.Count; i++)
            {
                var ngay = danhSachNgay[i];
                var card = TaoCardNgayTuDuLieu(ngay, i);
                flowPanel.Controls.Add(card);
            }
        }

        /// <summary>
        /// Tạo card cho một ngày
        /// </summary>
        private Panel TaoCardNgay(DailyWeather daily, int index)
        {
            var card = new Panel
            {
                Size = new Size(120, 130),
                Margin = new Padding(5, 5, 5, 5),
                BackColor = Color.FromArgb(80, 255, 255, 255),
                Cursor = Cursors.Hand,
                Tag = index
            };

            // Làm tròn góc
            card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 15, 15));

            // Thêm sự kiện click
            card.Click += CardNgay_Click;
            foreach (Control control in card.Controls)
            {
                control.Click += CardNgay_Click;
            }

            // Tên ngày
            var ngay = DateTimeOffset.FromUnixTimeSeconds(daily.Dt).ToLocalTime();
            var tenNgay = index == 0 ? "Hôm nay" : 
                         index == 1 ? "Ngày mai" : 
                         LayTenNgayTiengViet(ngay.DayOfWeek);
            
            var lblTenNgay = new Label
            {
                Text = tenNgay,
                Size = new Size(110, 25),
                Location = new Point(5, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Ngày tháng
            var lblNgayThang = new Label
            {
                Text = ngay.ToString("dd/MM"),
                Size = new Size(110, 20),
                Location = new Point(5, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent
            };

            // Icon thời tiết
            var picIcon = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(35, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Nhiệt độ cao
            var nhietDoCao = _laCelsius ? 
                Math.Round(daily.Temp?.Max ?? 0, 1) : 
                Math.Round((daily.Temp?.Max ?? 0) * 9.0 / 5.0 + 32, 1);
            var lblNhietDoCao = new Label
            {
                Text = $"{nhietDoCao}°",
                Size = new Size(50, 20),
                Location = new Point(10, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Nhiệt độ thấp
            var nhietDoThap = _laCelsius ? 
                Math.Round(daily.Temp?.Min ?? 0, 1) : 
                Math.Round((daily.Temp?.Min ?? 0) * 9.0 / 5.0 + 32, 1);
            var lblNhietDoThap = new Label
            {
                Text = $"{nhietDoThap}°",
                Size = new Size(50, 20),
                Location = new Point(60, 105),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            // Mô tả thời tiết
            var weather = daily.Weather?.Length > 0 ? daily.Weather[0] : null;
            if (weather != null)
            {
                card.Tag = new { Index = index, Description = weather.Description };
            }

            card.Controls.AddRange(new Control[] { 
                lblTenNgay, lblNgayThang, picIcon, lblNhietDoCao, lblNhietDoThap 
            });

            return card;
        }

        /// <summary>
        /// Tạo card từ dữ liệu DuBaoNgayItem
        /// </summary>
        private Panel TaoCardNgayTuDuLieu(DuBaoNgayItem ngay, int index)
        {
            var card = new Panel
            {
                Size = new Size(120, 130),
                Margin = new Padding(5, 5, 5, 5),
                BackColor = Color.FromArgb(80, 255, 255, 255),
                Cursor = Cursors.Hand,
                Tag = index
            };

            // Làm tròn góc
            card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 15, 15));

            // Thêm sự kiện click
            card.Click += CardNgay_Click;
            foreach (Control control in card.Controls)
            {
                control.Click += CardNgay_Click;
            }

            // Tên ngày
            var tenNgay = index == 0 ? "Hôm nay" : 
                         index == 1 ? "Ngày mai" : 
                         LayTenNgayTiengViet(ngay.Ngay.DayOfWeek);
            
            var lblTenNgay = new Label
            {
                Text = tenNgay,
                Size = new Size(110, 25),
                Location = new Point(5, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Ngày tháng
            var lblNgayThang = new Label
            {
                Text = ngay.Ngay.ToString("dd/MM"),
                Size = new Size(110, 20),
                Location = new Point(5, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent
            };

            // Icon thời tiết
            var picIcon = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(35, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Nhiệt độ cao
            var nhietDoCao = _laCelsius ? 
                Math.Round(ngay.NhietDoCao, 1) : 
                Math.Round(ngay.NhietDoCao * 9.0 / 5.0 + 32, 1);
            var lblNhietDoCao = new Label
            {
                Text = $"{nhietDoCao}°",
                Size = new Size(50, 20),
                Location = new Point(10, 105),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Nhiệt độ thấp
            var nhietDoThap = _laCelsius ? 
                Math.Round(ngay.NhietDoThap, 1) : 
                Math.Round(ngay.NhietDoThap * 9.0 / 5.0 + 32, 1);
            var lblNhietDoThap = new Label
            {
                Text = $"{nhietDoThap}°",
                Size = new Size(50, 20),
                Location = new Point(60, 105),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { 
                lblTenNgay, lblNgayThang, picIcon, lblNhietDoCao, lblNhietDoThap 
            });

            return card;
        }

        /// <summary>
        /// Sự kiện click vào card ngày
        /// </summary>
        private void CardNgay_Click(object? sender, EventArgs e)
        {
            if (sender is Panel card && card.Tag is int index)
            {
                ChonNgay(index);
            }
            else if (sender is Control control && control.Parent is Panel parentCard && parentCard.Tag is int parentIndex)
            {
                ChonNgay(index);
            }
        }

        /// <summary>
        /// Chọn ngày
        /// </summary>
        private void ChonNgay(int index)
        {
            if (_ngayDuocChon == index) return;

            // Bỏ highlight ngày cũ
            if (_ngayDuocChon >= 0 && _ngayDuocChon < flowPanel.Controls.Count)
            {
                var cardCu = flowPanel.Controls[_ngayDuocChon] as Panel;
                if (cardCu != null)
                {
                    cardCu.BackColor = Color.FromArgb(80, 255, 255, 255);
                }
            }

            // Highlight ngày mới
            _ngayDuocChon = index;
            if (index >= 0 && index < flowPanel.Controls.Count)
            {
                var cardMoi = flowPanel.Controls[index] as Panel;
                if (cardMoi != null)
                {
                    cardMoi.BackColor = Color.FromArgb(120, 255, 255, 255);
                }
            }

            // Thông báo sự kiện
            NgayDuocChon?.Invoke(this, index);
        }

        /// <summary>
        /// Cập nhật đơn vị hiển thị
        /// </summary>
        private void CapNhatDonViHienThi()
        {
            foreach (Panel card in flowPanel.Controls)
            {
                var labels = card.Controls.OfType<Label>().ToList();
                var lblNhietDoCao = labels.FirstOrDefault(l => l.Font.Bold && l.Text.Contains("°"));
                var lblNhietDoThap = labels.FirstOrDefault(l => !l.Font.Bold && l.Text.Contains("°"));

                if (lblNhietDoCao != null && lblNhietDoThap != null)
                {
                    // Lấy nhiệt độ gốc từ tag hoặc tính lại
                    var nhietDoCaoGoc = LayNhietDoGocTuCard(card, true);
                    var nhietDoThapGoc = LayNhietDoGocTuCard(card, false);

                    if (nhietDoCaoGoc.HasValue)
                    {
                        var nhietDoCao = _laCelsius ? 
                            Math.Round(nhietDoCaoGoc.Value, 1) : 
                            Math.Round(nhietDoCaoGoc.Value * 9.0 / 5.0 + 32, 1);
                        lblNhietDoCao.Text = $"{nhietDoCao}°";
                    }

                    if (nhietDoThapGoc.HasValue)
                    {
                        var nhietDoThap = _laCelsius ? 
                            Math.Round(nhietDoThapGoc.Value, 1) : 
                            Math.Round(nhietDoThapGoc.Value * 9.0 / 5.0 + 32, 1);
                        lblNhietDoThap.Text = $"{nhietDoThap}°";
                    }
                }
            }
        }

        /// <summary>
        /// Lấy nhiệt độ gốc từ card (Celsius)
        /// </summary>
        private double? LayNhietDoGocTuCard(Panel card, bool laCao)
        {
            // Có thể lưu nhiệt độ gốc trong Tag của card
            if (card.Tag is { } tag)
            {
                // Implement logic lấy nhiệt độ từ tag
            }
            
            // Hoặc parse từ text hiện tại
            var labels = card.Controls.OfType<Label>().ToList();
            var lblNhietDo = laCao ? 
                labels.FirstOrDefault(l => l.Font.Bold && l.Text.Contains("°")) :
                labels.FirstOrDefault(l => !l.Font.Bold && l.Text.Contains("°"));

            if (lblNhietDo != null)
            {
                var text = lblNhietDo.Text.Replace("°", "");
                if (double.TryParse(text, out double nhietDoHienTai))
                {
                    // Nếu đang hiển thị Fahrenheit, chuyển về Celsius
                    if (!_laCelsius)
                    {
                        return (nhietDoHienTai - 32) * 5.0 / 9.0;
                    }
                    return nhietDoHienTai;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Lấy tên ngày tiếng Việt
        /// </summary>
        private string LayTenNgayTiengViet(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "Chủ nhật",
                DayOfWeek.Monday => "Thứ hai",
                DayOfWeek.Tuesday => "Thứ ba",
                DayOfWeek.Wednesday => "Thứ tư",
                DayOfWeek.Thursday => "Thứ năm",
                DayOfWeek.Friday => "Thứ sáu",
                DayOfWeek.Saturday => "Thứ bảy",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Xóa tất cả dữ liệu
        /// </summary>
        public void XoaDuLieu()
        {
            flowPanel.Controls.Clear();
            _ngayDuocChon = -1;
        }

        /// <summary>
        /// Áp dụng style glassmorphism
        /// </summary>
        public void ApDungStyleGlassmorphism()
        {
            this.BackColor = Color.FromArgb(30, 255, 255, 255);
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 15, 15));
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