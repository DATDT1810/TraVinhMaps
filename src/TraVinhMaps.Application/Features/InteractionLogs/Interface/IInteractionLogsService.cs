// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TraVinhMaps.Application.Features.InteractionLogs.Interface;
public interface IInteractionLogsService
{
    Task<Domain.Entities.InteractionLogs> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.InteractionLogs>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.InteractionLogs> AddAsync(Domain.Entities.InteractionLogs entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.InteractionLogs entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.InteractionLogs entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.InteractionLogs, bool>> predicate = null, CancellationToken cancellationToken = default);
}
