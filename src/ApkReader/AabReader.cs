using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

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
        if (fileName.StartsWith("base/res/mipmap-"))
        {
            return true;
        }
        return false;
    }

    protected virtual void ReadResources(Stream stream, out Dictionary<string, byte[]> dic, out HashSet<string> abiSet)
    {
        abiSet = [];
        dic = new Dictionary<string, byte[]>(StringComparer.CurrentCultureIgnoreCase);

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
                else if (entry.FullName.StartsWith("base/lib/"))
                {
                    var relativePath = entry.FullName.Substring("base/lib/".Length);
                    int slashIndex = relativePath.IndexOf('/');

                    if (slashIndex > 0)
                    {
                        string abi = relativePath.Substring(0, slashIndex);
                        abiSet.Add(abi);
                    }
                }
            }
        }
    }

    public TAabInfo Read(Stream apkStream)
    {
        //解压ZIP文件
        //找到两个资源文件。
        ReadResources(apkStream, out var resources, out var abiSet);

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
        foreach (var res in resources)
        {
            if (res.Key.StartsWith("base/res/mipmap-"))
            {
                aabInfo.Icons.Add(ComputeMD5Hash(res.Key).Substring(0, 6), res.Key);
            }
        }
        foreach (var abi in abiSet)
        {
            aabInfo.Abis.Add(abi);
        }
        return aabInfo;

        static string ComputeMD5Hash(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }

    public virtual TAabInfo Read(string apkPath)
    {
        using (var fs = File.OpenRead(apkPath))
        {
            return Read(fs);
        }
    }
}
