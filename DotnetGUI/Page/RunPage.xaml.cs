using DotnetGUI.Class;
using DotnetGUI.Util;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using static DotnetGUI.Class.Globals;

namespace DotnetGUI.Page
{
    /// <summary>
    /// RunPage.xaml 的交互逻辑
    /// </summary>
    public partial class RunPage : ModernWpf.Controls.Page
    {
        #region Var
        public List<string> projFiles = new List<string>();
        #endregion

        #region Func
        public void StartLoad()
        {
            progressRing_Loading.Visibility = Visibility.Visible;
            label_Loading.Visibility = Visibility.Visible;
            grid_main.Effect = new BlurEffect { Radius = 10 };
            grid_main.IsEnabled = false;
        }

        public void EndLoad()
        {
            progressRing_Loading.Visibility = Visibility.Hidden;
            label_Loading.Visibility = Visibility.Hidden;
            grid_main.Effect = null;
            grid_main.IsEnabled = true;
        }

        public async void Initialize()
        {
            try
            {
                logger.Info("RunPage 开始初始化...");
                StartLoad();

                #region 扫描项目
                Globals.logger.Info($"开始扫描项目文件");
                projFiles?.Clear();

                await ScanFile(Globals.GlobalConfig!.DotnetConfig!.WorkingDirectory!);
                if (projFiles?.Count > 0)
                {
                    comboBox_ProjName.Items.Clear();
                    foreach (var item in projFiles)
                    {
                        Globals.logger.Info($"发现项目文件: {item}");
                        comboBox_ProjName.Items.Add(item);
                    }
                    comboBox_ProjName.SelectedIndex = 0;
                }
                else
                {
                    Globals.logger.Warn($"找不到任何项目文件");
                    var dialog = new ContentDialog
                    {
                        Title = "提示",
                        Content = "未在您的工作目录内找到任何项目或解决方案(.csproj .fsproj .vbproj .sln .slnx)\n\n请至少创建1个项目或解决方案!",
                        PrimaryButtonText = "创建",
                        SecondaryButtonText = "更改工作目录",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    if (Window.GetWindow(this) is MainWindow window &&
                        window.FindName("navView") is NavigationView navView &&
                        navView.FindName("navViewItem_Settings") is NavigationViewItem settingsPage &&
                        navView.FindName("navViewItem_New") is NavigationViewItem newPage)
                    {
                        await DialogManager.ShowDialogAsync(dialog, (() => navView.SelectedItem = newPage), (() => navView.SelectedItem = settingsPage), (() => navView.SelectedItem = settingsPage));

                    }
                    else
                        throw new Exception("未找到目标控件");
                }
                #endregion

                EndLoad();
                logger.Info("RunPage 初始化结束");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在初始化 RunPage 发生错误", ex);
            }
        }

        public async Task ScanFile(string path)
        {
            await Task.Run(async () =>
            {
                string[] files = Directory.GetFiles(path);
                foreach (var file in files)
                    if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
                        projFiles?.Add(file);

                string[] dirs = Directory.GetDirectories(path);
                foreach (var dir in dirs)
                    await ScanFile(dir);
            });
        }
        #endregion


        public RunPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private async void button_Run_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();
                logger.Info("开始运行项目");

                string proj = $" --project \"{(string)comboBox_ProjName.SelectedItem}\"";
                string arch = $" -a {(string)(((ComboBoxItem)comboBox_Arch.SelectedItem).Tag)}";
                string config = $" -c {textBox_Config.Text}";
                string force = toggleSwitch_Force.IsOn == true ? " --force" : null!;
                string noBuild = toggleSwitch_NoBuild.IsOn ? " --no-build" : null!;
                string noCache = toggleSwitch_NoCache.IsOn ? " --no-cache" : null!;
                string noRestore = toggleSwitch_NoRestore.IsOn ? " --no-restore" : null!;
                string self = toggleSwitch_Self.IsOn ? " --self-contained true" : " --self-contained false";
                string arg = string.IsNullOrEmpty(textBox_Args.Text) ? null! : $" -- {textBox_Args.Text}";

                string args = $"{proj}{arch}{config}{force}{noBuild}{noCache}{noRestore}{self}{arg}";
                logger.Info($"参数: dotnet run {args}");


                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run{args}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                    WorkingDirectory = Globals.GlobalConfig!.DotnetConfig!.WorkingDirectory
                });

                Globals.logger.Info($"==========[Dotnet开始运行]==========");

                string? line;
                while ((line = await process!.StandardOutput.ReadLineAsync()) != null)
                {
                    Globals.logger.Info($"{line}");
                    label_Loading.Content = line;
                }

                Globals.logger.Warn($"==========[Dotnet错误信息]===========");

                var error = new StringBuilder();
                string? errorLine;
                while ((errorLine = await process.StandardError.ReadLineAsync()) != null)
                {
                    Globals.logger.Warn($"{errorLine}");
                    error.AppendLine(errorLine);
                }


                await process.WaitForExitAsync();

                Globals.logger.Info($"==========[Dotnet结束运行]=========");

                string errorInfo = error.ToString();


                if (string.IsNullOrEmpty(errorInfo))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"已在\"{Globals.GlobalConfig.DotnetConfig.WorkingDirectory!}\"发布项目",
                        PrimaryButtonText = "定位",
                        CloseButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Globals.GlobalConfig.DotnetConfig.WorkingDirectory!,
                            UseShellExecute = true
                        });
                    }));
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"尝试在\"{Globals.GlobalConfig.DotnetConfig.WorkingDirectory!}\"发布项目，但在执行过程中发生以下错误:\n\n{errorInfo}",
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "定位",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Globals.GlobalConfig.DotnetConfig.WorkingDirectory!,
                            UseShellExecute = true
                        });
                    }));
                }

                logger.Info("结束运行项目");
                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "发布项目时发生错误", ex);
            }
        }
    }
}
