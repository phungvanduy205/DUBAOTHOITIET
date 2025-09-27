namespace THOITIET
{
    internal static class Program
    {
        /// <summary>
        ///  Điểm khởi đầu chính của ứng dụng.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Để tùy chỉnh cấu hình ứng dụng như thiết lập DPI cao hoặc font mặc định,
            // xem https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}