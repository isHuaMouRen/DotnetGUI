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
using Microsoft.Win32;

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
        public ModernWpf.Controls.Page _publishPage = new PublishPage();

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
                Globals.logger.Info($"程序开始构造初始化");
                #region 读写配置
                //如没有就写入
                if (!File.Exists(Globals.ConfigPath))
                {
                    Globals.logger.Info($"未检测到配置文件，即将创建");
                    Globals.GlobanConfig = new JsonConfig.Config.Root
                    {
                        UIConfig = new JsonConfig.Config.UIConfig
                        {
                            WindowSize = new Size(800, 450),
                            isFirstUse = true
                        },
                        DotnetConfig = new JsonConfig.Config.DotNetConfig
                        {
                            WorkingDirectory = $"{System.IO.Path.Combine(Globals.ExecutePath!, "Project")}",
                            DotnetState = null
                        }
                    };
                    Globals.SaveAllConfig();
                }

                //读取
                Globals.GlobanConfig = Json.ReadJson<JsonConfig.Config.Root>(Globals.ConfigPath);

                //应用
                this.Width = Globals.GlobanConfig.UIConfig!.WindowSize.Width;
                this.Height = Globals.GlobanConfig.UIConfig!.WindowSize.Height;

                navView.SelectedItem = navViewItem_Home;
                #endregion
                Globals.logger.Info($"程序结束构造初始化");
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
                Globals.logger.Info($"程序开始初始化");

                #region 启动参数检查
                Globals.logger.Info($"开始启动参数检测...");
                string[] args = Environment.GetCommandLineArgs();
                bool isShellExecute = false;
                bool isUpdateExecute = false;

                foreach (var arg in args)
                {
                    Globals.logger.Info($"启动参数: {arg}");
                    switch (arg)
                    {
                        case "-shell":
                            isShellExecute = true; break;
                        case "-update":
                            isUpdateExecute = true; break;
                    }
                }

                if (!Debugger.IsAttached)
                {
                    if (!isShellExecute)
                    {
                        Globals.logger.Info($"isShellExecute = false");
                        var dialog = new ContentDialog
                        {
                            Title = "提示",
                            Content = "检测到没有使用Shell执行，推荐使用Shell执行",
                            PrimaryButtonText = "退出",
                            CloseButtonText = "忽略",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        await DialogManager.ShowDialogAsync(dialog, (() => Environment.Exit(0)));
                    }
                    if (isUpdateExecute)
                    {
                        Globals.logger.Info($"isUpdateExecute = true");
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "更新完成",
                            Content = $"您已更新至 {Globals.AppVersion} , 尽情享受吧!",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        });
                    }
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

                #region OOBE

                if (Globals.GlobanConfig!.UIConfig!.isFirstUse)
                {
                    Globals.logger.Info($"检测到首次使用，开始OOBE环节");
                    bool isTutorialDone = false;
                    int totalStep = 2;

                    while (!isTutorialDone)
                    {
                        //欢迎
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"👋 欢迎!",
                            Content = $"欢迎使用.NET GUI!\n我们为您准备了一套初始化流程，以便快速配置软件！",
                            PrimaryButtonText = "下一步",
                            DefaultButton = ContentDialogButton.Primary
                        });

                        //工作目录
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"💾 配置工作目录 (1/{totalStep})",
                            Content = $"选择一个目录，作为您的工作目录，创建项目、生成和运行项目都将在此执行",
                            PrimaryButtonText = "选择",
                            DefaultButton = ContentDialogButton.Primary
                        });
                        bool isSelectDone = false;
                        while (!isSelectDone)
                        {
                            var folderDialog = new OpenFolderDialog
                            {
                                Title = "选择一个目录",
                                Multiselect = false
                            };
                            if (folderDialog.ShowDialog() == true)
                            {
                                if (Directory.Exists(folderDialog.FolderName))
                                {
                                    await DialogManager.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = $"💾 配置工作目录 (1/{totalStep})",
                                        Content = $"干得好！ \"{folderDialog.FolderName}\"真是一个完美的目录！让我们进行下一步",
                                        PrimaryButtonText = "下一步",
                                        DefaultButton = ContentDialogButton.Primary
                                    });
                                    Globals.GlobanConfig.DotnetConfig!.WorkingDirectory = folderDialog.FolderName;
                                    Globals.SaveAllConfig();
                                    isSelectDone = true;
                                }
                                else
                                {
                                    await DialogManager.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = "❔ 提示",
                                        Content = $"\"{folderDialog.FolderName}\" 似乎是一个空目录，您要？",
                                        PrimaryButtonText = "创建目录并继续",
                                        SecondaryButtonText = "重选",
                                        DefaultButton = ContentDialogButton.Primary
                                    }, (() =>
                                    {
                                        Directory.CreateDirectory(folderDialog.FolderName);
                                        Globals.GlobanConfig.DotnetConfig!.WorkingDirectory = folderDialog.FolderName;
                                        Globals.SaveAllConfig();
                                        isSelectDone = true;
                                    }));
                                }
                            }
                            else
                            {
                                await DialogManager.ShowDialogAsync(new ContentDialog
                                {
                                    Title = "❔ 提示",
                                    Content = $"您看起来并没有选择任何一个目录，如果不选择，程序将无法正常运行！",
                                    PrimaryButtonText = "重选",
                                    DefaultButton = ContentDialogButton.Primary
                                });
                            }
                        }

                        // .NET
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"🛠 配置.NET (2/{totalStep})",
                            Content = $"接下来将要配置.NET SDK ，这是整个程序的核心，全部命令都要依赖.NET SDK\n\n你可以立即前往官网下载，也可以在初始化流程结束后到此软件的下载中心下载",
                            PrimaryButtonText = "下一步",
                            SecondaryButtonText = "前往官网下载",
                            DefaultButton = ContentDialogButton.Primary
                        }, null, (() =>
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = $"https://dotnet.microsoft.com/zh-cn/download",
                                UseShellExecute = true
                            });
                        }));

                        // 完成

                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"🎉 恭喜！",
                            Content = $"您已经完成了所有初始化流程！尽情享受吧！",
                            PrimaryButtonText = "完成",
                            DefaultButton = ContentDialogButton.Primary
                        });

                        Globals.GlobanConfig.UIConfig.isFirstUse = false;
                        Globals.SaveAllConfig();

                        isTutorialDone = true;

                        Globals.logger.Info($"完成OOBE");
                    }
                }

                #endregion

                #region 工作目录可用性

                if (!Directory.Exists(Globals.GlobanConfig.DotnetConfig!.WorkingDirectory))
                {
                    Globals.logger.Warn("工作目录不可用!");

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = $"工作目录 \"{Globals.GlobanConfig.DotnetConfig.WorkingDirectory}\" 不可用！\n\n这可能导致程序频繁报错！",
                        PrimaryButtonText = "更换",
                        SecondaryButtonText = "创建",
                        CloseButtonText = "忽略",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        var dialog = new OpenFolderDialog
                        {
                            Title = "选择工作目录",
                            Multiselect = false
                        };
                        if (dialog.ShowDialog() == true)
                        {
                            Globals.GlobanConfig.DotnetConfig.WorkingDirectory = dialog.FolderName;
                            Globals.SaveAllConfig();
                        }
                    }), (() =>
                    {
                        Directory.CreateDirectory(Globals.GlobanConfig.DotnetConfig.WorkingDirectory!);
                    }));
                }

                #endregion

                Globals.logger.Info($"程序初始化完毕");
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
                    Globals.logger.Info($"用户选择 {item.Content} 页");
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
                    //Publish
                    else if (item == navViewItem_Publish)
                        frame_Navv.Navigate(_publishPage);

                    //About
                    else if (item == navViewItem_About)
                        frame_Navv.Navigate(_aboutPage);
                    //Settings
                    else if (item == navViewItem_Settings)
                        frame_Navv.Navigate(_settingsPage);
                    else
                        throw new Exception("内部错误: 不存在的NavView项");

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
                Globals.SaveAllConfig();
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