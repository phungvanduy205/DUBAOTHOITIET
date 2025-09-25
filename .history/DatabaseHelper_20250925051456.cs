using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Class quản lý kết nối và thao tác với SQL Server
    /// </summary>
    public class DatabaseHelper
    {
        private string connectionString;
        private const string TABLE_NAME = "WeatherHistory";

        public DatabaseHelper()
        {
            // Chuỗi kết nối SQL Server LocalDB - không chỉ định database cụ thể
            connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;";
        }

        public DatabaseHelper(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Kiểm tra kết nối database
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Thử khởi tạo LocalDB nếu chưa có
                if (ex.Message.Contains("Cannot open database") || ex.Message.Contains("Login failed"))
                {
                    try
                    {
                        await InitializeLocalDBAsync();
                        return true;
                    }
                    catch
                    {
                        MessageBox.Show($"Lỗi kết nối database: {ex.Message}\n\nVui lòng kiểm tra SQL Server LocalDB đã được cài đặt chưa.", 
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show($"Lỗi kết nối database: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// Khởi tạo LocalDB instance nếu cần
        /// </summary>
        private async Task InitializeLocalDBAsync()
        {
            try
            {
                // Thử kết nối với master database trước
                var masterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    
                    // Tạo database WeatherDB
                    string createDatabaseQuery = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WeatherDB')
                        BEGIN
                            CREATE DATABASE WeatherDB;
                        END";

                    using (var command = new SqlCommand(createDatabaseQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Cập nhật connection string
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=WeatherDB;Integrated Security=true;TrustServerCertificate=true;";
            }
            catch (Exception ex)
            {
                throw new Exception($"Không thể khởi tạo LocalDB: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo database và bảng nếu chưa tồn tại
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Tạo bảng WeatherHistory trong database WeatherDB
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string createTableQuery = $@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{TABLE_NAME}' AND xtype='U')
                        CREATE TABLE {TABLE_NAME} (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Location NVARCHAR(255) NOT NULL,
                            Latitude DECIMAL(10, 8) NOT NULL,
                            Longitude DECIMAL(11, 8) NOT NULL,
                            Temperature DECIMAL(5, 2) NOT NULL,
                            FeelsLike DECIMAL(5, 2) NOT NULL,
                            Humidity INT NOT NULL,
                            Pressure INT NOT NULL,
                            WindSpeed DECIMAL(5, 2) NOT NULL,
                            WindDirection INT NOT NULL,
                            Visibility DECIMAL(5, 2) NOT NULL,
                            WeatherDescription NVARCHAR(255) NOT NULL,
                            WeatherIcon NVARCHAR(50) NOT NULL,
                            RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                            Unit NVARCHAR(10) NOT NULL DEFAULT 'Celsius'
                        )";

                    using (var command = new SqlCommand(createTableQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo database: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Lưu dữ liệu thời tiết vào database
        /// </summary>
        public async Task<bool> SaveWeatherDataAsync(WeatherData weatherData)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string insertQuery = $@"
                        INSERT INTO {TABLE_NAME} 
                        (Location, Latitude, Longitude, Temperature, FeelsLike, Humidity, 
                         Pressure, WindSpeed, WindDirection, Visibility, WeatherDescription, 
                         WeatherIcon, Unit)
                        VALUES 
                        (@Location, @Latitude, @Longitude, @Temperature, @FeelsLike, @Humidity,
                         @Pressure, @WindSpeed, @WindDirection, @Visibility, @WeatherDescription,
                         @WeatherIcon, @Unit)";

                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Location", weatherData.Location);
                        command.Parameters.AddWithValue("@Latitude", weatherData.Latitude);
                        command.Parameters.AddWithValue("@Longitude", weatherData.Longitude);
                        command.Parameters.AddWithValue("@Temperature", weatherData.Temperature);
                        command.Parameters.AddWithValue("@FeelsLike", weatherData.FeelsLike);
                        command.Parameters.AddWithValue("@Humidity", weatherData.Humidity);
                        command.Parameters.AddWithValue("@Pressure", weatherData.Pressure);
                        command.Parameters.AddWithValue("@WindSpeed", weatherData.WindSpeed);
                        command.Parameters.AddWithValue("@WindDirection", weatherData.WindDirection);
                        command.Parameters.AddWithValue("@Visibility", weatherData.Visibility);
                        command.Parameters.AddWithValue("@WeatherDescription", weatherData.WeatherDescription);
                        command.Parameters.AddWithValue("@WeatherIcon", weatherData.WeatherIcon);
                        command.Parameters.AddWithValue("@Unit", weatherData.Unit);

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả dữ liệu lịch sử thời tiết
        /// </summary>
        public async Task<DataTable> GetAllWeatherHistoryAsync()
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = $@"
                        SELECT 
                            Id,
                            Location,
                            Temperature,
                            FeelsLike,
                            Humidity,
                            Pressure,
                            WindSpeed,
                            WindDirection,
                            Visibility,
                            WeatherDescription,
                            WeatherIcon,
                            Unit,
                            RecordedAt
                        FROM {TABLE_NAME} 
                        ORDER BY RecordedAt DESC";

                    using (var command = new SqlCommand(selectQuery, connection))
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lấy dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        /// <summary>
        /// Xóa dữ liệu lịch sử theo ID
        /// </summary>
        public async Task<bool> DeleteWeatherRecordAsync(int id)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string deleteQuery = $"DELETE FROM {TABLE_NAME} WHERE Id = @Id";

                    using (var command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Xóa tất cả dữ liệu lịch sử
        /// </summary>
        public async Task<bool> ClearAllWeatherHistoryAsync()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string deleteQuery = $"DELETE FROM {TABLE_NAME}";

                    using (var command = new SqlCommand(deleteQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa tất cả dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Lấy số lượng bản ghi trong database
        /// </summary>
        public async Task<int> GetRecordCountAsync()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string countQuery = $"SELECT COUNT(*) FROM {TABLE_NAME}";

                    using (var command = new SqlCommand(countQuery, connection))
                    {
                        return Convert.ToInt32(await command.ExecuteScalarAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đếm bản ghi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
    }

    /// <summary>
    /// Class chứa dữ liệu thời tiết để lưu vào database
    /// </summary>
    public class WeatherData
    {
        public string Location { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Temperature { get; set; }
        public decimal FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public decimal WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public decimal Visibility { get; set; }
        public string WeatherDescription { get; set; } = "";
        public string WeatherIcon { get; set; } = "";
        public string Unit { get; set; } = "Celsius";
    }
}