using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
