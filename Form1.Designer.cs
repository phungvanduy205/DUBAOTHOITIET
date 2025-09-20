namespace THOITIET
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            thanhTrenCung = new Panel();
            CongTacDonVi = new CheckBox();
            NutTimKiem = new Button();
            oTimKiemDiaDiem = new TextBox();
            boCucChinh = new TableLayoutPanel();
            khuVucTrai_HienTai = new Panel();
            anhIconThoiTiet = new PictureBox();
            nhanTenDiaDiem = new Label();
            nhanNhietDoHienTai = new Label();
            nhanTrangThai = new Label();
            nhanThongTinPhu = new Label();
            anhNenDong = new PictureBox();
            khuVucPhai_5Ngay = new Panel();
            khung5Ngay = new GroupBox();
            BangNhieuNgay = new FlowLayoutPanel();
            khuVucDuoi_24Gio = new Panel();
            khung24Gio = new GroupBox();
            BangTheoGio = new FlowLayoutPanel();
            tabDieuKhien = new TabControl();
            tabLichSu = new TabPage();
            NutXuatLichSu = new Button();
            BangLichSu = new DataGridView();
            thanhTrenCung.SuspendLayout();
            boCucChinh.SuspendLayout();
            khuVucTrai_HienTai.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)anhIconThoiTiet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)anhNenDong).BeginInit();
            khuVucPhai_5Ngay.SuspendLayout();
            khung5Ngay.SuspendLayout();
            khuVucDuoi_24Gio.SuspendLayout();
            khung24Gio.SuspendLayout();
            tabDieuKhien.SuspendLayout();
            tabLichSu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BangLichSu).BeginInit();
            SuspendLayout();
            // 
            // thanhTrenCung
            // 
            thanhTrenCung.Controls.Add(CongTacDonVi);
            thanhTrenCung.Controls.Add(NutTimKiem);
            thanhTrenCung.Controls.Add(oTimKiemDiaDiem);
            thanhTrenCung.Dock = DockStyle.Top;
            thanhTrenCung.Location = new Point(0, 0);
            thanhTrenCung.Name = "thanhTrenCung";
            thanhTrenCung.Padding = new Padding(10);
            thanhTrenCung.Size = new Size(1200, 52);
            thanhTrenCung.TabIndex = 0;
            // 
            // CongTacDonVi
            // 
            CongTacDonVi.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CongTacDonVi.AutoSize = true;
            CongTacDonVi.Location = new Point(1097, 16);
            CongTacDonVi.Name = "CongTacDonVi";
            CongTacDonVi.Size = new Size(93, 24);
            CongTacDonVi.TabIndex = 2;
            CongTacDonVi.Text = "Đổi °C/°F";
            CongTacDonVi.UseVisualStyleBackColor = true;
            CongTacDonVi.CheckedChanged += CongTacDonVi_CheckedChanged;
            // 
            // NutTimKiem
            // 
            NutTimKiem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NutTimKiem.Location = new Point(997, 12);
            NutTimKiem.Name = "NutTimKiem";
            NutTimKiem.Size = new Size(85, 29);
            NutTimKiem.TabIndex = 1;
            NutTimKiem.Text = "Tìm kiếm";
            NutTimKiem.UseVisualStyleBackColor = true;
            NutTimKiem.Click += NutTimKiem_Click;
            // 
            // oTimKiemDiaDiem
            // 
            oTimKiemDiaDiem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            oTimKiemDiaDiem.Location = new Point(13, 12);
            oTimKiemDiaDiem.Name = "oTimKiemDiaDiem";
            oTimKiemDiaDiem.PlaceholderText = "Nhập xã/phường, quận/huyện, tỉnh/thành...";
            oTimKiemDiaDiem.Size = new Size(978, 27);
            oTimKiemDiaDiem.TabIndex = 0;
            // 
            // boCucChinh
            // 
            boCucChinh.ColumnCount = 2;
            boCucChinh.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            boCucChinh.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            boCucChinh.Controls.Add(khuVucTrai_HienTai, 0, 0);
            boCucChinh.Controls.Add(khuVucPhai_5Ngay, 1, 0);
            boCucChinh.Controls.Add(khuVucDuoi_24Gio, 0, 1);
            boCucChinh.Controls.Add(tabDieuKhien, 1, 1);
            boCucChinh.Dock = DockStyle.Fill;
            boCucChinh.Location = new Point(0, 52);
            boCucChinh.Name = "boCucChinh";
            boCucChinh.RowCount = 2;
            boCucChinh.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            boCucChinh.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            boCucChinh.Size = new Size(1200, 648);
            boCucChinh.TabIndex = 1;
            // 
            // khuVucTrai_HienTai
            // 
            khuVucTrai_HienTai.Controls.Add(anhIconThoiTiet);
            khuVucTrai_HienTai.Controls.Add(nhanTenDiaDiem);
            khuVucTrai_HienTai.Controls.Add(nhanNhietDoHienTai);
            khuVucTrai_HienTai.Controls.Add(nhanTrangThai);
            khuVucTrai_HienTai.Controls.Add(nhanThongTinPhu);
            khuVucTrai_HienTai.Controls.Add(anhNenDong);
            khuVucTrai_HienTai.Dock = DockStyle.Fill;
            khuVucTrai_HienTai.Location = new Point(3, 3);
            khuVucTrai_HienTai.Name = "khuVucTrai_HienTai";
            khuVucTrai_HienTai.Size = new Size(714, 382);
            khuVucTrai_HienTai.TabIndex = 0;
            // 
            // anhIconThoiTiet
            // 
            anhIconThoiTiet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            anhIconThoiTiet.BackColor = Color.Transparent;
            anhIconThoiTiet.Location = new Point(597, 14);
            anhIconThoiTiet.Name = "anhIconThoiTiet";
            anhIconThoiTiet.Size = new Size(100, 100);
            anhIconThoiTiet.SizeMode = PictureBoxSizeMode.Zoom;
            anhIconThoiTiet.TabIndex = 4;
            anhIconThoiTiet.TabStop = false;
            // 
            // nhanTenDiaDiem
            // 
            nhanTenDiaDiem.AutoSize = true;
            nhanTenDiaDiem.BackColor = Color.Transparent;
            nhanTenDiaDiem.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            nhanTenDiaDiem.Location = new Point(16, 14);
            nhanTenDiaDiem.Name = "nhanTenDiaDiem";
            nhanTenDiaDiem.Size = new Size(222, 41);
            nhanTenDiaDiem.TabIndex = 0;
            nhanTenDiaDiem.Text = "Tên địa điểm...";
            // 
            // nhanNhietDoHienTai
            // 
            nhanNhietDoHienTai.AutoSize = true;
            nhanNhietDoHienTai.BackColor = Color.Transparent;
            nhanNhietDoHienTai.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            nhanNhietDoHienTai.Location = new Point(16, 60);
            nhanNhietDoHienTai.Name = "nhanNhietDoHienTai";
            nhanNhietDoHienTai.Size = new Size(143, 81);
            nhanNhietDoHienTai.TabIndex = 1;
            nhanNhietDoHienTai.Text = "--°C";
            // 
            // nhanTrangThai
            // 
            nhanTrangThai.AutoSize = true;
            nhanTrangThai.BackColor = Color.Transparent;
            nhanTrangThai.Font = new Font("Segoe UI", 16F);
            nhanTrangThai.Location = new Point(16, 141);
            nhanTrangThai.Name = "nhanTrangThai";
            nhanTrangThai.Size = new Size(153, 37);
            nhanTrangThai.TabIndex = 2;
            nhanTrangThai.Text = "Trạng thái...";
            // 
            // nhanThongTinPhu
            // 
            nhanThongTinPhu.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            nhanThongTinPhu.BackColor = Color.Transparent;
            nhanThongTinPhu.Font = new Font("Segoe UI", 14F);
            nhanThongTinPhu.Location = new Point(8, 197);
            nhanThongTinPhu.Name = "nhanThongTinPhu";
            nhanThongTinPhu.Size = new Size(681, 185);
            nhanThongTinPhu.TabIndex = 3;
            nhanThongTinPhu.Text = "Cảm giác thực tế, độ ẩm, gió, áp suất, tầm nhìn, mọc/lặn...";
            // 
            // anhNenDong
            // 
            anhNenDong.Dock = DockStyle.Fill;
            anhNenDong.Location = new Point(0, 0);
            anhNenDong.Name = "anhNenDong";
            anhNenDong.Size = new Size(714, 382);
            anhNenDong.SizeMode = PictureBoxSizeMode.StretchImage;
            anhNenDong.TabIndex = 5;
            anhNenDong.TabStop = false;
            // 
            // khuVucPhai_5Ngay
            // 
            khuVucPhai_5Ngay.Controls.Add(khung5Ngay);
            khuVucPhai_5Ngay.Dock = DockStyle.Fill;
            khuVucPhai_5Ngay.Location = new Point(723, 3);
            khuVucPhai_5Ngay.Name = "khuVucPhai_5Ngay";
            khuVucPhai_5Ngay.Padding = new Padding(8);
            khuVucPhai_5Ngay.Size = new Size(474, 382);
            khuVucPhai_5Ngay.TabIndex = 1;
            // 
            // khung5Ngay
            // 
            khung5Ngay.Controls.Add(BangNhieuNgay);
            khung5Ngay.Dock = DockStyle.Fill;
            khung5Ngay.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            khung5Ngay.Location = new Point(8, 8);
            khung5Ngay.Name = "khung5Ngay";
            khung5Ngay.Padding = new Padding(8);
            khung5Ngay.Size = new Size(458, 366);
            khung5Ngay.TabIndex = 0;
            khung5Ngay.TabStop = false;
            khung5Ngay.Text = "Dự báo 5 ngày";
            // 
            // BangNhieuNgay
            // 
            BangNhieuNgay.AutoScroll = true;
            BangNhieuNgay.Dock = DockStyle.Fill;
            BangNhieuNgay.FlowDirection = FlowDirection.TopDown;
            BangNhieuNgay.Location = new Point(8, 31);
            BangNhieuNgay.Name = "BangNhieuNgay";
            BangNhieuNgay.Padding = new Padding(4);
            BangNhieuNgay.Size = new Size(442, 327);
            BangNhieuNgay.TabIndex = 0;
            BangNhieuNgay.WrapContents = false;
            BangNhieuNgay.Paint += BangNhieuNgay_Paint;
            // 
            // khuVucDuoi_24Gio
            // 
            khuVucDuoi_24Gio.Controls.Add(khung24Gio);
            khuVucDuoi_24Gio.Dock = DockStyle.Fill;
            khuVucDuoi_24Gio.Location = new Point(3, 391);
            khuVucDuoi_24Gio.Name = "khuVucDuoi_24Gio";
            khuVucDuoi_24Gio.Padding = new Padding(8);
            khuVucDuoi_24Gio.Size = new Size(714, 254);
            khuVucDuoi_24Gio.TabIndex = 2;
            // 
            // khung24Gio
            // 
            khung24Gio.Controls.Add(BangTheoGio);
            khung24Gio.Dock = DockStyle.Fill;
            khung24Gio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            khung24Gio.Location = new Point(8, 8);
            khung24Gio.Name = "khung24Gio";
            khung24Gio.Padding = new Padding(8);
            khung24Gio.Size = new Size(698, 238);
            khung24Gio.TabIndex = 0;
            khung24Gio.TabStop = false;
            khung24Gio.Text = "Dự báo 24 giờ";
            // 
            // BangTheoGio
            // 
            BangTheoGio.AutoScroll = true;
            BangTheoGio.Dock = DockStyle.Fill;
            BangTheoGio.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            BangTheoGio.Location = new Point(8, 31);
            BangTheoGio.Name = "BangTheoGio";
            BangTheoGio.Padding = new Padding(4);
            BangTheoGio.Size = new Size(682, 199);
            BangTheoGio.TabIndex = 0;
            BangTheoGio.WrapContents = false;
            BangTheoGio.Paint += BangTheoGio_Paint;
            // 
            // tabDieuKhien
            // 
            tabDieuKhien.Controls.Add(tabLichSu);
            tabDieuKhien.Dock = DockStyle.Fill;
            tabDieuKhien.Location = new Point(723, 391);
            tabDieuKhien.Name = "tabDieuKhien";
            tabDieuKhien.SelectedIndex = 0;
            tabDieuKhien.Size = new Size(474, 254);
            tabDieuKhien.TabIndex = 3;
            // 
            // tabLichSu
            // 
            tabLichSu.Controls.Add(NutXuatLichSu);
            tabLichSu.Controls.Add(BangLichSu);
            tabLichSu.Location = new Point(4, 29);
            tabLichSu.Name = "tabLichSu";
            tabLichSu.Padding = new Padding(8);
            tabLichSu.Size = new Size(466, 221);
            tabLichSu.TabIndex = 0;
            tabLichSu.Text = "Lịch sử";
            tabLichSu.UseVisualStyleBackColor = true;
            // 
            // NutXuatLichSu
            // 
            NutXuatLichSu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            NutXuatLichSu.Location = new Point(334, 182);
            NutXuatLichSu.Name = "NutXuatLichSu";
            NutXuatLichSu.Size = new Size(124, 29);
            NutXuatLichSu.TabIndex = 1;
            NutXuatLichSu.Text = "Xuất lịch sử";
            NutXuatLichSu.UseVisualStyleBackColor = true;
            NutXuatLichSu.Click += NutXuatLichSu_Click;
            // 
            // BangLichSu
            // 
            BangLichSu.AllowUserToAddRows = false;
            BangLichSu.AllowUserToDeleteRows = false;
            BangLichSu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BangLichSu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            BangLichSu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            BangLichSu.Location = new Point(11, 11);
            BangLichSu.Name = "BangLichSu";
            BangLichSu.ReadOnly = true;
            BangLichSu.RowHeadersVisible = false;
            BangLichSu.RowHeadersWidth = 51;
            BangLichSu.Size = new Size(447, 165);
            BangLichSu.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 700);
            Controls.Add(boCucChinh);
            Controls.Add(thanhTrenCung);
            MinimumSize = new Size(1000, 600);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dự báo thời tiết";
            thanhTrenCung.ResumeLayout(false);
            thanhTrenCung.PerformLayout();
            boCucChinh.ResumeLayout(false);
            khuVucTrai_HienTai.ResumeLayout(false);
            khuVucTrai_HienTai.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)anhIconThoiTiet).EndInit();
            ((System.ComponentModel.ISupportInitialize)anhNenDong).EndInit();
            khuVucPhai_5Ngay.ResumeLayout(false);
            khung5Ngay.ResumeLayout(false);
            khuVucDuoi_24Gio.ResumeLayout(false);
            khung24Gio.ResumeLayout(false);
            tabDieuKhien.ResumeLayout(false);
            tabLichSu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)BangLichSu).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel thanhTrenCung;
		private System.Windows.Forms.CheckBox CongTacDonVi;
		private System.Windows.Forms.Button NutTimKiem;
		private System.Windows.Forms.TextBox oTimKiemDiaDiem;
		private System.Windows.Forms.TableLayoutPanel boCucChinh;
		private System.Windows.Forms.Panel khuVucTrai_HienTai;
		private System.Windows.Forms.PictureBox anhNenDong;
		private System.Windows.Forms.PictureBox anhIconThoiTiet;
		private System.Windows.Forms.Label nhanTenDiaDiem;
		private System.Windows.Forms.Label nhanNhietDoHienTai;
		private System.Windows.Forms.Label nhanTrangThai;
		private System.Windows.Forms.Label nhanThongTinPhu;
		private System.Windows.Forms.Panel khuVucPhai_5Ngay;
		private System.Windows.Forms.GroupBox khung5Ngay;
		private System.Windows.Forms.FlowLayoutPanel BangNhieuNgay;
		private System.Windows.Forms.Panel khuVucDuoi_24Gio;
		private System.Windows.Forms.GroupBox khung24Gio;
		private System.Windows.Forms.FlowLayoutPanel BangTheoGio;
		private System.Windows.Forms.TabControl tabDieuKhien;
		private System.Windows.Forms.TabPage tabLichSu;
		private System.Windows.Forms.Button NutXuatLichSu;
		private System.Windows.Forms.DataGridView BangLichSu;
    }
}
