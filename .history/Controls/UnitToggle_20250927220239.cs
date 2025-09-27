using System;
using System.Drawing;
using System.Windows.Forms;

namespace THOITIET.Controls
{
    public class UnitToggle : UserControl
    {
        private readonly Panel container;
        private readonly Button btnC;
        private readonly Button btnF;

        private bool laCelsius = true;
        public bool LaCelsius
        {
            get => laCelsius;
            set
            {
                if (laCelsius == value) return;
                laCelsius = value;
                CapNhatGiaoDien();
                DonViThayDoi?.Invoke(this, laCelsius);
            }
        }

        public event EventHandler<bool>? DonViThayDoi;

        public UnitToggle()
        {
            Size = new Size(96, 34);
            BackColor = Color.Transparent;

            container = new Panel
            {
                Size = new Size(96, 34),
                BackColor = Color.FromArgb(170, 255, 255, 255)
            };
            Controls.Add(container);

            btnC = new Button
            {
                Text = "°C",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(48, 30),
                Location = new Point(2, 2),
                TabStop = false
            };
            btnC.FlatAppearance.BorderSize = 0;

            btnF = new Button
            {
                Text = "°F",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(48, 30),
                Location = new Point(46, 2),
                TabStop = false
            };
            btnF.FlatAppearance.BorderSize = 0;

            btnC.Click += (s, e) => { if (!IsCelsius) IsCelsius = true; };
            btnF.Click += (s, e) => { if (IsCelsius) IsCelsius = false; };

            container.Controls.Add(btnC);
            container.Controls.Add(btnF);

            CapNhatGiaoDien();
            
            // Tạo region sau khi control đã được thêm vào form
            this.Load += (s, e) => ApplyRoundedCorners();
        }

        private void ApplyRoundedCorners()
        {
            try
            {
                // Bo tròn container
                var containerPath = new System.Drawing.Drawing2D.GraphicsPath();
                containerPath.AddArc(0, 0, 34, 34, 180, 90);
                containerPath.AddArc(container.Width - 34, 0, 34, 34, 270, 90);
                containerPath.AddArc(container.Width - 34, container.Height - 34, 34, 34, 0, 90);
                containerPath.AddArc(0, container.Height - 34, 34, 34, 90, 90);
                containerPath.CloseAllFigures();
                container.Region = new System.Drawing.Region(containerPath);

                // Bo tròn nút °C
                var btnCPath = new System.Drawing.Drawing2D.GraphicsPath();
                btnCPath.AddArc(0, 0, 30, 30, 180, 90);
                btnCPath.AddArc(btnC.Width - 30, 0, 30, 30, 270, 90);
                btnCPath.AddArc(btnC.Width - 30, btnC.Height - 30, 30, 30, 0, 90);
                btnCPath.AddArc(0, btnC.Height - 30, 30, 30, 90, 90);
                btnCPath.CloseAllFigures();
                btnC.Region = new System.Drawing.Region(btnCPath);

                // Bo tròn nút °F
                var btnFPath = new System.Drawing.Drawing2D.GraphicsPath();
                btnFPath.AddArc(0, 0, 30, 30, 180, 90);
                btnFPath.AddArc(btnF.Width - 30, 0, 30, 30, 270, 90);
                btnFPath.AddArc(btnF.Width - 30, btnF.Height - 30, 30, 30, 0, 90);
                btnFPath.AddArc(0, btnF.Height - 30, 30, 30, 90, 90);
                btnFPath.CloseAllFigures();
                btnF.Region = new System.Drawing.Region(btnFPath);
            }
            catch
            {
                // Nếu không thể tạo region, bỏ qua
            }
        }

        private void UpdateUI()
        {
            if (isCelsius)
            {
                btnC.BackColor = Color.FromArgb(230, 255, 255, 255);
                btnC.ForeColor = Color.FromArgb(33, 150, 243);
                btnF.BackColor = Color.Transparent;
                btnF.ForeColor = Color.White;
            }
            else
            {
                btnF.BackColor = Color.FromArgb(230, 255, 255, 255);
                btnF.ForeColor = Color.FromArgb(33, 150, 243);
                btnC.BackColor = Color.Transparent;
                btnC.ForeColor = Color.White;
            }
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}

