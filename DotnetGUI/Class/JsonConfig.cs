using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernWpf.Controls;
using System.Windows.Controls;

namespace DotnetGUI.Class
{
    public class JsonConfig
    {
        public class Config
        {
            public class Root
            {
                public JsonConfig.Config.UIConfig? UIConfig { get; set; }
                public JsonConfig.Config.DotNetConfig? DotnetConfig { get; set; }
            }

            public class UIConfig
            {
                public Size WindowSize { get; set; }
                public string? SelectPage { get; set; }
            }

            public class DotNetConfig
            {
                public string? WorkingDirectory { get; set; }
            }
        }
    }
}
