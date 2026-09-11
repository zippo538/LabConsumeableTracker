using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ApplicationUser, ScientistResponseDto>()
            .ForMember(
                dest => dest.Role,
                opt => opt.MapFrom(_ => UserRole.Scientist.ToString())
            );

        CreateMap<ApplicationUser, UserProfileResponseDto>()
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.Role,
                opt => opt.Ignore()
            );
    }
}