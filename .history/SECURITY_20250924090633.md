# Bảo mật API Key

## ⚠️ QUAN TRỌNG: Bảo vệ API Key

### ✅ Cách BẢO MẬT (Khuyến nghị):

1. **Sử dụng file cấu hình:**
   ```bash
   # Chạy script tạo file .env.local
   load_env.bat
   
   # Chỉnh sửa file .env.local với API key thật
   # File này sẽ KHÔNG được commit vào Git
   ```

2. **Kiểm tra .gitignore:**
   - File `.env.local` đã được bảo vệ
   - Không commit API key vào Git

### ❌ Cách KHÔNG BẢO MẬT:

1. **Không sử dụng set_env.bat/ps1** trong production
2. **Không hardcode** API key trong code
3. **Không commit** file chứa API key

### 🔒 Best Practices:

1. **Tạo file .env.local** riêng cho mỗi môi trường
2. **Sử dụng load_env.bat/ps1** để đọc cấu hình
3. **Kiểm tra .gitignore** trước khi commit
4. **Không chia sẻ** file .env.local

### 🚨 Nếu API key bị lộ:

1. **Thay đổi API key** ngay lập tức
2. **Kiểm tra logs** để xem có bị lạm dụng không
3. **Cập nhật file cấu hình** mới
4. **Xóa API key cũ** khỏi tất cả nơi