using AutoMapper;
using InventoryService.Dtos;
using InventoryService.Models;

namespace InventoryService.Profiles
{
    public class InventoryMappingProfile : Profile
    {
        public InventoryMappingProfile()
        {
            CreateMap<InventoryItem, InventoryItemResponseDto>()
                .ForMember(dest => dest.TotalOnHand, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityOnHand)))
                .ForMember(dest => dest.TotalReserved, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityReserved)))
                .ForMember(dest => dest.TotalAvailable, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityOnHand - level.QuantityReserved)))
                .ForMember(dest => dest.StockLevels, opt => opt.MapFrom(src => src.StockLevels ?? new List<StockLevel>()));

            CreateMap<StockLevel, StockLevelResponseDto>()
                .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse!.Code))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse!.Name))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.QuantityOnHand - src.QuantityReserved));

            CreateMap<InventoryItemUpsertDto, InventoryItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StockLevels, opt => opt.Ignore());

            CreateMap<StockLevelUpsertDto, StockLevel>()
                .ForMember(dest => dest.InventoryItemId, opt => opt.Ignore())
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryItem, opt => opt.Ignore());

            CreateMap<Warehouse, WarehouseResponseDto>()
                .ForMember(dest => dest.TotalOnHand, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityOnHand)))
                .ForMember(dest => dest.TotalReserved, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityReserved)))
                .ForMember(dest => dest.TotalAvailable, opt => opt.MapFrom(src => src.StockLevels!.Sum(level => level.QuantityOnHand - level.QuantityReserved)));

            CreateMap<WarehouseUpsertDto, Warehouse>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StockLevels, opt => opt.Ignore());

            CreateMap<StockTransaction, StockTransactionResponseDto>()
                .ForMember(dest => dest.ItemSku, opt => opt.MapFrom(src => src.InventoryItem!.Sku))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.InventoryItem!.Name))
                .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse!.Code))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse!.Name));
        }
    }
}
