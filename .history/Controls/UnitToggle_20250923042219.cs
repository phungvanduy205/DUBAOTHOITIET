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

        private bool isCelsius = true;
        public bool IsCelsius
        {
            get => isCelsius;
            set
            {
                if (isCelsius == value) return;
                isCelsius = value;
                UpdateUI();
                UnitChanged?.Invoke(this, isCelsius);
            }
        }

        public event EventHandler<bool>? UnitChanged;

        public UnitToggle()
        {
            Size = new Size(96, 34);
            BackColor = Color.Transparent;

            container = new Panel
            {
                Size = new Size(96, 34),
                BackColor = Color.FromArgb(170, 255, 255, 255)
            };
            container.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, container.Width, container.Height, 17, 17));
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

            UpdateUI();
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

