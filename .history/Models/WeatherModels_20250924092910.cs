using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace THOITIET.Models
{
    /// <summary>
    /// Class lưu trữ thông tin địa điểm đã lưu
    /// </summary>
    public class SavedLocation
    {
        public string Name { get; set; } = "";
        public double Lat { get; set; }
        public double Lon { get; set; }
        
        public SavedLocation(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }

    /// <summary>
    /// Class để quản lý địa điểm yêu thích
    /// </summary>
    public class FavoriteLocation
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Dữ liệu vị trí từ IP
    /// </summary>
    public class IpLocationData
    {
        [JsonProperty("status")]
        public string Status { get; set; } = "";

        [JsonProperty("city")]
        public string City { get; set; } = "";

        [JsonProperty("country")]
        public string Country { get; set; } = "";

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }
    }

    /// <summary>
    /// Kết quả geocoding
    /// </summary>
    public class GeocodingResult
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("country")]
        public string Country { get; set; } = "";

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("state")]
        public string State { get; set; } = "";
    }

    /// <summary>
    /// Response geocoding
    /// </summary>
    public class GeocodingResponse
    {
        [JsonProperty("results")]
        public GeocodingResult[] Results { get; set; } = new GeocodingResult[0];
    }

    /// <summary>
    /// Thông tin thời tiết hiện tại
    /// </summary>
    public class CurrentWeather
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }

        [JsonProperty("uvi")]
        public double Uvi { get; set; }

        [JsonProperty("visibility")]
        public int Visibility { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }

        [JsonProperty("weather")]
        public WeatherInfo[] Weather { get; set; } = new WeatherInfo[0];

        [JsonProperty("main")]
        public MainInfo Main { get; set; } = new MainInfo();

        [JsonProperty("wind")]
        public WindInfo Wind { get; set; } = new WindInfo();

        [JsonProperty("sys")]
        public SysInfo Sys { get; set; } = new SysInfo();
    }

    /// <summary>
    /// Thông tin thời tiết theo giờ
    /// </summary>
    public class HourlyWeather
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }

        [JsonProperty("uvi")]
        public double Uvi { get; set; }

        [JsonProperty("visibility")]
        public int Visibility { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }

        [JsonProperty("weather")]
        public WeatherInfo[] Weather { get; set; } = new WeatherInfo[0];

        [JsonProperty("rain")]
        public RainInfo Rain { get; set; } = new RainInfo();

        [JsonProperty("snow")]
        public SnowInfo Snow { get; set; } = new SnowInfo();
    }

    /// <summary>
    /// Thông tin thời tiết theo ngày
    /// </summary>
    public class DailyWeather
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("temp")]
        public TempInfo Temp { get; set; } = new TempInfo();

        [JsonProperty("feels_like")]
        public FeelsLikeInfo FeelsLike { get; set; } = new FeelsLikeInfo();

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }

        [JsonProperty("uvi")]
        public double Uvi { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_deg")]
        public int WindDeg { get; set; }

        [JsonProperty("weather")]
        public WeatherInfo[] Weather { get; set; } = new WeatherInfo[0];

        [JsonProperty("rain")]
        public RainInfo Rain { get; set; } = new RainInfo();

        [JsonProperty("snow")]
        public SnowInfo Snow { get; set; } = new SnowInfo();
    }

    /// <summary>
    /// Thông tin thời tiết chi tiết
    /// </summary>
    public class WeatherInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("main")]
        public string Main { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("icon")]
        public string Icon { get; set; } = "";
    }

    /// <summary>
    /// Thông tin chính
    /// </summary>
    public class MainInfo
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }

        [JsonProperty("feels_like")]
        public double FeelsLike { get; set; }

        [JsonProperty("humidity")]
        public int Humidity { get; set; }

        [JsonProperty("pressure")]
        public int Pressure { get; set; }
    }

    /// <summary>
    /// Thông tin gió
    /// </summary>
    public class WindInfo
    {
        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("deg")]
        public int Deg { get; set; }
    }

    /// <summary>
    /// Thông tin hệ thống
    /// </summary>
    public class SysInfo
    {
        [JsonProperty("sunrise")]
        public long Sunrise { get; set; }

        [JsonProperty("sunset")]
        public long Sunset { get; set; }
    }

    /// <summary>
    /// Thông tin nhiệt độ
    /// </summary>
    public class TempInfo
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

    /// <summary>
    /// Thông tin cảm giác như
    /// </summary>
    public class FeelsLikeInfo
    {
        [JsonProperty("day")]
        public double Day { get; set; }

        [JsonProperty("night")]
        public double Night { get; set; }

        [JsonProperty("eve")]
        public double Eve { get; set; }

        [JsonProperty("morn")]
        public double Morn { get; set; }
    }

    /// <summary>
    /// Thông tin mưa
    /// </summary>
    public class RainInfo
    {
        [JsonProperty("1h")]
        public double OneHour { get; set; }
    }

    /// <summary>
    /// Thông tin tuyết
    /// </summary>
    public class SnowInfo
    {
        [JsonProperty("1h")]
        public double OneHour { get; set; }
    }

    /// <summary>
    /// Response chính từ One Call API
    /// </summary>
    public class OneCallResponse
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; } = "";

        [JsonProperty("current")]
        public CurrentWeather Current { get; set; } = new CurrentWeather();

        [JsonProperty("hourly")]
        public HourlyWeather[] Hourly { get; set; } = new HourlyWeather[0];

        [JsonProperty("daily")]
        public DailyWeather[] Daily { get; set; } = new DailyWeather[0];
    }
}