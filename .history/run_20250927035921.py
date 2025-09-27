# save as: download_openweather_icons.py
import os, time, urllib.request

# Thư mục lưu icon
RES_DIR = os.path.join(os.getcwd(), "Resources")
os.makedirs(RES_DIR, exist_ok=True)

# Map mã icon OpenWeather -> tên file bạn đang dùng trong code
ICON_MAP = {
    # Trời quang
    "01d": "troi_quang_ngay.png",
    "01n": "troi_quang_dem.png",
    # Ít mây
    "02d": "it_may_ngay.png",
    "02n": "it_may_dem.png",
    # Mây rải rác
    "03d": "may_rac_rac_ngay.png",
    "03n": "may_rac_rac_dem.png",
    # Mây dày
    "04d": "may_day_ngay.png",
    "04n": "may_day_dem.png",
    # Mưa rào
    "09d": "mua_rao_ngay.png",
    "09n": "mua_rao_dem.png",
    # Mưa
    "10d": "mua_ngay.png",
    "10n": "mua_dem.png",
    # Giông bão
    "11d": "giong_bao_ngay.png",
    "11n": "giong_bao_dem.png",
    # Tuyết
    "13d": "tuyet_ngay.png",
    "13n": "tuyet_dem.png",
    # Sương mù
    "50d": "suong_mu_ngay.png",
    "50n": "suong_mu_dem.png",
}

BASE_URL = "https://openweathermap.org/img/wn/{code}@2x.png"

def download(url, dest):
    try:
        urllib.request.urlretrieve(url, dest)
        print(f"Saved: {os.path.basename(dest)}")
    except Exception as ex:
        print(f"Fail: {os.path.basename(dest)} -> {ex}")

def main():
    print(f"Saving to: {RES_DIR}")
    for code, filename in ICON_MAP.items():
        dest = os.path.join(RES_DIR, filename)
        if os.path.exists(dest):
            print(f"Skip (exists): {filename}")
            continue
        url = BASE_URL.format(code=code)
        download(url, dest)
        time.sleep(0.2)  # tránh spam

if __name__ == "__main__":
    main()