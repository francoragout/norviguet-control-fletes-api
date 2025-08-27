using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Order;
using norviguet_control_fletes_api.Models.Carrier;

namespace norviguet_control_fletes_api.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier != null ? src.Carrier.Name : null));

            CreateMap<CreateOrderDto, Order>();
        }
    }
}
