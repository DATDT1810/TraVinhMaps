// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TraVinhMaps.Application.Features.Review.Models;
using TraVinhMaps.Application.Features.SellingLink.Models;

namespace TraVinhMaps.Application.Features.Review.Mappers;
public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<Domain.Entities.Review, CreateReviewRequest>().ReverseMap();
        CreateMap<Domain.Entities.Reply, CreateReplyRequest>().ReverseMap();
    }
}
