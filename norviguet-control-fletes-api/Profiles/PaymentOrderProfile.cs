using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Payment;

namespace norviguet_control_fletes_api.Profiles
{
    public class PaymentOrderProfile : Profile
    {
        public PaymentOrderProfile()
        {
            CreateMap<PaymentOrder, PaymentOrderDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier.Name))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order.Status.ToString()));
            CreateMap<CreatePaymentOrderDto, PaymentOrder>();
            CreateMap<UpdatePaymentOrderDto, PaymentOrder>();
        }
    }
}
