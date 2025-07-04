// Copyright (c) 2012 Markus Jarderot
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System;

namespace ApkReader.Res;

[Serializable]
public class ResXMLTree_attribute
{
    public ResStringPool_ref Namespace { get; set; }
    public ResStringPool_ref Name { get; set; }
    public ResStringPool_ref RawValue { get; set; }
    public Res_value TypedValue { get; set; }
}
