using DotnetGUI.Class;
using DotnetGUI.Util;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ToolLib.Library.JsonLib;

namespace DotnetGUI.Page
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : ModernWpf.Controls.Page
    {
        #region Obj
        #endregion

        #region Var
        #endregion

        #region Func
        public void Initialize()
        {
            try
            {
                textBox_WorkingPath.Text = Globals.GlobanConfig.DotnetConfig.WorkingDirectory;
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"初始化 {typeof(SettingsPage).Name} 时发生错误", ex);
            }
        }
        #endregion

        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFolderDialog
                {
                    Title = "选择工作目录",
                    Multiselect = false
                };
                while (true)
                {                    
                    if (dialog.ShowDialog() == true)
                    {
                        var dialog2 = new ContentDialog
                        {
                            Title = "提示",
                            Content = $"是否要将 {dialog.FolderName} 设为.NET工作目录",
                            PrimaryButtonText = "确定",
                            SecondaryButtonText = "重选",
                            DefaultButton = ContentDialogButton.Primary
                        };
                        if (await dialog2.ShowAsync() == ContentDialogResult.Primary)
                            break;
                    }
                }

                textBox_WorkingPath.Text = dialog.FolderName;
                Globals.GlobanConfig.DotnetConfig.WorkingDirectory = dialog.FolderName;
                Json.WriteJson(Globals.ConfigPath, Globals.GlobanConfig);
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在选择目录时发生错误", ex);
                throw;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
    }
}
