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
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : string.Empty));
            CreateMap<CreatePaymentOrderDto, PaymentOrder>();
            CreateMap<UpdatePaymentOrderDto, PaymentOrder>();
        }
    }
}
