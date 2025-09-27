#!/usr/bin/env python3
"""
Script để sắp xếp lại code Form1.cs theo thứ tự mong muốn
"""

import re
import os

def read_file_content(file_path):
    """Đọc nội dung file"""
    with open(file_path, 'r', encoding='utf-8') as f:
        return f.read()

def write_file_content(file_path, content):
    """Ghi nội dung file"""
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)

def extract_methods_by_keywords(content, keywords):
    """Trích xuất các method dựa trên keywords"""
    methods = []
    lines = content.split('\n')
    
    i = 0
    while i < len(lines):
        line = lines[i]
        
        # Tìm method declaration
        if re.match(r'^\s*(private|public|protected)\s+(async\s+)?(void|Task|string|int|bool|double|float|object|Image|Chart|WebView2|PictureBox|TabControl|List<.*>|OneCallResponse|GeocodingResponse|GeocodingResult|SavedLocation|FavoriteLocation)\s+\w+\s*\(', line):
            method_name = re.search(r'\s+(\w+)\s*\(', line)
            if method_name:
                method_name = method_name.group(1)
                
                # Kiểm tra xem method có chứa keyword nào không
                for keyword in keywords:
                    if keyword.lower() in method_name.lower():
                        # Tìm toàn bộ method
                        method_lines = [line]
                        brace_count = line.count('{') - line.count('}')
                        i += 1
                        
                        while i < len(lines) and brace_count > 0:
                            method_lines.append(lines[i])
                            brace_count += lines[i].count('{') - lines[i].count('}')
                            i += 1
                        
                        methods.append('\n'.join(method_lines))
                        break
                else:
                    i += 1
            else:
                i += 1
        else:
            i += 1
    
    return methods

def organize_form1():
    """Sắp xếp lại Form1.cs"""
    
    # Đọc file gốc
    content = read_file_content('Form1.cs')
    
    # Định nghĩa các nhóm method
    groups = {
        "1️⃣ NHẬP ĐỊA ĐIỂM, TÌM KIẾM, LƯU ĐỊA ĐIỂM, ĐỔI °C/°F": [
            "txtDiaDiem_KeyDown", "btnTimKiem_Click", "btnLuu_Click", "btnChuyenDoiDonVi_Click",
            "nutLuuDiaDiem_Click", "NutTimKiem_Click", "CongTacDonVi_Click", "oTimKiemDiaDiem_KeyDown",
            "oTimKiemDiaDiem_KeyPress", "TimKiemDiaDiem", "LuuDiaDiem", "LuuDiaDiemSilent",
            "nutChuyenDoiDiaDiem_Click", "nutXoaDiaDiem_Click", "CapNhatUIKhiChuyenDoiDonVi",
            "ConvertCelsiusToFahrenheit", "ConvertFahrenheitToCelsius", "ConvertTemperatureFromText",
            "GetTemperatureInUnit", "TryConvertSimpleTemperatureLabel", "ConvertDailyHighLowLabel"
        ],
        "2️⃣ GỌI API & THÔNG TIN MÔ TẢ": [
            "LayDuLieuThoiTiet", "ParseKetQuaApi", "CapNhatThongTinChung", "LoadWeatherByIP",
            "LoadWeatherForDefaultLocation", "LoadInitialWeatherData", "CapNhatThoiTiet",
            "CapNhatPanelChiTiet", "CapNhatPanelChiTietFromApi", "CapNhatPanelChiTietFromHourlyApi",
            "CapNhatPanelChiTietFromDailyApi", "HienThiThongTin", "CapNhatDiaDiem", "CapNhatThoiGian"
        ],
        "3️⃣ THỜI TIẾT 24H & 5 NGÀY": [
            "HienThiDuBao24h", "HienThiDuBao5Ngay", "LoadDuBao24h", "LoadForecast5Days",
            "DoDuLieuBangTheoGio", "DoDuLieuBangNhieuNgay", "TaoDuLieuMau24Gio", "TaoDuLieuMau5Ngay",
            "TaoPanelDuBaoNgayMoi", "OnDayCardClicked", "HighlightSelectedDayCard", "Show24hChartForDay",
            "TaoCardGio", "TaoCardNgay", "TaoPanelDuBaoGio", "TaoPanelDuBaoNgay"
        ],
        "4️⃣ BIỂU ĐỒ": [
            "VeBieuDoNhietDo24h", "VeBieuDoNhietDo5Ngay", "ShowChart", "ExportChart",
            "InitializeTemperatureChart", "CreateDailyComparisonChart", "CreateRainProbabilityChart",
            "CreateRainfallSummary"
        ],
        "5️⃣ BẢN ĐỒ": [
            "LoadBanDo", "CapNhatMarkerTheoViTri", "ShowMap", "LoadWindyMap", "EnsureWindyBrowser",
            "TabDieuKhien_SelectedIndexChanged"
        ],
        "6️⃣ BACKGROUND THAY ĐỔI THEO THỜI TIẾT": [
            "CapNhatBackground", "CapNhatIconDong", "SetBackground", "ForceSetBackgroundInLoad",
            "TestBackground", "InitializeBackgroundPictureBox", "SetDefaultBackgroundOnStartup",
            "CapNhatMauChuTheoThoiGian", "CapNhatMauChuPanelChiTiet", "HienThiIconVaNen",
            "TaoNenToanCuc", "ChonTenNenTheoIconCode", "TaoBackgroundTest", "TaoFileIconThuc"
        ]
    }
    
    # Tạo file mới với cấu trúc được sắp xếp
    organized_content = """using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Web.WebView2.WinForms;

namespace THOITIET
{
    /// <summary>
    /// Form chính: xử lý sự kiện, gọi dịch vụ, cập nhật giao diện
    /// Cấu trúc được sắp xếp theo thứ tự:
    /// 1️⃣ Nhập địa điểm, tìm kiếm, lưu địa điểm, đổi °C/°F
    /// 2️⃣ Gọi API & thông tin mô tả
    /// 3️⃣ Thời tiết 24h & 5 ngày
    /// 4️⃣ Biểu đồ
    /// 5️⃣ Bản đồ
    /// 6️⃣ Background thay đổi theo thời tiết
    /// </summary>
    public partial class Form1 : Form
    {
        #region ===== FIELDS & PROPERTIES =====
        
        // Cờ đơn vị: true = °C (metric), false = °F (imperial)
        private bool donViCelsius = true;

        // Dữ liệu thời tiết từ API
        private OneCallResponse weatherData;
        private string currentLocation = "";
        private double currentLat = 0;
        private double currentLon = 0;

        // File lưu địa điểm
        private readonly string locationsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_locations.txt");

        // Danh sách địa điểm đã lưu (giữ để binding UI, nguồn lấy từ file txt)
        private List<SavedLocation> savedLocations = new List<SavedLocation>();

        // Timer tự động cập nhật mỗi 1 giờ
        private readonly System.Windows.Forms.Timer dongHoCapNhat = new System.Windows.Forms.Timer();

        // Dịch vụ gọi API
        private readonly DichVuThoiTiet dichVu = new DichVuThoiTiet();

        // Các fields mới cho tính năng nâng cao
        private PictureBox? backgroundPictureBox;
        private Chart? temperatureChart;
        private WebView2? windyView;
        private TabControl? tabChartMap;
        private const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
        private List<FavoriteLocation> favoriteLocations = new List<FavoriteLocation>();
        private int selectedDayIndex = 0; // Ngày được chọn trong dự báo 5 ngày

        // Throttle nền: lưu trạng thái lần trước
        private int? lastWeatherId = null;
        private bool? lastIsNight = null;

        // Lưu địa điểm
        private int currentLocationIndex = 0;
        
        #endregion

        #region ===== CONSTRUCTOR & INITIALIZATION =====
        
        public Form1()
        {
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR START ===");
            InitializeComponent();
            CauHinhKhoiTao();
            ApDungStyleGlassmorphism();

            // Không tạo segmented runtime nữa (đã có UnitToggle trong Designer)
            
            // Đồng bộ hóa donViCelsius với unitToggle.IsCelsius
            donViCelsius = unitToggle.IsCelsius;
            
            // Đăng ký event UnitChanged để cập nhật hiển thị từ dữ liệu Kelvin
            unitToggle.UnitChanged += async (sender, isCelsius) => {
                donViCelsius = isCelsius;
                System.Diagnostics.Debug.WriteLine($"UnitToggle changed to: {(isCelsius ? "Celsius" : "Fahrenheit")}");
                if (weatherData != null)
                    await CapNhatThoiTiet();
            };
            
            // Bo tròn thanh tìm kiếm
            this.Load += (s, e) => {
                ApplyRoundedCorners(oTimKiemDiaDiem, 10);
                ApplyRoundedCorners(khung24Gio, 15);
                ApplyRoundedCorners(khung5Ngay, 15);
            };
            
            // Khởi tạo file lưu địa điểm nếu chưa có
            try
            {
                if (!File.Exists(locationsFilePath))
                {
                    File.WriteAllText(locationsFilePath, "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create locations file error: {ex.Message}");
            }

            NapDiaDiemDaLuu();

            // Xóa panel gợi ý cũ nếu có
            var oldSuggestionPanel = Controls.Find("suggestionPanel", true).FirstOrDefault();
            if (oldSuggestionPanel != null)
            {
                Controls.Remove(oldSuggestionPanel);
                oldSuggestionPanel.Dispose();
            }

            // Tạo background động
            InitializeBackgroundPictureBox();
            
            // Set background mặc định ngay khi khởi động dựa trên thời gian hiện tại
            System.Diagnostics.Debug.WriteLine("Calling SetDefaultBackgroundOnStartup...");
            SetDefaultBackgroundOnStartup();

            // Tạo nội dung cho các panel chi tiết
            TaoNoiDungPanelChiTiet();

            // Tải dữ liệu thời tiết ban đầu từ địa điểm hiện tại
            _ = LoadInitialWeatherData();

            // Tạo file icon thật
            TaoFileIconThuc();

            // Không đặt địa điểm mặc định - để trống cho đến khi API load

            // Xóa gợi ý tìm kiếm
            System.Diagnostics.Debug.WriteLine("=== FORM1 CONSTRUCTOR END ===");
        }
        
        #endregion
"""
    
    # Thêm các nhóm method
    for group_name, keywords in groups.items():
        organized_content += f"\n        #region ===== {group_name} =====\n        \n"
        
        # Trích xuất methods cho nhóm này
        methods = extract_methods_by_keywords(content, keywords)
        
        if methods:
            for method in methods:
                organized_content += f"        {method}\n        \n"
        else:
            organized_content += "        // TODO: Thêm các method liên quan đến nhóm này\n        \n"
        
        organized_content += "        #endregion\n"
    
    # Thêm phần cuối
    organized_content += """
        #region ===== HELPER METHODS =====
        
        // TODO: Thêm các helper methods chung ở đây
        
        #endregion

    }

    /// <summary>
    /// Class lưu trữ thông tin địa điểm đã lưu
    /// </summary>
    public class SavedLocation
    {
        public string Name { get; set; } = "";
        public double Lat { get; set; }
        public double Lon { get; set; }
        
        public SavedLocation(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }

    // Class để quản lý địa điểm yêu thích
    public class FavoriteLocation
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}"""
    
    # Ghi file mới
    write_file_content('Form1_organized_final.cs', organized_content)
    print("✅ Đã tạo file Form1_organized_final.cs với cấu trúc được sắp xếp!")

if __name__ == "__main__":
    organize_form1()
