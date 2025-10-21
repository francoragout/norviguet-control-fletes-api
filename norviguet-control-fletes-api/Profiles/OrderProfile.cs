using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Order;

namespace norviguet_control_fletes_api.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier != null ? src.Carrier.Name : null))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
                .ForMember(dest => dest.CustomerLocation, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Location : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Name : null));
            CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();
            CreateMap<UpdateOrderStatusDto, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseOrderStatus(src.Status)));
        }

        private static OrderStatus ParseOrderStatus(string? status)
        {
            return Enum.TryParse<OrderStatus>(status, true, out var parsed) ? parsed : OrderStatus.Pending;
        }
    }
}
