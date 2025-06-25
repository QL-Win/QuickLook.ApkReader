using ApkReader.Res;
using System.Collections.Generic;

namespace ApkReader.Arsc;

public class ArscPackage
{
    public ArscPackage()
    {
        Tables = [];
    }

    public uint Id { get; set; }
    public string Name { get; set; }
    public ResStringPool TypeStringPool { get; set; }
    public ResStringPool KeyStringPool { get; set; }
    public uint[] TypeSpecsData { get; set; }
    public IList<ArscTable> Tables { get; }
}
