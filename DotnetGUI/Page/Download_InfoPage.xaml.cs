using DotnetGUI.Class;
using DotnetGUI.Util;
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
using ToolLib.Library.JsonLib;

namespace DotnetGUI.Page
{
    /// <summary>
    /// Download_InfoPage.xaml 的交互逻辑
    /// </summary>
    public partial class Download_InfoPage : ModernWpf.Controls.Page
    {
        #region Obj
        #endregion

        #region Var
        public int SelectIndex;
        #endregion

        #region Func
        public void StartLoad()
        {
            progressRing_Load.Visibility = Visibility.Visible;
            label_Load.Visibility = Visibility.Visible;

            grid_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Main.IsEnabled = false;
        }

        public void EndLoad()
        {
            progressRing_Load.Visibility=Visibility.Collapsed;
            label_Load.Visibility=Visibility.Collapsed;

            grid_Main.Effect = null;
            grid_Main.IsEnabled = true;
        }

        public async void Initialize()
        {
            StartLoad();
            try
            {
                using (var client = new HttpClient())
                {
                    MessageBox.Show(Globals.DotnetIndex.release_index[SelectIndex].releases_json);
                    Globals.DotnetVersionInfo = Json.ReadJson<JsonConfig.DotnetVersionInfo.Root>(await client.GetStringAsync(Globals.DotnetIndex.release_index[SelectIndex].releases_json));
                    
                    listBox.Items.Clear();
                    for (int i = 0; i < Globals.DotnetVersionInfo.releases.Length; i++)
                    {
                        var item = new ListBoxItem
                        {
                            Content = $"{Globals.DotnetVersionInfo.releases[i].release_version}"
                        };
                        listBox.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"在初始化 {typeof(Download_InfoPage).Name} 时发生错误", ex);
            }
            EndLoad();
        }
        #endregion



        public Download_InfoPage(int selectIndex)
        {
            InitializeComponent();
            SelectIndex = selectIndex;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
    }
}
