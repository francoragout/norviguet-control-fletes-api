using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Invoice;

namespace norviguet_control_fletes_api.Profiles
{
    public class InvoiceProfile: Profile
    {
        public InvoiceProfile()
        {
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier.Name));
            CreateMap<CreateInvoiceDto, Invoice>();
            CreateMap<UpdateInvoiceDto, Invoice>();
        }
    }
}
