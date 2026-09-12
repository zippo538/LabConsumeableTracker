using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Mappings
{
    public class LotMappingProfile : Profile
    {
        public LotMappingProfile()
        {
            CreateMap<Lot, LotDto>();


            CreateMap<CreateLotDto, Lot>()
            .ConstructUsing(source => Lot.Create(
                source.ItemId,
                source.LotNumber,
                source.SupplierLotNumber,
                new DateTimeOffset(source.ReceivedAt),
                source.SupplierName,
                source.InitialQuantity,
                source.ExpiryDate,
                source.StorageLocation));

            CreateMap<UpdateLotDto, Lot>();
        }


    }
}