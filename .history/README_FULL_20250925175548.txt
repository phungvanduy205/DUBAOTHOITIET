THOITIET - HƯỚNG DẪN ĐẦY ĐỦ (PLAIN TEXT)
=========================================

Mục đích tài liệu
-----------------
Tài liệu này hướng dẫn chi tiết từ A→Z để chạy, sửa, và phân công công việc cho nhóm 3 người đối với ứng dụng Windows Forms "THOITIET". Định dạng .txt để bạn có thể mở bằng bất kỳ trình soạn thảo nào.

Nội dung chính
---------------
1) Yêu cầu hệ thống
2) Cài đặt API key (2 cách)
3) Chạy chương trình (CLI và Visual Studio)
4) Chức năng chính của ứng dụng
5) Nguyên tắc hiển thị mô tả (mapping 1-1) và gợi ý theo trạng thái
6) Phân công công việc cho 3 người + file chạm vào
7) Điểm tích hợp giữa các thành viên
8) Checklist trước khi giao nộp
9) Sửa ở đâu nếu muốn thay đổi X
10) Khắc phục sự cố phổ biến
11) Quy trình Git để làm nhóm
12) Phụ lục: Các mô tả phổ biến và dịch tiếng Việt

---------------------------------------------
1) YÊU CẦU HỆ THỐNG
---------------------------------------------
- Windows 10/11
- .NET SDK 8.0 trở lên (gõ: dotnet --info để kiểm tra)
- Internet hoạt động

---------------------------------------------
2) CÀI ĐẶT API KEY (2 CÁCH)
---------------------------------------------
Ứng dụng sử dụng OpenWeather APIs. Bạn cần API key hợp lệ: https://openweathermap.org/api

Option A - Biến môi trường (khuyến nghị):
- OPENWEATHER_API_KEY   (bắt buộc)
- GEOAPIFY_API_KEY      (tùy chọn nếu dùng geocoding Geoapify)
- GOOGLE_GEOCODING_API_KEY (tùy chọn nếu dùng geocoding Google)

Cách đặt nhanh trên Windows PowerShell (thay YOUR_KEY):
  setx OPENWEATHER_API_KEY "YOUR_KEY"
  setx GEOAPIFY_API_KEY ""
  setx GOOGLE_GEOCODING_API_KEY ""

Option B - File api_config.txt (đặt cùng thư mục .sln hoặc thư mục chạy):
  OPENWEATHER_API_KEY=YOUR_KEY
  GOOGLE_GEOCODING_API_KEY=
  GEOAPIFY_API_KEY=

Lưu ý quan trọng:
- Tất cả URL gọi OpenWeather đều dùng lang=en để nhận mô tả tiếng Anh gốc.
- Sau đó app map 1-1 sang tiếng Việt. Như vậy sẽ không có mô tả "mây đen u ám" (không tồn tại trong API gốc).

---------------------------------------------
3) CHẠY CHƯƠNG TRÌNH
---------------------------------------------
Bằng dòng lệnh:
  cd THOITIET
  dotnet build THOITIET.sln
  dotnet run --project THOITIET.csproj

Bằng Visual Studio:
  1) Mở THOITIET.sln
  2) Chọn Debug | Any CPU
  3) F5 để chạy, Ctrl+F5 để chạy không debug

Lưu ý: Nếu đang chạy ứng dụng, hãy đóng app trước khi build lại để tránh lỗi "file đang được sử dụng".

---------------------------------------------
4) CÁC CHỨC NĂNG CHÍNH
---------------------------------------------
- Tìm kiếm địa điểm (tên tỉnh/thành, xã/phường) → hiển thị thời tiết hiện tại
- Dự báo 24 giờ: danh sách các ô giờ, có nhiệt độ, icon, mô tả rút gọn
- Dự báo 5 ngày: mỗi ngày có nhiệt cao/thấp, mô tả, ghi chú mưa/gió
- Icon:
  * Icon chính ~180x180
  * Icon trong bảng 24h/5 ngày ~40x40
  * Không dùng icon trong biểu đồ
- Nền động thay đổi theo trạng thái (thunderstorm, rain, snow, clouds, clear...) và theo ban đêm/ban ngày
- Nhãn 'nhanTrangThai': dòng 1 = mô tả tiếng Việt chuẩn; dòng 2 = 2 gợi ý theo trạng thái

---------------------------------------------
5) NGUYÊN TẮC MÔ TẢ & GỢI Ý
---------------------------------------------
- API luôn gọi với lang=en để nhận description tiếng Anh gốc (ví dụ: "overcast clouds").
- Hàm GetVietnameseWeatherDescription (trong Form1.cs) map 1-1 sang tiếng Việt:
    overcast clouds → Nhiều mây
    heavy rain      → Mưa to
    very heavy rain → Mưa rất to
  ... Tuyệt đối KHÔNG dùng các diễn giải tuỳ ý như "mây đen u ám".
- Hàm GetWeatherSuggestions(string weatherDesc) chỉ dựa trên trạng thái (chuỗi mô tả tiếng Anh gốc). Không dựa vào nhiệt độ/ độ ẩm/ gió/ lượng mưa.

---------------------------------------------
6) PHÂN CÔNG CÔNG VIỆC CHO 3 NGƯỜI
---------------------------------------------
Người A – Data/API (backend trong app WinForms)
- Files chính: DichVuThoiTiet.cs, WeatherApiClasses.cs, saved_locations.json
- Việc cần làm:
  * Gọi API OpenWeather: thời tiết hiện tại, dự báo 24h (One Call), 5 ngày
  * Đảm bảo tất cả URL sử dụng lang=en
  * Xử lý lỗi mạng, timeout, log URL/response
  * Đồng bộ đơn vị (metric/imperial), trả về icon code, description gốc (EN)
- Kiểm tra:
  * Debug.WriteLine hiện description tiếng Anh (ví dụ "overcast clouds")
  * Không còn bất kỳ URL nào có lang=vi

Người B – UI/Presentation
- Files chính: Form1.Designer.cs, Form1.cs (các hàm UI)
- Việc cần làm:
  * Bố cục `detailGridPanel` 2 cột × 3 hàng (5 panel + 1 ô trống)
  * Tạo panel chi tiết: chữ tiêu đề/ giá trị không bị cắt chữ
  * Icon chính 180px; icon 24h/5 ngày 40px; biểu đồ không icon
  * Load danh sách 24h/5 ngày; click 1 ô giờ thì cập nhật khu vực chính
  * Nền động theo weatherMain/weatherId, cơ chế ban đêm
- Kiểm tra:
  * Chữ "Áp suất khí quyển", "Tầm nhìn xa", "Cảm giác như" luôn hiển thị đủ
  * Icon đúng kích thước, biểu đồ không có icon

Người C – Localization & Suggestions/Docs
- Files chính: Form1.cs (2 hàm: GetVietnameseWeatherDescription, GetWeatherSuggestions), README.md/README_FULL.txt
- Việc cần làm:
  * Duy trì đầy đủ danh sách mapping 1-1 (~50+ mô tả). Tuyệt đối không đổi nghĩa
  * Giữ gợi ý theo trạng thái (chỉ dựa vào weatherDesc)
  * Đảm bảo tất cả nơi hiển thị mô tả đều đi qua mapping
  * Cập nhật tài liệu hướng dẫn sử dụng, troubleshooting
- Kiểm tra:
  * Khi API trả "overcast clouds" thì UI hiện "Nhiều mây"; không bao giờ hiện "mây đen u ám"
  * Gợi ý hợp lý theo trạng thái (mưa, sương mù, tuyết, ...), không dùng "trời đẹp" khi đang mưa

---------------------------------------------
7) ĐIỂM TÍCH HỢP GIỮA CÁC THÀNH VIÊN
---------------------------------------------
- A trả về object có `weather.description` = EN; B/C dựa vào đó để render/mapping/gợi ý.
- C cập nhật mapping & suggestions; B gọi đúng hàm khi set label.
- Tránh ghi đè: dùng các nhánh Git riêng và PR.

---------------------------------------------
8) CHECKLIST TRƯỚC KHI GIAO NỘP
---------------------------------------------
[ ] Mọi URL OpenWeather có lang=en (DichVuThoiTiet.cs, WeatherApiClasses.cs)
[ ] Những chỗ set text đều gọi GetVietnameseWeatherDescription(...)
[ ] `nhanTrangThai` = mô tả VN + dòng gợi ý (chỉ theo trạng thái)
[ ] Icon: chính 180px; 24h/5 ngày 40px; biểu đồ không icon
[ ] Chữ không bị cắt; bố cục 2×3 đúng 5 panel
[ ] Build thành công; đóng app trước khi build lại

---------------------------------------------
9) SỬA Ở ĐÂU NẾU MUỐN THAY ĐỔI X
---------------------------------------------
- Đổi mô tả tiếng Việt: Form1.cs → GetVietnameseWeatherDescription
- Đổi logic gợi ý: Form1.cs → GetWeatherSuggestions
- Đổi icon: Form1.cs → GetWeatherIcon, GetWeatherIconFromEmoji*, thư mục Resources
- Đổi nền: Form1.cs → SetBackground
- Đổi bố cục: Form1.Designer.cs (detailGridPanel, kích cỡ control)
- Đổi ngôn ngữ API: DichVuThoiTiet.cs/WeatherApiClasses.cs (tham số lang=...)

---------------------------------------------
10) KHẮC PHỤC SỰ CỐ PHỔ BIẾN
---------------------------------------------
- Vẫn thấy "mây đen u ám":
  * Build lại (đóng app đang chạy trước khi build)
  * Kiểm tra tất cả URL đã dùng lang=en
  * Kiểm tra mapping có: {"overcast clouds", "Nhiều mây"}

- Lỗi "Could not copy ... exe is being used":
  * Đóng ứng dụng đang chạy, đảm bảo thư mục bin/obj không bị khoá

- Không hiển thị gợi ý:
  * Ứng dụng đã bỏ banner cuộn, gợi ý hiển thị ngay dưới `nhanTrangThai`

- Cảnh báo nullability khi build:
  * Không ảnh hưởng chạy; có thể tối ưu khi có thời gian

---------------------------------------------
11) QUY TRÌNH GIT LÀM NHÓM
---------------------------------------------
- Nhánh:
  * A: feature/api-data
  * B: feature/ui-layout
  * C: feature/i18n-suggestions-docs
- Bước làm việc:
  1) git checkout -b feature/<ten>
  2) Commit nhỏ, rõ ràng
  3) Pull request → review chéo (A↔B, B↔C, ...)
  4) Merge vào main sau khi check build

---------------------------------------------
12) PHỤ LỤC: VÍ DỤ MAPPING MÔ TẢ
---------------------------------------------
- clear sky → Trời quang
- few clouds → Ít mây
- scattered clouds → Mây thưa
- broken clouds → Mây rải rác
- overcast clouds → Nhiều mây
- light rain → Mưa nhẹ
- moderate rain → Mưa vừa
- heavy rain → Mưa to
- very heavy rain → Mưa rất to
- extreme rain → Mưa cực to
- light intensity shower rain → Mưa rào nhẹ
- shower rain → Mưa rào
- heavy intensity shower rain → Mưa rào to
- ragged shower rain → Mưa rào không đều
- mist → Sương mù
- fog → Sương mù dày
- haze → Sương mù nhẹ
- smoke → Khói
- dust/sand/volcanic ash → Bụi/Cát/Tro núi lửa
- snow/light/heavy → Tuyết/Tuyết nhẹ/Tuyết to
- sleet/rain and snow → Mưa tuyết / Mưa và tuyết
- thunderstorm* → Bão (các biến thể có mưa phùn nhẹ/vừa/to ...)

Hết.

