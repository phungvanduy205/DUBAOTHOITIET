# Hướng dẫn sử dụng chức năng Lịch sử Thời tiết Nâng cao

## Tổng quan
Ứng dụng thời tiết đã được nâng cấp với giao diện lịch sử thời tiết tương tự như dự báo 5 ngày, bao gồm các biểu đồ phân tích và bộ lọc thời gian.

## Giao diện mới

### 1. Tab "Lịch sử thời tiết"
- **Vị trí**: Bên cạnh tab "Biểu đồ nhiệt độ" và "Bản đồ"
- **Giao diện**: Tương tự như dự báo 5 ngày với các panel ngày
- **Chức năng**: Hiển thị dữ liệu lịch sử theo dạng ngày, dễ dàng so sánh

### 2. Bộ lọc thời gian
- **1 tuần**: Hiển thị dữ liệu 7 ngày gần nhất
- **1 tháng**: Hiển thị dữ liệu 30 ngày gần nhất
- **3 tháng**: Hiển thị dữ liệu 90 ngày gần nhất
- **6 tháng**: Hiển thị dữ liệu 180 ngày gần nhất
- **1 năm**: Hiển thị dữ liệu 365 ngày gần nhất
- **Tất cả**: Hiển thị toàn bộ dữ liệu lịch sử

### 3. Hiển thị dữ liệu theo ngày
Mỗi ngày hiển thị:
- **Ngày tháng**: Thứ, ngày/tháng/năm
- **Nhiệt độ trung bình**: Tính trung bình các lần lưu trong ngày
- **Độ ẩm trung bình**: Tính trung bình độ ẩm trong ngày
- **Mô tả thời tiết**: Thông tin thời tiết chính
- **Nút biểu đồ**: 📊 để xem biểu đồ chi tiết

## Biểu đồ phân tích

### 1. Nhiệt độ theo tháng
- **Loại**: Biểu đồ đường (Line Chart)
- **Dữ liệu**: Nhiệt độ trung bình theo từng tháng
- **Màu sắc**: Đỏ
- **Mục đích**: Theo dõi xu hướng nhiệt độ theo thời gian

### 2. Trung bình nhiệt độ
- **Loại**: Biểu đồ cột (Column Chart)
- **Dữ liệu**: Nhiệt độ trung bình theo tháng
- **Màu sắc**: Xanh dương
- **Mục đích**: So sánh nhiệt độ giữa các tháng

### 3. Lượng mưa theo tháng
- **Loại**: Biểu đồ thanh (Bar Chart)
- **Dữ liệu**: Số ngày mưa trong tháng
- **Màu sắc**: Xanh lá
- **Mục đích**: Phân tích mùa mưa và khô

## Cách sử dụng

### Lưu dữ liệu thời tiết
1. Tìm kiếm địa điểm và xem thông tin thời tiết
2. Chuyển sang tab "Lịch sử thời tiết"
3. Nhấn nút "Lưu hiện tại"
4. Dữ liệu sẽ được lưu vào database

### Xem lịch sử theo thời gian
1. Chọn bộ lọc thời gian từ dropdown
2. Dữ liệu sẽ tự động cập nhật theo khoảng thời gian đã chọn
3. Cuộn để xem các ngày khác nhau

### Xem biểu đồ phân tích
1. Nhấn nút 📊 trên bất kỳ ngày nào
2. Chọn loại biểu đồ từ dropdown
3. Biểu đồ sẽ hiển thị dữ liệu theo loại đã chọn

### Quản lý dữ liệu
- **Làm mới**: Nhấn "Làm mới" để cập nhật dữ liệu mới nhất
- **Xóa tất cả**: Nhấn "Xóa tất cả" để xóa toàn bộ dữ liệu lịch sử

## Tính năng nâng cao

### 1. Phân tích xu hướng
- Theo dõi nhiệt độ tăng/giảm theo thời gian
- Phân tích mùa mưa và khô
- So sánh dữ liệu giữa các tháng

### 2. Lưu trữ lâu dài
- Dữ liệu được lưu trữ vĩnh viễn trong SQL Server
- Không bị mất khi đóng ứng dụng
- Có thể xuất dữ liệu ra file

### 3. Giao diện thân thiện
- Thiết kế tương tự dự báo 5 ngày
- Dễ dàng chuyển đổi giữa các chế độ xem
- Biểu đồ trực quan và dễ hiểu

## Yêu cầu hệ thống
- SQL Server LocalDB
- .NET 8.0 Runtime
- Windows Forms DataVisualization (cho biểu đồ)

## Lưu ý kỹ thuật
- Database tự động tạo khi lần đầu chạy
- Dữ liệu được nhóm theo ngày để hiển thị
- Biểu đồ được tạo động từ dữ liệu database
- Hỗ trợ cuộn và tìm kiếm trong lịch sử

## Mở rộng tương lai
- Xuất dữ liệu ra Excel/CSV
- Thống kê chi tiết hơn
- So sánh giữa các địa điểm
- Dự báo dựa trên dữ liệu lịch sử
- Thông báo khi có thay đổi thời tiết bất thường