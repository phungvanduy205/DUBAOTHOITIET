using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace THOITIET
{
    // API Key cho OpenWeatherMap (lấy từ biến môi trường)
    public static class ApiConfig
    {
        public static string API_KEY => "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        public static string GEOCODING_API_KEY => "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        
        private static string GetApiKey(string keyName)
        {
            // Thử đọc từ biến môi trường trước
            var envValue = Environment.GetEnvironmentVariable(keyName);
            if (!string.IsNullOrEmpty(envValue))
            {
                System.Diagnostics.Debug.WriteLine($"API Key {keyName} từ biến môi trường: {envValue.Substring(0, 8)}...");
                return envValue;
            }
                
            // Nếu không có, đọc từ file cấu hình
            try
            {
                var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_config.txt");
                System.Diagnostics.Debug.WriteLine($"Tìm file cấu hình tại: {configFile}");
                System.Diagnostics.Debug.WriteLine($"File tồn tại: {File.Exists(configFile)}");
                
                if (File.Exists(configFile))
                {
                    var lines = File.ReadAllLines(configFile);
                    System.Diagnostics.Debug.WriteLine($"Số dòng trong file: {lines.Length}");
                    foreach (var line in lines)
                    {
                        System.Diagnostics.Debug.WriteLine($"Dòng: {line}");
                        if (line.StartsWith(keyName + "="))
                        {
                            var key = line.Substring(keyName.Length + 1).Trim();
                            System.Diagnostics.Debug.WriteLine($"API Key {keyName} từ file: {key.Substring(0, Math.Min(8, key.Length))}...");
                            return key;
                        }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show($"Không tìm thấy file cấu hình tại: {configFile}", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đọc file cấu hình: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Lỗi đọc file cấu hình: {ex.Message}", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            // Nếu vẫn không có, trả về placeholder
            System.Diagnostics.Debug.WriteLine($"Không tìm thấy API Key {keyName}, sử dụng placeholder");
            return "your_api_key_here";
        }
        public const string BASE_URL = "https://api.openweathermap.org/data/3.0/onecall";
        public const string GEOCODING_URL = "https://api.openweathermap.org/geo/1.0/direct";
    }

    // Class để xử lý API One Call 3.0
    public class WeatherApiService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<OneCallResponse> GetCurrentWeatherAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={ApiConfig.API_KEY}&units=metric&lang=vi&exclude=minutely,alerts";
                System.Diagnostics.Debug.WriteLine($"URL API 3.0: {url}");
                string json = await httpClient.GetStringAsync(url);
                System.Diagnostics.Debug.WriteLine($"Response API 3.0: {json}");
                
                // Debug chi tiết wind_speed
                if (json.Contains("\"wind_speed\""))
                {
                    System.Diagnostics.Debug.WriteLine("✅ JSON có wind_speed field");
                    // Tìm giá trị wind_speed trong JSON
                    var windSpeedMatch = System.Text.RegularExpressions.Regex.Match(json, "\"wind_speed\":\\s*([0-9.]+)");
                    if (windSpeedMatch.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"Wind Speed trong JSON: {windSpeedMatch.Groups[1].Value}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ JSON KHÔNG có wind_speed field");
                    System.Diagnostics.Debug.WriteLine("Có thể API 3.0 trả về cấu trúc khác hoặc API key không có quyền");
                }
                
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API 3.0: {ex.Message}");
                System.Windows.Forms.MessageBox.Show($"Lỗi API 3.0: {ex.Message}\n\nCó thể do:\n1. API key không có quyền truy cập One Call API 3.0\n2. Cần subscription riêng cho One Call 3.0\n3. API key không hợp lệ", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
        }

        public static async Task<OneCallResponse> GetWeatherDataAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={ApiConfig.API_KEY}&units=metric&lang=vi&exclude=minutely,alerts";
                string json = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API: {ex.Message}");
                return null;
            }
        }

        public static async Task<GeocodingResponse> GetCoordinatesAsync(string cityName)
        {
            try
            {
                string url = $"{ApiConfig.GEOCODING_URL}?q={cityName}&limit=1&appid={ApiConfig.GEOCODING_API_KEY}";
                string json = await httpClient.GetStringAsync(url);
                var results = JsonConvert.DeserializeObject<GeocodingResult[]>(json);
                return results?.Length > 0 ? new GeocodingResponse { Results = results } : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi Geocoding: {ex.Message}");
                return null;
            }
        }

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
                    var result = new GeocodingResult
                    {
                        Name = ipData.City,
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
    }

    // Class cho Geocoding API
    public class GeocodingResponse
    {
        public GeocodingResult[] Results { get; set; }
    }

    public class GeocodingResult
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class IpLocationData
    {
        public string Status { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    // Class cho One Call 3.0 API Response
    public class OneCallResponse
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Timezone { get; set; }
        public int TimezoneOffset { get; set; }
        public CurrentWeather Current { get; set; }
        public HourlyWeather[] Hourly { get; set; }
        public DailyWeather[] Daily { get; set; }
    }

    public class CurrentWeatherResponse
    {
        public string Name { get; set; }
        public MainWeather Main { get; set; }
        public WeatherCondition[] Weather { get; set; }
        public WindInfo Wind { get; set; }
        public int Visibility { get; set; }
        public SysInfo Sys { get; set; }
    }

    public class MainWeather
    {
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WindInfo
    {
        public double Speed { get; set; }
        public int Deg { get; set; }
    }

    public class SysInfo
    {
        public long Sunrise { get; set; }
        public long Sunset { get; set; }
    }

    public class CurrentWeather
    {
        public long Dt { get; set; }
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double DewPoint { get; set; }
        public double Uvi { get; set; }
        public int Clouds { get; set; }
        public int Visibility { get; set; }
        public double WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public WeatherCondition[] Weather { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class HourlyWeather
    {
        public long Dt { get; set; }
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double DewPoint { get; set; }
        public double Uvi { get; set; }
        public int Clouds { get; set; }
        public int Visibility { get; set; }
        public double WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public WeatherCondition[] Weather { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class DailyWeather
    {
        public long Dt { get; set; }
        public long Sunrise { get; set; }
        public long Sunset { get; set; }
        public Temperature Temp { get; set; }
        public Temperature FeelsLike { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double DewPoint { get; set; }
        public double WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public WeatherCondition[] Weather { get; set; }
        public int Clouds { get; set; }
        public double Uvi { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class Temperature
    {
        public double Day { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Night { get; set; }
        public double Eve { get; set; }
        public double Morn { get; set; }
    }

    public class WeatherCondition
    {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class Rain
    {
        public double? OneHour { get; set; }
    }

    public class Snow
    {
        public double? OneHour { get; set; }
    }

    // Custom JsonConverter để xử lý Rain và Snow có thể là số hoặc object
    public class RainSnowConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Rain) || objectType == typeof(Snow);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                // Nếu là số, tạo object với OneHour
                var value = Convert.ToDouble(reader.Value);
                if (objectType == typeof(Rain))
                    return new Rain { OneHour = value };
                else
                    return new Snow { OneHour = value };
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Nếu là object, deserialize bình thường
                return serializer.Deserialize(reader, objectType);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}