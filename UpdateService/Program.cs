using System.Reflection;
using System.IO.Compression;
using System.Diagnostics;

namespace UpdateService
{
    internal class Program
    {
        static void Main(string[] args)
        {
			try
			{
                string updateFilePath = null!;
                string executePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-updatefile" && i + 1 < args.Length) 
                        updateFilePath = args[i + 1];
                }
                if (string.IsNullOrEmpty(updateFilePath))
                    throw new Exception("请传入参数-updatefile");
                if (!File.Exists(updateFilePath))
                    throw new Exception($"更新包不存在: {updateFilePath}");

                Console.WriteLine($"更新文件路径: {updateFilePath}\n程序执行路径: {executePath}");
                Console.WriteLine("等待主程序完全退出...");

                if (!WaitForProcessExit("DotnetGUI", 30))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("警告: 主程序在 30 秒内仍未退出, 文件替换可能失败！");
                    Console.ResetColor();
                }

                Console.WriteLine("开始解压文件...");
                ZipFile.ExtractToDirectory(updateFilePath, executePath, true);
                Console.WriteLine("解压完毕, 启动主程序...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(executePath, "DotnetGUI.exe"),
                    Arguments = $"-shell -update",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });

                return;
			}
			catch (Exception ex)
			{
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"发生错误: {ex.Message}\n  详细信息: {ex}");
                Console.ResetColor();
                Console.Write("程序异常终止，更新失败，按任意键退出...");
                Console.ReadKey();
			}
        }

        static bool WaitForProcessExit(string processName, int timeoutSeconds = 30)
        {
            var processes = Process.GetProcessesByName(processName);
            var stopwatch = Stopwatch.StartNew();

            while (processes.Length > 0 && stopwatch.Elapsed.TotalSeconds < timeoutSeconds)
            {
                Thread.Sleep(500);
                processes = Process.GetProcessesByName(processName);
            }

            return processes.Length == 0;
        }
    }
}
