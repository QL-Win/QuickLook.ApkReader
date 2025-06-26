using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ApkReader;

internal class AabInfoHandler : IAabInfoHandler<AabInfo>
{
    public void Execute(byte[] androidManifest, byte[] resourcesPb, AabInfo aabInfo)
    {
        string text = Encoding.ASCII.GetString(androidManifest);

        var m = Regex.Match(text, @"package(?:\\)?[^\w]+([a-zA-Z0-9_.]+)");
        if (m.Success) aabInfo.PackageName = m.Groups[1].Value;

        m = Regex.Match(text, @"versionCode(?:\\)?[^\w]+([0-9]+)");
        if (m.Success) aabInfo.VersionCode = m.Groups[1].Value;

        m = Regex.Match(text, @"versionName(?:\\)?[^\w]+([0-9.]+)");
        if (m.Success) aabInfo.VersionName = m.Groups[1].Value;

        m = Regex.Match(text, @"uses-sdk.*?minSdkVersion(?:\\)?[^\w]+([0-9]+)", RegexOptions.Singleline);
        if (m.Success) aabInfo.MinSdkVersion = m.Groups[1].Value;

        m = Regex.Match(text, @"uses-sdk.*?targetSdkVersion(?:\\)?[^\w]+([0-9]+)", RegexOptions.Singleline);
        if (m.Success) aabInfo.TargetSdkVersion = m.Groups[1].Value;

        var matches = Regex.Matches(text, @"uses-permission.*?name(?:\\)?[^\w]+([a-zA-Z0-9_.]+)", RegexOptions.Singleline);
        foreach (Match match in matches)
        {
            aabInfo.Permissions.Add(match.Groups[1].Value);
        }

        m = Regex.Match(text, @"application.*?label(?:\\)?[^\w]+(@[a-zA-Z0-9/_]+)", RegexOptions.Singleline);
        if (m.Success) aabInfo.Label = m.Groups[1].Value;

        m = Regex.Match(text, @"application.*?icon(?:\\)?[^\w]+(@[a-zA-Z0-9/_]+)", RegexOptions.Singleline);
        if (m.Success) aabInfo.Icon = m.Groups[1].Value;

        string content = Encoding.UTF8.GetString(resourcesPb);
        var appNameTail = content.Substring(content.IndexOf("app_name"));
        var result = ExtractNextVisibleStringAfter(appNameTail, "app_name");

        aabInfo.Label = result;
    }

    private static string ExtractNextVisibleStringAfter(string text, string marker)
    {
        int index = text.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0) return string.Empty;

        int pos = index + marker.Length;

        while (pos < text.Length)
        {
            // Skip control characters (non-printable ASCII)
            while (pos < text.Length && (text[pos] < 0x20 || text[pos] > 0x7E))
                pos++;

            if (pos >= text.Length) break;

            // Found first printable character, start extracting string
            int start = pos;
            while (pos < text.Length && text[pos] >= 0x20 && text[pos] <= 0x7E)
                pos++;

            if (pos > start)
            {
                string candidate = text.Substring(start, pos - start);
                // Filter out too short or meaningless strings (e.g. "2")
                if (candidate.Length >= 3)
                    return candidate;
            }
        }

        // Not found, return empty string
        return string.Empty;
    }
}
