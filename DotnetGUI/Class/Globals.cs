using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGUI.Class
{
    public static class Globals
    {
        public static string? ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string? TempPath = Path.GetTempPath();
        public static readonly string AppVersion = "Indev 1.2.0.0";
        public static string ConfigPath = $"{ExecutePath}\\config.json";
        public static JsonConfig.Config.Root? GlobanConfig = null;

        public static JsonConfig.DotnetDownloadIndex.Root DotnetIndex = null!;
        public static JsonConfig.DotnetVersionInfo.Root DotnetVersionInfo = null!;
    }
}
