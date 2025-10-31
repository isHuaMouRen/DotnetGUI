using DotnetGUI.Class;
using DotnetGUI.Util;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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
    /// Download_InfoPage.xaml 的交互逻辑
    /// </summary>
    public partial class Download_InfoPage : ModernWpf.Controls.Page
    {
        #region Obj
        public CancellationTokenSource cts = new CancellationTokenSource();
        #endregion

        #region Var
        public int SelectIndex;
        #endregion

        #region Func
        public void StartLoad()
        {
            progressRing_Load.Visibility = Visibility.Visible;
            label_Load.Visibility = Visibility.Visible;

            grid_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Main.IsEnabled = false;
        }

        public void EndLoad()
        {
            progressRing_Load.Visibility = Visibility.Collapsed;
            label_Load.Visibility = Visibility.Collapsed;

            grid_Main.Effect = null;
            grid_Main.IsEnabled = true;
        }

        public async void Initialize()
        {
            StartLoad();
            try
            {
                using (var client = new HttpClient())
                {
                    Globals.DotnetVersionInfo = Json.ReadJson<JsonConfig.DotnetVersionInfo.Root>(await client.GetStringAsync(Globals.DotnetIndex.release_index[SelectIndex].releases_json));

                    label_MainVersion.Content = $"下载.NET SDK {Globals.DotnetIndex.release_index[SelectIndex].channel_version}";


                    listBox.Items.Clear();
                    for (int i = 0; i < Globals.DotnetVersionInfo.releases.Length; i++)
                    {
                        string support;

                        if (Globals.DotnetVersionInfo.releases[i].security)
                            support = "受保护";
                        else
                            support = "不受保护";

                        var item = new ListBoxItem
                        {
                            Content = $"{Globals.DotnetVersionInfo.releases[i].release_version} | {support}"
                        };
                        listBox.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"在初始化 {typeof(Download_InfoPage).Name} 时发生错误", ex);
            }
            EndLoad();
        }
        #endregion



        public Download_InfoPage(int selectIndex)
        {
            InitializeComponent();
            SelectIndex = selectIndex;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private async void button_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "下载确认",
                    Content = $".NET主版本: {Globals.DotnetIndex.release_index[SelectIndex].channel_version}\n.NET SDK版本: {Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].sdk.version}\n发布日期: {Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].release_date}\n\n是否下载",
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                await DialogManager.ShowDialogAsync(dialog, (async () =>
                {
                    try
                    {
                        string savePath = $"{Globals.TempPath}\\dotnet-sdk-win-x64.exe";
                        string? url = null;
                        string? hash = null;

                        for (int i = 0; i < Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].sdk.files.Length; i++)
                        {
                            if (Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].sdk.files[i].name == "dotnet-sdk-win-x64.exe")
                            {
                                url = Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].sdk.files[i].url;
                                hash = Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].sdk.files[i].hash;
                            }
                        }

                        if (url == null || hash == null)
                            throw new Exception("未寻找到目标安装程序");
                        else
                        {
                            if (File.Exists(savePath))
                                File.Delete(savePath);

                            StartLoad();
                            button_Cancel.Visibility = Visibility.Visible;
                            await Downloader.DownloadFileAsync(url, savePath, ((p) => { label_Load.Content = $"下载中 {Math.Round(p, 2)}% ..."; }), cts.Token);

                            label_Load.Content = "执行安装程序...";

                            var process = new Process();
                            process.StartInfo = new ProcessStartInfo
                            {
                                FileName = savePath,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };

                            process.Start();
                            await Task.Run(() => process.WaitForExit());

                            if (process.ExitCode == 0)
                            {
                                var dialog2 = new ContentDialog
                                {
                                    Title = "提示",
                                    Content = $"安装成功，程序正常退出(ExitCode: 0)",
                                    PrimaryButtonText = "完成",
                                    DefaultButton = ContentDialogButton.Primary
                                };
                                await DialogManager.ShowDialogAsync(dialog2);
                            }
                            else
                            {
                                var dialog2 = new ContentDialog
                                {
                                    Title = "提示",
                                    Content = $"程序非正常退出(ExitCode: {process.ExitCode})，如已经正常安装，那么可以忽略此提示",
                                    PrimaryButtonText = "完成",
                                    DefaultButton = ContentDialogButton.Primary
                                };
                                await DialogManager.ShowDialogAsync(dialog2);
                            }

                            button_Cancel.Visibility = Visibility.Hidden;
                            EndLoad();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        button_Cancel.Visibility = Visibility.Hidden;
                        EndLoad();
                    }
                    catch (Exception ex)
                    {
                        ErrorReportDialog.Show("发生错误", "下载.NET发生错误", ex);
                    }                    
                }));

            }            
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在下载.NET时发生错误", ex);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            button_Download.IsEnabled = true;
            button_Download.Content = $"下载 {Globals.DotnetVersionInfo.releases[listBox.SelectedIndex].release_version}";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await DialogManager.ShowDialogAsync(new ContentDialog
            {
                Title = "提示",
                Content = "确定取消下载?",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => cts?.Cancel()));
            
        }
    }
}
