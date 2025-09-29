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
        public static string API_KEY => LayKhoaApi("OPENWEATHER_API_KEY") ?? throw new InvalidOperationException("Vui lòng cấu hình OPENWEATHER_API_KEY");
        public static string GEOCODING_API_KEY => LayKhoaApi("GEOCODING_API_KEY") ?? throw new InvalidOperationException("Vui lòng cấu hình GEOCODING_API_KEY");
        public static string GEOAPIFY_API_KEY => LayKhoaApi("GEOAPIFY_API_KEY") ?? throw new InvalidOperationException("Vui lòng cấu hình GEOAPIFY_API_KEY");
        public static string WINDY_API_KEY => LayKhoaApi("WINDY_API_KEY") ?? throw new InvalidOperationException("Vui lòng cấu hình WINDY_API_KEY");
        public static string WEATHERAPI_KEY => LayKhoaApi("WEATHERAPI_KEY") ?? throw new InvalidOperationException("Vui lòng cấu hình WEATHERAPI_KEY");
        public const string WEATHERAPI_BASE_URL = "https://api.weatherapi.com/v1";
        
        private static string LayKhoaApi(string tenKhoa)
        {
            try
            {
                // Tìm file .env
                var envPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
                    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", ".env"),
                    ".env"
                };

                foreach (var envFile in envPaths)
                {
                    if (File.Exists(envFile))
                    {
                        var lines = File.ReadAllLines(envFile);
                        foreach (var line in lines)
                        {
                            // Bỏ qua comment và dòng trống
                            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                                continue;
                                
                            if (line.StartsWith(tenKhoa + "="))
                            {
                                var key = line.Substring(tenKhoa.Length + 1).Trim();
                                if (!string.IsNullOrEmpty(key))
                                    return key;
                            }
                        }
                    }
                }
                
                // Nếu không tìm thấy, hiển thị hướng dẫn
                var message = $"Không tìm thấy cấu hình API Keys.\n\n" +
                             "Sử dụng file .env:\n" +
                             "1. Copy file env.example thành .env\n" +
                             "2. Điền API keys thực vào file .env\n" +
                             "3. Chạy lại ứng dụng\n\n" +
                             "Ví dụ nội dung file .env:\n" +
                             "OPENWEATHER_API_KEY=your_api_key_here\n" +
                             "GEOCODING_API_KEY=your_geocoding_key_here\n" +
                             "GEOAPIFY_API_KEY=your_geoapify_key_here\n" +
                             "WINDY_API_KEY=your_windy_key_here";
                
                System.Windows.Forms.MessageBox.Show(message, "Cấu hình API Keys", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Lỗi đọc file .env: {ex.Message}", "Gỡ lỗi", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return "your_api_key_here";
        }
        public const string BASE_URL = "https://api.openweathermap.org/data/3.0/onecall";
        public const string GEOCODING_URL = "https://api.openweathermap.org/geo/1.0/direct";
    }

    // Class để xử lý WeatherAPI.com
    public class WeatherApiService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<WeatherApiResponse> GetCurrentWeatherAsync(string cityName)
        {
            string url = $"{CauHinhApi.WEATHERAPI_BASE_URL}/current.json?key={CauHinhApi.WEATHERAPI_KEY}&q={Uri.EscapeDataString(cityName)}&aqi=no";
            string json = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherApiResponse>(json);
        }

        public static async Task<WeatherForecastResponse> GetForecastAsync(string cityName, int days = 5)
        {
            string url = $"{CauHinhApi.WEATHERAPI_BASE_URL}/forecast.json?key={CauHinhApi.WEATHERAPI_KEY}&q={Uri.EscapeDataString(cityName)}&days={days}&aqi=no&alerts=no";
            string json = await httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherForecastResponse>(json);
        }
    }

    // Class để xử lý API One Call 3.0 (Legacy)
    public class OpenWeatherApiService
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

    // WeatherAPI.com Models
    public class WeatherApiResponse
    {
        public Location Location { get; set; }
        public Current Current { get; set; }
    }

    public class WeatherForecastResponse
    {
        public Location Location { get; set; }
        public Forecast Forecast { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string TzId { get; set; }
        public long LocaltimeEpoch { get; set; }
        public string Localtime { get; set; }
    }

    public class Current
    {
        public long LastUpdatedEpoch { get; set; }
        public string LastUpdated { get; set; }
        public double TempC { get; set; }
        public double TempF { get; set; }
        public int IsDay { get; set; }
        public Condition Condition { get; set; }
        public double WindMph { get; set; }
        public double WindKph { get; set; }
        public int WindDegree { get; set; }
        public string WindDir { get; set; }
        public double PressureMb { get; set; }
        public double PressureIn { get; set; }
        public double PrecipMm { get; set; }
        public double PrecipIn { get; set; }
        public int Humidity { get; set; }
        public int Cloud { get; set; }
        public double FeelslikeC { get; set; }
        public double FeelslikeF { get; set; }
        public double VisKm { get; set; }
        public double VisMiles { get; set; }
        public double Uv { get; set; }
        public double GustMph { get; set; }
        public double GustKph { get; set; }
    }

    public class Condition
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public int Code { get; set; }
    }

    public class Forecast
    {
        public List<ForecastDay> Forecastday { get; set; }
    }

    public class ForecastDay
    {
        public string Date { get; set; }
        public long DateEpoch { get; set; }
        public Day Day { get; set; }
        public Astro Astro { get; set; }
        public List<Hour> Hour { get; set; }
    }

    public class Day
    {
        public double MaxtempC { get; set; }
        public double MaxtempF { get; set; }
        public double MintempC { get; set; }
        public double MintempF { get; set; }
        public double AvgtempC { get; set; }
        public double AvgtempF { get; set; }
        public double MaxwindMph { get; set; }
        public double MaxwindKph { get; set; }
        public double TotalprecipMm { get; set; }
        public double TotalprecipIn { get; set; }
        public double TotalsnowCm { get; set; }
        public double AvgvisKm { get; set; }
        public double AvgvisMiles { get; set; }
        public double Avghumidity { get; set; }
        public int DailyWillItRain { get; set; }
        public int DailyChanceOfRain { get; set; }
        public int DailyWillItSnow { get; set; }
        public int DailyChanceOfSnow { get; set; }
        public Condition Condition { get; set; }
        public double Uv { get; set; }
    }

    public class Astro
    {
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string Moonrise { get; set; }
        public string Moonset { get; set; }
        public string MoonPhase { get; set; }
        public string MoonIllumination { get; set; }
        public int IsMoonUp { get; set; }
        public int IsSunUp { get; set; }
    }

    public class Hour
    {
        public long TimeEpoch { get; set; }
        public string Time { get; set; }
        public double TempC { get; set; }
        public double TempF { get; set; }
        public int IsDay { get; set; }
        public Condition Condition { get; set; }
        public double WindMph { get; set; }
        public double WindKph { get; set; }
        public int WindDegree { get; set; }
        public string WindDir { get; set; }
        public double PressureMb { get; set; }
        public double PressureIn { get; set; }
        public double PrecipMm { get; set; }
        public double PrecipIn { get; set; }
        public int Humidity { get; set; }
        public int Cloud { get; set; }
        public double FeelslikeC { get; set; }
        public double FeelslikeF { get; set; }
        public double WindchillC { get; set; }
        public double WindchillF { get; set; }
        public double HeatindexC { get; set; }
        public double HeatindexF { get; set; }
        public double DewpointC { get; set; }
        public double DewpointF { get; set; }
        public int WillItRain { get; set; }
        public int ChanceOfRain { get; set; }
        public int WillItSnow { get; set; }
        public int ChanceOfSnow { get; set; }
        public double VisKm { get; set; }
        public double VisMiles { get; set; }
        public double GustMph { get; set; }
        public double GustKph { get; set; }
        public double Uv { get; set; }
    }
}