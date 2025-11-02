using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DotnetGUI.Util
{
    public class NavgationViewController
    {
        public void Navgation(DependencyObject current, string item)
        {
            if (Window.GetWindow(current) is MainWindow window &&
                window.FindName("navView") is NavigationView navView &&
                navView.FindName(item) is NavigationViewItem navViewItem
                )
            {
                navView.SelectedItem = navViewItem;
            }
            else
                throw new Exception("找不到目标控件");
        }
    }
}
