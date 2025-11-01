using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Payment;

namespace norviguet_control_fletes_api.Profiles
{
    public class PaymentOrderProfile : Profile
    {
        public PaymentOrderProfile()
        {
            CreateMap<PaymentOrder, PaymentOrderDto>();
            CreateMap<CreatePaymentOrderDto, PaymentOrder>();
            CreateMap<UpdatePaymentOrderDto, PaymentOrder>();
        }
    }
}
