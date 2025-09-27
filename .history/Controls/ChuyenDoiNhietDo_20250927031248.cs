using System;
using System.Drawing;
using System.Windows.Forms;

namespace THOITIET.Controls
{
    /// <summary>
    /// Control chuyển đổi nhiệt độ giữa Celsius và Fahrenheit
    /// </summary>
    public partial class ChuyenDoiNhietDo : UserControl
    {
        public event EventHandler<bool>? DonViThayDoi;

        private bool _laCelsius = true;
        private Button btnChuyenDoi;
        private Label lblDonVi;

        public bool LaCelsius
        {
            get => _laCelsius;
            set
            {
                if (_laCelsius != value)
                {
                    _laCelsius = value;
                    CapNhatHienThi();
                    DonViThayDoi?.Invoke(this, _laCelsius);
                }
            }
        }

        public ChuyenDoiNhietDo()
        {
            KhoiTaoGiaoDien();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Size = new Size(120, 40);
            this.BackColor = Color.Transparent;

            // Button chuyển đổi
            btnChuyenDoi = new Button
            {
                Text = "°C",
                Size = new Size(50, 30),
                Location = new Point(5, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 255, 255, 255),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChuyenDoi.FlatAppearance.BorderSize = 0;
            btnChuyenDoi.Click += BtnChuyenDoi_Click;

            // Label đơn vị
            lblDonVi = new Label
            {
                Text = "Celsius",
                Size = new Size(60, 30),
                Location = new Point(60, 5),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent
            };

            this.Controls.Add(btnChuyenDoi);
            this.Controls.Add(lblDonVi);
        }

        private void BtnChuyenDoi_Click(object? sender, EventArgs e)
        {
            LaCelsius = !LaCelsius;
        }

        private void CapNhatHienThi()
        {
            if (_laCelsius)
            {
                btnChuyenDoi.Text = "°C";
                lblDonVi.Text = "Celsius";
                btnChuyenDoi.BackColor = Color.FromArgb(100, 255, 255, 255);
            }
            else
            {
                btnChuyenDoi.Text = "°F";
                lblDonVi.Text = "Fahrenheit";
                btnChuyenDoi.BackColor = Color.FromArgb(100, 255, 200, 100);
            }
        }

        /// <summary>
        /// Chuyển đổi nhiệt độ từ Kelvin sang đơn vị hiện tại
        /// </summary>
        public double ChuyenDoiTuKelvin(double nhietDoKelvin)
        {
            if (_laCelsius)
            {
                return Math.Round(nhietDoKelvin - 273.15, 1);
            }
            else
            {
                return Math.Round((nhietDoKelvin - 273.15) * 9.0 / 5.0 + 32, 1);
            }
        }

        /// <summary>
        /// Chuyển đổi nhiệt độ từ đơn vị hiện tại sang Kelvin
        /// </summary>
        public double ChuyenDoiSangKelvin(double nhietDo)
        {
            if (_laCelsius)
            {
                return nhietDo + 273.15;
            }
            else
            {
                return (nhietDo - 32) * 5.0 / 9.0 + 273.15;
            }
        }

        /// < <summary>
        /// Chuyển đổi nhiệt độ từ đơn vị này sang đơn vị khác
        /// </summary>
        public double ChuyenDoi(double nhietDo, bool tuCelsius)
        {
            if (tuCelsius == _laCelsius)
            {
                return nhietDo; // Không cần chuyển đổi
            }

            if (tuCelsius) // Từ Celsius sang Fahrenheit
            {
                return Math.Round(nhietDo * 9.0 / 5.0 + 32, 1);
            }
            else // Từ Fahrenheit sang Celsius
            {
                return Math.Round((nhietDo - 32) * 5.0 / 9.0, 1);
            }
        }

        /// <summary>
        /// Lấy ký hiệu đơn vị hiện tại
        /// </summary>
        public string LayKyHieuDonVi()
        {
            return _laCelsius ? "°C" : "°F";
        }

        /// <summary>
        /// Lấy tên đơn vị hiện tại
        /// </summary>
        public string LayTenDonVi()
        {
            return _laCelsius ? "Celsius" : "Fahrenheit";
        }

        /// <summary>
        /// Áp dụng style glassmorphism cho control
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