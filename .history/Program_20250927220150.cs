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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}