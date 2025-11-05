using DotnetGUI.Class;
using DotnetGUI.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using HuaZi.Library.Json;

namespace DotnetGUI.Page
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : ModernWpf.Controls.Page
    {
        #region Obj
        #endregion

        #region Var
        #endregion

        #region Func
        public async void Initialize()
        {
            try
            {
                Globals.logger.Info($"DownloadPage 开始初始化");
                StartLoad();
                using (var client = new HttpClient())
                {
                    button_Download.IsEnabled = false;
                    listBox.SelectedIndex = -1;

                    string result = await client.GetStringAsync("https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/releases-index.json");
                    Globals.logger.Info($"获取.NET主索引: {result}");
                    Globals.DotnetIndex = Json.ReadJson<JsonConfig.DotnetDownloadIndex.Root>(result);

                    listBox.Items.Clear();
                    for (int i = 0; i < Globals.DotnetIndex.release_index!.Length; i++)
                    {
                        if (Globals.DotnetIndex.release_index[i].product == ".NET")
                        {
                            string isSupport;
                            string SupportTime;

                            if (Globals.DotnetIndex.release_index[i].security)
                                isSupport = "受保护";
                            else
                                isSupport = "不受保护";

                            if (Globals.DotnetIndex.release_index[i].release_type == "lts")
                                SupportTime = "长期支持";
                            else
                                SupportTime = "短期支持";

                            var item = new ListBoxItem
                            {
                                Content = $".NET {Globals.DotnetIndex.release_index[i].channel_version} | {SupportTime} | {isSupport}"
                            };
                            listBox.Items.Add(item);
                        }
                    }
                }
                EndLoad();
                Globals.logger.Info($"DownloadPage 结束初始化");
            }
            catch (Exception ex)
            {
                
                EndLoad();
                ErrorReportDialog.Show("发生错误", $"在初始化 {typeof(DownloadPage).Name} 时发生错误", ex);
            }
        }

        public void StartLoad()
        {
            Globals.logger.Info($"开始加载");
            progressRing_Load.Visibility = Visibility.Visible;
            label_Load.Visibility = Visibility.Visible;

            grid_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Main.IsEnabled = false;
         }

        public void EndLoad()
        {
            Globals.logger.Info($"结束加载");
            progressRing_Load.Visibility = Visibility.Collapsed;
            label_Load.Visibility = Visibility.Collapsed;

            grid_Main.Effect = null;
            grid_Main.IsEnabled = true;
        }
        #endregion


        public DownloadPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (listBox.SelectedIndex >= 0)
                {
                    button_Download.IsEnabled = true;
                    label_Version.Content = ".NET " + Globals.DotnetIndex.release_index![listBox.SelectedIndex].channel_version;
                    label_SubVersion.Content = "最新版本: " + Globals.DotnetIndex.release_index[listBox.SelectedIndex].latest_release;
                    label_ReleaseDate.Content = "发布日期: " + Globals.DotnetIndex.release_index[listBox.SelectedIndex].latest_version_date;
                }
                

            }
            catch (Exception ex)
            {
                
                ErrorReportDialog.Show("发生错误", "在加载版本信息时发生错误", ex);
            }
        }

        private void button_Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.logger.Info($"导航进入详细下载页...");
                var frame = this.NavigationService;
                frame.Navigate(new Download_InfoPage(listBox.SelectedIndex));
            }
            catch (Exception ex)
            {
                
                ErrorReportDialog.Show("发生错误", "在尝试加载 Download_InfoPage 时发生错误", ex);
            }
        }
    }
}
