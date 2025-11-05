using HuaZi.Library.Json;
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
using ModernWpf.Media.Animation;
using static DotnetGUI.Class.Globals;
using HuaZi.Library.Hash;

namespace DotnetGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Obj
        public DrillInNavigationTransitionInfo frameAnimation = new DrillInNavigationTransitionInfo();
        #endregion

        #region Var
        public Dictionary<string, Type> PageMap = new Dictionary<string, Type>();
        #endregion

        #region Func
        public void Initialize()
        {
            try
            {
                logger.Info($"程序开始构造初始化");

                #region 预加载Pages
                void AddPage(Type t) => PageMap.Add(t.Name, t);
                AddPage(typeof(HomePage));
                AddPage(typeof(DownloadPage));
                AddPage(typeof(ConsolePage));
                AddPage(typeof(NewPage));
                AddPage(typeof(BuildPage));
                AddPage(typeof(PublishPage));
                AddPage(typeof(AboutPage));
                AddPage(typeof(SettingsPage));

                foreach (var item in PageMap)
                {
                    logger.Info($"已预加载Page: {item}");
                }

                #endregion

                #region 读写配置
                //如没有就写入
                if (!File.Exists(Globals.ConfigPath))
                {
                    logger.Info($"未检测到配置文件，即将创建");
                    Globals.GlobalConfig = new JsonConfig.Config.Root
                    {
                        UIConfig = new JsonConfig.Config.UIConfig
                        {
                            WindowSize = new Size(800, 450),
                            isFirstUse = true,
                            Theme="Light"
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
                Globals.GlobalConfig = Json.ReadJson<JsonConfig.Config.Root>(Globals.ConfigPath);

                //应用
                this.Width = Globals.GlobalConfig.UIConfig!.WindowSize.Width;
                this.Height = Globals.GlobalConfig.UIConfig!.WindowSize.Height;

                navView.SelectedItem = navViewItem_Home;

                Globals.SetTheme(navView);

                #endregion

                logger.Info($"程序结束构造初始化");
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
                logger.Info($"程序开始初始化");

                #region 启动参数检查
                logger.Info($"开始启动参数检测...");
                string[] args = Environment.GetCommandLineArgs();
                bool isShellExecute = false;
                bool isUpdateExecute = false;

                foreach (var arg in args)
                {
                    logger.Info($"启动参数: {arg}");
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
                        logger.Info($"isShellExecute = false");
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
                        logger.Info($"isUpdateExecute = true");
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

                if (Globals.GlobalConfig!.UIConfig!.isFirstUse)
                {
                    logger.Info($"检测到首次使用，开始OOBE环节");
                    bool isTutorialDone = false;
                    int totalStep = 3;

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

                        //主题
                        var radioButtonLight = new RadioButton
                        {
                            Content = "明亮",
                            IsChecked = true,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        radioButtonLight.Click += ((s, e) => { Globals.GlobalConfig.UIConfig.Theme = "Light"; Globals.SetTheme(navView); });
                        var radioButtonDark = new RadioButton
                        {
                            Content = "暗黑",
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        radioButtonDark.Click += ((s, e) => { Globals.GlobalConfig.UIConfig.Theme = "Dark"; Globals.SetTheme(navView); });
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"✨ 选择主题 (1/{totalStep})",
                            Content = new StackPanel
                            {
                                Children =
                                {
                                    radioButtonLight,
                                    radioButtonDark,
                                    new Label
                                    {
                                        Content=$"选择一个你喜欢的主题吧！"
                                    }
                                }
                            },
                            PrimaryButtonText = "下一步"
                        });

                        //工作目录
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = $"💾 配置工作目录 (2/{totalStep})",
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
                                        Title = $"💾 配置工作目录 (2/{totalStep})",
                                        Content = $"干得好！ \"{folderDialog.FolderName}\"真是一个完美的目录！让我们进行下一步",
                                        PrimaryButtonText = "下一步",
                                        DefaultButton = ContentDialogButton.Primary
                                    });
                                    Globals.GlobalConfig.DotnetConfig!.WorkingDirectory = folderDialog.FolderName;
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
                                        Globals.GlobalConfig.DotnetConfig!.WorkingDirectory = folderDialog.FolderName;
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
                            Title = $"🛠 配置.NET (3/{totalStep})",
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

                        Globals.GlobalConfig.UIConfig.isFirstUse = false;
                        Globals.SaveAllConfig();

                        isTutorialDone = true;

                        logger.Info($"完成OOBE");
                    }
                }

                #endregion

                #region 工作目录可用性

                if (!Directory.Exists(Globals.GlobalConfig.DotnetConfig!.WorkingDirectory))
                {
                    logger.Warn("工作目录不可用!");

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = $"工作目录 \"{Globals.GlobalConfig.DotnetConfig.WorkingDirectory}\" 不可用！\n\n这可能导致程序频繁报错！",
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
                            Globals.GlobalConfig.DotnetConfig.WorkingDirectory = dialog.FolderName;
                            Globals.SaveAllConfig();
                        }
                    }), (() =>
                    {
                        Directory.CreateDirectory(Globals.GlobalConfig.DotnetConfig.WorkingDirectory!);
                    }));
                }

                #endregion                

                logger.Info($"程序初始化完毕");
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
                    logger.Info($"用户选择 {item.Content} 页");

                    frame_Navv.Navigate(PageMap[(string)item.Tag], null, frameAnimation);
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
                Globals.GlobalConfig!.UIConfig!.WindowSize = new Size(this.Width, this.Height);
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