using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace THOITIET
{
    public class BackgroundService
    {
        private int? lastWeatherId = null;
        private bool? lastIsNight = null;

        public BackgroundService()
        {
        }

        public void SetBackground(string weatherMain = "Clear", int weatherId = 800, Panel mainPanel = null)
        {
            try
            {
                if (mainPanel == null) return;

                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;

                if (lastWeatherId == weatherId && lastIsNight == isNight)
                    return;

                lastWeatherId = weatherId;
                lastIsNight = isNight;

                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath)) return;

                Image? backgroundImage = null;

                switch (weatherMain.ToLower())
                {
                    case "clear":
                        if (isNight)
                        {
                            var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                            if (File.Exists(demPath))
                                backgroundImage = Image.FromFile(demPath);
                        }
                        else
                        {
                            var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                            if (File.Exists(quangPath))
                                backgroundImage = Image.FromFile(quangPath);
                        }
                        break;
                    default:
                        if (isNight)
                        {
                            var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                            if (File.Exists(demPath))
                                backgroundImage = Image.FromFile(demPath);
                        }
                        else
                        {
                            var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                            if (File.Exists(quangPath))
                                backgroundImage = Image.FromFile(quangPath);
                        }
                        break;
                }

                if (backgroundImage != null)
                {
                    mainPanel.BackgroundImage = backgroundImage;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi SetBackground: {ex.Message}");
            }
        }

        public void ForceSetBackgroundInLoad(Panel mainPanel)
        {
            try
            {
                if (mainPanel == null) return;

                var currentHour = DateTime.Now.Hour;
                var isNight = currentHour < 6 || currentHour >= 18;

                var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath)) return;

                Image? backgroundImage = null;

                if (isNight)
                {
                    var demPath = Path.Combine(resourcesPath, "nen_ban_dem.png");
                    if (File.Exists(demPath))
                        backgroundImage = Image.FromFile(demPath);
                }
                else
                {
                    var quangPath = Path.Combine(resourcesPath, "nen_troi_quang.jpg");
                    if (File.Exists(quangPath))
                        backgroundImage = Image.FromFile(quangPath);
                }

                if (backgroundImage != null)
                {
                    mainPanel.BackgroundImage = backgroundImage;
                    mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ForceSetBackgroundInLoad error: {ex.Message}");
            }
        }
    }
}