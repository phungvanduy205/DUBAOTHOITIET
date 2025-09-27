using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý background thay đổi theo thời tiết
    /// 6️⃣ Background thay đổi theo thời tiết
    /// </summary>
    public class BackgroundService
    {
        private int? lastWeatherId = null;
        private bool? lastIsNight = null;
        private PictureBox? backgroundPictureBox;

        public BackgroundService()
        {
        }

        #region Background Management

        /// <summary>
        /// Thiết lập nền theo thời tiết
        /// </summary>
        public void SetBackground(string weatherMain = "Clear", int weatherId = 800, Panel mainPanel = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SetBackground được gọi với weatherMain: {weatherMain}, weatherId: {weatherId} ===");
                
                if (mainPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("mainPanel is NULL trong SetBackground!");
                    return;
                }

                // Kiểm tra thời gian
                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;

                // Throttle: chỉ thay đổi nếu có sự khác biệt
                if (lastWeatherId == weatherId && lastIsNight == isNight)
                {
                    System.Diagnostics.Debug.WriteLine("SetBackground: Bỏ qua vì không có thay đổi (throttle)");
                    return;
                }

                // Cập nhật trạng thái
                lastWeatherId = weatherId;
                lastIsNight = isNight;

                // Đường dẫn đến thư mục Resources
                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"SetBackground: Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }

                Image? backgroundImage = null;

                // Chọn nền theo thời tiết và thời gian
                switch (weatherMain.ToLower())
                {
                    case "clear":
                        if (isNight)
                        {
                            // Ban đêm: nền đêm yên tĩnh
                            var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                            if (File.Exists(demPath))
                            {
                                backgroundImage = Image.FromFile(demPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_ban_dem.png (clear night - {weatherId})");
                            }
                        }
                        else
                        {
                            // Ban ngày: trời quang
                            var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                            if (File.Exists(quangPath))
                            {
                                backgroundImage = Image.FromFile(quangPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: nen_troi_quang.jpg (clear day - {weatherId})");
                            }
                        }
                        break;

                    case "clouds":
                        if (isNight)
                        {
                            var mayDemPath = Path.Combine(resourcesPath, "may_day_dem.png");
                            if (File.Exists(mayDemPath))
                            {
                                backgroundImage = Image.FromFile(mayDemPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: may_day_dem.png (clouds night - {weatherId})");
                            }
                        }
                        else
                        {
                            var mayNgayPath = Path.Combine(resourcesPath, "may_day_ngay.png");
                            if (File.Exists(mayNgayPath))
                            {
                                backgroundImage = Image.FromFile(mayNgayPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: may_day_ngay.png (clouds day - {weatherId})");
                            }
                        }
                        break;

                    case "rain":
                        if (isNight)
                        {
                            var muaDemPath = Path.Combine(resourcesPath, "mua_dem.png");
                            if (File.Exists(muaDemPath))
                            {
                                backgroundImage = Image.FromFile(muaDemPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: mua_dem.png (rain night - {weatherId})");
                            }
                        }
                        else
                        {
                            var muaNgayPath = Path.Combine(resourcesPath, "mua_ngay.png");
                            if (File.Exists(muaNgayPath))
                            {
                                backgroundImage = Image.FromFile(muaNgayPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: mua_ngay.png (rain day - {weatherId})");
                            }
                        }
                        break;

                    case "thunderstorm":
                        if (isNight)
                        {
                            var giongDemPath = Path.Combine(resourcesPath, "giong_bao_dem.png");
                            if (File.Exists(giongDemPath))
                            {
                                backgroundImage = Image.FromFile(giongDemPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: giong_bao_dem.png (thunderstorm night - {weatherId})");
                            }
                        }
                        else
                        {
                            var giongNgayPath = Path.Combine(resourcesPath, "giong_bao_ngay.png");
                            if (File.Exists(giongNgayPath))
                            {
                                backgroundImage = Image.FromFile(giongNgayPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: giong_bao_ngay.png (thunderstorm day - {weatherId})");
                            }
                        }
                        break;

                    case "snow":
                        if (isNight)
                        {
                            var tuyetDemPath = Path.Combine(resourcesPath, "tuyet_dem.png");
                            if (File.Exists(tuyetDemPath))
                            {
                                backgroundImage = Image.FromFile(tuyetDemPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: tuyet_dem.png (snow night - {weatherId})");
                            }
                        }
                        else
                        {
                            var tuyetNgayPath = Path.Combine(resourcesPath, "tuyet_ngay.png");
                            if (File.Exists(tuyetNgayPath))
                            {
                                backgroundImage = Image.FromFile(tuyetNgayPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: tuyet_ngay.png (snow day - {weatherId})");
                            }
                        }
                        break;

                    case "mist":
                    case "fog":
                    case "haze":
                        if (isNight)
                        {
                            var suongDemPath = Path.Combine(resourcesPath, "suong_mu_dem.png");
                            if (File.Exists(suongDemPath))
                            {
                                backgroundImage = Image.FromFile(suongDemPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: suong_mu_dem.png (mist night - {weatherId})");
                            }
                        }
                        else
                        {
                            var suongNgayPath = Path.Combine(resourcesPath, "suong_mu_ngay.png");
                            if (File.Exists(suongNgayPath))
                            {
                                backgroundImage = Image.FromFile(suongNgayPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền: suong_mu_ngay.png (mist day - {weatherId})");
                            }
                        }
                        break;

                    default:
                        // Fallback: sử dụng nền mặc định
                        if (isNight)
                        {
                            var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                            if (File.Exists(demPath))
                            {
                                backgroundImage = Image.FromFile(demPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền mặc định: nen_ban_dem.png (default night - {weatherId})");
                            }
                        }
                        else
                        {
                            var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                            if (File.Exists(quangPath))
                            {
                                backgroundImage = Image.FromFile(quangPath);
                                System.Diagnostics.Debug.WriteLine($"Chọn nền mặc định: nen_troi_quang.jpg (default day - {weatherId})");
                            }
                        }
                        break;
                }

                // Áp dụng nền
                if (backgroundImage != null)
                {
                    mainPanel.BackgroundImage = backgroundImage;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                    System.Diagnostics.Debug.WriteLine($"Đã áp dụng nền thành công");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Không tìm thấy file nền phù hợp");
                }

                System.Diagnostics.Debug.WriteLine($"=== SetBackground hoàn thành thành công ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi SetBackground: {ex.Message}");
            }
        }

        /// <summary>
        /// Force set nền trong Form1_Load
        /// </summary>
        public void ForceSetBackgroundInLoad(Panel mainPanel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ForceSetBackgroundInLoad ===");
                
                if (mainPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("mainPanel is NULL trong ForceSetBackgroundInLoad!");
                    return;
                }

                // Kiểm tra thời gian
                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;

                System.Diagnostics.Debug.WriteLine($"ForceSetBackground: Thời gian {DateTime.Now:HH:mm}, IsNight: {isNight}");

                // Đường dẫn đến thư mục Resources
                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"ForceSetBackground: Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }

                Image? backgroundImage = null;

                if (isNight)
                {
                    var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                    if (File.Exists(demPath))
                    {
                        backgroundImage = Image.FromFile(demPath);
                        System.Diagnostics.Debug.WriteLine("ForceSetBackground: Chọn nền ban đêm");
                    }
                }
                else
                {
                    var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                    if (File.Exists(quangPath))
                    {
                        backgroundImage = Image.FromFile(quangPath);
                        System.Diagnostics.Debug.WriteLine("ForceSetBackground: Chọn nền ban ngày");
                    }
                }

                if (backgroundImage != null)
                {
                    mainPanel.BackgroundImage = backgroundImage;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                    System.Diagnostics.Debug.WriteLine("ForceSetBackground: Đã force set background thành công");
                }

                System.Diagnostics.Debug.WriteLine("=== End ForceSetBackgroundInLoad ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ForceSetBackgroundInLoad error: {ex.Message}");
            }
        }

        /// <summary>
        /// Thiết lập nền mặc định khi khởi động
        /// </summary>
        public void SetDefaultBackgroundOnStartup(Panel mainPanel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== SetDefaultBackgroundOnStartup ===");
                
                if (mainPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("mainPanel is NULL trong SetDefaultBackgroundOnStartup!");
                    return;
                }

                // Kiểm tra thời gian
                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;

                // Đường dẫn đến thư mục Resources
                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    System.Diagnostics.Debug.WriteLine($"SetDefaultBackgroundOnStartup: Thư mục Resources không tồn tại: {resourcesPath}");
                    return;
                }

                Image? backgroundImage = null;

                if (isNight)
                {
                    var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                    if (File.Exists(demPath))
                    {
                        backgroundImage = Image.FromFile(demPath);
                        System.Diagnostics.Debug.WriteLine("SetDefaultBackgroundOnStartup: Chọn nền ban đêm");
                    }
                }
                else
                {
                    var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                    if (File.Exists(quangPath))
                    {
                        backgroundImage = Image.FromFile(quangPath);
                        System.Diagnostics.Debug.WriteLine("SetDefaultBackgroundOnStartup: Chọn nền ban ngày");
                    }
                }

                if (backgroundImage != null)
                {
                    mainPanel.BackgroundImage = backgroundImage;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                    System.Diagnostics.Debug.WriteLine("SetDefaultBackgroundOnStartup: Đã set background mặc định thành công");
                }

                System.Diagnostics.Debug.WriteLine("=== End SetDefaultBackgroundOnStartup ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetDefaultBackgroundOnStartup error: {ex.Message}");
            }
        }

        #endregion

        #region Color Management

        /// <summary>
        /// Cập nhật màu chữ theo thời gian
        /// </summary>
        public void UpdateTextColorByTime(Control parentControl)
        {
            try
            {
                if (parentControl == null) return;

                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;
                var textColor = isNight ? Color.White : Color.Black;

                // Cập nhật màu chữ cho tất cả controls
                UpdateControlTextColor(parentControl, textColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật màu chữ theo thời gian: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật màu chữ cho panel chi tiết
        /// </summary>
        public void UpdateDetailPanelTextColor(Panel detailPanel)
        {
            try
            {
                if (detailPanel == null) return;

                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;
                var textColor = isNight ? Color.White : Color.Black;

                // Cập nhật màu chữ cho tất cả labels trong panel
                foreach (Control control in detailPanel.Controls)
                {
                    if (control is Label label)
                    {
                        label.ForeColor = textColor;
                    }
                    else if (control is Panel panel)
                    {
                        UpdateDetailPanelTextColor(panel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật màu chữ panel chi tiết: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Cập nhật màu chữ cho control và tất cả controls con
        /// </summary>
        private void UpdateControlTextColor(Control control, Color textColor)
        {
            try
            {
                if (control is Label label)
                {
                    label.ForeColor = textColor;
                }
                else if (control is Button button)
                {
                    button.ForeColor = textColor;
                }
                else if (control is TextBox textBox)
                {
                    textBox.ForeColor = textColor;
                }

                // Đệ quy cho tất cả controls con
                foreach (Control childControl in control.Controls)
                {
                    UpdateControlTextColor(childControl, textColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cập nhật màu chữ control: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi tạo background picture box
        /// </summary>
        public void InitializeBackgroundPictureBox(Panel mainPanel)
        {
            try
            {
                if (mainPanel == null) return;

                // Tạo PictureBox cho background nếu chưa có
                if (backgroundPictureBox == null)
                {
                    backgroundPictureBox = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        BackColor = Color.Transparent
                    };
                }

                System.Diagnostics.Debug.WriteLine("Background PictureBox đã được khởi tạo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo Background PictureBox: {ex.Message}");
            }
        }

        /// <summary>
        /// Test background
        /// </summary>
        public void TestBackground(Panel mainPanel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TestBackground ===");
                
                if (mainPanel == null)
                {
                    System.Diagnostics.Debug.WriteLine("mainPanel is NULL trong TestBackground!");
                    return;
                }

                // Test với các loại thời tiết khác nhau
                var testWeathers = new[] { "Clear", "Clouds", "Rain", "Thunderstorm", "Snow" };
                
                foreach (var weather in testWeathers)
                {
                    System.Diagnostics.Debug.WriteLine($"Testing background for: {weather}");
                    SetBackground(weather, 800, mainPanel);
                    System.Threading.Thread.Sleep(1000); // Delay 1 giây
                }

                System.Diagnostics.Debug.WriteLine("=== End TestBackground ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestBackground error: {ex.Message}");
            }
        }

        #endregion
    }
}
