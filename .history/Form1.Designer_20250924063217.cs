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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            thanhTrenCung = new Panel();
            nutChuyenDoiDiaDiem = new Button();
            nutLuuDiaDiem = new Button();
            NutTimKiem = new Button();
            unitToggle = new THOITIET.Controls.UnitToggle();
            oTimKiemDiaDiem = new TextBox();
            CongTacDonVi = new CheckBox();
            listBoxGoiY = new ListBox();
            listBoxDiaDiemDaLuu = new ListBox();
            nutXoaDiaDiem = new Button();
            nutChonMacDinh = new Button();
            tabDieuKhien = new TabControl();
            tabChart = new TabPage();
            tabMap = new TabPage();
            NutXuatLichSu = new Button();
            BangLichSu = new DataGridView();
            khuVucDuoi_24Gio = new Panel();
            khung24Gio = new GroupBox();
            BangTheoGio = new FlowLayoutPanel();
            boCucChinh = new TableLayoutPanel();
            khuVucTrai_HienTai = new Panel();
            anhIconThoiTiet = new PictureBox();
            nhanTenDiaDiem = new Label();
            nhanThoiGian = new Label();
            nhanNhietDoHienTai = new Label();
            nhanTrangThai = new Label();
            detailGridPanel = new TableLayoutPanel();
            feelsLikePanel = new Panel();
            humidityPanel = new Panel();
            windPanel = new Panel();
            pressurePanel = new Panel();
            visibilityPanel = new Panel();
            anhNenDong = new PictureBox();
            khuVucPhai_5Ngay = new Panel();
            khung5Ngay = new GroupBox();
            BangNhieuNgay = new FlowLayoutPanel();
            thanhTrenCung.SuspendLayout();
            tabDieuKhien.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BangLichSu).BeginInit();
            khuVucDuoi_24Gio.SuspendLayout();
            khung24Gio.SuspendLayout();
            boCucChinh.SuspendLayout();
            khuVucTrai_HienTai.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)anhIconThoiTiet).BeginInit();
            detailGridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)anhNenDong).BeginInit();
            khuVucPhai_5Ngay.SuspendLayout();
            khung5Ngay.SuspendLayout();
            SuspendLayout();
            // 
            // thanhTrenCung
            // 
            thanhTrenCung.BackColor = Color.Transparent;
            thanhTrenCung.Controls.Add(nutChuyenDoiDiaDiem);
            thanhTrenCung.Controls.Add(nutLuuDiaDiem);
            thanhTrenCung.Controls.Add(NutTimKiem);
            thanhTrenCung.Controls.Add(unitToggle);
            thanhTrenCung.Controls.Add(oTimKiemDiaDiem);
            thanhTrenCung.Dock = DockStyle.Top;
            thanhTrenCung.Location = new Point(0, 0);
            thanhTrenCung.Name = "thanhTrenCung";
            thanhTrenCung.Padding = new Padding(10);
            thanhTrenCung.Size = new Size(1331, 57);
            thanhTrenCung.TabIndex = 0;
            thanhTrenCung.Paint += thanhTrenCung_Paint;
            // 
            // nutChuyenDoiDiaDiem
            // 
            nutChuyenDoiDiaDiem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nutChuyenDoiDiaDiem.BackColor = Color.WhiteSmoke;
            nutChuyenDoiDiaDiem.BackgroundImageLayout = ImageLayout.Stretch;
            nutChuyenDoiDiaDiem.FlatAppearance.BorderSize = 0;
            nutChuyenDoiDiaDiem.FlatStyle = FlatStyle.Flat;
            nutChuyenDoiDiaDiem.Font = new Font("Segoe UI", 12F);
            nutChuyenDoiDiaDiem.Location = new Point(1092, 13);
            nutChuyenDoiDiaDiem.Name = "nutChuyenDoiDiaDiem";
            nutChuyenDoiDiaDiem.Size = new Size(57, 34);
            nutChuyenDoiDiaDiem.TabIndex = 2;
            nutChuyenDoiDiaDiem.Text = "⇄";
            nutChuyenDoiDiaDiem.UseVisualStyleBackColor = false;
            nutChuyenDoiDiaDiem.Click += nutChuyenDoiDiaDiem_Click;
            // 
            // nutLuuDiaDiem
            // 
            nutLuuDiaDiem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nutLuuDiaDiem.BackColor = Color.WhiteSmoke;
            nutLuuDiaDiem.FlatAppearance.BorderSize = 0;
            nutLuuDiaDiem.FlatStyle = FlatStyle.Flat;
            nutLuuDiaDiem.Location = new Point(1155, 13);
            nutLuuDiaDiem.Name = "nutLuuDiaDiem";
            nutLuuDiaDiem.Size = new Size(54, 34);
            nutLuuDiaDiem.TabIndex = 3;
            nutLuuDiaDiem.Text = "Lưu";
            nutLuuDiaDiem.UseVisualStyleBackColor = false;
            nutLuuDiaDiem.Click += nutLuuDiaDiem_Click;
            // 
            // NutTimKiem
            // 
            NutTimKiem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NutTimKiem.BackColor = Color.White;
            NutTimKiem.FlatAppearance.BorderSize = 0;
            NutTimKiem.FlatStyle = FlatStyle.Flat;
            NutTimKiem.Font = new Font("Segoe UI", 12F);
            NutTimKiem.Location = new Point(1031, 13);
            NutTimKiem.Name = "NutTimKiem";
            NutTimKiem.Size = new Size(42, 34);
            NutTimKiem.TabIndex = 0;
            NutTimKiem.Text = "🔍";
            NutTimKiem.UseVisualStyleBackColor = false;
            NutTimKiem.Click += NutTimKiem_Click;
            // 
            // unitToggle
            // 
            unitToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            unitToggle.BackColor = Color.Transparent;
            unitToggle.Font = new Font("Segoe UI", 10F);
            unitToggle.IsCelsius = true;
            unitToggle.Location = new Point(1215, 13);
            unitToggle.Name = "unitToggle";
            unitToggle.Size = new Size(104, 34);
            unitToggle.TabIndex = 1;
            // 
            // oTimKiemDiaDiem
            // 
            oTimKiemDiaDiem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            oTimKiemDiaDiem.BorderStyle = BorderStyle.None;
            oTimKiemDiaDiem.Font = new Font("Segoe UI", 12F);
            oTimKiemDiaDiem.Location = new Point(19, 13);
            oTimKiemDiaDiem.Multiline = true;
            oTimKiemDiaDiem.Name = "oTimKiemDiaDiem";
            oTimKiemDiaDiem.PlaceholderText = "🔍Tìm kiếm địa điểm...";
            oTimKiemDiaDiem.Size = new Size(1067, 34);
            oTimKiemDiaDiem.TabIndex = 0;
            oTimKiemDiaDiem.KeyDown += oTimKiemDiaDiem_KeyDown;
            oTimKiemDiaDiem.KeyPress += oTimKiemDiaDiem_KeyPress;
            // 
            // CongTacDonVi
            // 
            CongTacDonVi.Location = new Point(0, 0);
            CongTacDonVi.Name = "CongTacDonVi";
            CongTacDonVi.Size = new Size(104, 24);
            CongTacDonVi.TabIndex = 0;
            CongTacDonVi.Visible = false;
            // 
            // listBoxGoiY
            // 
            listBoxGoiY.Location = new Point(0, 0);
            listBoxGoiY.Name = "listBoxGoiY";
            listBoxGoiY.Size = new Size(120, 96);
            listBoxGoiY.TabIndex = 0;
            // 
            // listBoxDiaDiemDaLuu
            // 
            listBoxDiaDiemDaLuu.Location = new Point(10, 10);
            listBoxDiaDiemDaLuu.Name = "listBoxDiaDiemDaLuu";
            listBoxDiaDiemDaLuu.Size = new Size(200, 144);
            listBoxDiaDiemDaLuu.TabIndex = 0;
            listBoxDiaDiemDaLuu.SelectedIndexChanged += listBoxDiaDiemDaLuu_SelectedIndexChanged;
            // 
            // nutXoaDiaDiem
            // 
            nutXoaDiaDiem.Location = new Point(220, 120);
            nutXoaDiaDiem.Name = "nutXoaDiaDiem";
            nutXoaDiaDiem.Size = new Size(80, 30);
            nutXoaDiaDiem.TabIndex = 1;
            nutXoaDiaDiem.Text = "Xóa";
            nutXoaDiaDiem.UseVisualStyleBackColor = true;
            nutXoaDiaDiem.Click += nutXoaDiaDiem_Click;
            // 
            // nutChonMacDinh
            // 
            nutChonMacDinh.Location = new Point(220, 80);
            nutChonMacDinh.Name = "nutChonMacDinh";
            nutChonMacDinh.Size = new Size(80, 30);
            nutChonMacDinh.TabIndex = 2;
            nutChonMacDinh.Text = "Mặc định";
            nutChonMacDinh.UseVisualStyleBackColor = true;
            // 
            // tabDieuKhien
            // 
            tabDieuKhien.Controls.Add(tabChart);
            tabDieuKhien.Controls.Add(tabMap);
            tabDieuKhien.Dock = DockStyle.Fill;
            tabDieuKhien.Name = "tabDieuKhien";
            tabDieuKhien.SelectedIndex = 0;
            tabDieuKhien.TabIndex = 3;
            // 
            // tabChart
            // 
            tabChart.Dock = DockStyle.Fill;
            tabChart.Location = new Point(4, 29);
            tabChart.Name = "tabChart";
            tabChart.Padding = new Padding(2);
            tabChart.TabIndex = 0;
            tabChart.Text = "Biểu đồ nhiệt độ";
            tabChart.UseVisualStyleBackColor = true;
            // 
            // tabMap
            // 
            tabMap.Dock = DockStyle.Fill;
            tabMap.Location = new Point(4, 29);
            tabMap.Name = "tabMap";
            tabMap.Padding = new Padding(2);
            tabMap.TabIndex = 1;
            tabMap.Text = "Bản đồ";
            tabMap.UseVisualStyleBackColor = true;
            // 
            // NutXuatLichSu
            // 
            NutXuatLichSu.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            NutXuatLichSu.Location = new Point(387, 212);
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
            BangLichSu.BackgroundColor = SystemColors.ButtonHighlight;
            BangLichSu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            BangLichSu.GridColor = SystemColors.ControlLightLight;
            BangLichSu.Location = new Point(11, 11);
            BangLichSu.Name = "BangLichSu";
            BangLichSu.ReadOnly = true;
            BangLichSu.RowHeadersVisible = false;
            BangLichSu.RowHeadersWidth = 51;
            BangLichSu.Size = new Size(500, 195);
            BangLichSu.TabIndex = 0;
            // 
            // khuVucDuoi_24Gio
            // 
            khuVucDuoi_24Gio.BackColor = Color.Transparent;
            khuVucDuoi_24Gio.Controls.Add(khung24Gio);
            khuVucDuoi_24Gio.Dock = DockStyle.Fill;
            khuVucDuoi_24Gio.Location = new Point(3, 401);
            khuVucDuoi_24Gio.Name = "khuVucDuoi_24Gio";
            khuVucDuoi_24Gio.Padding = new Padding(8);
            khuVucDuoi_24Gio.Size = new Size(726, 320);
            khuVucDuoi_24Gio.TabIndex = 2;
            // 
            // khung24Gio
            // 
            khung24Gio.Controls.Add(BangTheoGio);
            khung24Gio.Dock = DockStyle.Fill;
            khung24Gio.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            khung24Gio.ForeColor = SystemColors.ControlLightLight;
            khung24Gio.Location = new Point(8, 8);
            khung24Gio.Name = "khung24Gio";
            khung24Gio.Padding = new Padding(8);
            khung24Gio.Size = new Size(710, 304);
            khung24Gio.TabIndex = 0;
            khung24Gio.TabStop = false;
            khung24Gio.Text = "Dự báo 24 giờ";
            // 
            // BangTheoGio
            // 
            BangTheoGio.AutoScroll = true;
            BangTheoGio.BackColor = Color.FromArgb(50, 0, 0, 0);
            BangTheoGio.Dock = DockStyle.Fill;
            BangTheoGio.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            BangTheoGio.Location = new Point(8, 31);
            BangTheoGio.Name = "BangTheoGio";
            BangTheoGio.Padding = new Padding(4);
            BangTheoGio.Size = new Size(694, 265);
            BangTheoGio.TabIndex = 0;
            BangTheoGio.WrapContents = false;
            // 
            // boCucChinh
            // 
            boCucChinh.BackColor = Color.Transparent;
            boCucChinh.ColumnCount = 2;
            boCucChinh.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            boCucChinh.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            boCucChinh.Controls.Add(khuVucTrai_HienTai, 0, 0);
            boCucChinh.Controls.Add(khuVucPhai_5Ngay, 1, 0);
            boCucChinh.Controls.Add(khuVucDuoi_24Gio, 0, 1);
            boCucChinh.Controls.Add(tabDieuKhien, 1, 1);
            boCucChinh.Dock = DockStyle.Fill;
            boCucChinh.Location = new Point(0, 57);
            boCucChinh.Name = "boCucChinh";
            boCucChinh.RowCount = 2;
            boCucChinh.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            boCucChinh.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            boCucChinh.Size = new Size(1331, 724);
            boCucChinh.TabIndex = 1;
            // 
            // khuVucTrai_HienTai
            // 
            khuVucTrai_HienTai.BackColor = Color.Transparent;
            khuVucTrai_HienTai.Controls.Add(anhIconThoiTiet);
            khuVucTrai_HienTai.Controls.Add(nhanTenDiaDiem);
            khuVucTrai_HienTai.Controls.Add(nhanThoiGian);
            khuVucTrai_HienTai.Controls.Add(nhanNhietDoHienTai);
            khuVucTrai_HienTai.Controls.Add(nhanTrangThai);
            khuVucTrai_HienTai.Controls.Add(detailGridPanel);
            khuVucTrai_HienTai.Controls.Add(anhNenDong);
            khuVucTrai_HienTai.Dock = DockStyle.Fill;
            khuVucTrai_HienTai.Location = new Point(3, 3);
            khuVucTrai_HienTai.Name = "khuVucTrai_HienTai";
            khuVucTrai_HienTai.Size = new Size(726, 392);
            khuVucTrai_HienTai.TabIndex = 0;
            // 
            // anhIconThoiTiet
            // 
            anhIconThoiTiet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            anhIconThoiTiet.BackColor = Color.Transparent;
            anhIconThoiTiet.BackgroundImageLayout = ImageLayout.Stretch;
            anhIconThoiTiet.Location = new Point(173, 67);
            anhIconThoiTiet.Name = "anhIconThoiTiet";
            anhIconThoiTiet.Size = new Size(74, 67);
            anhIconThoiTiet.SizeMode = PictureBoxSizeMode.Zoom;
            anhIconThoiTiet.TabIndex = 4;
            anhIconThoiTiet.TabStop = false;
            // 
            // nhanTenDiaDiem
            // 
            nhanTenDiaDiem.AutoSize = true;
            nhanTenDiaDiem.BackColor = Color.Transparent;
            nhanTenDiaDiem.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            nhanTenDiaDiem.Location = new Point(0, 3);
            nhanTenDiaDiem.Name = "nhanTenDiaDiem";
            nhanTenDiaDiem.Size = new Size(153, 37);
            nhanTenDiaDiem.TabIndex = 0;
            nhanTenDiaDiem.Text = "Địa điểm...";
            // 
            // nhanThoiGian
            // 
            nhanThoiGian.AutoSize = true;
            nhanThoiGian.BackColor = Color.Transparent;
            nhanThoiGian.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            nhanThoiGian.ForeColor = Color.White;
            nhanThoiGian.Location = new Point(8, 39);
            nhanThoiGian.Name = "nhanThoiGian";
            nhanThoiGian.Size = new Size(117, 28);
            nhanThoiGian.TabIndex = 0;
            nhanThoiGian.Text = "Thời gian...";
            // 
            // nhanNhietDoHienTai
            // 
            nhanNhietDoHienTai.AutoSize = true;
            nhanNhietDoHienTai.BackColor = Color.Transparent;
            nhanNhietDoHienTai.Font = new Font("Segoe UI", 40F, FontStyle.Bold);
            nhanNhietDoHienTai.Location = new Point(8, 67);
            nhanNhietDoHienTai.Name = "nhanNhietDoHienTai";
            nhanNhietDoHienTai.Size = new Size(159, 89);
            nhanNhietDoHienTai.TabIndex = 1;
            nhanNhietDoHienTai.Text = "--°C";
            // 
            // nhanTrangThai
            // 
            nhanTrangThai.AutoSize = true;
            nhanTrangThai.BackColor = Color.Transparent;
            nhanTrangThai.Font = new Font("Segoe UI", 14F);
            nhanTrangThai.Location = new Point(8, 151);
            nhanTrangThai.Name = "nhanTrangThai";
            nhanTrangThai.Size = new Size(135, 32);
            nhanTrangThai.TabIndex = 2;
            nhanTrangThai.Text = "Trạng thái...";
            // 
            // detailGridPanel
            // 
            detailGridPanel.BackColor = Color.Transparent;
            detailGridPanel.ColumnCount = 3;
            detailGridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            detailGridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            detailGridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            detailGridPanel.Controls.Add(feelsLikePanel, 0, 0);
            detailGridPanel.Controls.Add(humidityPanel, 1, 0);
            detailGridPanel.Controls.Add(windPanel, 2, 0);
            detailGridPanel.Controls.Add(pressurePanel, 0, 1);
            detailGridPanel.Controls.Add(visibilityPanel, 1, 1);
            detailGridPanel.Dock = DockStyle.Bottom;
            detailGridPanel.Location = new Point(0, 196);
            detailGridPanel.Name = "detailGridPanel";
            detailGridPanel.Padding = new Padding(5);
            detailGridPanel.RowCount = 2;
            detailGridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            detailGridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            detailGridPanel.Size = new Size(726, 196);
            detailGridPanel.TabIndex = 3;
            // 
            // feelsLikePanel
            // 
            feelsLikePanel.BackColor = Color.FromArgb(120, 255, 255, 255);
            feelsLikePanel.Dock = DockStyle.Fill;
            feelsLikePanel.Location = new Point(8, 8);
            feelsLikePanel.Name = "feelsLikePanel";
            feelsLikePanel.Padding = new Padding(10);
            feelsLikePanel.Size = new Size(232, 87);
            feelsLikePanel.TabIndex = 0;
            // 
            // humidityPanel
            // 
            humidityPanel.BackColor = Color.FromArgb(120, 255, 255, 255);
            humidityPanel.Dock = DockStyle.Fill;
            humidityPanel.Location = new Point(246, 8);
            humidityPanel.Name = "humidityPanel";
            humidityPanel.Padding = new Padding(10);
            humidityPanel.Size = new Size(232, 87);
            humidityPanel.TabIndex = 1;
            // 
            // windPanel
            // 
            windPanel.BackColor = Color.FromArgb(120, 255, 255, 255);
            windPanel.Dock = DockStyle.Fill;
            windPanel.Location = new Point(484, 8);
            windPanel.Name = "windPanel";
            windPanel.Padding = new Padding(10);
            windPanel.Size = new Size(234, 87);
            windPanel.TabIndex = 2;
            // 
            // pressurePanel
            // 
            pressurePanel.BackColor = Color.FromArgb(120, 255, 255, 255);
            pressurePanel.Dock = DockStyle.Fill;
            pressurePanel.Location = new Point(8, 101);
            pressurePanel.Name = "pressurePanel";
            pressurePanel.Padding = new Padding(10);
            pressurePanel.Size = new Size(232, 87);
            pressurePanel.TabIndex = 3;
            // 
            // visibilityPanel
            // 
            visibilityPanel.BackColor = Color.FromArgb(120, 255, 255, 255);
            visibilityPanel.Dock = DockStyle.Fill;
            visibilityPanel.Location = new Point(246, 101);
            visibilityPanel.Name = "visibilityPanel";
            visibilityPanel.Padding = new Padding(10);
            visibilityPanel.Size = new Size(232, 87);
            visibilityPanel.TabIndex = 4;
            // 
            // anhNenDong
            // 
            anhNenDong.BackColor = Color.Transparent;
            anhNenDong.Dock = DockStyle.Fill;
            anhNenDong.ErrorImage = null;
            anhNenDong.Location = new Point(0, 0);
            anhNenDong.Name = "anhNenDong";
            anhNenDong.Size = new Size(726, 392);
            anhNenDong.SizeMode = PictureBoxSizeMode.StretchImage;
            anhNenDong.TabIndex = 5;
            anhNenDong.TabStop = false;
            anhNenDong.Click += anhNenDong_Click;
            // 
            // khuVucPhai_5Ngay
            // 
            khuVucPhai_5Ngay.BackColor = Color.Transparent;
            khuVucPhai_5Ngay.Controls.Add(khung5Ngay);
            khuVucPhai_5Ngay.Dock = DockStyle.Fill;
            khuVucPhai_5Ngay.Location = new Point(735, 3);
            khuVucPhai_5Ngay.Name = "khuVucPhai_5Ngay";
            khuVucPhai_5Ngay.Padding = new Padding(8);
            khuVucPhai_5Ngay.Size = new Size(593, 392);
            khuVucPhai_5Ngay.TabIndex = 1;
            // 
            // khung5Ngay
            // 
            khung5Ngay.Controls.Add(BangNhieuNgay);
            khung5Ngay.Dock = DockStyle.Fill;
            khung5Ngay.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            khung5Ngay.ForeColor = SystemColors.ControlLightLight;
            khung5Ngay.Location = new Point(8, 8);
            khung5Ngay.Name = "khung5Ngay";
            khung5Ngay.Padding = new Padding(8);
            khung5Ngay.Size = new Size(577, 376);
            khung5Ngay.TabIndex = 0;
            khung5Ngay.TabStop = false;
            khung5Ngay.Text = "Dự báo 5 ngày";
            // 
            // BangNhieuNgay
            // 
            BangNhieuNgay.AutoScroll = true;
            BangNhieuNgay.BackColor = Color.FromArgb(50, 0, 0, 0);
            BangNhieuNgay.Dock = DockStyle.Fill;
            BangNhieuNgay.FlowDirection = FlowDirection.TopDown;
            BangNhieuNgay.Location = new Point(8, 31);
            BangNhieuNgay.Name = "BangNhieuNgay";
            BangNhieuNgay.Padding = new Padding(4);
            BangNhieuNgay.Size = new Size(561, 337);
            BangNhieuNgay.TabIndex = 0;
            BangNhieuNgay.WrapContents = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(135, 206, 250);
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1331, 781);
            Controls.Add(boCucChinh);
            Controls.Add(thanhTrenCung);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(34, 0);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = " ";
            thanhTrenCung.ResumeLayout(false);
            thanhTrenCung.PerformLayout();
            tabDieuKhien.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)BangLichSu).EndInit();
            khuVucDuoi_24Gio.ResumeLayout(false);
            khung24Gio.ResumeLayout(false);
            boCucChinh.ResumeLayout(false);
            khuVucTrai_HienTai.ResumeLayout(false);
            khuVucTrai_HienTai.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)anhIconThoiTiet).EndInit();
            detailGridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)anhNenDong).EndInit();
            khuVucPhai_5Ngay.ResumeLayout(false);
            khung5Ngay.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel thanhTrenCung;
 		private System.Windows.Forms.CheckBox CongTacDonVi;
 		private System.Windows.Forms.Button NutTimKiem;
 		private System.Windows.Forms.TextBox oTimKiemDiaDiem;
        private THOITIET.Controls.UnitToggle unitToggle;
         private System.Windows.Forms.Button nutChuyenDoiDiaDiem;
         private System.Windows.Forms.ListBox listBoxGoiY;
        private System.Windows.Forms.ListBox listBoxDiaDiemDaLuu;
        private System.Windows.Forms.Button nutLuuDiaDiem;
        private System.Windows.Forms.Button nutXoaDiaDiem;
        private System.Windows.Forms.Button nutChonMacDinh;
        private TabControl tabDieuKhien;
        private TabPage tabChart;
        private TabPage tabMap;
         private Button NutXuatLichSu;
         private DataGridView BangLichSu;
         private Panel khuVucDuoi_24Gio;
         private GroupBox khung24Gio;
         private FlowLayoutPanel BangTheoGio;
         private TableLayoutPanel boCucChinh;
         private Panel khuVucPhai_5Ngay;
         private GroupBox khung5Ngay;
         private FlowLayoutPanel BangNhieuNgay;
         private Panel khuVucTrai_HienTai;
         private PictureBox anhIconThoiTiet;
        private Label nhanTenDiaDiem;
        private Label nhanThoiGian;
        private Label nhanNhietDoHienTai;
        private Label nhanTrangThai;
         private PictureBox anhNenDong;
         private TableLayoutPanel detailGridPanel;
         private Panel feelsLikePanel;
         private Panel humidityPanel;
         private Panel windPanel;
         private Panel pressurePanel;
         private Panel visibilityPanel;
    }
}