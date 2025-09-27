using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace THOITIET
{
    public class DichVuThoiTiet
    {
        private readonly string API_KEY = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? "e3758b5bafed0fc3b4fa2cf4434f1dc1";
        private readonly HttpClient http = new HttpClient();
        private readonly string GOOGLE_API_KEY = Environment.GetEnvironmentVariable("GOOGLE_GEOCODING_API_KEY") ?? "";
        private readonly string GEOAPIFY_API_KEY = Environment.GetEnvironmentVariable("GEOAPIFY_API_KEY") ?? "30009cf7650b4e6aaad866fd961c2e4d";
        public async Task<List<ViTri>> TimDiaDiem(string tenXa)
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var ketQua = new List<ViTri>();

            // Ưu tiên Việt Nam: nếu người dùng không ghi quốc gia, thêm ",VN"
            var truyVanGoc = tenXa.Contains(",") ? tenXa : (tenXa + ",VN");
            var truyVanKhongDau = ChuanHoaKhongDau(truyVanGoc);

            // 0) Nếu có GEOAPIFY_API_KEY → thử Geoapify trước (bias Việt Nam)
            if (!string.IsNullOrWhiteSpace(GEOAPIFY_API_KEY))
            {
                var gpf = await TimDiaDiemQuaGeoapify(truyVanGoc);
                if (gpf.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
                {
                    gpf = await TimDiaDiemQuaGeoapify(truyVanKhongDau);
                }
                if (gpf.Count > 0) return gpf;
            }

            // Sau đó nếu có GOOGLE_API_KEY thì thử Google (bias Việt Nam)
            if (!string.IsNullOrWhiteSpace(GOOGLE_API_KEY))
            {
                var g = await TimDiaDiemQuaGoogle(truyVanGoc);
                if (g.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
                {
                    g = await TimDiaDiemQuaGoogle(truyVanKhongDau);
                }
                if (g.Count > 0) return g;
            }

            // Thử với chuỗi gốc
            var url1 = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(truyVanGoc)}&limit=5&appid={API_KEY}";
            var json = await http.GetStringAsync(url1);
            var arr = JArray.Parse(json);

            var kq = new List<ViTri>();
            foreach (var it in arr)
            {
                var ten = (string?)it["name"] ?? "";
                var tinh = (string?)it["state"] ?? "";
                var qg = (string?)it["country"] ?? "";
                var lat = (double?)it["lat"] ?? 0;
                var lon = (double?)it["lon"] ?? 0;

                var tenDayDu = string.Join(", ", new[] { ten, tinh, qg }.Where(s => !string.IsNullOrWhiteSpace(s)));
                kq.Add(new ViTri
                {
                    TenDayDu = tenDayDu,
                    ViDo = lat,
                    KinhDo = lon
                });
            }
            // Nếu không có kết quả, thử lại với chuỗi KHÔNG DẤU
            if (kq.Count == 0 && !string.Equals(truyVanGoc, truyVanKhongDau, StringComparison.Ordinal))
            {
                var url2 = $"https://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(truyVanKhongDau)}&limit=5&appid={API_KEY}";
                var json2 = await http.GetStringAsync(url2);
                var arr2 = JArray.Parse(json2);
                foreach (var it in arr2)
                {
                    var ten = (string?)it["name"] ?? "";
                    var tinh = (string?)it["state"] ?? "";
                    var qg = (string?)it["country"] ?? "";
                    var lat = (double?)it["lat"] ?? 0;
                    var lon = (double?)it["lon"] ?? 0;

                    var tenDayDu = string.Join(", ", new[] { ten, tinh, qg }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    kq.Add(new ViTri
                    {
                        TenDayDu = tenDayDu,
                        ViDo = lat,
                        KinhDo = lon
                    });
                }
            }
            // Ưu tiên kết quả ở Việt Nam, sắp xếp theo mức độ phù hợp (có state trước)
            var uuTienVn = kq.Where(v => v.TenDayDu.EndsWith(", VN") || v.TenDayDu.EndsWith(",VN"))
                              .OrderByDescending(v => v.TenDayDu.Split(',').Length)
                              .ToList();
            if (uuTienVn.Any()) return uuTienVn;
            return kq;
        }

        /// <summary>
        /// Gọi Google Geocoding API (bias Việt Nam) để tìm địa điểm, trả về danh sách vị trí.
        /// Cần đặt biến môi trường GOOGLE_GEOCODING_API_KEY.
        /// </summary>
        private async Task<List<ViTri>> TimDiaDiemQuaGoogle(string diaChi)
        {
            var ds = new List<ViTri>();
            if (string.IsNullOrWhiteSpace(GOOGLE_API_KEY)) return ds;

            // Bias Việt Nam: region=vn, components=country:VN, bounds theo bbox VN.
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
            catch
            {
                // bỏ qua lỗi Google
            }

            // Ưu tiên kết quả thuộc Việt Nam
            return ds.OrderByDescending(v => (v.TenDayDu?.Contains("Vietnam", StringComparison.OrdinalIgnoreCase) ?? false)
                                       || (v.TenDayDu?.Contains("Việt Nam", StringComparison.OrdinalIgnoreCase) ?? false))
                     .ThenBy(v => v.TenDayDu?.Length ?? int.MaxValue)
                     .ToList();
        }

        /// <summary>
        /// Gọi Geoapify Geocoding API (bias Việt Nam) để tìm địa điểm, trả về danh sách vị trí.
        /// Cần đặt biến môi trường GEOAPIFY_API_KEY.
        /// </summary>
        private async Task<List<ViTri>> TimDiaDiemQuaGeoapify(string diaChi)
        {
            var ds = new List<ViTri>();
            if (string.IsNullOrWhiteSpace(GEOAPIFY_API_KEY)) return ds;

            // Sử dụng filter countrycode:vn, trả về dạng JSON đơn giản
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
            catch
            {
                // bỏ qua lỗi Geoapify
            }

            return ds;
        }

        /// <summary>
        /// Chuẩn hóa chuỗi tiếng Việt thành không dấu để cải thiện khả năng tìm kiếm.
        /// </summary>
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

        /// <summary>
        /// Lấy thời tiết hiện tại với đơn vị metric/imperial
        /// </summary>
        public async Task<ThoiTietHienTai> LayThoiTietHienTai(double lat, double lon, string donVi = "metric")
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

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
        /// Lấy dự báo theo giờ (tối đa số giờ mong muốn). Sử dụng One Call API 3.0
        /// </summary>
        public async Task<List<DuBaoTheoGioItem>> LayDuBaoTheoGio(double lat, double lon, string donVi = "metric", int soGio = 24)
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&exclude=current,minutely,daily,alerts&units={donVi}&lang=en&appid={API_KEY}";
            var json = await http.GetStringAsync(url);
            var o = JObject.Parse(json);
            var hourly = o["hourly"] as JArray ?? new JArray();

            return hourly.Take(soGio).Select(h => new DuBaoTheoGioItem
            {
                ThoiGian = DateTimeOffset.FromUnixTimeSeconds((long?)h["dt"] ?? 0).ToLocalTime().DateTime,
                NhietDo = (double?)h["temp"] ?? 0,
                MaThoiTiet = (int?)(h["weather"]?.FirstOrDefault()? ["id"]) ?? 800,
                IconCode = ((string?)(h["weather"]?.FirstOrDefault()? ["icon"]))?.Trim() ?? ""
            }).ToList();
        }

        /// <summary>
        /// Lấy dự báo 5 ngày (mỗi 3 giờ), gom nhóm theo ngày để lấy cao/thấp
        /// </summary>
        public async Task<List<DuBaoNgayItem>> LayDuBao5Ngay(double lat, double lon, string donVi = "metric")
        {
            if (string.IsNullOrWhiteSpace(API_KEY)) throw new InvalidOperationException("Vui lòng điền API_KEY OpenWeather.");

            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat.ToString(CultureInfo.InvariantCulture)}&lon={lon.ToString(CultureInfo.InvariantCulture)}&units={donVi}&lang=en&appid={API_KEY}";
                
                var json = await http.GetStringAsync(url);
                var o = JObject.Parse(json);
                var list = o["list"] as JArray ?? new JArray();


                var nhoms = list.Select(x => new
                {
                    ThoiGian = DateTime.Parse((string?)x["dt_txt"] ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    Nhiet = (double?)x["main"]? ["temp"] ?? 0,
                    Ma = (int?)(x["weather"]?.FirstOrDefault()? ["id"]) ?? 800,
                    MoTa = (string?)(x["weather"]?.FirstOrDefault()? ["description"]) ?? "",
                    Icon = (string?)(x["weather"]?.FirstOrDefault()? ["icon"]) ?? ""
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
                // System.Diagnostics.Debug.WriteLine($"Lỗi API 5 ngày: {ex.Message}");
                return new List<DuBaoNgayItem>();
            }
        }

    }

    #region Kiểu dữ liệu sử dụng trong Form

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


    #endregion
}

