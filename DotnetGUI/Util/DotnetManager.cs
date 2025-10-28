using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGUI.Util
{
    public class DotnetManager
    {
        public static void SetSettings()
        {
			try
			{

			}
			catch (Exception ex)
			{
                ErrorReportDialog.Show("发生错误", "在为.NET应用设置时发生错误", ex);
			}
        }
    }
}
