using DotnetGUI.Class;
using DotnetGUI.Util;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
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

namespace DotnetGUI.Page
{
    /// <summary>
    /// BuildPage.xaml 的交互逻辑
    /// </summary>
    public partial class BuildPage : ModernWpf.Controls.Page
    {
        #region Obj
        #endregion

        #region Var
        public List<string>? projFiles = new List<string>();
        #endregion

        #region Func
        public void StartLoad()
        {
            progressRing_Loading.Visibility = Visibility.Visible;
            label_Loading.Visibility = Visibility.Visible;
            grid_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Main.IsEnabled = false;
        }

        public void EndLoad()
        {
            progressRing_Loading.Visibility = Visibility.Hidden;
            label_Loading.Visibility = Visibility.Hidden;
            grid_Main.Effect = null;
            grid_Main.IsEnabled = true;
        }

        public async void Initialize()
        {
            try
            {
                StartLoad();

                #region 扫描项目文件
                projFiles.Clear();
                ScanFile(Globals.GlobanConfig.DotnetConfig.WorkingDirectory!);
                if (projFiles.Count > 0)
                {
                    comboBox_Proj.Items.Clear();
                    foreach (var item in projFiles)
                        comboBox_Proj.Items.Add(item);
                    comboBox_Proj.SelectedIndex = 0;
                }
                else
                {
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

                #region 设置初始值

                comboBox_Arch.SelectedIndex = 0;

                #endregion
                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "初始化 BuildPage 发生错误", ex);
            }
        }

        //遍历项目文件夹
        public void ScanFile(string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
                if (file.EndsWith("proj", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
                    projFiles.Add(file);

            string[] dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
                ScanFile(dir);
        }
        #endregion


        public BuildPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private async void button_Build_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                string? projName = (string)comboBox_Proj.SelectedItem;
                string? arch = $" -a {((ComboBoxItem)comboBox_Arch.SelectedItem).Tag}";
                string? config; if (string.IsNullOrEmpty(textBox_Config.Text)) throw new Exception("必须传入一个生成配置"); else config = $" -c {textBox_Config.Text}";
                string? force = toggleSwitch_Force.IsOn == true ? " --force" : null;
                string? sc = toggleSwitch_SelfCon.IsOn == true ? " --sc true" : " --sc false";
                string? ucr = toggleSwitch_Ucr.IsOn == true ? " --ucr" : null;

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build {projName}{arch}{config}{force}{sc}{ucr}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = Globals.GlobanConfig.DotnetConfig.WorkingDirectory
                });

                string? line;
                while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    label_Loading.Content = line;


                var error = new StringBuilder();
                string? errorLine;
                while ((errorLine = await process.StandardError.ReadLineAsync()) != null)
                    error.AppendLine(errorLine);


                await process.WaitForExitAsync();

                string errorInfo = error.ToString();


                if (string.IsNullOrEmpty(errorInfo))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"已在\"{Globals.GlobanConfig.DotnetConfig.WorkingDirectory}\\bin\"生成项目",
                        PrimaryButtonText = "定位",
                        CloseButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(Globals.GlobanConfig.DotnetConfig.WorkingDirectory!, "bin"),
                            UseShellExecute = true
                        });
                    }));
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"尝试在\"{Globals.GlobanConfig.DotnetConfig.WorkingDirectory}\\bin\"生成项目，但在执行过程中发生以下错误:\n\n{errorInfo}",
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "定位",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(Globals.GlobanConfig.DotnetConfig.WorkingDirectory!,"bin"),
                            UseShellExecute = true
                        });
                    }));
                }

                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在生成项目时发生错误", ex);
            }
        }
    }
}
