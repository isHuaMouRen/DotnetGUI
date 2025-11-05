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
using HuaZi.Library.Json;
using HuaZi.Library.Logger;
using System.Net.Http;
using DotnetGUI.Util;
using HuaZi.Library.Downloader;
using System.Diagnostics;
using HuaZi.Library.Hash;

namespace DotnetGUI.Class
{
    public static class Globals
    {
        #region Var
        public static readonly string? ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string? TempPath = Path.GetTempPath();
        public static readonly string AppVersion = "Alpha 1.0.0.0";
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

        public static async Task CheckUpdate(Action<string> action)
        {
            Globals.logger.Info($"开始检查更新...");

            using (var client = new HttpClient())
            {
                string result = await client.GetStringAsync($"{Globals.UpdateRootUrl}latest.json");
                JsonConfig.UpdateIndex.Root updateIndex = Json.ReadJson<JsonConfig.UpdateIndex.Root>(result);

                Globals.logger.Info($"获得更新索引: {result}");
                Globals.logger.Info($"最新版本: {updateIndex.latest_version}  当前版本: {Globals.AppVersion}");

                if (updateIndex.latest_version == Globals.AppVersion)
                {
                    Globals.logger.Info($"无可用更新");
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "无可用更新",
                        Content = $"您使用的是最新的 {updateIndex.latest_version} 版本, 无需更新",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
                }
                else
                {
                    Globals.logger.Info($"发现可用更新");
                    bool isUpdate = false;
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "发现可用更新",
                        Content = $"现在可以更新到 {updateIndex.latest_version}\n\n是否更新?",
                        PrimaryButtonText = "更新",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() => isUpdate = true));

                    if (isUpdate)
                    {
                        string savePath = System.IO.Path.Combine(Globals.TempPath!, "update.zip");
                        Globals.logger.Info($"开始更新，保存位置: {savePath}");
                        if (File.Exists(savePath))
                            File.Delete(savePath);

                        Globals.logger.Info($"开始下载任务...");
                        await Downloader.DownloadFileAsync($"{Globals.UpdateRootUrl}update.zip", savePath, ((pgs) => action($"更新文件下载中 {Math.Round(pgs, 2)}% ...")), new CancellationToken());
                        Globals.logger.Info($"下载任务结束");

                        string fileHash = await Hash.FileSHA256Async(savePath);
                        logger.Info($"文件SHA256: {fileHash}\n服务器发送的Hash: {updateIndex}");
                        if (!string.Equals(fileHash,updateIndex.hash,StringComparison.OrdinalIgnoreCase))
                        {
                            bool isReturn = false;
                            await DialogManager.ShowDialogAsync(new ContentDialog
                            {
                                Title = $"警告",
                                Content = $"文件效验不通过，哈希值与服务器上的不一致，下载到的文件可能已经损坏。",
                                PrimaryButtonText = "取消更新",
                                SecondaryButtonText = "忽略",
                                DefaultButton = ContentDialogButton.Secondary
                            }, (() => isReturn = true));
                            if (isReturn)
                                return;
                        }

                        action("下载更新文件成功, 即将重启...");
                        await Task.Delay(2000);

                        Globals.logger.Info($"调用更新服务...");

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(Globals.ExecutePath!, "UpdateService.exe"),
                            Arguments = $"-updatefile \"{savePath}\"",
                            UseShellExecute = true
                        });
                        Globals.logger.Info($"程序退出(ExitCode: 0)");
                        Environment.Exit(0);
                    }
                }
            }

            Globals.logger.Info($"检查更新结束");
        }
        #endregion

    }
}
