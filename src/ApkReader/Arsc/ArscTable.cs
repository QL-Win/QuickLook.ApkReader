using ApkReader.Res;
using System.Collections.Generic;

namespace ApkReader.Arsc;

public class ArscTable
{
    public ArscTable()
    {
        Values = new Dictionary<uint, Res_value>();
    }

    public ResTable_config Config { get; set; }
    public IDictionary<uint, Res_value> Values { get; }
}
