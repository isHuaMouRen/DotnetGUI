using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToolLib.Library.JsonLib;
using ToolLib.Library.LogLib;

namespace DotnetGUI.Class
{
    public static class Globals
    {
        #region Var
        public static readonly string? ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string? TempPath = Path.GetTempPath();
        public static readonly string AppVersion = "Indev 2.3.0.0";
        public static readonly string ConfigPath = $"{ExecutePath}\\config.json";
        public static JsonConfig.Config.Root? GlobalConfig = null;

        public static JsonConfig.DotnetDownloadIndex.Root DotnetIndex = null!;
        public static JsonConfig.DotnetVersionInfo.Root DotnetVersionInfo = null!;

        public static readonly string UpdateRootUrl = "https://gitee.com/huamouren110/UpdateService/raw/main/DotnetGUI/";
        #endregion

        #region Obj
        public static Logger logger = new Logger(Path.Combine(ExecutePath!, "log"));
        #endregion

        #region Func
        /// <summary>
        /// 保存所有配置
        /// </summary>
        public static void SaveAllConfig()
        {
            Json.WriteJson(ConfigPath, GlobalConfig);
        }

        /// <summary>
        /// 设置主题
        /// </summary>
        public static void SetTheme(NavigationView navView)
        {
            logger.Info($"设置主题: {GlobalConfig!.UIConfig!.Theme}");
            if (GlobalConfig!.UIConfig!.Theme == "Light")
            {                
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Light;
                navView.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            else if (GlobalConfig!.UIConfig!.Theme == "Dark")
            {
                ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark;
                navView.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            }
        }
        #endregion

    }
}
