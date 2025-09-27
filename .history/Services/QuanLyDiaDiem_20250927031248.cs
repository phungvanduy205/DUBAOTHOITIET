using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Quản lý địa điểm - lưu, xóa, lấy danh sách từ cơ sở dữ liệu
    /// </summary>
    public class QuanLyDiaDiem
    {
        private readonly string _connectionString;

        public QuanLyDiaDiem()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=THOITIET;Trusted_Connection=True;TrustServerCertificate=True";
            KhoiTaoCoSoDuLieu();
        }

        /// <summary>
        /// Khởi tạo cơ sở dữ liệu và bảng
        /// </summary>
        private void KhoiTaoCoSoDuLieu()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);
                var databaseName = builder.InitialCatalog;
                var masterCs = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" }.ConnectionString;

                // Tạo database nếu chưa có
                using (var conn = new SqlConnection(masterCs))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"IF DB_ID(@db) IS NULL CREATE DATABASE [{databaseName}]";
                    cmd.Parameters.AddWithValue("@db", databaseName);
                    cmd.ExecuteNonQuery();
                }

                // Tạo bảng SavedLocations nếu chưa có
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
IF OBJECT_ID('dbo.SavedLocations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(256) NOT NULL,
        NormalizedName NVARCHAR(256) NOT NULL,
        Lat FLOAT NOT NULL,
        Lon FLOAT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_SavedLocations_NormalizedName ON dbo.SavedLocations(NormalizedName);
END";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo cơ sở dữ liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu địa điểm vào cơ sở dữ liệu
        /// </summary>
        public async Task<bool> LuuDiaDiem(string tenDiaDiem, double viDo, double kinhDo)
        {
            try
            {
                var tenChuanHoa = ChuanHoaTen(tenDiaDiem);
                
                // Kiểm tra xem địa điểm đã tồn tại chưa
                if (await KiemTraDiaDiemTonTai(tenChuanHoa, viDo, kinhDo))
                {
                    return true; // Đã tồn tại, coi như thành công
                }

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
INSERT INTO dbo.SavedLocations(Name, NormalizedName, Lat, Lon) 
VALUES(@name, @normalizedName, @lat, @lon)";
                    cmd.Parameters.AddWithValue("@name", tenDiaDiem);
                    cmd.Parameters.AddWithValue("@normalizedName", tenChuanHoa);
                    cmd.Parameters.AddWithValue("@lat", viDo);
                    cmd.Parameters.AddWithValue("@lon", kinhDo);
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách địa điểm đã lưu
        /// </summary>
        public async Task<List<SavedLocation>> LayDanhSachDiaDiem()
        {
            var ketQua = new List<SavedLocation>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "SELECT Name, Lat, Lon FROM dbo.SavedLocations ORDER BY CreatedAt DESC";
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var name = reader.GetString(0);
                            var lat = reader.GetDouble(1);
                            var lon = reader.GetDouble(2);
                            ketQua.Add(new SavedLocation(name, lat, lon));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy danh sách địa điểm: {ex.Message}");
            }
            return ketQua;
        }

        /// <summary>
        /// Xóa địa điểm khỏi cơ sở dữ liệu
        /// </summary>
        public async Task<bool> XoaDiaDiem(string tenDiaDiem)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "DELETE FROM dbo.SavedLocations WHERE Name = @name";
                    cmd.Parameters.AddWithValue("@name", tenDiaDiem);
                    var soDongBiAnhHuong = await cmd.ExecuteNonQueryAsync();
                    return soDongBiAnhHuong > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa địa điểm: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tìm kiếm địa điểm theo tên
        /// </summary>
        public async Task<List<SavedLocation>> TimKiemDiaDiem(string tuKhoa)
        {
            var ketQua = new List<SavedLocation>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT Name, Lat, Lon FROM dbo.SavedLocations 
WHERE Name LIKE @keyword OR NormalizedName LIKE @normalizedKeyword
ORDER BY CreatedAt DESC";
                    cmd.Parameters.AddWithValue("@keyword", $"%{tuKhoa}%");
                    cmd.Parameters.AddWithValue("@normalizedKeyword", $"%{ChuanHoaTen(tuKhoa)}%");
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var name = reader.GetString(0);
                            var lat = reader.GetDouble(1);
                            var lon = reader.GetDouble(2);
                            ketQua.Add(new SavedLocation(name, lat, lon));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tìm kiếm địa điểm: {ex.Message}");
            }
            return ketQua;
        }

        /// <summary>
        /// Kiểm tra địa điểm đã tồn tại chưa
        /// </summary>
        private async Task<bool> KiemTraDiaDiemTonTai(string tenChuanHoa, double viDo, double kinhDo, double epsilon = 0.01)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT TOP 1 1 FROM dbo.SavedLocations
WHERE NormalizedName = @normalizedName
   OR (ABS(Lat - @lat) <= @eps AND ABS(Lon - @lon) <= @eps)";
                    cmd.Parameters.AddWithValue("@normalizedName", tenChuanHoa);
                    cmd.Parameters.AddWithValue("@lat", viDo);
                    cmd.Parameters.AddWithValue("@lon", kinhDo);
                    cmd.Parameters.AddWithValue("@eps", epsilon);
                    
                    var ketQua = await cmd.ExecuteScalarAsync();
                    return ketQua != null && ketQua != DBNull.Value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra địa điểm tồn tại: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Chuẩn hóa tên địa điểm để so sánh
        /// </summary>
        private static string ChuanHoaTen(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return string.Empty;
            
            // Loại bỏ dấu phẩy thừa và khoảng trắng
            ten = ten.Replace(" ,", ",").Trim().Trim(',').Trim();
            
            // Chuyển về không dấu
            var formD = ten.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }
            var ketQua = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            ketQua = ketQua.Replace('đ', 'd').Replace('Đ', 'D');
            
            return ketQua.ToLowerInvariant();
        }
    }
}