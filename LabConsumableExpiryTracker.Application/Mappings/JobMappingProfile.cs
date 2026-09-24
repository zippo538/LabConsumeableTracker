using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Mappings;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(
                destination => destination.JobNumber,
                option => option.MapFrom(
                    source => source.JobNumber
                )
            ) .ForMember(
                destination => destination.Description,
                option => option.MapFrom(
                    source => source.Description
                )
            )
            .ForMember(
                destination => destination.Status,
                option => option.MapFrom(
                    source => source.Status
                )
            )
            .ForMember(
                destination => destination.StartedAt,
                option => option.MapFrom(
                    source => source.StartedAt
                )
            )
            .ForMember(
                destination => destination.CompletedAt,
                option => option.MapFrom(
                    source => source.CompletedAt
                )
            )
            ;
        CreateMap<CreateJobDto, Job>();
        CreateMap<UpdateJobDto, Job>();
    }
}