using DotnetGUI.Class;
using DotnetGUI.Util;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotnetGUI.Page
{
    /// <summary>
    /// ConsolePage.xaml 的交互逻辑
    /// </summary>
    public partial class ConsolePage : ModernWpf.Controls.Page
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
                   
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在初始化 ConsolePage 时发生错误", ex);
            }
        }

        public async void SendCommand(string args)
        {
            try
            {
                Globals.logger.Info($"发送指令: dotnet {args}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = args,
                        WorkingDirectory = Globals.GlobanConfig!.DotnetConfig!.WorkingDirectory,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardErrorEncoding = Encoding.UTF8,
                        StandardOutputEncoding = Encoding.UTF8
                    }
                };

                process.Start();

                textBox_Output.AppendText($"==========[{args}]=========={Environment.NewLine}");
                Globals.logger.Info($"==========[Dotnet开始运行]==========");

                await Task.WhenAll(
                        ReadOutputAsync(process.StandardOutput),
                        ReadOutputAsync(process.StandardError)
                    );

                await Task.Run(() => process.WaitForExit());
                Globals.logger.Info($"==========[Dotnet结束运行]==========");
            }
            catch (Exception ex)
            {
                
                ErrorReportDialog.Show("发生错误", "在发送命令时发生错误", ex);
            }
        }

        public async Task ReadOutputAsync(StreamReader reader)
        {
            string? line;
            while((line=await reader.ReadLineAsync()) != null)
            {
                textBox_Output.AppendText($"{line}{Environment.NewLine}");
                Globals.logger.Info($"{line}");
                textBox_Output.ScrollToEnd();
            }
        }
        #endregion


        public ConsolePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void button_Send_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(textBox_Command.Text);
            textBox_Command.Text = null;
        }

        private void textBox_Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendCommand(textBox_Command.Text);
                textBox_Command.Text = null;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button btn)
            {
                if (btn == button_ShowHelp)
                    SendCommand("-h");
                else if (btn == button_ShowSDKandRt)
                {
                    SendCommand("--list-sdks"); SendCommand("--list-runtimes");
                }
            }
            

            
        }
    }
}
