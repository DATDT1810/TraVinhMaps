// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TraVinhMaps.Application.Features.OcopType.Interface;
public interface IOcopTypeService
{
    Task<Domain.Entities.OcopType> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.OcopType> GetOcopTypeByName(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.OcopType>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.OcopType> AddAsync(Domain.Entities.OcopType entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.OcopType entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.OcopType, bool>> predicate = null, CancellationToken cancellationToken = default);
}
