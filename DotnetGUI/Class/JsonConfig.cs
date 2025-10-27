using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGUI.Class
{
    public class JsonConfig
    {
        public class Config
        {
            public class Root
            {
                public JsonConfig.Config.UIConfig? UIConfig { get; set; }
            }

            public class UIConfig
            {
                public Size WindowSize { get; set; }
                public int SelectPage { get; set; }
            }
        }
    }
}
