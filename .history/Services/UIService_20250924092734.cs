using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace THOITIET.Services
{
    /// <summary>
    /// Service xử lý các chức năng UI
    /// </summary>
    public class UIService
    {
        /// <summary>
        /// Tạo panel chi tiết với icon, tiêu đề và giá trị
        /// </summary>
        public static Panel CreateDetailPanel(Panel parent, string icon, string title, string value, Point location, Size size)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.None
            };

            // Tạo bo tròn góc
            ApplyRoundedCorners(panel, 15);

            // Label icon
            var iconLabel = new Label
            {
                Text = icon,
                Location = new Point(15, 15),
                Size = new Size(30, 30),
                Font = new Font("Segoe UI Emoji", 16F),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Label tiêu đề
            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(55, 10),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(100, 100, 100),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                MaximumSize = new Size(80, 20)
            };

            // Label giá trị
            var valueLabel = new Label
            {
                Text = value,
                Location = new Point(55, 30),
                Size = new Size(80, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                MaximumSize = new Size(80, 25)
            };

            panel.Controls.Add(iconLabel);
            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);

            return panel;
        }

        /// <summary>
        /// Áp dụng bo tròn góc cho control
        /// </summary>
        public static void ApplyRoundedCorners(Control control, int radius)
        {
            control.Paint += (sender, e) =>
            {
                var rect = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
                var path = new GraphicsPath();
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();

                control.Region = new Region(path);
            };
        }

        /// <summary>
        /// Tạo context menu cho địa điểm
        /// </summary>
        public static ContextMenuStrip CreateLocationContextMenu(string[] locations, Action<string> onLocationSelected, Action<string> onLocationRemoved)
        {
            var contextMenu = new ContextMenuStrip();

            foreach (var location in locations)
            {
                // Tạo panel con chứa tên địa điểm và 2 nút
                var innerPanel = new Panel
                {
                    Width = 200,
                    Height = 30
                };

                // Label tên địa điểm (click để chọn)
                var locationLabel = new Label
                {
                    Text = location,
                    Location = new Point(5, 5),
                    Size = new Size(120, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    BackColor = Color.Transparent
                };

                locationLabel.Click += (s, args) => {
                    onLocationSelected?.Invoke(location);
                    contextMenu.Close();
                };

                // Nút xóa
                var removeButton = new Button
                {
                    Text = "×",
                    Location = new Point(130, 3),
                    Size = new Size(25, 24),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.Red,
                    BackColor = Color.FromArgb(255, 240, 240),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };

                removeButton.FlatAppearance.BorderSize = 0;
                removeButton.Click += (s, args) => {
                    onLocationRemoved?.Invoke(location);
                    contextMenu.Close();
                };

                innerPanel.Controls.Add(locationLabel);
                innerPanel.Controls.Add(removeButton);

                var toolStripItem = new ToolStripControlHost(innerPanel);
                contextMenu.Items.Add(toolStripItem);
            }

            return contextMenu;
        }

        /// <summary>
        /// Hiển thị thông báo lỗi
        /// </summary>
        public static void ShowError(string message, string title = "Lỗi")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Hiển thị thông báo thành công
        /// </summary>
        public static void ShowSuccess(string message, string title = "Thành công")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Hiển thị thông báo cảnh báo
        /// </summary>
        public static void ShowWarning(string message, string title = "Cảnh báo")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Hiển thị dialog xác nhận
        /// </summary>
        public static bool ShowConfirm(string message, string title = "Xác nhận")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Cập nhật trạng thái loading cho control
        /// </summary>
        public static void SetLoadingState(Control control, bool isLoading)
        {
            if (isLoading)
            {
                control.Enabled = false;
                control.Cursor = Cursors.WaitCursor;
            }
            else
            {
                control.Enabled = true;
                control.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Tạo tooltip cho control
        /// </summary>
        public static void SetTooltip(Control control, string text)
        {
            var tooltip = new ToolTip();
            tooltip.SetToolTip(control, text);
        }
    }
}