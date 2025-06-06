// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;

namespace TraVinhMaps.Application.Features.SellingLink.Mappers;
public class SellingLinkMapper
{
    private static readonly Lazy<IMapper> Lazy = new Lazy<IMapper>(() =>
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
            cfg.AddProfile<SellingLinkMappingProfile>();
        });
        var mapper = config.CreateMapper();
        return mapper;
    });
    public static IMapper Mapper => Lazy.Value;
}
