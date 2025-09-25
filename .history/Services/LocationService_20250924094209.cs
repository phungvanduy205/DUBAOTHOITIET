using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Service quản lý địa điểm đã lưu
    /// </summary>
    public class LocationService
    {
        private const string SAVED_LOCATIONS_FILE = "saved_locations.txt";
        private const string LOCATIONS_JSON_FILE = "saved_locations.json";
        private const string FAVORITE_LOCATIONS_FILE = "favorite_locations.json";

        /// <summary>
        /// Lưu địa điểm vào file
        /// </summary>
        public static void SaveLocation(string name, double lat, double lon, List<SavedLocation> savedLocations)
        {
            try
            {
                // Kiểm tra xem địa điểm đã tồn tại chưa
                if (savedLocations.Any(loc => loc.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return; // Đã tồn tại, không lưu trùng
                }

                // Thêm vào danh sách
                var newLocation = new SavedLocation(name, lat, lon);
                savedLocations.Add(newLocation);

                // Lưu vào file
                var lines = savedLocations.Select(loc => $"{loc.Name}|{loc.Lat}|{loc.Lon}");
                File.WriteAllLines(SAVED_LOCATIONS_FILE, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Nạp danh sách địa điểm đã lưu từ file
        /// </summary>
        public static void LoadSavedLocations(List<SavedLocation> savedLocations, ListBox listBox)
        {
            try
            {
                if (!File.Exists(SAVED_LOCATIONS_FILE))
                {
                    listBox.Items.Clear();
                    return;
                }

                var lines = File.ReadAllLines(SAVED_LOCATIONS_FILE);
                savedLocations.Clear();
                listBox.Items.Clear();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length == 3 &&
                        double.TryParse(parts[1], out double lat) &&
                        double.TryParse(parts[2], out double lon))
                    {
                        var location = new SavedLocation(parts[0], lat, lon);
                        savedLocations.Add(location);
                        listBox.Items.Add(location);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp địa điểm đã lưu: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu danh sách địa điểm yêu thích
        /// </summary>
        public static void SaveFavoriteLocations(List<FavoriteLocation> favoriteLocations)
        {
            try
            {
                var json = JsonConvert.SerializeObject(favoriteLocations, Formatting.Indented);
                File.WriteAllText(FAVORITE_LOCATIONS_FILE, json);
                System.Diagnostics.Debug.WriteLine($"Đã lưu {favoriteLocations.Count} địa điểm yêu thích");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu địa điểm yêu thích: {ex.Message}");
            }
        }

        /// <summary>
        /// Load danh sách địa điểm yêu thích
        /// </summary>
        public static void LoadFavoriteLocations(List<FavoriteLocation> favoriteLocations)
        {
            try
            {
                if (File.Exists(FAVORITE_LOCATIONS_FILE))
                {
                    var json = File.ReadAllText(FAVORITE_LOCATIONS_FILE);
                    favoriteLocations.Clear();
                    var loaded = JsonConvert.DeserializeObject<List<FavoriteLocation>>(json);
                    if (loaded != null)
                    {
                        favoriteLocations.AddRange(loaded);
                    }
                    System.Diagnostics.Debug.WriteLine($"Đã tải {favoriteLocations.Count} địa điểm yêu thích");
                }
                else
                {
                    favoriteLocations.Clear();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load địa điểm yêu thích: {ex.Message}");
                favoriteLocations.Clear();
            }
        }

        /// <summary>
        /// Lưu danh sách tên địa điểm
        /// </summary>
        public static void SaveLocationNames(List<string> savedLocationNames)
        {
            try
            {
                var data = new { locations = savedLocationNames };
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(LOCATIONS_JSON_FILE, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lưu danh sách địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Load danh sách tên địa điểm
        /// </summary>
        public static void LoadLocationNames(List<string> savedLocationNames)
        {
            try
            {
                if (File.Exists(LOCATIONS_JSON_FILE))
                {
                    var json = File.ReadAllText(LOCATIONS_JSON_FILE);
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    if (data?.locations != null)
                    {
                        savedLocationNames.Clear();
                        savedLocationNames.AddRange(data.locations.ToObject<List<string>>());
                    }
                }
                
                // Nếu chưa có địa điểm nào, thêm một số địa điểm mẫu
                if (savedLocationNames.Count == 0)
                {
                    savedLocationNames.Add("Hanoi");
                    savedLocationNames.Add("Ho Chi Minh City");
                    savedLocationNames.Add("Da Nang");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load danh sách địa điểm: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa địa điểm khỏi danh sách
        /// </summary>
        public static void RemoveLocation(int index, List<SavedLocation> savedLocations)
        {
            try
            {
                if (index >= 0 && index < savedLocations.Count)
                {
                    var locationToRemove = savedLocations[index];
                    savedLocations.RemoveAt(index);

                    // Lưu lại file
                    var lines = savedLocations.Select(loc => $"{loc.Name}|{loc.Lat}|{loc.Lon}");
                    File.WriteAllLines(SAVED_LOCATIONS_FILE, lines);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa địa điểm: {ex.Message}");
            }
        }
    }
}