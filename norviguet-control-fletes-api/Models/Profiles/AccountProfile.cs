using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.Account;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<User, AccountDto>();
        }
    }
}
