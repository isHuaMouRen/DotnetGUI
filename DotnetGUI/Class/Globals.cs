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
        public static readonly string? ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string? TempPath = Path.GetTempPath();
        public static readonly string AppVersion = "Indev 1.9.0.0";
        public static readonly string ConfigPath = $"{ExecutePath}\\config.json";
        public static JsonConfig.Config.Root? GlobanConfig = null;

        public static JsonConfig.DotnetDownloadIndex.Root DotnetIndex = null!;
        public static JsonConfig.DotnetVersionInfo.Root DotnetVersionInfo = null!;

        public static readonly string UpdateRootUrl = "https://gitee.com/huamouren110/UpdateService/raw/main/DotnetGUI/";
    }
}
