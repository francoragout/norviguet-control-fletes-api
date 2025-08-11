using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Customer;

namespace norviguet_control_fletes_api.Profiles
{
    public class CustomerProfile: Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
        }
    }
}
