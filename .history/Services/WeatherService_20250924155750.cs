using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Service xử lý các chức năng liên quan đến thời tiết
    /// </summary>
    public class WeatherService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Lấy dữ liệu thời tiết hiện tại theo tọa độ
        /// </summary>
        public static async Task<OneCallResponse> GetCurrentWeatherAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={ApiConfig.API_KEY}&lang=vi&exclude=minutely,alerts";
                System.Diagnostics.Debug.WriteLine($"URL API 3.0: {url}");
                string json = await httpClient.GetStringAsync(url);
                System.Diagnostics.Debug.WriteLine($"Response API 3.0: {json}");
                
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API 3.0: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy dữ liệu thời tiết đầy đủ (One Call API)
        /// </summary>
        public static async Task<OneCallResponse> GetWeatherDataAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={ApiConfig.API_KEY}&lang=vi&exclude=minutely,alerts";
                string json = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy dữ liệu thời tiết: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy vị trí hiện tại từ IP
        /// </summary>
        public static async Task<GeocodingResponse> GetCurrentLocationAsync()
        {
            try
            {
                // Lấy IP hiện tại
                string ip = await httpClient.GetStringAsync("https://api.ipify.org");
                
                // Lấy địa điểm từ IP
                string url = $"http://ip-api.com/json/{ip}";
                string json = await httpClient.GetStringAsync(url);
                var ipData = JsonConvert.DeserializeObject<IpLocationData>(json);
                
                if (ipData != null && ipData.Status == "success")
                {
                    // Thử reverse geocoding từ lat/lon để lấy tên tỉnh/thành chính xác
                    var reverse = await ReverseGeocodeAsync(ipData.Lat, ipData.Lon);
                    var displayName = reverse?.Name;
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = ipData.City; // fallback
                    }

                    var result = new GeocodingResult
                    {
                        Name = displayName,
                        Country = ipData.Country,
                        Lat = ipData.Lat,
                        Lon = ipData.Lon
                    };
                    return new GeocodingResponse { Results = new[] { result } };
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy địa điểm hiện tại: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Reverse geocoding qua OpenWeather Geo API
        /// </summary>
        public static async Task<GeocodingResult> ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/geo/1.0/reverse?lat={lat}&lon={lon}&limit=1&appid={ApiConfig.API_KEY}";
                string json = await httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<GeocodingResult[]>(json);
                return results?.Length > 0 ? results[0] : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi reverse geocoding: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy tọa độ từ tên địa điểm
        /// </summary>
        public static async Task<GeocodingResponse> GetCoordinatesAsync(string locationName)
        {
            try
            {
                string url = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(locationName)}&limit=5&appid={ApiConfig.API_KEY}";
                string json = await httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<GeocodingResult[]>(json);
                return new GeocodingResponse { Results = results };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi geocoding: {ex.Message}");
                return null;
            }
        }
    }
}