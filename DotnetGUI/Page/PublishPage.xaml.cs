using DotnetGUI.Class;
using DotnetGUI.Util;
using Microsoft.Win32;
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

namespace DotnetGUI.Page
{
    /// <summary>
    /// PublishPage.xaml 的交互逻辑
    /// </summary>
    public partial class PublishPage : ModernWpf.Controls.Page
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

                #region 扫描项目

                projFiles?.Clear();

                await ScanFile(Globals.GlobanConfig!.DotnetConfig!.WorkingDirectory!);
                if (projFiles?.Count > 0)
                {
                    comboBox_ProjName.Items.Clear();
                    foreach (var item in projFiles)
                        comboBox_ProjName.Items.Add(item);
                    comboBox_ProjName.SelectedIndex = 0;
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

                radioButton_OutputDefault_Click(radioButton_OutputDefault, null!);

                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在初始化 PublishPage 发生错误", ex);
            }
        }

        //遍历项目文件夹
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


        public PublishPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void radioButton_OutputDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioButton_OutputDefault.IsChecked == true)
                {
                    textBox_Output.IsEnabled = false;
                    button_Output_Broswer.IsEnabled = false;
                }
                else if (radioButton_OutputCustom.IsChecked == true)
                {
                    textBox_Output.IsEnabled = true;
                    button_Output_Broswer.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "发生错误", ex);
            }
        }

        private void button_Output_Broswer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "选择输出目录",
                    Multiselect = false
                };
                if (dialog.ShowDialog() == true)
                    textBox_Output.Text = dialog.FolderName;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "发生错误", ex);
            }
        }

        private async void button_Publish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                string projName = $" \"{comboBox_ProjName.SelectedItem}\"";
                string arch = $" -a {(string)(((ComboBoxItem)comboBox_Arch.SelectedItem).Tag)}";
                string force = toggleSwitch_Force.IsOn == true ? " --force" : null!;
                string noDeps = toggleSwitch_NoDeps.IsOn == true ? " --no-dependencies" : null!;
                string noLogo = toggleSwitch_NoLogo.IsOn == true ? " --nologo" : null!;
                string noRestore = toggleSwitch_NoRestore.IsOn == true ? " --no-restore" : null!;
                string output = radioButton_OutputCustom.IsChecked == true ? $" -o {textBox_Output.Text}" : null!;
                string selfCon = toggleSwitch_SelfCon.IsOn == true ? " --self-contained true" : " --self-contained false";
                string ucr = toggleSwitch_Ucr.IsOn == true ? " --ucr" : null!;

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish{projName}{arch}{force}{noDeps}{noLogo}{noRestore}{output}{selfCon}{ucr}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                    WorkingDirectory = Globals.GlobanConfig!.DotnetConfig!.WorkingDirectory
                });

                string? line;
                while ((line = await process!.StandardOutput.ReadLineAsync()) != null)
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
                        Content = $"已在\"{(radioButton_OutputCustom.IsChecked == true ? textBox_Output.Text : Globals.GlobanConfig.DotnetConfig.WorkingDirectory!)}\"发布项目",
                        PrimaryButtonText = "定位",
                        CloseButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = (radioButton_OutputCustom.IsChecked == true ? textBox_Output.Text : Globals.GlobanConfig.DotnetConfig.WorkingDirectory!),
                            UseShellExecute = true
                        });
                    }));
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"尝试在\"{(radioButton_OutputCustom.IsChecked == true ? textBox_Output.Text : Globals.GlobanConfig.DotnetConfig.WorkingDirectory!)}\"发布项目，但在执行过程中发生以下错误:\n\n{errorInfo}",
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "定位",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = (radioButton_OutputCustom.IsChecked == true ? textBox_Output.Text : Globals.GlobanConfig.DotnetConfig.WorkingDirectory!),
                            UseShellExecute = true
                        });
                    }));
                }



                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在发布项目或解决方案发生错误", ex);
            }
        }
    }
}
