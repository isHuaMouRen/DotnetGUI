using DotnetGUI.Class;
using DotnetGUI.Util;
using Markdig;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToolLib.Library.DownloaderLib;
using ToolLib.Library.JsonLib;

namespace DotnetGUI.Page
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : ModernWpf.Controls.Page
    {
        #region Obj
        //public WebView2 webView = new WebView2 { Margin = new Thickness(0, 0, 0, 10) };
        #endregion

        #region Var
        #endregion

        #region Func
        public void Initialize()
        {
            try
            {              
                //workDirectory
                textBox_WorkingPath.Text = Globals.GlobanConfig.DotnetConfig.WorkingDirectory;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"初始化 {typeof(SettingsPage).Name} 时发生错误", ex);
            }
        }
        
        public void StartLoad()
        {
            progressRing_Loading.Visibility = Visibility.Visible;
            label_Loading.Visibility = Visibility.Visible;
            tabControl.Effect = new BlurEffect { Radius = 10 };
            tabControl.IsEnabled = false;
        }

        public void EndLoad()
        {
            progressRing_Loading.Visibility = Visibility.Hidden;
            label_Loading.Visibility = Visibility.Hidden;
            tabControl.Effect = null;
            tabControl.IsEnabled = true;
        }
        #endregion

        public SettingsPage()
        {
            InitializeComponent();            
        }

        //workingDirectory
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "选择工作目录",
                    Multiselect = false
                };
                while (true)
                {
                    if (dialog.ShowDialog() == true)
                    {
                        var dialog2 = new ContentDialog
                        {
                            Title = "提示",
                            Content = $"是否要将 {dialog.FolderName} 设为.NET工作目录",
                            PrimaryButtonText = "确定",
                            SecondaryButtonText = "重选",
                            DefaultButton = ContentDialogButton.Primary,
                        };
                        if (await dialog2.ShowAsync() == ContentDialogResult.Primary)
                        {
                            textBox_WorkingPath.Text = dialog.FolderName;
                            Globals.GlobanConfig.DotnetConfig.WorkingDirectory = dialog.FolderName;
                            Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
                            break;
                        }
                    }
                    else
                        break;
                }

                
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在选择目录时发生错误", ex);
                throw;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private async void button_CheckNET_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.StartInfo.Arguments = "--list-sdks";
                process.Start();
                var sdkOutputTask = process.StandardOutput.ReadToEndAsync();
                var sdkErrorTask = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(sdkOutputTask, sdkErrorTask);
                process.WaitForExit();
                string sdkInfo = sdkErrorTask.Result;
                if (string.IsNullOrEmpty(sdkInfo))
                    sdkInfo = sdkOutputTask.Result;


                process.StartInfo.Arguments = "--list-runtimes";
                process.Start();
                var runtimeOutputTask = process.StandardOutput.ReadToEndAsync();
                var runtimeErrorTask = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(runtimeOutputTask, runtimeErrorTask);
                process.WaitForExit();
                string runtimeInfo = runtimeErrorTask.Result;
                if (string.IsNullOrEmpty(runtimeInfo))
                    runtimeInfo = runtimeOutputTask.Result;


                process.StartInfo.Arguments = "--version";
                process.Start();
                var versionOutputTask = process.StandardOutput.ReadToEndAsync();
                var versionErrorTask = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(versionOutputTask, versionErrorTask);
                process.WaitForExit();
                string versionInfo = versionErrorTask.Result;
                if (string.IsNullOrEmpty(versionInfo))
                    versionInfo = versionOutputTask.Result;


                await new ContentDialog
                {
                    Title = "Result",
                    Content = $"当前SDK:\n  {versionInfo}\n可用SDK ({sdkInfo.Split('\n').Length - 1}):\n  {sdkInfo}\n可用Runtime ({runtimeInfo.Split('\n').Length - 1}):\n  {runtimeInfo}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "检查.NET SDK状态时发生错误", ex);
            }
        }



        // 检查更新
        private async void button_CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();
                using (var client = new HttpClient())
                {
                    string indexUrl = $"{Globals.UpdateRootUrl}latest.json";
                    JsonConfig.UpdateIndex.Root indexFile = Json.ReadJson<JsonConfig.UpdateIndex.Root>(await client.GetStringAsync(indexUrl));

                    if (indexFile.latest_version != Globals.AppVersion)
                    {
                        //webView.CoreWebView2.NavigateToString($"<!DOCTYPE html>\r\n<html lang=\"zh_CN\">\r\n\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>*</title>\r\n    <style>\r\n        body {{\r\n            background-color: white;\r\n            color: black;\r\n            font-family: '微软雅黑', sans-serif;\r\n            transform: scale(0.8);\r\n            transform-origin: left top;\r\n            line-height: 0.5;\r\n        }}\r\n    </style>\r\n</head>\r\n\r\n<body>\r\n{Markdown.ToHtml(changelog)}    \r\n</body>\r\n\r\n</html>");

                        var dialog = new ContentDialog
                        {
                            Title = $"可更新至 {indexFile.latest_version}",
                            Content = "是否前往下载",
                            PrimaryButtonText = "前往",
                            CloseButtonText = "取消",
                            DefaultButton = ContentDialogButton.Primary
                        };

                        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = indexFile.url,
                                UseShellExecute = true
                            });

                        //web.NavigateToString($"<!DOCTYPE html>\r\n<html lang=\"zh_CN\">\r\n\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>*</title>\r\n    <style>\r\n        body {{\r\n            background-color: white;\r\n            color: black;\r\n            font-family: '微软雅黑', sans-serif;\r\n            transform: scale(0.8);\r\n            transform-origin: left top;\r\n            line-height: 0.8;\r\n        }}\r\n    </style>\r\n</head>\r\n\r\n<body>\r\n{Markdown.ToHtml(changelog)}\r\n</body>\r\n\r\n</html>");
                        
                        /*if(await dialog.ShowAsync() == ContentDialogResult.Primary)
                        {
                            string? savePath = $"{System.IO.Path.Combine(Globals.TempPath!, "update.zip")}";

                            await Downloader.DownloadFileAsync(indexFile.url!, savePath, ((p) => { label_Loading.Content = $"下载中 {Math.Round(p, 2)}% ..."; }), new CancellationToken());

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = System.IO.Path.Combine(Directory.GetParent(Globals.ExecutePath!).FullName, "DotnetGUI.exe"),
                                Arguments = $"-updatefile {savePath}",
                                UseShellExecute = true
                            });
                            Environment.Exit(0);
                        }*/
                    }
                    else
                        await new ContentDialog
                        {
                            Title = "无可用更新",
                            Content = $"您已经是最新版 {Globals.AppVersion} , 无需更新！",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        }.ShowAsync();

                }
                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在检测更新时发生错误", ex);
            }
        }
    }
}
