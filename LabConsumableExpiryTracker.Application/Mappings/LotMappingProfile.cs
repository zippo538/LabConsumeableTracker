using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Mappings
{
    public class LotMappingProfile : Profile
    {
        public LotMappingProfile()
        {
            CreateMap<Lot, LotDto>();


            CreateMap<CreateLotDto, Lot>()
            .ConstructUsing(source => new Lot(
                Guid.NewGuid(),
                source.ItemId,
                source.LotNumber,
                source.SupplierLotNumber,
                new DateTimeOffset(source.ReceivedAt),
                source.SupplierName,
                source.InitialQuantity,
                source.RemainingQuantity,
                source.ExpiryDate,
                source.StorageLocation,
                LotStatus.Active));

            CreateMap<UpdateLotDto, Lot>();
        }


    }
}