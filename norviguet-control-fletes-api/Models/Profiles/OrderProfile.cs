using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer == null ? null : $"{src.Customer.Name} {src.Customer.BusinessName}".Trim()))
                .ForMember(dest => dest.CarriersCount, opt => opt.MapFrom(src =>
                    src.DeliveryNotes.Select(dn => dn.CarrierId).Distinct().Count()))
                .ForMember(dest => dest.DeliveryNotesCount, opt => opt.MapFrom(src =>
                    src.DeliveryNotes.Count(dn => dn.Status != DeliveryNoteStatus.Cancelled)));
            CreateMap<OrderCreateDto, Order>();
            CreateMap<OrderUpdateDto, Order>();
            CreateMap<OrderStatusUpdateDto, Order>();
        }
    }
}
