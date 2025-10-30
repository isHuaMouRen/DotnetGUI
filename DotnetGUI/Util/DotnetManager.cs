using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGUI.Util
{
    public class DotnetManager
    {
        /// <summary>
        /// 检测状态
        /// </summary>
        /// <returns>正常返回版本号。如不存在则返回null</returns>
        public static async Task<string> CheckState()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await Task.Run(() => process.WaitForExit());

            if (!string.IsNullOrEmpty(errorTask.Result))
                return null!;
            else
                return outputTask.Result;
        }
    }
}
