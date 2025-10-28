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
            }
        }

        public class DotnetDownloadIndex
        {
            public class Root
            {
                [JsonPropertyName("release-index")]
                public JsonConfig.DotnetDownloadIndex.VersionInfo[]? release_index { get; set; }
            }

            public class VersionInfo
            {
                [JsonPropertyName("channel-version")]
                public string? channel_version { get; set; }
                [JsonPropertyName("latest-release")]
                public string? latest_release { get; set; }
                [JsonPropertyName("latest-release-date")]
                public string? latest_version_date { get; set; }
                public bool security { get; set; }
                [JsonPropertyName("latest-sdk")]
                public string? latest_sdk { get; set; }
                public string? product { get; set; }
                [JsonPropertyName("release-type")]
                public string? release_type { get; set; }
                [JsonPropertyName("release.json")]
                public string? release_json { get; set; }
            }
        }

        public class DotnetVersionInfo
        {
            public class Root
            {
                [JsonPropertyName("channel-version")]
                public string? channel_version { get; set; }
                [JsonPropertyName("latest-release")]
                public string? latest_release { get; set; }
                [JsonPropertyName("latest-release-date")]
                public string? latest_version_date { get; set; }
                [JsonPropertyName("latest-sdk")]
                public string? latest_sdk { get; set; }
                public string? product { get; set; }
                [JsonPropertyName("release-type")]
                public string? release_type { get; set; }
            }

            public class ReleaseInfo
            {
                [JsonPropertyName("release-date")]
                public string? release_date { get; set; }
                [JsonPropertyName("release-version")]
                public string? release_version { get; set; }
                public JsonConfig.DotnetVersionInfo.SdkInfo? sdk { get; set; }
            }

            public class SdkInfo
            {
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
