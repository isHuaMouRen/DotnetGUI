using DotnetGUI.Class;
using DotnetGUI.Util;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    /// NewPage.xaml 的交互逻辑
    /// </summary>
    public partial class NewPage : ModernWpf.Controls.Page
    {
        #region Obj
        #endregion

        #region Var
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
                #region 加载模板
                comboBox_Template.Items.Clear();

                
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "new list",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                using var p = Process.Start(psi);
                string output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();

                if (p.ExitCode != 0)
                    throw new Exception("dotnet new list 执行失败");

                
                var templates = new List<(string Name, string Short)>();
                bool headerPassed = false;
                foreach (var line in output.Split('\n'))
                {
                    if (!headerPassed && line.StartsWith("---")) { headerPassed = true; continue; }
                    if (!headerPassed) continue;

                    //名称   短名称   语言   标记
                    var m = Regex.Match(line, @"^\s*(.+?)\s{2,}(\S.*?)\s{2,}([\[\]\w#,]+)\s{2,}.+$");
                    if (!m.Success) continue;

                    string name = m.Groups[1].Value.Trim();
                    string shortPart = m.Groups[2].Value.Trim();
                    var shorts = shortPart.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0);
                    if (!shorts.Any()) continue;

                    templates.Add((name, shorts.First()));
                }

                
                var priority = new[] {
                "控制台应用","类库","解决方案文件",
                "WPF 应用程序","Windows 窗体应用",
                "ASP.NET Core 空","ASP.NET Core Web 应用","ASP.NET Core Web API",
                "Blazor Web 应用","辅助角色服务",
                "xUnit 测试项目","MSTest 测试项目","NUnit 3 测试项目"
                };
                var ordered = templates
                    .OrderBy(t => priority.Contains(t.Name) ? Array.IndexOf(priority, t.Name) : int.MaxValue)
                    .ThenBy(t => t.Name);

                
                comboBox_Template.Items.Clear();
                foreach (var (name, shortName) in ordered)
                {
                    comboBox_Template.Items.Add(new ComboBoxItem
                    {
                        Content = name,
                        Tag = shortName
                    });
                }
                if (comboBox_Template.Items.Count > 0) comboBox_Template.SelectedIndex = 0;
                #endregion

                EndLoad();
            }
            catch (Exception ex)
            {
                comboBox_Template.Items.Clear();
                ErrorReportDialog.Show("发生错误", "初始化 NewPage 时发生错误", ex);
            }
        }
        #endregion


        public NewPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void button_GoSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Window.GetWindow(this) is MainWindow mainWindow)
                    if (mainWindow.FindName("navView") is NavigationView navView)
                        if (navView.FindName("navViewItem_Settings") is NavigationViewItem navItem)
                            navView.SelectedItem = navItem;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在尝试跳转 SettingsPage 发生错误", ex);
            }
        }

        private async void button_Done_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                string template = (string)((ComboBoxItem)comboBox_Template.SelectedItem).Tag;
                bool isForce = toggleSwitch_Force.IsOn;
                string? force = isForce ? "--force" : null;
                string? language = radioButton_FS.IsChecked == true ? "-lang \"F#\"" :
                                  radioButton_VB.IsChecked == true ? "-lang \"VB\"" :
                                  radioButton_CS.IsChecked == true ? "-lang \"C#\"" : null;
                string projName = textBox_Name.Text;
                if (string.IsNullOrEmpty(projName)) throw new Exception("必须传入项目名");

                string arg = $"{template} {force} {language} -n {projName} -o {Globals.GlobanConfig.DotnetConfig.WorkingDirectory}";


                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"new {arg}",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                });

                string? line;
                while ((line = await process.StandardOutput.ReadLineAsync()) != null)
                    label_Loading.Content = line;


                var error = new StringBuilder();
                string? errorLine;
                while ((errorLine = await process.StandardError.ReadLineAsync()) != null)
                {
                    error.AppendLine(errorLine);
                }


                await process.WaitForExitAsync();

                string errorInfo = error.ToString();


                if (string.IsNullOrEmpty(errorInfo))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"已在\"{Globals.GlobanConfig.DotnetConfig.WorkingDirectory}\"创建项目",
                        PrimaryButtonText = "定位",
                        CloseButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Globals.GlobanConfig.DotnetConfig.WorkingDirectory,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "执行完毕",
                        Content = $"尝试在\"{Globals.GlobanConfig.DotnetConfig.WorkingDirectory}\"创建项目，但在执行过程中发生以下错误:\n\n{errorInfo}",
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "定位",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    if (await dialog.ShowAsync() == ContentDialogResult.Secondary)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Globals.GlobanConfig.DotnetConfig.WorkingDirectory,
                            UseShellExecute = true
                        });
                    }
                }

                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在创建项目时发生错误", ex);
            }
        }
    }
}
