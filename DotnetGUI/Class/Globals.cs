using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotnetGUI.Class
{
    public static class Globals
    {
        public static string? ExecutePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ConfigPath = $"{ExecutePath}\\config";
        public static JsonConfig.Config.Root? GlobanConfig = null;
    }
}
