// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TraVinhMaps.Application.Features.Feedback.Models;

namespace TraVinhMaps.Application.Features.Feedback.Mapper;
public class FeedbackMappingProfile : Profile
{
    public FeedbackMappingProfile()
    {
        // Mapping from Domain.Entities.Feedback to FeedbackRequest
        CreateMap<Domain.Entities.Feedback, FeedbackRequest>()
            .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignore Images since FeedbackRequest uses IFormFile
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content)); // Map only Content

        // Mapping from FeedbackRequest to Domain.Entities.Feedback
        CreateMap<FeedbackRequest, Domain.Entities.Feedback>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Ignore UserId, will be set manually in service
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Images, opt => opt.Ignore()); // Ignore Images, will be handled in ImageFeedbackService

        // Mapping from Domain.Entities.Feedback to FeedbackResponse
        CreateMap<Domain.Entities.Feedback, FeedbackResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => "Unknown"))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
