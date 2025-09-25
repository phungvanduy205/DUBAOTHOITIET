# Hướng dẫn khắc phục lỗi ứng dụng thời tiết

## Lỗi "The source contains no DataRows"

### 🔍 Nguyên nhân
Lỗi này xảy ra khi:
- Database chưa có dữ liệu lịch sử thời tiết
- Cố gắng lọc dữ liệu từ DataTable rỗng
- Chưa lưu dữ liệu thời tiết nào vào database

### ✅ Giải pháp đã áp dụng

#### 1. Kiểm tra DataTable rỗng
```csharp
// Kiểm tra nếu DataTable rỗng
if (dataTable == null || dataTable.Rows.Count == 0)
{
    return new DataTable(); // Trả về DataTable rỗng
}
```

#### 2. Kiểm tra dữ liệu sau khi lọc
```csharp
// Kiểm tra nếu không có dữ liệu sau khi lọc
if (filteredRows.Count == 0)
{
    return new DataTable(); // Trả về DataTable rỗng
}
```

#### 3. Hiển thị thông báo thân thiện
```csharp
// Hiển thị thông báo không có dữ liệu
var noDataLabel = new Label
{
    Text = "Không có dữ liệu lịch sử cho khoảng thời gian đã chọn.\nHãy lưu dữ liệu thời tiết hiện tại để bắt đầu xây dựng lịch sử.",
    ForeColor = Color.White,
    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
    TextAlign = ContentAlignment.MiddleCenter,
    Dock = DockStyle.Fill
};
```

## Các lỗi thường gặp khác

### 1. Lỗi kết nối database
**Triệu chứng**: "Cannot open database 'WeatherDB'"
**Giải pháp**: 
- Kiểm tra SQL Server LocalDB đã cài đặt
- Chạy lại ứng dụng để tự động tạo database

### 2. Lỗi API thời tiết
**Triệu chứng**: "Không thể lấy dữ liệu thời tiết"
**Giải pháp**:
- Kiểm tra kết nối internet
- Kiểm tra API key trong `api_config.txt`
- Thử tìm kiếm địa điểm khác

### 3. Lỗi hiển thị biểu đồ
**Triệu chứng**: Biểu đồ không hiển thị
**Giải pháp**:
- Kiểm tra có dữ liệu lịch sử không
- Thử lưu dữ liệu thời tiết hiện tại
- Chọn bộ lọc thời gian khác

## Cách khắc phục từng bước

### Bước 1: Kiểm tra database
1. Mở tab "Lịch sử thời tiết"
2. Nếu thấy thông báo "Không có dữ liệu" → Bình thường
3. Nếu thấy lỗi database → Cần khắc phục

### Bước 2: Lưu dữ liệu đầu tiên
1. Tìm kiếm địa điểm (ví dụ: "Hanoi")
2. Chờ dữ liệu thời tiết hiển thị
3. Nhấn nút "Lưu hiện tại"
4. Kiểm tra thông báo "Đã lưu thành công"

### Bước 3: Kiểm tra auto-save
1. Xem label "Auto-save: Bật" (màu xanh)
2. Nếu màu đỏ → Có lỗi cần xử lý
3. Đợi 6 giờ để auto-save hoạt động

### Bước 4: Xem lịch sử
1. Chọn bộ lọc thời gian
2. Dữ liệu sẽ hiển thị theo ngày
3. Nhấn nút 📊 để xem biểu đồ

## Phòng ngừa lỗi

### 1. Luôn kiểm tra dữ liệu trước khi xử lý
```csharp
if (dataTable != null && dataTable.Rows.Count > 0)
{
    // Xử lý dữ liệu
}
```

### 2. Sử dụng try-catch để bắt lỗi
```csharp
try
{
    // Code xử lý dữ liệu
}
catch (Exception ex)
{
    // Xử lý lỗi
    MessageBox.Show($"Lỗi: {ex.Message}");
}
```

### 3. Kiểm tra điều kiện trước khi thực hiện
```csharp
// Kiểm tra có dữ liệu thời tiết không
if (weatherData == null || string.IsNullOrEmpty(currentLocation))
{
    MessageBox.Show("Không có dữ liệu thời tiết để lưu.");
    return;
}
```

## Debug và Logging

### 1. Sử dụng Debug Console
- Mở Visual Studio
- Xem Output window
- Tìm các thông báo "Auto-save: ..."

### 2. Kiểm tra trạng thái
- Label "Auto-save" hiển thị trạng thái
- Màu xanh: Bình thường
- Màu đỏ: Có lỗi

### 3. Kiểm tra database
- Mở SQL Server Management Studio
- Kết nối đến (localdb)\MSSQLLocalDB
- Kiểm tra database WeatherDB và bảng WeatherHistory

## Liên hệ hỗ trợ

Nếu vẫn gặp lỗi sau khi thực hiện các bước trên:

1. **Ghi lại lỗi**: Chụp màn hình lỗi
2. **Kiểm tra log**: Xem Debug Console
3. **Thử lại**: Khởi động lại ứng dụng
4. **Báo cáo**: Mô tả chi tiết các bước đã thực hiện

## Tóm tắt

Lỗi "The source contains no DataRows" đã được khắc phục bằng cách:
- ✅ Kiểm tra DataTable rỗng trước khi xử lý
- ✅ Hiển thị thông báo thân thiện khi không có dữ liệu
- ✅ Xử lý lỗi một cách graceful
- ✅ Hướng dẫn người dùng cách lưu dữ liệu đầu tiên

Ứng dụng giờ đây sẽ hoạt động mượt mà ngay cả khi chưa có dữ liệu lịch sử! 🎉