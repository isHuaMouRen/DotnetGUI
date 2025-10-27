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
        #endregion

        #region Var
        #endregion

        #region Func
        public async void Initialize()
        {
            try
            {
                #region 启动参数检测
                string[] args = Environment.GetCommandLineArgs();
                bool isShellLaunch = false;
                
                foreach(var arg in args)
                {
                    if (arg == "-shell")
                        isShellLaunch = true;
                }

                if (!Debugger.IsAttached)
                {
                    if (!isShellLaunch)
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "提示",
                            Content = "检测到没有使用Shell运行，请运行外部的可执行文件。直接运行此程序可能会导致意想不到的后果",
                            PrimaryButtonText = "退出",
                            CloseButtonText = "忽略",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                            Environment.Exit(0);
                    }
                }
                #endregion

                #region 读写配置
                //如没有就写入
                if (!File.Exists(Globals.ConfigPath))
                {
                    Globals.GlobanConfig = new JsonConfig.Config.Root
                    {
                        UIConfig = new JsonConfig.Config.UIConfig
                        {
                            WindowSize = new Size(800, 450),
                            SelectPage = 0
                        }
                    };
                    Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
                }

                //读取
                Globals.GlobanConfig = Json.ReadJson<JsonConfig.Config.Root>(Globals.ConfigPath);

                //应用
                this.Width = Globals.GlobanConfig.UIConfig!.WindowSize.Width;
                this.Height = Globals.GlobanConfig.UIConfig!.WindowSize.Height;
                #endregion


            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在初始化程序时发生错误", ex);
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (navView.SelectedItem is NavigationViewItem item)
                {
                    if (item == navViewItem_Home)
                        frame_Navv.Navigate(_homePage);
                        

                    
                }
                else
                    throw new Exception("非法的值");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载页面时发生错误", ex);
            }
        }
    }
}