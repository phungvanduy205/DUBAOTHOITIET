using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THOITIET
{
    /// <summary>
    /// Service xử lý các chức năng liên quan đến địa điểm
    /// 1️⃣ Nhập địa điểm, tìm kiếm, lưu địa điểm, đổi °C/°F
    /// </summary>
    public class LocationService
    {
        private readonly string locationsFilePath;
        private List<SavedLocation> savedLocations;
        private readonly DichVuThoiTiet weatherService;

        public LocationService()
        {
            locationsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saved_locations.txt");
            savedLocations = new List<SavedLocation>();
            weatherService = new DichVuThoiTiet();
        }

        #region File Operations

        /// <summary>
        /// Lưu danh sách địa điểm vào file txt
        /// </summary>
        public void SaveLocationsToFile()
        {
            try
            {
                var lines = new List<string>();
                foreach (var loc in savedLocations)
                {
                    lines.Add($"{loc.Name}|{loc.Lat}|{loc.Lon}");
                }
                File.WriteAllLines(locationsFilePath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu file địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Đọc danh sách địa điểm từ file txt
        /// </summary>
        public void LoadLocationsFromFile()
        {
            try
            {
                if (!File.Exists(locationsFilePath))
                    return;

                var lines = File.ReadAllLines(locationsFilePath);
                savedLocations.Clear();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('|');
                    if (parts.Length == 3 &&
                        double.TryParse(parts[1], out double lat) &&
                        double.TryParse(parts[2], out double lon))
                    {
                        savedLocations.Add(new SavedLocation(parts[0], lat, lon));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo file lưu địa điểm nếu chưa có
        /// </summary>
        public void InitializeLocationsFile()
        {
            try
            {
                if (!File.Exists(locationsFilePath))
                {
                    File.WriteAllText(locationsFilePath, "");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create locations file error: {ex.Message}");
            }
        }

        #endregion

        #region Location Management

        /// <summary>
        /// Lưu địa điểm vào danh sách và file
        /// </summary>
        public void SaveLocation(string name, double lat, double lon)
        {
            try
            {
                // Chuẩn hóa tên địa điểm
                var cleanedName = name.Trim();
                if (string.IsNullOrEmpty(cleanedName))
                {
                    MessageBox.Show("Tên địa điểm không được để trống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra trùng lặp
                var normalizedNew = NormalizeName(cleanedName);
                if (savedLocations.Any(loc =>
                        NormalizeName(loc.Name) == normalizedNew ||
                        CoordinatesEqual(loc.Lat, loc.Lon, lat, lon)))
                {
                    MessageBox.Show("Địa điểm này đã được lưu rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Thêm vào danh sách
                var newLocation = new SavedLocation(cleanedName, lat, lon);
                savedLocations.Add(newLocation);

                // Lưu vào file txt
                SaveLocationsToFile();

                MessageBox.Show($"Đã lưu địa điểm: {cleanedName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm: {ex.Message}");
                MessageBox.Show($"Lỗi khi lưu địa điểm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Lưu địa điểm tự động (không hiện thông báo)
        /// </summary>
        public void SaveLocationSilent(string locationName, double lat, double lon)
        {
            try
            {
                var cleanedName = locationName.Trim();
                if (string.IsNullOrEmpty(cleanedName))
                    return;

                // Kiểm tra trùng lặp
                var normalizedNew = NormalizeName(cleanedName);
                if (savedLocations.Any(loc =>
                        NormalizeName(loc.Name) == normalizedNew ||
                        CoordinatesEqual(loc.Lat, loc.Lon, lat, lon)))
                {
                    return; // Bỏ qua nếu đã tồn tại
                }

                // Thêm vào danh sách và lưu vào file
                var newLocation = new SavedLocation(cleanedName, lat, lon);
                savedLocations.Add(newLocation);
                SaveLocationsToFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm tự động: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách địa điểm đã lưu
        /// </summary>
        public List<SavedLocation> GetSavedLocations()
        {
            LoadLocationsFromFile();
            return savedLocations.ToList();
        }

        /// <summary>
        /// Lấy danh sách tên địa điểm đã lưu
        /// </summary>
        public List<string> GetSavedLocationNames()
        {
            LoadLocationsFromFile();
            return savedLocations.Select(l => l.Name).ToList();
        }

        #endregion

        #region Search and Geocoding

        /// <summary>
        /// Tìm kiếm địa điểm và lấy dữ liệu thời tiết
        /// </summary>
        public async Task<(bool success, string location, double lat, double lon, OneCallResponse? weatherData)> SearchLocationAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return (false, "", 0, 0, null);

                // Gọi API tìm kiếm địa điểm
                var geocodingResult = await weatherService.TimDiaDiem(searchTerm);
                if (geocodingResult == null || geocodingResult.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy địa điểm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return (false, "", 0, 0, null);
                }

                var location = geocodingResult[0];
                var lat = location.Lat;
                var lon = location.Lon;
                var locationName = $"{location.Ten}, {location.Tinh}, {location.QuocGia}";

                // Lấy dữ liệu thời tiết
                var weatherData = await weatherService.LayThoiTietHienTai(lat, lon);
                if (weatherData == null)
                {
                    MessageBox.Show("Không thể lấy dữ liệu thời tiết!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return (false, "", 0, 0, null);
                }

                return (true, locationName, lat, lon, weatherData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tìm kiếm địa điểm: {ex.Message}");
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (false, "", 0, 0, null);
            }
        }

        /// <summary>
        /// Lấy vị trí hiện tại theo IP
        /// </summary>
        public async Task<(bool success, string location, double lat, double lon, OneCallResponse? weatherData)> GetCurrentLocationAsync()
        {
            try
            {
                var currentLocation = await weatherService.GetCurrentLocationAsync();
                if (currentLocation == null)
                {
                    return (false, "", 0, 0, null);
                }

                var lat = currentLocation.Lat;
                var lon = currentLocation.Lon;
                var locationName = $"{currentLocation.Name}, {currentLocation.State}, {currentLocation.Country}";

                // Lấy dữ liệu thời tiết
                var weatherData = await weatherService.GetWeatherDataAsync(lat, lon);
                if (weatherData == null)
                {
                    return (false, "", 0, 0, null);
                }

                return (true, locationName, lat, lon, weatherData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy vị trí hiện tại: {ex.Message}");
                return (false, "", 0, 0, null);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Chuẩn hóa tên địa điểm để so sánh
        /// </summary>
        private string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            
            return name.ToLowerInvariant()
                      .Replace(" ", "")
                      .Replace(",", "")
                      .Replace(".", "")
                      .Replace("-", "")
                      .Replace("_", "");
        }

        /// <summary>
        /// So sánh tọa độ (với sai số nhỏ)
        /// </summary>
        private bool CoordinatesEqual(double lat1, double lon1, double lat2, double lon2)
        {
            const double tolerance = 0.001; // Sai số 0.001 độ
            return Math.Abs(lat1 - lat2) < tolerance && Math.Abs(lon1 - lon2) < tolerance;
        }

        #endregion
    }
}
