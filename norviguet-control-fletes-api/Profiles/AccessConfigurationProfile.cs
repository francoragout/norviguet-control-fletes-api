using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.AccessConfiguration;

namespace norviguet_control_fletes_api.Profiles
{
    public class AccessConfigurationProfile : Profile
    {
        public AccessConfigurationProfile()
        {
            CreateMap<AccessConfiguration, AccessConfigurationDto>();
            CreateMap<CreateAccessConfigurationDto, AccessConfiguration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore Id for creation
        }
    }
    
}
