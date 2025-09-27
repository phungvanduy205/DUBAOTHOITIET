using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Quản lý thời tiết - lưu và truy xuất dữ liệu thời tiết từ cơ sở dữ liệu
    /// </summary>
    public class QuanLyThoiTiet
    {
        private readonly string _connectionString;

        public QuanLyThoiTiet()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=THOITIET;Trusted_Connection=True;TrustServerCertificate=True";
            KhoiTaoCoSoDuLieu();
        }

        /// <summary>
        /// Khởi tạo cơ sở dữ liệu và bảng thời tiết
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

                // Tạo bảng WeatherSnapshots nếu chưa có
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
IF OBJECT_ID('dbo.WeatherSnapshots', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeatherSnapshots (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ThoiGian DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        DiaDiem NVARCHAR(256) NOT NULL,
        ViDo FLOAT NOT NULL,
        KinhDo FLOAT NOT NULL,
        NhietDo FLOAT NULL,
        NhietDoCamGiac FLOAT NULL,
        DoAm INT NULL,
        ApSuat INT NULL,
        TocDoGio FLOAT NULL,
        TamNhin INT NULL,
        MatTroiMoc BIGINT NULL,
        MatTroiLan BIGINT NULL,
        MaThoiTiet INT NULL,
        MoTaThoiTiet NVARCHAR(256) NULL,
        IconThoiTiet NVARCHAR(16) NULL,
        NhietDoCaoNgay FLOAT NULL,
        NhietDoThapNgay FLOAT NULL,
        NhietDo24Gio NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_WeatherSnapshots_ThoiGian ON dbo.WeatherSnapshots(ThoiGian DESC);
    CREATE INDEX IX_WeatherSnapshots_DiaDiem ON dbo.WeatherSnapshots(DiaDiem);
END";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo cơ sở dữ liệu thời tiết: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu dữ liệu thời tiết vào cơ sở dữ liệu
        /// </summary>
        public async Task<bool> LuuDuLieuThoiTiet(string diaDiem, double viDo, double kinhDo, OneCallResponse duLieu)
        {
            try
            {
                if (duLieu?.Current == null) return false;

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
INSERT INTO dbo.WeatherSnapshots
(
    DiaDiem, ViDo, KinhDo,
    NhietDo, NhietDoCamGiac, DoAm, ApSuat, TocDoGio, TamNhin,
    MatTroiMoc, MatTroiLan, MaThoiTiet, MoTaThoiTiet, IconThoiTiet,
    NhietDoCaoNgay, NhietDoThapNgay, NhietDo24Gio
)
VALUES
(
    @DiaDiem, @ViDo, @KinhDo,
    @NhietDo, @NhietDoCamGiac, @DoAm, @ApSuat, @TocDoGio, @TamNhin,
    @MatTroiMoc, @MatTroiLan, @MaThoiTiet, @MoTaThoiTiet, @IconThoiTiet,
    @NhietDoCaoNgay, @NhietDoThapNgay, @NhietDo24Gio
)";

                    var current = duLieu.Current;
                    var weather0 = (current.Weather != null && current.Weather.Length > 0) ? current.Weather[0] : null;
                    
                    // Lấy nhiệt độ cao/thấp trong ngày từ daily data
                    double? nhietDoCaoNgay = null;
                    double? nhietDoThapNgay = null;
                    if (duLieu.Daily != null && duLieu.Daily.Length > 0)
                    {
                        nhietDoCaoNgay = duLieu.Daily[0].Temp?.Max;
                        nhietDoThapNgay = duLieu.Daily[0].Temp?.Min;
                    }
                    
                    // Lưu dữ liệu 24h dưới dạng JSON string
                    string nhietDo24GioJson = null;
                    if (duLieu.Hourly != null && duLieu.Hourly.Length > 0)
                    {
                        var hourlyData = duLieu.Hourly.Take(24).Select(h => new {
                            ThoiGian = DateTimeOffset.FromUnixTimeSeconds(h.Dt).ToLocalTime().ToString("HH:mm"),
                            NhietDo = h.Temp
                        }).ToArray();
                        nhietDo24GioJson = Newtonsoft.Json.JsonConvert.SerializeObject(hourlyData);
                    }

                    cmd.Parameters.AddWithValue("@DiaDiem", (object)diaDiem ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ViDo", viDo);
                    cmd.Parameters.AddWithValue("@KinhDo", kinhDo);
                    cmd.Parameters.AddWithValue("@NhietDo", current.Temp);
                    cmd.Parameters.AddWithValue("@NhietDoCamGiac", current.FeelsLike);
                    cmd.Parameters.AddWithValue("@DoAm", current.Humidity);
                    cmd.Parameters.AddWithValue("@ApSuat", current.Pressure);
                    cmd.Parameters.AddWithValue("@TocDoGio", current.WindSpeed);
                    cmd.Parameters.AddWithValue("@TamNhin", current.Visibility);
                    cmd.Parameters.AddWithValue("@MatTroiMoc", duLieu.Daily != null && duLieu.Daily.Length > 0 ? (object)duLieu.Daily[0].Sunrise : DBNull.Value);
                    cmd.Parameters.AddWithValue("@MatTroiLan", duLieu.Daily != null && duLieu.Daily.Length > 0 ? (object)duLieu.Daily[0].Sunset : DBNull.Value);
                    cmd.Parameters.AddWithValue("@MaThoiTiet", (object?)weather0?.Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@MoTaThoiTiet", (object?)weather0?.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IconThoiTiet", (object?)weather0?.Icon ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NhietDoCaoNgay", (object?)nhietDoCaoNgay ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NhietDoThapNgay", (object?)nhietDoThapNgay ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NhietDo24Gio", (object?)nhietDo24GioJson ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu dữ liệu thời tiết: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy lịch sử thời tiết theo địa điểm
        /// </summary>
        public async Task<List<DuLieuThoiTietLuu>> LayLichSuThoiTiet(string diaDiem, int soNgay = 7)
        {
            var ketQua = new List<DuLieuThoiTietLuu>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT TOP (@soNgay) 
    ThoiGian, DiaDiem, ViDo, KinhDo, NhietDo, NhietDoCamGiac, 
    DoAm, ApSuat, TocDoGio, TamNhin, MatTroiMoc, MatTroiLan,
    MaThoiTiet, MoTaThoiTiet, IconThoiTiet, NhietDoCaoNgay, 
    NhietDoThapNgay, NhietDo24Gio
FROM dbo.WeatherSnapshots 
WHERE DiaDiem = @diaDiem 
ORDER BY ThoiGian DESC";
                    cmd.Parameters.AddWithValue("@diaDiem", diaDiem);
                    cmd.Parameters.AddWithValue("@soNgay", soNgay * 24); // 24 bản ghi mỗi ngày
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ketQua.Add(new DuLieuThoiTietLuu
                            {
                                ThoiGian = reader.GetDateTime(0),
                                DiaDiem = reader.GetString(1),
                                ViDo = reader.GetDouble(2),
                                KinhDo = reader.GetDouble(3),
                                NhietDo = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4),
                                NhietDoCamGiac = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                DoAm = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                ApSuat = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                TocDoGio = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                                TamNhin = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                                MatTroiMoc = reader.IsDBNull(10) ? (long?)null : reader.GetInt64(10),
                                MatTroiLan = reader.IsDBNull(11) ? (long?)null : reader.GetInt64(11),
                                MaThoiTiet = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                                MoTaThoiTiet = reader.IsDBNull(13) ? null : reader.GetString(13),
                                IconThoiTiet = reader.IsDBNull(14) ? null : reader.GetString(14),
                                NhietDoCaoNgay = reader.IsDBNull(15) ? (double?)null : reader.GetDouble(15),
                                NhietDoThapNgay = reader.IsDBNull(16) ? (double?)null : reader.GetDouble(16),
                                NhietDo24Gio = reader.IsDBNull(17) ? null : reader.GetString(17)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy lịch sử thời tiết: {ex.Message}");
            }
            return ketQua;
        }

        /// <summary>
        /// Lấy dữ liệu thời tiết gần nhất theo địa điểm
        /// </summary>
        public async Task<DuLieuThoiTietLuu?> LayDuLieuGanNhat(string diaDiem)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"
SELECT TOP 1 
    ThoiGian, DiaDiem, ViDo, KinhDo, NhietDo, NhietDoCamGiac, 
    DoAm, ApSuat, TocDoGio, TamNhin, MatTroiMoc, MatTroiLan,
    MaThoiTiet, MoTaThoiTiet, IconThoiTiet, NhietDoCaoNgay, 
    NhietDoThapNgay, NhietDo24Gio
FROM dbo.WeatherSnapshots 
WHERE DiaDiem = @diaDiem 
ORDER BY ThoiGian DESC";
                    cmd.Parameters.AddWithValue("@diaDiem", diaDiem);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new DuLieuThoiTietLuu
                            {
                                ThoiGian = reader.GetDateTime(0),
                                DiaDiem = reader.GetString(1),
                                ViDo = reader.GetDouble(2),
                                KinhDo = reader.GetDouble(3),
                                NhietDo = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4),
                                NhietDoCamGiac = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                DoAm = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                ApSuat = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                TocDoGio = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                                TamNhin = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                                MatTroiMoc = reader.IsDBNull(10) ? (long?)null : reader.GetInt64(10),
                                MatTroiLan = reader.IsDBNull(11) ? (long?)null : reader.GetInt64(11),
                                MaThoiTiet = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                                MoTaThoiTiet = reader.IsDBNull(13) ? null : reader.GetString(13),
                                IconThoiTiet = reader.IsDBNull(14) ? null : reader.GetString(14),
                                NhietDoCaoNgay = reader.IsDBNull(15) ? (double?)null : reader.GetDouble(15),
                                NhietDoThapNgay = reader.IsDBNull(16) ? (double?)null : reader.GetDouble(16),
                                NhietDo24Gio = reader.IsDBNull(17) ? null : reader.GetString(17)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy dữ liệu gần nhất: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Xóa dữ liệu thời tiết cũ (trước 30 ngày)
        /// </summary>
        public async Task<bool> XoaDuLieuCu()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "DELETE FROM dbo.WeatherSnapshots WHERE ThoiGian < DATEADD(day, -30, GETDATE())";
                    var soDongBiAnhHuong = await cmd.ExecuteNonQueryAsync();
                    System.Diagnostics.Debug.WriteLine($"Đã xóa {soDongBiAnhHuong} bản ghi thời tiết cũ");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa dữ liệu cũ: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Dữ liệu thời tiết đã lưu trong cơ sở dữ liệu
    /// </summary>
    public class DuLieuThoiTietLuu
    {
        public DateTime ThoiGian { get; set; }
        public string DiaDiem { get; set; } = "";
        public double ViDo { get; set; }
        public double KinhDo { get; set; }
        public double? NhietDo { get; set; }
        public double? NhietDoCamGiac { get; set; }
        public int? DoAm { get; set; }
        public int? ApSuat { get; set; }
        public double? TocDoGio { get; set; }
        public int? TamNhin { get; set; }
        public long? MatTroiMoc { get; set; }
        public long? MatTroiLan { get; set; }
        public int? MaThoiTiet { get; set; }
        public string? MoTaThoiTiet { get; set; }
        public string? IconThoiTiet { get; set; }
        public double? NhietDoCaoNgay { get; set; }
        public double? NhietDoThapNgay { get; set; }
        public string? NhietDo24Gio { get; set; }
    }
}