using DotnetGUI.Class;
using DotnetGUI.Util;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
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
    }
}
