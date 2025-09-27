using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using THOITIET.Models;

namespace THOITIET.Services
{
    /// <summary>
    /// Xử lý tìm kiếm địa điểm và sự kiện nút tìm kiếm
    /// </summary>
    public class XuLyTimKiem
    {
        private readonly DichVuThoiTiet dichVuThoiTiet;
        private readonly QuanLyDiaDiem quanLyDiaDiem;
        private readonly QuanLyThoiTiet quanLyThoiTiet;

        public XuLyTimKiem()
        {
            dichVuThoiTiet = new DichVuThoiTiet();
            quanLyDiaDiem = new QuanLyDiaDiem();
            quanLyThoiTiet = new QuanLyThoiTiet();
        }

        /// <summary>
        /// Tìm kiếm địa điểm và lấy dữ liệu thời tiết
        /// </summary>
        public async Task<KetQuaTimKiem> TimKiemDiaDiem(string tenDiaDiem)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenDiaDiem))
                {
                    return new KetQuaTimKiem
                    {
                        ThanhCong = false,
                        ThongBaoLoi = "Vui lòng nhập tên địa điểm"
                    };
                }

                // Tìm địa điểm
                var danhSachViTri = await dichVuThoiTiet.TimDiaDiem(tenDiaDiem);
                if (danhSachViTri.Count == 0)
                {
                    return new KetQuaTimKiem
                    {
                        ThanhCong = false,
                        ThongBaoLoi = "Không tìm thấy địa điểm. Vui lòng thử lại với tên địa điểm khác."
                    };
                }

                var viTri = danhSachViTri.First();
                
                // Lấy dữ liệu thời tiết đầy đủ
                var duLieuThoiTiet = await dichVuThoiTiet.LayDuLieuDayDu(viTri.ViDo, viTri.KinhDo);
                if (duLieuThoiTiet == null)
                {
                    return new KetQuaTimKiem
                    {
                        ThanhCong = false,
                        ThongBaoLoi = "Không thể lấy dữ liệu thời tiết. Vui lòng kiểm tra kết nối mạng."
                    };
                }

                // Lưu địa điểm vào DB
                await quanLyDiaDiem.LuuDiaDiem(viTri.TenDayDu, viTri.ViDo, viTri.KinhDo);

                // Lưu dữ liệu thời tiết vào DB
                await quanLyThoiTiet.LuuDuLieuThoiTiet(viTri.TenDayDu, viTri.ViDo, viTri.KinhDo, duLieuThoiTiet);

                return new KetQuaTimKiem
                {
                    ThanhCong = true,
                    ViTri = viTri,
                    DuLieuThoiTiet = duLieuThoiTiet,
                    ThongBaoLoi = ""
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tìm kiếm địa điểm: {ex.Message}");
                return new KetQuaTimKiem
                {
                    ThanhCong = false,
                    ThongBaoLoi = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy địa điểm hiện tại theo IP
        /// </summary>
        public async Task<KetQuaTimKiem> LayDiaDiemHienTai()
        {
            try
            {
                // Sử dụng IP để lấy vị trí (có thể implement sau)
                // Tạm thời trả về Hà Nội
                var viTri = new ViTri
                {
                    TenDayDu = "Hà Nội, VN",
                    ViDo = 21.0285,
                    KinhDo = 105.8542
                };

                var duLieuThoiTiet = await dichVuThoiTiet.LayDuLieuDayDu(viTri.ViDo, viTri.KinhDo);
                if (duLieuThoiTiet == null)
                {
                    return new KetQuaTimKiem
                    {
                        ThanhCong = false,
                        ThongBaoLoi = "Không thể lấy dữ liệu thời tiết cho vị trí hiện tại."
                    };
                }

                return new KetQuaTimKiem
                {
                    ThanhCong = true,
                    ViTri = viTri,
                    DuLieuThoiTiet = duLieuThoiTiet,
                    ThongBaoLoi = ""
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy địa điểm hiện tại: {ex.Message}");
                return new KetQuaTimKiem
                {
                    ThanhCong = false,
                    ThongBaoLoi = $"Có lỗi xảy ra: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách gợi ý địa điểm
        /// </summary>
        public async Task<List<string>> LayGoiYDiaDiem(string tuKhoa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tuKhoa) || tuKhoa.Length < 2)
                    return new List<string>();

                // Lấy từ danh sách đã lưu trước
                var danhSachDaLuu = await quanLyDiaDiem.LayDanhSachDiaDiem();
                var goiYTuDaLuu = danhSachDaLuu
                    .Where(d => d.Name.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.Name)
                    .Take(5)
                    .ToList();

                // Nếu chưa đủ, tìm kiếm qua API
                if (goiYTuDaLuu.Count < 5)
                {
                    var ketQuaTimKiem = await dichVuThoiTiet.TimDiaDiem(tuKhoa);
                    var goiYTuAPI = ketQuaTimKiem
                        .Select(v => v.TenDayDu)
                        .Where(t => !goiYTuDaLuu.Contains(t))
                        .Take(5 - goiYTuDaLuu.Count)
                        .ToList();

                    goiYTuDaLuu.AddRange(goiYTuAPI);
                }

                return goiYTuDaLuu;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi lấy gợi ý: {ex.Message}");
                return new List<string>();
            }
        }
    }

    /// <summary>
    /// Kết quả tìm kiếm địa điểm
    /// </summary>
    public class KetQuaTimKiem
    {
        public bool ThanhCong { get; set; }
        public string ThongBaoLoi { get; set; } = "";
        public ViTri? ViTri { get; set; }
        public OneCallResponse? DuLieuThoiTiet { get; set; }
    }
}