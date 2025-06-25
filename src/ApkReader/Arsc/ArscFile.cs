using ApkReader.Res;
using System;
using System.Collections.Generic;

namespace ApkReader.Arsc;

public class ArscFile
{
    public ArscFile()
    {
        Packages = [];
    }

    public long Length { get; set; }
    public uint PackageCount => Convert.ToUInt32(Packages.Count);
    public ResStringPool GlobalStringPool { get; set; }
    public IList<ArscPackage> Packages { get; }

    internal void Clear()
    {
        if (GlobalStringPool != null)
        {
            GlobalStringPool.StringData.Clear();
            GlobalStringPool.StyleData.Clear();
        }
        foreach (var package in Packages)
        {
            if (package.TypeSpecsData != null && package.TypeSpecsData.Length > 0)
            {
                package.TypeSpecsData = null;
            }
            if (package.KeyStringPool != null)
            {
                package.KeyStringPool.StringData.Clear();
                package.KeyStringPool.StyleData.Clear();
            }
            if (package.TypeStringPool != null)
            {
                package.TypeStringPool.StringData.Clear();
                package.TypeStringPool.StyleData.Clear();
            }
            foreach (var table in package.Tables)
            {
                table.Values.Clear();
            }
        }
        Packages.Clear();
    }
}
