using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Dịch vụ gọi API OpenWeather - viết lại gọn gàng
    /// </summary>
    public class DichVuThoiTiet
    {
        private readonly string API_KEY = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        private readonly string GOOGLE_API_KEY = Environment.GetEnvironmentVariable("GOOGLE_GEOCODING_API_KEY") ?? "";
        private readonly string GEOAPIFY_API_KEY = Environment.GetEnvironmentVariable("GEOAPIFY_API_KEY") ?? "30009cf7650b4e6aaad866fd961c2e4d";
        private readonly HttpClient http = new HttpClient();

        /// <summary>
        /// Tìm địa điểm theo tên - ưu tiên Việt Nam
        /// </summary>
        public async Task<List<ViTri>> TimDiaDiem(string tenXa)
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) 
                throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var ketQua = new List<ViTri>();
            var truyVanGoc = tenXa.Contains(",") ? tenXa : (tenXa + ",VN");
            var truyVanKhongDau = ChuanHoaKhongDau(truyVanGoc);

            // Thử Geoapify trước (bias Việt Nam)
            if (!string.IsNullOrWhiteSpace(GEOAPIFY_API_KEY))
            {
                var gpf = await TimDiaDiemQuaGeoapify(truyVanGoc);
                if (gpf.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
                {
                    gpf = await TimDiaDiemQuaGeoapify(truyVanKhongDau);
                }
                if (gpf.Count > 0) return gpf;
            }

            // Thử Google (bias Việt Nam)
            if (!string.IsNullOrWhiteSpace(GOOGLE_API_KEY))
            {
                var g = await TimDiaDiemQuaGoogle(truyVanGoc);
                if (g.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
                {
                    g = await TimDiaDiemQuaGoogle(truyVanKhongDau);
                }
                if (g.Count > 0) return g;
            }

            // Fallback: OpenWeather Geocoding
            var kq = await TimDiaDiemQuaOpenWeather(truyVanGoc);
            if (kq.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
            {
                kq = await TimDiaDiemQuaOpenWeather(truyVanKhongDau);
            }

            // Ưu tiên kết quả ở Việt Nam
            var uuTienVn = kq.Where(v => v.TenDayDu.EndsWith(", VN") || v.TenDayDu.EndsWith(",VN"))
                              .OrderByDescending(v => v.TenDayDu.Split(',').Length)
                              .ToList();
            return uuTienVn.Any() ? uuTienVn : kq;
        }

        /// <summary>
        /// Lấy thời tiết hiện tại
        /// </summary>
        public async Task<ThoiTietHienTai> LayThoiTietHienTai(double lat, double lon, string donVi = "metric")
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) 
                throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&units={donVi}&lang=en&appid={API_KEY}";
            var json = await http.GetStringAsync(url);
            var o = JObject.Parse(json);

            var main = o["main"];
            var wind = o["wind"];
            var vis = (double?)o["visibility"] ?? 0;
            var sys = o["sys"];
            var weather = o["weather"]?.FirstOrDefault();

            return new ThoiTietHienTai
            {
                NhietDo = (double?)main?["temp"] ?? 0,
                NhietDoCamGiac = (double?)main?["feels_like"] ?? 0,
                DoAm = (int?)main?["humidity"] ?? 0,
                ApSuat = (int?)main?["pressure"] ?? 0,
                TocDoGio = (double?)wind?["speed"] ?? 0,
                TamNhin = (long)vis,
                MatTroiMoc = (long?)sys?["sunrise"] ?? 0,
                MatTroiLan = (long?)sys?["sunset"] ?? 0,
                MaThoiTiet = (int?)weather?["id"] ?? 800,
                TrangThaiMoTa = (string?)weather?["description"] ?? "",
                IconCode = ((string?)weather?["icon"] ?? "").Trim()
            };
        }

        /// <summary>
        /// Lấy dự báo theo giờ (24h)
        /// </summary>
        public async Task<List<DuBaoTheoGioItem>> LayDuBaoTheoGio(double lat, double lon, string donVi = "metric", int soGio = 24)
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) 
                throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&exclude=current,minutely,daily,alerts&units={donVi}&lang=en&appid={API_KEY}";
            var json = await http.GetStringAsync(url);
            var o = JObject.Parse(json);
            var hourly = o["hourly"] as JArray ?? new JArray();

            return hourly.Take(soGio).Select(h => new DuBaoTheoGioItem
            {
                ThoiGian = DateTimeOffset.FromUnixTimeSeconds((long?)h["dt"] ?? 0).ToLocalTime().DateTime,
                NhietDo = (double?)h["temp"] ?? 0,
                MaThoiTiet = (int?)(h["weather"]?.FirstOrDefault()?["id"]) ?? 800,
                IconCode = ((string?)(h["weather"]?.FirstOrDefault()?["icon"]))?.Trim() ?? ""
            }).ToList();
        }

        /// <summary>
        /// Lấy dự báo 5 ngày
        /// </summary>
        public async Task<List<DuBaoNgayItem>> LayDuBao5Ngay(double lat, double lon, string donVi = "metric")
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) 
                throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&units={donVi}&lang=en&appid={API_KEY}";
                var json = await http.GetStringAsync(url);
                var o = JObject.Parse(json);
                var list = o["list"] as JArray ?? new JArray();

                var nhoms = list.Select(x => new
                {
                    ThoiGian = DateTime.Parse((string?)x["dt_txt"] ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    Nhiet = (double?)x["main"]?["temp"] ?? 0,
                    Ma = (int?)(x["weather"]?.FirstOrDefault()?["id"]) ?? 800,
                    MoTa = (string?)(x["weather"]?.FirstOrDefault()?["description"]) ?? "",
                    Icon = (string?)(x["weather"]?.FirstOrDefault()?["icon"]) ?? ""
                })
                .GroupBy(x => x.ThoiGian.Date)
                .Take(5)
                .Select(g => new DuBaoNgayItem
                {
                    Ngay = g.Key,
                    NhietDoCao = g.Max(z => z.Nhiet),
                    NhietDoThap = g.Min(z => z.Nhiet),
                    MaThoiTiet = g.GroupBy(z => z.Ma).OrderByDescending(gg => gg.Count()).First().Key,
                    TrangThaiMoTa = g.GroupBy(z => z.MoTa).OrderByDescending(gg => gg.Count()).First().Key,
                    IconCode = (g.GroupBy(z => z.Icon).OrderByDescending(gg => gg.Count()).First().Key ?? "").Trim()
                }).ToList();

                return nhoms;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi API 5 ngày: {ex.Message}");
                return new List<DuBaoNgayItem>();
            }
        }

        /// <summary>
        /// Lấy dữ liệu One Call API 3.0 (đầy đủ)
        /// </summary>
        public async Task<OneCallResponse> LayDuLieuDayDu(double lat, double lon, string donVi = "metric")
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) 
                throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&exclude=minutely,alerts&units={donVi}&lang=en&appid={API_KEY}";
            var json = await http.GetStringAsync(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OneCallResponse>(json);
        }

        #region Private Methods

        private async Task<List<ViTri>> TimDiaDiemQuaOpenWeather(string diaChi)
        {
            var url = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(diaChi)}&limit=5&appid={API_KEY}";
            var json = await http.GetStringAsync(url);
            var arr = JArray.Parse(json);

            return arr.Select(it => new ViTri
            {
                TenDayDu = string.Join(", ", new[] { (string?)it["name"], (string?)it["state"], (string?)it["country"] }.Where(s => !string.IsNullOrWhiteSpace(s))),
                ViDo = (double?)it["lat"] ?? 0,
                KinhDo = (double?)it["lon"] ?? 0
            }).ToList();
        }

        private async Task<List<ViTri>> TimDiaDiemQuaGoogle(string diaChi)
        {
            var ds = new List<ViTri>();
            if (string.IsNullOrWhiteSpace(GOOGLE_API_KEY)) return ds;

            var address = Uri.EscapeDataString(diaChi);
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&region=vn&components=country:VN&bounds=8.18,102.14|23.39,109.47&key={GOOGLE_API_KEY}";
            
            try
            {
                var json = await http.GetStringAsync(url);
                var o = JObject.Parse(json);
                var results = o["results"] as JArray ?? new JArray();
                
                foreach (var r in results)
                {
                    var loc = r["geometry"]?["location"];
                    if (loc == null) continue;
                    
                    var lat = (double?)loc["lat"] ?? 0;
                    var lon = (double?)loc["lng"] ?? 0;
                    var formatted = (string?)r["formatted_address"] ?? "";
                    
                    if (Math.Abs(lat) < double.Epsilon && Math.Abs(lon) < double.Epsilon) continue;
                    
                    ds.Add(new ViTri
                    {
                        TenDayDu = formatted,
                        ViDo = lat,
                        KinhDo = lon
                    });
                }
            }
            catch { }

            return ds.OrderByDescending(v => (v.TenDayDu?.Contains("Vietnam", StringComparison.OrdinalIgnoreCase) ?? false)
                                       || (v.TenDayDu?.Contains("Việt Nam", StringComparison.OrdinalIgnoreCase) ?? false))
                     .ThenBy(v => v.TenDayDu?.Length ?? int.MaxValue)
                     .ToList();
        }

        private async Task<List<ViTri>> TimDiaDiemQuaGeoapify(string diaChi)
        {
            var ds = new List<ViTri>();
            if (string.IsNullOrWhiteSpace(GEOAPIFY_API_KEY)) return ds;

            var text = Uri.EscapeDataString(diaChi);
            var url = $"https://api.geoapify.com/v1/geocode/search?text={text}&filter=countrycode:vn&lang=vi&format=json&apiKey={GEOAPIFY_API_KEY}";
            
            try
            {
                var json = await http.GetStringAsync(url);
                var o = JObject.Parse(json);
                var results = o["results"] as JArray ?? new JArray();
                
                foreach (var r in results)
                {
                    var lat = (double?)r["lat"] ?? 0;
                    var lon = (double?)r["lon"] ?? 0;
                    var formatted = (string?)r["formatted"] ?? "";
                    
                    if (Math.Abs(lat) < double.Epsilon && Math.Abs(lon) < double.Epsilon) continue;
                    
                    ds.Add(new ViTri
                    {
                        TenDayDu = formatted,
                        ViDo = lat,
                        KinhDo = lon
                    });
                }
            }
            catch { }

            return ds;
        }

        private static string ChuanHoaKhongDau(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var s = input.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }
            var res = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            res = res.Replace('đ', 'd').Replace('Đ', 'D');
            return res;
        }

        #endregion
    }
}