
using AutoMapper;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Mappings
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            CreateMap<Lot, LotDto>();
            CreateMap<Item, ItemDto>()
            .ForMember(
            destination => destination.Lots,
            options => options.MapFrom(
            source => source.Lots))
            .ForMember(
            destination => destination.TotalRemainingQuantity,
            options => options.Ignore())
            .ForMember(
            destination => destination.StockStatus,
            options => options.Ignore());

            CreateMap<UpdateItemDto, Item>();

            CreateMap<Lot, LotSummaryDto>()
            .ForMember(
            destination => destination.LotId,
            options => options.MapFrom(source => source.Id))
            .ForMember(
            destination => destination.Status,
            options => options.MapFrom(source => source.AdministrativeStatus.ToString()));

            CreateMap<Item, ItemDto>()
                .ForMember(
                    destination => destination.TotalRemainingQuantity,
                    options => options.MapFrom(source =>
                        source.Lots.Sum(lot => lot.RemainingQuantity)))
                .ForMember(
                    destination => destination.IsLowStock,
                    options => options.MapFrom(source =>
                        source.Lots.Sum(lot => lot.RemainingQuantity)
                        <= source.MinimumStock))
                .ForMember(
                    destination => destination.Lots,
                    options => options.MapFrom(source => source.Lots));
        }
    }
}