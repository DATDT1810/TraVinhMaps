// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.UnitOfWorks;
using TraVinhMaps.Domain.Entities;

namespace TraVinhMaps.Application.Repositories;
public interface IOcopTypeRepository : IBaseRepository<OcopType>
{
    Task<OcopType> GetOcopTypeByName(string name, CancellationToken cancellationToken = default);
}
