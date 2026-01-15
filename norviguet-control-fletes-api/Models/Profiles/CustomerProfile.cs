using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class CustomerProfile: Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>();
        }
    }
}
