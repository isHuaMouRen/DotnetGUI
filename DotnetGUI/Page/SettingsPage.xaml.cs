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
                textBox_WorkingPath.Text = Globals.GlobanConfig!.DotnetConfig!.WorkingDirectory;
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
                bool isWhile = true;
                while (isWhile)
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
                        await DialogManager.ShowDialogAsync(dialog2, (() =>
                        {
                            textBox_WorkingPath.Text = dialog.FolderName;
                            Globals.GlobanConfig!.DotnetConfig!.WorkingDirectory = dialog.FolderName;
                            Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
                            isWhile = false;
                        }));
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


                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "Result",
                    Content = $"当前SDK:\n  {versionInfo}\n可用SDK ({sdkInfo.Split('\n').Length - 1}):\n  {sdkInfo}\n可用Runtime ({runtimeInfo.Split('\n').Length - 1}):\n  {runtimeInfo}",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });
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
                    JsonConfig.UpdateIndex.Root updateIndex = Json.ReadJson<JsonConfig.UpdateIndex.Root>(await client.GetStringAsync($"{Globals.UpdateRootUrl}latest.json"));

                    if (updateIndex.latest_version == Globals.AppVersion)
                    {
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
                            if (File.Exists(savePath))
                                File.Delete(savePath);

                            await Downloader.DownloadFileAsync($"{Globals.UpdateRootUrl}update.zip", savePath, ((pgs) => label_Loading.Content = $"下载更新文件中 {Math.Round(pgs, 2)}% ..."), new CancellationToken());

                            label_Loading.Content = $"下载更新文件成功, 即将重启...";
                            await Task.Delay(2000);

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = System.IO.Path.Combine(Globals.ExecutePath!, "UpdateService.exe"),
                                Arguments = $"-updatefile \"{savePath}\"",
                                UseShellExecute = false
                            });
                            Environment.Exit(0);
                        }
                    }
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
