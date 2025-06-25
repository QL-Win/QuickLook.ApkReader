using ApkReader.Arsc;
using System.Xml;

namespace ApkReader;

public interface IApkInfoHandler<in TApkInfo> where TApkInfo : ApkInfo
{
    public void Execute(XmlDocument androidManifest, ArscFile resources, TApkInfo apkInfo);
}
