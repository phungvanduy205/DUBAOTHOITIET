#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script để sắp xếp lại Form1.cs và Form1.Designer.cs theo thứ tự đã định
"""

import re
import os

def reorganize_form1_cs():
    """Sắp xếp lại Form1.cs theo thứ tự đã định"""
    
    # Đọc file Form1.cs
    with open('Form1.cs', 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Tìm các method và sắp xếp theo thứ tự
    methods = {}
    
    # 1️⃣ Nhập địa điểm, tìm kiếm, lưu địa điểm, đổi °C/°F
    section1_methods = [
        'Form1_Load',
        'oTimKiemDiaDiem_KeyDown',
        'oTimKiemDiaDiem_KeyPress', 
        'NutTimKiem_Click',
        'nutLuuDiaDiem_Click',
        'nutChuyenDoiDiaDiem_Click',
        'nutXoaDiaDiem_Click',
        'listBoxDiaDiemDaLuu_SelectedIndexChanged',
        'LuuDiaDiem',
        'LuuDiaDiemSilent',
        'NapDiaDiemDaLuu',
        'SaveLocationsToFile',
        'LoadLocationsFromFile',
        'NormalizeName',
        'CoordinatesEqual'
    ]
    
    # 2️⃣ Gọi API & thông tin mô tả
    section2_methods = [
        'LoadInitialWeatherData',
        'LoadWeatherByIP',
        'LayDuLieuThoiTiet',
        'ParseKetQuaApi',
        'CapNhatThongTinChung',
        'CapNhatThoiTiet',
        'CapNhatThoiGian',
        'TaoNoiDungPanelChiTiet',
        'TaoFileIconThuc'
    ]
    
    # 3️⃣ Thời tiết 24h & 5 ngày
    section3_methods = [
        'HienThiDuBao24h',
        'HienThiDuBao5Ngay',
        'TaoPanelTheoGio',
        'TaoPanelNgay',
        'GetWeatherIconPath'
    ]
    
    # 4️⃣ Biểu đồ
    section4_methods = [
        'VeBieuDoNhietDo24h',
        'VeBieuDoNhietDo5Ngay',
        'InitializeChart',
        'CreateTemperatureChart'
    ]
    
    # 5️⃣ Bản đồ
    section5_methods = [
        'LoadBanDo',
        'CapNhatMarkerTheoViTri',
        'InitializeWebView2',
        'LoadWindyMap'
    ]
    
    # 6️⃣ Background thay đổi theo thời tiết
    section6_methods = [
        'CapNhatBackground',
        'CapNhatIconDong',
        'SetBackground',
        'SetDefaultBackgroundOnStartup',
        'TestBackground',
        'ForceSetBackgroundInLoad',
        'InitializeBackgroundPictureBox',
        'ApplyRoundedCorners',
        'ApDungStyleGlassmorphism',
        'CauHinhKhoiTao',
        'thanhTrenCung_Paint',
        'anhNenDong_Click'
    ]
    
    # Tìm tất cả methods trong file
    method_pattern = r'(private\s+(?:async\s+)?(?:void|Task|Task<[^>]+>)\s+(\w+)\s*\([^)]*\)\s*\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\})'
    matches = re.findall(method_pattern, content, re.MULTILINE | re.DOTALL)
    
    # Tạo dictionary mapping method name -> full method
    for full_method, method_name in matches:
        methods[method_name] = full_method
    
    # Tạo nội dung mới với thứ tự đã định
    new_content = []
    
    # Thêm phần đầu (using, namespace, class declaration, fields)
    header_pattern = r'(.*?)(?=private\s+(?:async\s+)?(?:void|Task|Task<[^>]+>)\s+\w+\s*\()'
    header_match = re.search(header_pattern, content, re.MULTILINE | re.DOTALL)
    if header_match:
        new_content.append(header_match.group(1).strip())
    
    # Thêm các section theo thứ tự
    sections = [
        ("1️⃣ NHẬP ĐỊA ĐIỂM, TÌM KIẾM, LƯU ĐỊA ĐIỂM, ĐỔI °C/°F", section1_methods),
        ("2️⃣ GỌI API & THÔNG TIN MÔ TẢ", section2_methods),
        ("3️⃣ THỜI TIẾT 24H & 5 NGÀY", section3_methods),
        ("4️⃣ BIỂU ĐỒ", section4_methods),
        ("5️⃣ BẢN ĐỒ", section5_methods),
        ("6️⃣ BACKGROUND THAY ĐỔI THEO THỜI TIẾT", section6_methods)
    ]
    
    for section_title, method_list in sections:
        new_content.append(f"\n        #region ===== {section_title} =====")
        
        for method_name in method_list:
            if method_name in methods:
                new_content.append(f"\n        {methods[method_name]}")
                del methods[method_name]  # Xóa để tránh duplicate
        
        new_content.append("        #endregion")
    
    # Thêm các method còn lại (không thuộc section nào)
    if methods:
        new_content.append("\n        #region ===== CÁC METHOD KHÁC =====")
        for method_name, method_content in methods.items():
            new_content.append(f"\n        {method_content}")
        new_content.append("        #endregion")
    
    # Thêm phần cuối (closing brace)
    new_content.append("\n    }\n}")
    
    # Ghi file mới
    with open('Form1_reorganized.cs', 'w', encoding='utf-8') as f:
        f.write('\n'.join(new_content))
    
    print("✅ Đã tạo Form1_reorganized.cs")

def reorganize_form1_designer():
    """Sắp xếp lại Form1.Designer.cs theo thứ tự hợp lý"""
    
    with open('Form1.Designer.cs', 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Tìm các control declarations
    control_pattern = r'(private\s+\w+\.\w+\s+\w+;)'
    controls = re.findall(control_pattern, content)
    
    # Sắp xếp controls theo nhóm chức năng
    search_controls = []
    main_controls = []
    detail_controls = []
    chart_controls = []
    map_controls = []
    background_controls = []
    
    for control in controls:
        control_name = control.split()[-1].rstrip(';')
        
        if any(x in control_name.lower() for x in ['timkiem', 'diaDiem', 'luu', 'chuyen', 'unit', 'toggle']):
            search_controls.append(control)
        elif any(x in control_name.lower() for x in ['tab', 'chart', 'map']):
            if 'chart' in control_name.lower():
                chart_controls.append(control)
            elif 'map' in control_name.lower():
                map_controls.append(control)
            else:
                main_controls.append(control)
        elif any(x in control_name.lower() for x in ['24', 'gio', '5', 'ngay', 'bang']):
            detail_controls.append(control)
        elif any(x in control_name.lower() for x in ['background', 'nen', 'anh']):
            background_controls.append(control)
        else:
            main_controls.append(control)
    
    # Tạo nội dung mới
    new_content = []
    
    # Header
    header_end = content.find('private System.Windows.Forms.Panel thanhTrenCung;')
    if header_end > 0:
        new_content.append(content[:header_end].strip())
    
    # Controls theo nhóm
    new_content.append("\n        #region ===== SEARCH & LOCATION CONTROLS =====")
    new_content.extend(search_controls)
    new_content.append("        #endregion")
    
    new_content.append("\n        #region ===== MAIN LAYOUT CONTROLS =====")
    new_content.extend(main_controls)
    new_content.append("        #endregion")
    
    new_content.append("\n        #region ===== DETAIL CONTROLS (24H & 5 DAYS) =====")
    new_content.extend(detail_controls)
    new_content.append("        #endregion")
    
    new_content.append("\n        #region ===== CHART CONTROLS =====")
    new_content.extend(chart_controls)
    new_content.append("        #endregion")
    
    new_content.append("\n        #region ===== MAP CONTROLS =====")
    new_content.extend(map_controls)
    new_content.append("        #endregion")
    
    new_content.append("\n        #region ===== BACKGROUND CONTROLS =====")
    new_content.extend(background_controls)
    new_content.append("        #endregion")
    
    # Footer
    new_content.append("\n    }\n}")
    
    # Ghi file mới
    with open('Form1.Designer_reorganized.cs', 'w', encoding='utf-8') as f:
        f.write('\n'.join(new_content))
    
    print("✅ Đã tạo Form1.Designer_reorganized.cs")

if __name__ == "__main__":
    print("🔄 Bắt đầu sắp xếp lại Form1.cs và Form1.Designer.cs...")
    
    try:
        reorganize_form1_cs()
        reorganize_form1_designer()
        print("\n✅ Hoàn thành! Các file đã được sắp xếp lại:")
        print("   - Form1_reorganized.cs")
        print("   - Form1.Designer_reorganized.cs")
        print("\n📝 Bạn có thể xem và so sánh với file gốc trước khi thay thế.")
        
    except Exception as e:
        print(f"❌ Lỗi: {e}")
