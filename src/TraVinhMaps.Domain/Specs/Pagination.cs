// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TraVinhMaps.Domain.Specs;
public class Pagination<T> where T : class
{
    public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        Count = count;
        Data = data;
    }

    public Pagination()
    {
    }

    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long Count { get; set; }
    public IReadOnlyList<T> Data { get; set; }
}
