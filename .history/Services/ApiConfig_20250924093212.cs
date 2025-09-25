namespace THOITIET.Services
{
    /// <summary>
    /// Cấu hình API keys và URLs
    /// </summary>
    public static class ApiConfig
    {
        public static string API_KEY
        {
            get
            {
                // Thay thế bằng API key thực của bạn
                return "your_api_key_here";
            }
        }

        public const string BASE_URL = "https://api.openweathermap.org/data/3.0/onecall";
        public const string GEOCODING_URL = "https://api.openweathermap.org/geo/1.0/direct";
        public const string WINDY_API_KEY = "NI44O5nRjXST4TKiDk0x7hzaWnpHHiCP";
    }
}