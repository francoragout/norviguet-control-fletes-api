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
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? (string.IsNullOrEmpty(src.Customer.BusinessName) ? src.Customer.Name : $"{src.Customer.Name} {src.Customer.BusinessName}") : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? (string.IsNullOrEmpty(src.Seller.Zone) ? src.Seller.Name : $"{src.Seller.Name} {src.Seller.Zone}") : null))
                .ForMember(dest => dest.PendingDeliveryNoteNumbersCount, opt => opt.MapFrom(src => src.DeliveryNotes != null ? src.DeliveryNotes.Count(dn => dn.Status == DeliveryNoteStatus.Pending) : 0))
                .ForMember(dest => dest.HasInvoice, opt => opt.MapFrom(src => src.Invoice != null));
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
