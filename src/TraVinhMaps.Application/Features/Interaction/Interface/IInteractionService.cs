// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TraVinhMaps.Application.Features.Interaction.Models;

namespace TraVinhMaps.Application.Features.Interaction.Interface;
public interface IInteractionService
{
    Task<Domain.Entities.Interaction> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Interaction>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.Interaction> AddAsync(CreateInteractionRequest entity, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Interaction> AddTextAsync(string userId,CreateInteractionRequest entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateInteractionRequest updateInteractionRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.Interaction entity, CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<Domain.Entities.Interaction, bool>> predicate = null, CancellationToken cancellationToken = default);
}
