// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TraVinhMaps.Application.Features.Interaction.Models;
using TraVinhMaps.Application.Features.OcopType.Models;

namespace TraVinhMaps.Application.Features.Interaction.Mappers;
public class InteractionMappingProfile : Profile
{
    public InteractionMappingProfile()
    {
        CreateMap<Domain.Entities.Interaction, CreateInteractionRequest>().ReverseMap();
        CreateMap<Domain.Entities.Interaction, UpdateInteractionRequest>().ReverseMap();
    }
}
