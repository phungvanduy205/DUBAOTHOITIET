using System;
using System.Data.SqlClient;

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
    LocationName, Lat, Lon,
    TempKelvin, FeelsLikeKelvin, Humidity, Pressure, WindSpeed, Visibility,
    SunriseUnix, SunsetUnix, WeatherCode, WeatherDescription, WeatherIcon
)
VALUES
(
    @LocationName, @Lat, @Lon,
    @Temp, @FeelsLike, @Humidity, @Pressure, @WindSpeed, @Visibility,
    @Sunrise, @Sunset, @Code, @Desc, @Icon
)";

                var current = data.Current;
                var weather0 = (current.Weather != null && current.Weather.Length > 0) ? current.Weather[0] : null;

                cmd.Parameters.AddWithValue("@LocationName", (object)locationName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Lat", lat);
                cmd.Parameters.AddWithValue("@Lon", lon);
                cmd.Parameters.AddWithValue("@Temp", current.Temp);
                cmd.Parameters.AddWithValue("@FeelsLike", current.FeelsLike);
                cmd.Parameters.AddWithValue("@Humidity", current.Humidity);
                cmd.Parameters.AddWithValue("@Pressure", current.Pressure);
                cmd.Parameters.AddWithValue("@WindSpeed", current.WindSpeed);
                cmd.Parameters.AddWithValue("@Visibility", current.Visibility);
                cmd.Parameters.AddWithValue("@Sunrise", data.Daily != null && data.Daily.Length > 0 ? (object)data.Daily[0].Sunrise : DBNull.Value);
                cmd.Parameters.AddWithValue("@Sunset", data.Daily != null && data.Daily.Length > 0 ? (object)data.Daily[0].Sunset : DBNull.Value);
                cmd.Parameters.AddWithValue("@Code", (object?)weather0?.Id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Desc", (object?)weather0?.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Icon", (object?)weather0?.Icon ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

