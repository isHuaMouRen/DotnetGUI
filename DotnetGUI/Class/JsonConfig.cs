using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernWpf.Controls;
using System.Windows.Controls;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

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
                public string? DotnetState { get; set; }
            }
        }

        public class DotnetDownloadIndex
        {
            public class Root
            {
                [JsonProperty("releases-index")]
                public JsonConfig.DotnetDownloadIndex.VersionInfo[]? release_index { get; set; }
            }

            public class VersionInfo
            {
                [JsonProperty("channel-version")]
                public string? channel_version { get; set; }
                [JsonProperty("latest-release")]
                public string? latest_release { get; set; }
                [JsonProperty("latest-release-date")]
                public string? latest_version_date { get; set; }
                public bool security { get; set; }
                [JsonProperty("latest-sdk")]
                public string? latest_sdk { get; set; }
                public string? product { get; set; }
                [JsonProperty("release-type")]
                public string? release_type { get; set; }
                [JsonProperty("releases.json")]
                public string? releases_json { get; set; }
            }
        }

        public class DotnetVersionInfo
        {
            public class Root
            {
                [JsonProperty("channel-version")]
                public string? channel_version { get; set; }
                [JsonProperty("latest-release")]
                public string? latest_release { get; set; }
                [JsonProperty("latest-release-date")]
                public string? latest_version_date { get; set; }
                [JsonProperty("latest-sdk")]
                public string? latest_sdk { get; set; }
                public string? product { get; set; }
                [JsonProperty("release-type")]
                public string? release_type { get; set; }
                public JsonConfig.DotnetVersionInfo.ReleaseInfo[]? releases { get; set; }
            }

            public class ReleaseInfo
            {
                [JsonProperty("release-date")]
                public string? release_date { get; set; }
                [JsonProperty("release-version")]
                public string? release_version { get; set; }
                public bool security { get; set; }
                public JsonConfig.DotnetVersionInfo.SdkInfo? sdk { get; set; }
            }

            public class SdkInfo
            {
                public string? version { get; set; }
                public JsonConfig.DotnetVersionInfo.FileInfo[]? files { get; set; }
            }

            public class FileInfo
            {
                public string? name { get; set; }
                public string? url { get; set; }
                public string? hash { get; set; }
            }
        }
    }
}
