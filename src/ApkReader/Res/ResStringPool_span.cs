﻿// Copyright (c) 2012 Markus Jarderot
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System;

namespace ApkReader.Res;

[Serializable]
public class ResStringPool_span
{
    public ResStringPool_ref Name { get; set; }
    public uint FirstChar { get; set; }
    public uint LastChar { get; set; }

    public bool IsEnd
    {
        get => Name.Index == null;
        set
        {
            if (value)
            {
                Name.Index = null;
            }
            else if (IsEnd)
            {
                Name.Index = 0;
            }
        }
    }
}
