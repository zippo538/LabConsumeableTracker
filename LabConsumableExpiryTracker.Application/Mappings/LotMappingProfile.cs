using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Mappings
{
    public class LotMappingProfile : Profile
    {
        public LotMappingProfile()
        {
            CreateMap<Lot, LotDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.Name : null))
            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.Code : null))
            .ForMember(dest => dest.BaseUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.BaseUnit.ToString() : null));
            ;


            CreateMap<CreateLotDto, Lot>()
            .ConstructUsing(source => Lot.Create(
                Guid.Parse(source.ItemId),
                source.LotNumber,
                source.SupplierLotNumber,
                source.ReceivedAt.ToUniversalTime(),
                source.SupplierName,
                source.InitialQuantity,
                source.ExpiryDate,
                source.StorageLocation));
            CreateMap<UpdateLotDto, Lot>();
        }


    }
}