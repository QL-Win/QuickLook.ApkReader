using ApkReader.Arsc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace ApkReader;

public class ApkReader : ApkReader<ApkInfo>;

public class ApkReader<TApkInfo> where TApkInfo : ApkInfo, new()
{
    private List<IApkInfoHandler<TApkInfo>> _apkInfoHandlers;

    public IList<IApkInfoHandler<TApkInfo>> ApkInfoHandlers
    {
        get
        {
            if (_apkInfoHandlers == null)
            {
                lock (this)
                {
                    _apkInfoHandlers ??= [.. GetApkInfoHandlers()];
                }
            }
            return _apkInfoHandlers;
        }
    }

    protected virtual IEnumerable<IApkInfoHandler<TApkInfo>> GetApkInfoHandlers()
    {
        yield return new ApkInfoHandler();
    }

    protected virtual bool CheckIsResources(string fileName)
    {
        if ("AndroidManifest.xml".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }
        if ("resources.arsc".Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
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

    public TApkInfo Read(Stream apkStream)
    {
        //解压ZIP文件
        //找到两个资源文件。
        var resources = ReadResources(apkStream);
        if (!resources.ContainsKey("AndroidManifest.xml"))
        {
            throw new ApkReaderException("can not find 'AndroidManifest.xml' in apk file.");
        }
        if (!resources.ContainsKey("resources.arsc"))
        {
            throw new ApkReaderException("can not find 'resources.arsc' in apk file.");
        }
        XmlDocument xmlDocument;
        ArscFile arscFile;
        using (var ms = new MemoryStream(resources["AndroidManifest.xml"]))
        {
            xmlDocument = BinaryXmlConvert.ToXmlDocument(ms);
        }
        using (var ms = new MemoryStream(resources["resources.arsc"]))
        {
            arscFile = ArscReader.Read(ms);
        }
        var apkInfo = new TApkInfo();
        foreach (var handler in ApkInfoHandlers)
        {
            handler.Execute(xmlDocument, arscFile, apkInfo);
        }
        return apkInfo;
    }

    public virtual TApkInfo Read(string apkPath)
    {
        using (var fs = File.OpenRead(apkPath))
        {
            return Read(fs);
        }
    }
}
