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
            this.oTimKiemDiaDiem = new System.Windows.Forms.TextBox();
            this.nhanTenDiaDiem = new System.Windows.Forms.Label();
            this.nhanNhietDoHienTai = new System.Windows.Forms.Label();
            this.nhanTrangThai = new System.Windows.Forms.Label();
            this.BangTheoGio = new System.Windows.Forms.FlowLayoutPanel();
            this.BangNhieuNgay = new System.Windows.Forms.FlowLayoutPanel();
            this.listBoxDiaDiemDaLuu = new System.Windows.Forms.ListBox();
            this.nutLuuDiaDiem = new System.Windows.Forms.Button();
            this.nutXoaDiaDiem = new System.Windows.Forms.Button();
            this.tabDieuKhien = new System.Windows.Forms.TabControl();
            this.tabChart = new System.Windows.Forms.TabPage();
            this.tabMap = new System.Windows.Forms.TabPage();
            this.nutTimKiem = new System.Windows.Forms.Button();
            this.temperatureChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.SuspendLayout();
            // 
            // oTimKiemDiaDiem
            // 
            this.oTimKiemDiaDiem.Location = new System.Drawing.Point(20, 20);
            this.oTimKiemDiaDiem.Name = "oTimKiemDiaDiem";
            this.oTimKiemDiaDiem.Size = new System.Drawing.Size(200, 23);
            this.oTimKiemDiaDiem.TabIndex = 0;
            // 
            // nhanTenDiaDiem
            // 
            this.nhanTenDiaDiem.AutoSize = true;
            this.nhanTenDiaDiem.Location = new System.Drawing.Point(20, 60);
            this.nhanTenDiaDiem.Name = "nhanTenDiaDiem";
            this.nhanTenDiaDiem.Size = new System.Drawing.Size(100, 15);
            this.nhanTenDiaDiem.TabIndex = 1;
            this.nhanTenDiaDiem.Text = "Chưa có dữ liệu";
            // 
            // nhanNhietDoHienTai
            // 
            this.nhanNhietDoHienTai.AutoSize = true;
            this.nhanNhietDoHienTai.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.nhanNhietDoHienTai.Location = new System.Drawing.Point(20, 80);
            this.nhanNhietDoHienTai.Name = "nhanNhietDoHienTai";
            this.nhanNhietDoHienTai.Size = new System.Drawing.Size(100, 45);
            this.nhanNhietDoHienTai.TabIndex = 2;
            this.nhanNhietDoHienTai.Text = "--°C";
            // 
            // nhanTrangThai
            // 
            this.nhanTrangThai.AutoSize = true;
            this.nhanTrangThai.Location = new System.Drawing.Point(20, 130);
            this.nhanTrangThai.Name = "nhanTrangThai";
            this.nhanTrangThai.Size = new System.Drawing.Size(100, 15);
            this.nhanTrangThai.TabIndex = 3;
            this.nhanTrangThai.Text = "Chưa có dữ liệu";
            // 
            // BangTheoGio
            // 
            this.BangTheoGio.Location = new System.Drawing.Point(20, 160);
            this.BangTheoGio.Name = "BangTheoGio";
            this.BangTheoGio.Size = new System.Drawing.Size(400, 100);
            this.BangTheoGio.TabIndex = 4;
            // 
            // BangNhieuNgay
            // 
            this.BangNhieuNgay.Location = new System.Drawing.Point(20, 280);
            this.BangNhieuNgay.Name = "BangNhieuNgay";
            this.BangNhieuNgay.Size = new System.Drawing.Size(400, 100);
            this.BangNhieuNgay.TabIndex = 5;
            // 
            // listBoxDiaDiemDaLuu
            // 
            this.listBoxDiaDiemDaLuu.Location = new System.Drawing.Point(450, 20);
            this.listBoxDiaDiemDaLuu.Name = "listBoxDiaDiemDaLuu";
            this.listBoxDiaDiemDaLuu.Size = new System.Drawing.Size(200, 150);
            this.listBoxDiaDiemDaLuu.TabIndex = 6;
            // 
            // nutLuuDiaDiem
            // 
            this.nutLuuDiaDiem.Location = new System.Drawing.Point(450, 180);
            this.nutLuuDiaDiem.Name = "nutLuuDiaDiem";
            this.nutLuuDiaDiem.Size = new System.Drawing.Size(80, 30);
            this.nutLuuDiaDiem.TabIndex = 7;
            this.nutLuuDiaDiem.Text = "Lưu";
            this.nutLuuDiaDiem.UseVisualStyleBackColor = true;
            this.nutLuuDiaDiem.Click += new System.EventHandler(this.NutLuuDiaDiem_Click);
            // 
            // nutXoaDiaDiem
            // 
            this.nutXoaDiaDiem.Location = new System.Drawing.Point(540, 180);
            this.nutXoaDiaDiem.Name = "nutXoaDiaDiem";
            this.nutXoaDiaDiem.Size = new System.Drawing.Size(80, 30);
            this.nutXoaDiaDiem.TabIndex = 8;
            this.nutXoaDiaDiem.Text = "Xóa";
            this.nutXoaDiaDiem.UseVisualStyleBackColor = true;
            this.nutXoaDiaDiem.Click += new System.EventHandler(this.NutXoaDiaDiem_Click);
            // 
            // tabDieuKhien
            // 
            this.tabDieuKhien.Controls.Add(this.tabChart);
            this.tabDieuKhien.Controls.Add(this.tabMap);
            this.tabDieuKhien.Location = new System.Drawing.Point(20, 400);
            this.tabDieuKhien.Name = "tabDieuKhien";
            this.tabDieuKhien.SelectedIndex = 0;
            this.tabDieuKhien.Size = new System.Drawing.Size(600, 300);
            this.tabDieuKhien.TabIndex = 9;
            this.tabDieuKhien.SelectedIndexChanged += new System.EventHandler(this.TabDieuKhien_SelectedIndexChanged);
            // 
            // tabChart
            // 
            this.tabChart.Controls.Add(this.temperatureChart);
            this.tabChart.Location = new System.Drawing.Point(4, 24);
            this.tabChart.Name = "tabChart";
            this.tabChart.Padding = new System.Windows.Forms.Padding(3);
            this.tabChart.Size = new System.Drawing.Size(592, 272);
            this.tabChart.TabIndex = 0;
            this.tabChart.Text = "Biểu đồ";
            this.tabChart.UseVisualStyleBackColor = true;
            // 
            // tabMap
            // 
            this.tabMap.Location = new System.Drawing.Point(4, 24);
            this.tabMap.Name = "tabMap";
            this.tabMap.Padding = new System.Windows.Forms.Padding(3);
            this.tabMap.Size = new System.Drawing.Size(592, 272);
            this.tabMap.TabIndex = 1;
            this.tabMap.Text = "Bản đồ";
            this.tabMap.UseVisualStyleBackColor = true;
            // 
            // nutTimKiem
            // 
            this.nutTimKiem.Location = new System.Drawing.Point(230, 20);
            this.nutTimKiem.Name = "nutTimKiem";
            this.nutTimKiem.Size = new System.Drawing.Size(80, 23);
            this.nutTimKiem.TabIndex = 10;
            this.nutTimKiem.Text = "Tìm kiếm";
            this.nutTimKiem.UseVisualStyleBackColor = true;
            this.nutTimKiem.Click += new System.EventHandler(this.NutTimKiem_Click);
            // 
            // temperatureChart
            // 
            this.temperatureChart.Location = new System.Drawing.Point(0, 0);
            this.temperatureChart.Name = "temperatureChart";
            this.temperatureChart.Size = new System.Drawing.Size(592, 272);
            this.temperatureChart.TabIndex = 0;
            this.temperatureChart.Text = "Biểu đồ nhiệt độ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.nutTimKiem);
            this.Controls.Add(this.tabDieuKhien);
            this.Controls.Add(this.nutXoaDiaDiem);
            this.Controls.Add(this.nutLuuDiaDiem);
            this.Controls.Add(this.listBoxDiaDiemDaLuu);
            this.Controls.Add(this.BangNhieuNgay);
            this.Controls.Add(this.BangTheoGio);
            this.Controls.Add(this.nhanTrangThai);
            this.Controls.Add(this.nhanNhietDoHienTai);
            this.Controls.Add(this.nhanTenDiaDiem);
            this.Controls.Add(this.oTimKiemDiaDiem);
            this.Name = "Form1";
            this.Text = "Ứng dụng thời tiết";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

    }
}