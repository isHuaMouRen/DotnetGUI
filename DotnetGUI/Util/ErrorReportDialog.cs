using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DotnetGUI.Util
{
    public static class ErrorReportDialog
    {
        public static async void Show(string title,string message,Exception ex)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = new StackPanel
                {
                    Children =
                    {
                        new System.Windows.Controls.ProgressBar
                        {
                            Value=100,
                            Margin=new System.Windows.Thickness(0,0,0,10),
                            Foreground=new SolidColorBrush(Color.FromRgb(255,50,50))
                        },
                        new Label
                        {
                            Content=message,
                            Margin=new System.Windows.Thickness(0,0,0,10)
                        },
                        new TextBox
                        {
                            Text=ex.ToString(),
                            IsReadOnly=true
                        }
                    }
                },
                PrimaryButtonText = "继续",
                SecondaryButtonText = "终止",
                DefaultButton = ContentDialogButton.Primary
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
                Environment.Exit(1);
        }
    }
}
