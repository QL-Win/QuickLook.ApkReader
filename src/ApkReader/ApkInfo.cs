using System.Collections.Generic;

namespace ApkReader;

public class ApkInfo
{
    public string VersionName { get; set; }
    public string VersionCode { get; set; }

    public string TargetSdkVersion { get; set; }
    public List<string> Permissions { get; } = [];
    public string PackageName { get; set; }
    public string MinSdkVersion { get; set; }
    public string Icon { get; set; }
    public Dictionary<string, string> Icons { get; } = [];
    public string Label { get; set; }
    public Dictionary<string, string> Labels { get; } = [];
    public bool HasIcon => Icons.Count > 0 || !string.IsNullOrEmpty(Icon);
    public List<string> Locales { get; } = [];
    public List<string> Densities { get; } = [];
    public string LaunchableActivity { get; set; }
}
