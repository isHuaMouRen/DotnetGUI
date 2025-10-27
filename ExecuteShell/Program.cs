using System.Diagnostics;
using System.Reflection;

namespace ExecuteShell
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Initialize();

        }

        public static void Initialize()
        {
            try
            {
                string ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                string BinPath = Path.Combine(ExecutePath, "bin");
                string ProgramPath = Path.Combine(BinPath, "DotnetGUI.exe");

                Process.Start(new ProcessStartInfo
                {
                    FileName = ProgramPath,
                    Arguments = "-shell",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"在尝试启动主程序时发生错误\n\n{ex}", $"{Path.GetFileName(Assembly.GetExecutingAssembly().Location)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}