using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace THOITIET
{
    public static class CauHinhApi
    {
        public static string API_KEY => "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        public static string GEOCODING_API_KEY => "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        
        private static string LayKhoaApi(string tenKhoa)
        {
            var envValue = Environment.GetEnvironmentVariable(tenKhoa);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
            try
            {
                var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api_config.txt");
                if (File.Exists(configFile))
                {
                    var lines = File.ReadAllLines(configFile);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith(tenKhoa + "="))
                        {
                            var key = line.Substring(tenKhoa.Length + 1).Trim();
                            return key;
                        }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show($"Không tìm thấy file cấu hình tại: {configFile}", "Gỡ lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi đọc file cấu hình: {ex.Message}", "Gỡ lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            // Nếu vẫn không có, trả về placeholder
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
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={CauHinhApi.API_KEY}&lang=en&exclude=minutely,alerts";
                string json = await httpClient.GetStringAsync(url);
                
                // Debug chi tiết wind_speed
                if (json.Contains("\"wind_speed\""))
                {
                    // Tìm giá trị wind_speed trong JSON
                    var windSpeedMatch = System.Text.RegularExpressions.Regex.Match(json, "\"wind_speed\":\\s*([0-9.]+)");
                    if (windSpeedMatch.Success)
                    {
                    }
                }
                else
                {
                }
                
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi API 3.0: {ex.Message}\n\nCó thể do:\n1. API key không có quyền truy cập One Call API 3.0\n2. Cần subscription riêng cho One Call 3.0\n3. API key không hợp lệ", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
        }

        public static async Task<OneCallResponse> GetWeatherDataAsync(double lat, double lon)
        {
            try
            {
                string url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lon}&appid={CauHinhApi.API_KEY}&lang=en&exclude=minutely,alerts";
                string json = await httpClient.GetStringAsync(url);
                
                // Debug chi tiết wind_speed
                if (json.Contains("\"wind_speed\""))
                {
                    var windSpeedMatch = System.Text.RegularExpressions.Regex.Match(json, "\"wind_speed\":\\s*([0-9.]+)");
                    if (windSpeedMatch.Success)
                    {
                    }
                }
                else
                {
                }
                
                return JsonConvert.DeserializeObject<OneCallResponse>(json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi API 3.0: {ex.Message}\n\nCó thể do:\n1. API key không có quyền truy cập One Call API 3.0\n2. Cần subscription riêng cho One Call 3.0\n3. API key không hợp lệ", "Debug", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return null;
            }
        }

        public static async Task<GeocodingResponse> GetCoordinatesAsync(string cityName)
        {
            try
            {
                // Thử tìm kiếm ưu tiên Việt Nam trước để tránh nhầm sang Trung Quốc (Taiyuan)
                string qEncoded = Uri.EscapeDataString(cityName);
                string urlVn = $"{CauHinhApi.GEOCODING_URL}?q={qEncoded},VN&limit=5&appid={CauHinhApi.GEOCODING_API_KEY}";
                string urlAny = $"{CauHinhApi.GEOCODING_URL}?q={qEncoded}&limit=5&appid={CauHinhApi.GEOCODING_API_KEY}";

                GeocodingResult[] resultsVn = Array.Empty<GeocodingResult>();
                GeocodingResult[] resultsAny = Array.Empty<GeocodingResult>();

                try
                {
                    string jsonVn = await httpClient.GetStringAsync(urlVn);
                    resultsVn = JsonConvert.DeserializeObject<GeocodingResult[]>(jsonVn) ?? Array.Empty<GeocodingResult>();
                }
                catch { /* bỏ qua, sẽ fallback */ }

                if (resultsVn.Length == 0)
                {
                    string jsonAny = await httpClient.GetStringAsync(urlAny);
                    resultsAny = JsonConvert.DeserializeObject<GeocodingResult[]>(jsonAny) ?? Array.Empty<GeocodingResult>();
                }

                // Ưu tiên kết quả ở Việt Nam, nếu có
                var chosen = resultsVn.FirstOrDefault() ?? resultsAny.FirstOrDefault();
                return chosen != null ? new GeocodingResponse { Results = new[] { chosen } } : null;
            }
            catch (Exception ex)
            {
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
                // System.Diagnostics.Debug.WriteLine($"Lỗi lấy địa điểm hiện tại: {ex.Message}");
                return null;
            }
        }

        // Reverse geocoding qua OpenWeather Geo API (lấy state/tỉnh nếu có)
        public static async Task<GeocodingResult> ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                string url = $"http://api.openweathermap.org/geo/1.0/reverse?lat={lat}&lon={lon}&limit=1&appid={CauHinhApi.GEOCODING_API_KEY}";
                string json = await httpClient.GetStringAsync(url);
                var arr = JsonConvert.DeserializeObject<List<Newtonsoft.Json.Linq.JObject>>(json);
                if (arr != null && arr.Count > 0)
                {
                    var obj = arr[0];
                    string name = obj.Value<string>("name") ?? string.Empty;
                    string state = obj.Value<string>("state") ?? string.Empty;
                    string country = obj.Value<string>("country") ?? string.Empty;

                    string combined = string.IsNullOrWhiteSpace(state) ? name : $"{name}, {state}";
                    return new GeocodingResult
                    {
                        Name = string.IsNullOrWhiteSpace(combined) ? name : combined,
                        Country = country,
                        Lat = lat,
                        Lon = lon
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                // System.Diagnostics.Debug.WriteLine($"Lỗi reverse geocoding: {ex.Message}");
                return null;
            }
        }
    }

    public static class TemperatureConverter
    {
        public static double ToCelsius(double kelvin) => kelvin - 273.15;
        public static double ToFahrenheit(double kelvin) => (kelvin - 273.15) * 9.0 / 5.0 + 32.0;
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
        [JsonProperty("lat")]
        public double Lat { get; set; }
        [JsonProperty("lon")]
        public double Lon { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
        [JsonProperty("timezone_offset")]
        public int TimezoneOffset { get; set; }
        [JsonProperty("current")]
        public CurrentWeather Current { get; set; }
        [JsonProperty("hourly")]
        public HourlyWeather[] Hourly { get; set; }
        [JsonProperty("daily")]
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
        [JsonProperty("dt")]
        public long Dt { get; set; }
        [JsonProperty("temp")]
        public double Temp { get; set; }
        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }
        [JsonProperty("pressure")]
        public int Pressure { get; set; }
        [JsonProperty("humidity")]
        public int Humidity { get; set; }
        [JsonProperty("dew_point")]
        public double DewPoint { get; set; }
        [JsonProperty("uvi")]
        public double Uvi { get; set; }
        [JsonProperty("clouds")]
        public int Clouds { get; set; }
        [JsonProperty("visibility")]
        public int Visibility { get; set; }
        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }
        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }
        [JsonProperty("weather")]
        public WeatherCondition[] Weather { get; set; }
        [JsonProperty("rain")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonProperty("snow")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class HourlyWeather
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }
        [JsonProperty("temp")]
        public double Temp { get; set; }
        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }
        [JsonProperty("pressure")]
        public int Pressure { get; set; }
        [JsonProperty("humidity")]
        public int Humidity { get; set; }
        [JsonProperty("dew_point")]
        public double DewPoint { get; set; }
        [JsonProperty("uvi")]
        public double Uvi { get; set; }
        [JsonProperty("clouds")]
        public int Clouds { get; set; }
        [JsonProperty("visibility")]
        public int Visibility { get; set; }
        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }
        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }
        [JsonProperty("weather")]
        public WeatherCondition[] Weather { get; set; }
        [JsonProperty("rain")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonProperty("snow")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class DailyWeather
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }
        [JsonProperty("sunrise")]
        public long Sunrise { get; set; }
        [JsonProperty("sunset")]
        public long Sunset { get; set; }
        [JsonProperty("temp")]
        public Temperature Temp { get; set; }
        [JsonProperty("feels_like")]
        public Temperature FeelsLike { get; set; }
        [JsonProperty("pressure")]
        public int Pressure { get; set; }
        [JsonProperty("humidity")]
        public int Humidity { get; set; }
        [JsonProperty("dew_point")]
        public double DewPoint { get; set; }
        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }
        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }
        [JsonProperty("weather")]
        public WeatherCondition[] Weather { get; set; }
        [JsonProperty("clouds")]
        public int Clouds { get; set; }
        [JsonProperty("uvi")]
        public double Uvi { get; set; }
        [JsonProperty("rain")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Rain { get; set; }
        [JsonProperty("snow")]
        [JsonConverter(typeof(RainSnowConverter))]
        public object Snow { get; set; }
    }

    public class Temperature
    {
        [JsonProperty("day")]
        public double Day { get; set; }
        [JsonProperty("min")]
        public double Min { get; set; }
        [JsonProperty("max")]
        public double Max { get; set; }
        [JsonProperty("night")]
        public double Night { get; set; }
        [JsonProperty("eve")]
        public double Eve { get; set; }
        [JsonProperty("morn")]
        public double Morn { get; set; }
    }

    public class WeatherCondition
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("main")]
        public string Main { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("icon")]
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