using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.Login;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Mappings;

public class LoginMappingProfile : Profile
{
    public LoginMappingProfile()
    {
        CreateMap<ApplicationUser, LoginResponseDto>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(src => src.FullName)
            );
    }
}