using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;

namespace norviguet_control_fletes_api.Profiles
{
    public class CarrierProfile : Profile
    {
        public CarrierProfile()
        {
            CreateMap<Carrier, CarrierDto>();
            CreateMap<CreateCarrierDto, Carrier>();
            CreateMap<UpdateCarrierDto, Carrier>();
        }
    }
}
