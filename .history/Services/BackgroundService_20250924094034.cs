using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace THOITIET.Services
{
    /// <summary>
    /// Service xử lý background động theo thời tiết
    /// </summary>
    public class BackgroundService
    {
        private PictureBox? backgroundPictureBox;
        private Form parentForm;

        public BackgroundService(Form form)
        {
            parentForm = form;
            InitializeBackgroundPictureBox();
        }

        /// <summary>
        /// Khởi tạo PictureBox cho background
        /// </summary>
        private void InitializeBackgroundPictureBox()
        {
            backgroundPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            parentForm.Controls.Add(backgroundPictureBox);
            backgroundPictureBox.SendToBack();
        }

        /// <summary>
        /// Set background theo thời tiết
        /// </summary>
        public void SetBackground(string weatherMain, int weatherId)
        {
            try
            {
                var backgroundImage = GetWeatherBackground(weatherMain, weatherId);
                if (backgroundImage != null)
                {
                    backgroundPictureBox.Image = backgroundImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi set background: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy hình nền theo thời tiết
        /// </summary>
        private Image GetWeatherBackground(string weatherMain, int weatherId)
        {
            try
            {
                // Tạo gradient background dựa trên thời tiết
                var bitmap = new Bitmap(parentForm.Width, parentForm.Height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var colors = GetWeatherColors(weatherMain, weatherId);
                    var brush = new LinearGradientBrush(
                        new Point(0, 0),
                        new Point(0, parentForm.Height),
                        colors.Item1,
                        colors.Item2
                    );

                    graphics.FillRectangle(brush, 0, 0, parentForm.Width, parentForm.Height);
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo background: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy màu sắc theo thời tiết
        /// </summary>
        private (Color, Color) GetWeatherColors(string weatherMain, int weatherId)
        {
            return weatherMain?.ToLower() switch
            {
                "clear" => (Color.FromArgb(135, 206, 250), Color.FromArgb(255, 255, 255)), // Sky blue to white
                "clouds" => (Color.FromArgb(176, 196, 222), Color.FromArgb(240, 248, 255)), // Light steel blue
                "rain" => (Color.FromArgb(105, 105, 105), Color.FromArgb(192, 192, 192)), // Dim gray to silver
                "drizzle" => (Color.FromArgb(119, 136, 153), Color.FromArgb(211, 211, 211)), // Light slate gray
                "thunderstorm" => (Color.FromArgb(47, 79, 79), Color.FromArgb(105, 105, 105)), // Dark slate gray
                "snow" => (Color.FromArgb(248, 248, 255), Color.FromArgb(255, 255, 255)), // Ghost white to white
                "mist" or "fog" or "haze" => (Color.FromArgb(169, 169, 169), Color.FromArgb(220, 220, 220)), // Dark gray to light gray
                _ => (Color.FromArgb(135, 206, 250), Color.FromArgb(255, 255, 255)) // Default sky blue
            };
        }

        /// <summary>
        /// Set background mặc định theo thời gian
        /// </summary>
        public void SetDefaultBackgroundOnStartup()
        {
            try
            {
                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour > 18;

                if (isNight)
                {
                    SetBackground("night", 0);
                }
                else
                {
                    SetBackground("clear", 800);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi set background mặc định: {ex.Message}");
            }
        }

        /// <summary>
        /// Test background
        /// </summary>
        public void TestBackground()
        {
            try
            {
                SetBackground("clear", 800);
                System.Diagnostics.Debug.WriteLine("Test background: Clear sky");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi test background: {ex.Message}");
            }
        }

        /// <summary>
        /// Force set background trong Form1_Load
        /// </summary>
        public void ForceSetBackgroundInLoad()
        {
            try
            {
                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour > 18;

                if (isNight)
                {
                    SetBackground("night", 0);
                }
                else
                {
                    SetBackground("clear", 800);
                }

                System.Diagnostics.Debug.WriteLine($"Force set background: {(isNight ? "Night" : "Day")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi force set background: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            backgroundPictureBox?.Dispose();
        }
    }
}