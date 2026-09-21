using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Common.Mappings;

public class DisposalMappingProfile : Profile
{
    public DisposalMappingProfile()
    {
        CreateMap<Disposal, DisposalDto>();

        CreateMap<Lot, LotSnapshotDto>()
            .ForMember(dest => dest.AdministrativeStatus, opt => opt.MapFrom(src => src.AdministrativeStatus.ToString()))
            .ForMember(dest => dest.RowVersion, opt => opt.MapFrom(src => Convert.ToBase64String(src.RowVersion)));

    }
}
