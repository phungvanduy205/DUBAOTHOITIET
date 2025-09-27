using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using THOITIET.Models;

namespace THOITIET.Controls
{
    /// <summary>
    /// Control hiển thị dự báo thời tiết theo giờ (24h)
    /// </summary>
    public partial class DuBaoTheoGioControl : UserControl
    {
        private FlowLayoutPanel flowPanel;
        private bool _laCelsius = true;

        public bool LaCelsius
        {
            get => _laCelsius;
            set
            {
                if (_laCelsius != value)
                {
                    _laCelsius = value;
                    // Cập nhật lại hiển thị nếu có dữ liệu
                    if (flowPanel.Controls.Count > 0)
                    {
                        CapNhatDonViHienThi();
                    }
                }
            }
        }

        public DuBaoTheoGioControl()
        {
            KhoiTaoGiaoDien();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Size = new Size(800, 120);
            this.BackColor = Color.Transparent;

            // FlowLayoutPanel để chứa các card giờ
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
        /// Cập nhật dữ liệu dự báo theo giờ
        /// </summary>
        public void CapNhatDuBaoTheoGio(OneCallResponse duLieu, bool laCelsius = true)
        {
            if (duLieu?.Hourly == null) return;

            _laCelsius = laCelsius;
            flowPanel.Controls.Clear();

            var soGio = Math.Min(24, duLieu.Hourly.Length);
            for (int i = 0; i < soGio; i++)
            {
                var hourly = duLieu.Hourly[i];
                var card = TaoCardGio(hourly, i);
                flowPanel.Controls.Add(card);
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu dự báo theo giờ từ danh sách
        /// </summary>
        public void CapNhatDuBaoTheoGio(List<DuBaoTheoGioItem> danhSachGio, bool laCelsius = true)
        {
            _laCelsius = laCelsius;
            flowPanel.Controls.Clear();

            foreach (var gio in danhSachGio)
            {
                var card = TaoCardGioTuDuLieu(gio);
                flowPanel.Controls.Add(card);
            }
        }

        /// <summary>
        /// Tạo card cho một giờ
        /// </summary>
        private Panel TaoCardGio(HourlyWeather hourly, int index)
        {
            var card = new Panel
            {
                Size = new Size(80, 100),
                Margin = new Padding(5, 5, 5, 5),
                BackColor = Color.FromArgb(80, 255, 255, 255),
                Cursor = Cursors.Hand
            };

            // Làm tròn góc
            card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 10, 10));

            // Thời gian
            var thoiGian = DateTimeOffset.FromUnixTimeSeconds(hourly.Dt).ToLocalTime();
            var lblThoiGian = new Label
            {
                Text = index == 0 ? "Bây giờ" : thoiGian.ToString("HH:mm"),
                Size = new Size(70, 20),
                Location = new Point(5, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Icon thời tiết
            var picIcon = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(20, 25),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Nhiệt độ
            var nhietDo = _laCelsius ? 
                Math.Round(hourly.Temp, 1) : 
                Math.Round(hourly.Temp * 9.0 / 5.0 + 32, 1);
            var lblNhietDo = new Label
            {
                Text = $"{nhietDo}°",
                Size = new Size(70, 20),
                Location = new Point(5, 70),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Mô tả thời tiết (nếu có)
            var weather = hourly.Weather?.Length > 0 ? hourly.Weather[0] : null;
            if (weather != null)
            {
                // Có thể thêm tooltip hoặc label mô tả
                card.Tag = weather.Description;
            }

            card.Controls.AddRange(new Control[] { lblThoiGian, picIcon, lblNhietDo });
            return card;
        }

        /// <summary>
        /// Tạo card từ dữ liệu DuBaoTheoGioItem
        /// </summary>
        private Panel TaoCardGioTuDuLieu(DuBaoTheoGioItem gio)
        {
            var card = new Panel
            {
                Size = new Size(80, 100),
                Margin = new Padding(5, 5, 5, 5),
                BackColor = Color.FromArgb(80, 255, 255, 255),
                Cursor = Cursors.Hand
            };

            // Làm tròn góc
            card.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 10, 10));

            // Thời gian
            var lblThoiGian = new Label
            {
                Text = gio.ThoiGian.ToString("HH:mm"),
                Size = new Size(70, 20),
                Location = new Point(5, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            // Icon thời tiết
            var picIcon = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(20, 25),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Nhiệt độ
            var nhietDo = _laCelsius ? 
                Math.Round(gio.NhietDo, 1) : 
                Math.Round(gio.NhietDo * 9.0 / 5.0 + 32, 1);
            var lblNhietDo = new Label
            {
                Text = $"{nhietDo}°",
                Size = new Size(70, 20),
                Location = new Point(5, 70),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            card.Controls.AddRange(new Control[] { lblThoiGian, picIcon, lblNhietDo });
            return card;
        }

        /// <summary>
        /// Cập nhật đơn vị hiển thị
        /// </summary>
        private void CapNhatDonViHienThi()
        {
            foreach (Panel card in flowPanel.Controls)
            {
                var lblNhietDo = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Bold && l.Text.Contains("°"));
                if (lblNhietDo != null)
                {
                    // Lấy nhiệt độ gốc từ tag hoặc tính lại
                    var nhietDoGoc = LayNhietDoGocTuCard(card);
                    if (nhietDoGoc.HasValue)
                    {
                        var nhietDo = _laCelsius ? 
                            Math.Round(nhietDoGoc.Value, 1) : 
                            Math.Round(nhietDoGoc.Value * 9.0 / 5.0 + 32, 1);
                        lblNhietDo.Text = $"{nhietDo}°";
                    }
                }
            }
        }

        /// <summary>
        /// Lấy nhiệt độ gốc từ card (Celsius)
        /// </summary>
        private double? LayNhietDoGocTuCard(Panel card)
        {
            // Có thể lưu nhiệt độ gốc trong Tag của card
            if (card.Tag is double nhietDo)
                return nhietDo;
            
            // Hoặc parse từ text hiện tại
            var lblNhietDo = card.Controls.OfType<Label>().FirstOrDefault(l => l.Font.Bold && l.Text.Contains("°"));
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
        /// Xóa tất cả dữ liệu
        /// </summary>
        public void XoaDuLieu()
        {
            flowPanel.Controls.Clear();
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