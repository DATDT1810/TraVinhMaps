// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using TraVinhMaps.Application.Repositories;
using TraVinhMaps.Domain.Entities;
using TraVinhMaps.Infrastructure.CustomRepositories;
using TraVinhMaps.Infrastructure.Db;

namespace TraVinhMaps.Infrastructure.Repositories;
public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
{
    public CompanyRepository(IDbContext dbContext) : base(dbContext)
    {

    }
    public async Task<Company> GetCompanyByName(string name, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Company>.Filter.Eq(c => c.Name, name);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
