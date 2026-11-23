using System;
using System.Windows.Forms;
using LootSearch.UI;

namespace LootSearch
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            }
            catch
            {
                // 老系统忽略即可
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
