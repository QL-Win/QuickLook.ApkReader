using System.Xml;

namespace ApkReader;

public interface IAabInfoHandler<in TAabInfo> where TAabInfo : AabInfo
{
    public void Execute(byte[] androidManifest, byte[] resourcesPb, TAabInfo apkInfo);
}
