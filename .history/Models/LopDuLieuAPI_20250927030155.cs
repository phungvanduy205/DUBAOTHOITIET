using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace THOITIET.Models
{
    /// <summary>
    /// Các class model nhận dữ liệu từ API OpenWeather
    /// </summary>
    
    public class ViTri
    {
        public string TenDayDu { get; set; } = "";
        public double ViDo { get; set; }
        public double KinhDo { get; set; }
    }

    public class ThoiTietHienTai
    {
        public double NhietDo { get; set; }
        public double NhietDoCamGiac { get; set; }
        public int DoAm { get; set; }
        public int ApSuat { get; set; }
        public double TocDoGio { get; set; }
        public long TamNhin { get; set; }
        public long MatTroiMoc { get; set; }
        public long MatTroiLan { get; set; }
        public int MaThoiTiet { get; set; }
        public string? TrangThaiMoTa { get; set; }
        public string IconCode { get; set; } = "";
    }

    public class DuBaoTheoGioItem
    {
        public DateTime ThoiGian { get; set; }
        public double NhietDo { get; set; }
        public int MaThoiTiet { get; set; }
        public string IconCode { get; set; } = "";
    }

    public class DuBaoNgayItem
    {
        public DateTime Ngay { get; set; }
        public double NhietDoCao { get; set; }
        public double NhietDoThap { get; set; }
        public int MaThoiTiet { get; set; }
        public string TrangThaiMoTa { get; set; } = "";
        public string IconCode { get; set; } = "";
    }

    // One Call API 3.0 Response
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

    // Geocoding API
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

    // Saved Location
    public class SavedLocation
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public SavedLocation(string name, double lat, double lon)
        {
            Name = name;
            Lat = lat;
            Lon = lon;
        }
    }
}