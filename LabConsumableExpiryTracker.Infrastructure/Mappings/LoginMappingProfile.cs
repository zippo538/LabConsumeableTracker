using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Infrastructure.Models;

namespace LabConsumableExpiryTracker.Infrastructure.Mappings;

public class LoginMappingProfile : Profile
{
    public LoginMappingProfile()
    {
        CreateMap<ApplicationUser, LoginResponseDto>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            );
    }
}
