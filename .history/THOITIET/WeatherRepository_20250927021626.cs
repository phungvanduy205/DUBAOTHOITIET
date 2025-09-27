using System;
using System.Data.SqlClient;
using System.Linq;

namespace THOITIET
{
    public class WeatherRepository
    {
        private readonly string _connectionString;

        public WeatherRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void EnsureCreated()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;
            var masterCs = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" }.ConnectionString;

            using (var conn = new SqlConnection(masterCs))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"IF DB_ID(@db) IS NULL CREATE DATABASE [{databaseName}]";
                cmd.Parameters.AddWithValue("@db", databaseName);
                cmd.ExecuteNonQuery();
            }

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

        public void SaveCurrentSnapshot(string locationName, double lat, double lon, OneCallResponse data)
        {
            if (data == null || data.Current == null) return;

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

                var current = data.Current;
                var weather0 = (current.Weather != null && current.Weather.Length > 0) ? current.Weather[0] : null;
                
                // Lấy nhiệt độ cao/thấp trong ngày từ daily data
                double? nhietDoCaoNgay = null;
                double? nhietDoThapNgay = null;
                if (data.Daily != null && data.Daily.Length > 0)
                {
                    nhietDoCaoNgay = data.Daily[0].Temp?.Max;
                    nhietDoThapNgay = data.Daily[0].Temp?.Min;
                }
                
                // Lưu dữ liệu 24h dưới dạng JSON string
                string nhietDo24GioJson = null;
                if (data.Hourly != null && data.Hourly.Length > 0)
                {
                    var hourlyData = data.Hourly.Take(24).Select(h => new {
                        ThoiGian = DateTimeOffset.FromUnixTimeSeconds(h.Dt).ToLocalTime().ToString("HH:mm"),
                        NhietDo = h.Temp
                    }).ToArray();
                    nhietDo24GioJson = Newtonsoft.Json.JsonConvert.SerializeObject(hourlyData);
                }

                cmd.Parameters.AddWithValue("@DiaDiem", (object)locationName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ViDo", lat);
                cmd.Parameters.AddWithValue("@KinhDo", lon);
                cmd.Parameters.AddWithValue("@NhietDo", current.Temp);
                cmd.Parameters.AddWithValue("@NhietDoCamGiac", current.FeelsLike);
                cmd.Parameters.AddWithValue("@DoAm", current.Humidity);
                cmd.Parameters.AddWithValue("@ApSuat", current.Pressure);
                cmd.Parameters.AddWithValue("@TocDoGio", current.WindSpeed);
                cmd.Parameters.AddWithValue("@TamNhin", current.Visibility);
                cmd.Parameters.AddWithValue("@MatTroiMoc", data.Daily != null && data.Daily.Length > 0 ? (object)data.Daily[0].Sunrise : DBNull.Value);
                cmd.Parameters.AddWithValue("@MatTroiLan", data.Daily != null && data.Daily.Length > 0 ? (object)data.Daily[0].Sunset : DBNull.Value);
                cmd.Parameters.AddWithValue("@MaThoiTiet", (object?)weather0?.Id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MoTaThoiTiet", (object?)weather0?.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IconThoiTiet", (object?)weather0?.Icon ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NhietDoCaoNgay", (object?)nhietDoCaoNgay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NhietDoThapNgay", (object?)nhietDoThapNgay ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NhietDo24Gio", (object?)nhietDo24GioJson ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

