using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

private static async Task DownloadAllOpenWeatherIcons(){
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string resourcesPath = Path.Combine(baseDir, "Resources");
    Directory.CreateDirectory(resourcesPath);

    var icons = new Dictionary<string, string>
    {
        { "01d", "troi_quang_ngay.png" },
        { "01n", "troi_quang_dem.png" },
        { "02d", "it_may_ngay.png" },
        { "02n", "it_may_dem.png" },
        { "03d", "may_rac_rac_ngay.png" },
        { "03n", "may_rac_rac_dem.png" },
        { "04d", "may_day_ngay.png" },
        { "04n", "may_day_dem.png" },
        { "09d", "mua_rao_ngay.png" },
        { "09n", "mua_rao_dem.png" },
        { "10d", "mua_ngay.png" },
        { "10n", "mua_dem.png" },
        { "11d", "giong_bao_ngay.png" },
        { "11n", "giong_bao_dem.png" },
        { "13d", "tuyet_ngay.png" },
        { "13n", "tuyet_dem.png" },
        { "50d", "suong_mu_ngay.png" },
        { "50n", "suong_mu_dem.png" }
    };

    using var http = new HttpClient();

    foreach (var kv in icons)
    {
        string url = $"https://openweathermap.org/img/wn/{kv.Key}@2x.png";
        string filePath = Path.Combine(resourcesPath, kv.Value);

        if (File.Exists(filePath)) continue;

        try
        {
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(filePath, bytes);
            System.Diagnostics.Debug.WriteLine($"Saved: {kv.Value}");
            await Task.Delay(200); // tránh spam
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fail {kv.Value}: {ex.Message}");
        }
    }
}