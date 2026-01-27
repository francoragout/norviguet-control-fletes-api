using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.PaymentOrder;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class PaymentOrderProfile: Profile
    {
        public PaymentOrderProfile()
        {
            CreateMap<PaymentOrder, PaymentOrderDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier.Name))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order.Status.ToString()));
            CreateMap<PaymentOrderCreateDto, PaymentOrder>();
            CreateMap<PaymentOrderUpdateDto, PaymentOrder>();
        }
    }
}
