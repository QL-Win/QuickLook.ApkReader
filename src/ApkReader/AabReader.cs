using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace ApkReader;

public class AabReader : AabReader<AabInfo>;

public class AabReader<TAabInfo> where TAabInfo : AabInfo, new()
{
    private List<IAabInfoHandler<TAabInfo>> _aabInfoHandlers;

    public IList<IAabInfoHandler<TAabInfo>> AabInfoHandlers
    {
        get
        {
            if (_aabInfoHandlers == null)
            {
                lock (this)
                {
                    _aabInfoHandlers ??= [.. GetAabInfoHandlers()];
                }
            }
            return _aabInfoHandlers;
        }
    }

    protected virtual IEnumerable<IAabInfoHandler<TAabInfo>> GetAabInfoHandlers()
    {
        yield return new AabInfoHandler();
    }

    protected virtual bool CheckIsResources(string fileName)
    {
        if ("base/manifest/AndroidManifest.xml".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }
        if ("base/resources.pb".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }
        return false;
    }

    protected virtual Dictionary<string, byte[]> ReadResources(Stream stream)
    {
        var dic = new Dictionary<string, byte[]>(StringComparer.CurrentCultureIgnoreCase);
        using (var zip = new ZipArchive(stream))
        {
            foreach (var entry in zip.Entries)
            {
                Console.WriteLine($"Processing entry: {entry.FullName}");

                if (CheckIsResources(entry.FullName))
                {
                    using (var fs = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        dic[entry.FullName] = ms.ToArray();
                    }
                }
            }
        }
        return dic;
    }

    public TAabInfo Read(Stream apkStream)
    {
        //解压ZIP文件
        //找到两个资源文件。
        var resources = ReadResources(apkStream);
        if (!resources.ContainsKey("base/manifest/AndroidManifest.xml"))
        {
            throw new ApkReaderException("can not find 'AndroidManifest.xml' in aab file.");
        }
        if (!resources.ContainsKey("base/resources.pb"))
        {
            throw new ApkReaderException("can not find 'resources.pb' in aab file.");
        }
        byte[] androidManifest;
        byte[] resourcesPb;
        using (var ms = new MemoryStream(resources["base/manifest/AndroidManifest.xml"]))
        {
            androidManifest = ms.ToArray();
        }
        using (var ms = new MemoryStream(resources["base/resources.pb"]))
        {
            resourcesPb = ms.ToArray();
        }
        var aabInfo = new TAabInfo();
        foreach (var handler in AabInfoHandlers)
        {
            handler.Execute(androidManifest, resourcesPb, aabInfo);
        }
        return aabInfo;
    }

    public virtual TAabInfo Read(string apkPath)
    {
        using (var fs = File.OpenRead(apkPath))
        {
            return Read(fs);
        }
    }
}
