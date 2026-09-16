using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Common.Mappings;

public class DisposalMappingProfile : Profile
{
    public DisposalMappingProfile()
    {
        CreateMap<Disposal, DisposalDto>();
    }
}
