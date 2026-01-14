using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
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
