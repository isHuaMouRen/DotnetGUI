using ToolLib.Library.AutoStartLib;
using ToolLib.Library.CmdLib;
using ToolLib.Library.DownloaderLib;
using ToolLib.Library.GdiToolLib;
using ToolLib.Library.HashLib;
using ToolLib.Library.HexLib;
using ToolLib.Library.HookLib;
using ToolLib.Library.HotkeyManagerLib;
using ToolLib.Library.IniLib;
using ToolLib.Library.InputLib;
using ToolLib.Library.JsonLib;
using ToolLib.Library.LogLib;
using ToolLib.Library.MemoryLib;
using ToolLib.Library.XmlLib;

using DotnetGUI.Util;
using ModernWpf.Controls;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.IO;
using DotnetGUI.Class;
using DotnetGUI.Page;

namespace DotnetGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Obj
        //预加载Page
        public ModernWpf.Controls.Page _homePage = new HomePage();
        public ModernWpf.Controls.Page _downloadPage = new DownloadPage();
        public ModernWpf.Controls.Page _consolePage = new ConsolePage();
        public ModernWpf.Controls.Page _newPage = new NewPage();
        public ModernWpf.Controls.Page _buildPage = new BuildPage();

        public ModernWpf.Controls.Page _aboutPage = new AboutPage();
        public ModernWpf.Controls.Page _settingsPage = new SettingsPage();
        #endregion

        #region Var
        #endregion

        #region Func
        public void Initialize()
        {
            try
            {

                #region 读写配置
                //如没有就写入
                if (!File.Exists(Globals.ConfigPath))
                {
                    Globals.GlobanConfig = new JsonConfig.Config.Root
                    {
                        UIConfig = new JsonConfig.Config.UIConfig
                        {
                            WindowSize = new Size(800, 450),
                            SelectPage = "Home"
                        },
                        DotnetConfig = new JsonConfig.Config.DotNetConfig
                        {
                            WorkingDirectory = $"{System.IO.Path.Combine(Globals.ExecutePath!, "Project")}",
                            DotnetState = null
                        }
                    };
                    Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
                }

                //读取
                Globals.GlobanConfig = Json.ReadJson<JsonConfig.Config.Root>(Globals.ConfigPath);

                //应用
                this.Width = Globals.GlobanConfig.UIConfig!.WindowSize.Width;
                this.Height = Globals.GlobanConfig.UIConfig!.WindowSize.Height;

                // --nav项
                foreach (var item in navView.MenuItems)
                    if (item is NavigationViewItem currentItem)
                        if (currentItem.Tag.ToString() == Globals.GlobanConfig.UIConfig.SelectPage)
                            navView.SelectedItem = currentItem;
                // --nav foot项
                foreach (var footItem in navView.FooterMenuItems)
                    if (footItem is NavigationViewItem currentFootItem)
                        if (currentFootItem.Tag.ToString() == Globals.GlobanConfig.UIConfig.SelectPage)
                            navView.SelectedItem = currentFootItem;
                #endregion

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在初始化程序时发生错误", ex);
            }
        }

        public async void LoadedInitialize()
        {
            try
            {
                #region 启动参数检查
                string[] args = Environment.GetCommandLineArgs();
                bool isShellExecute = false;
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-shell":
                            isShellExecute = true; break;
                    }
                }

                if (!Debugger.IsAttached)
                {
                    if (!isShellExecute)
                    {
                        var dialog= new ContentDialog
                        {
                            Title = "提示",
                            Content = "检测到没有使用Shell执行，推荐使用Shell执行",
                            PrimaryButtonText = "退出",
                            CloseButtonText = "忽略",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        await DialogManager.ShowDialogAsync(dialog, (() => Environment.Exit(0)));
                    }
                    
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = "你正在使用自行构建的版本，再此版本出现的任何异常都不要去Github报告！",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog);
                }
                #endregion

                #region .NET环境监测

                var result = await DotnetManager.CheckState();
                if (string.IsNullOrEmpty(result))
                {
                    var dialog = new ContentDialog
                    {
                        Title = "警告",
                        Content = "检测到你电脑上没有安装.NET SDK，这将导致此软件的所有功能均无法使用!",
                        PrimaryButtonText = "前往下载页",
                        CloseButtonText = "忽略",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await DialogManager.ShowDialogAsync(dialog, (() => navView.SelectedItem = navViewItem_Download));                        
                }

                #endregion
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载后初始化时发生错误", ex);
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedInitialize();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (navView.SelectedItem is NavigationViewItem item)
                {
                    //Home
                    if (item == navViewItem_Home)
                        frame_Navv.Navigate(_homePage);
                    //Download
                    else if (item == navViewItem_Download)
                        frame_Navv.Navigate(_downloadPage);
                    //Console
                    else if (item == navViewItem_Console)
                        frame_Navv.Navigate(_consolePage);
                    //New
                    else if (item == navViewItem_New)
                        frame_Navv.Navigate(_newPage);
                    //Build
                    else if (item == navViewItem_Build)
                        frame_Navv.Navigate(_buildPage);

                    //About
                    else if (item == navViewItem_About)
                        frame_Navv.Navigate(_aboutPage);
                    //Settings
                    else if (item == navViewItem_Settings)
                        frame_Navv.Navigate(_settingsPage);
                    else
                        throw new Exception("内部错误: 不存在的NavView项");



                    Globals.GlobanConfig!.UIConfig!.SelectPage = item.Tag.ToString();
                    Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
                }
                else
                    throw new Exception("非法的值");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载页面时发生错误", ex);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Globals.GlobanConfig!.UIConfig!.WindowSize = new Size(this.Width, this.Height);
                Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在窗口改变大小时发生错误", ex);
            }
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            frame_Navv.GoBack();
        }

        private void frame_Navv_Navigated(object sender, NavigationEventArgs e)
        {
            if (frame_Navv.Content is Download_InfoPage)
            {
                navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
                navView.IsBackEnabled = true;
            }
            else
            {
                navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
                navView.IsBackEnabled = false;
            }

        }
    }
}