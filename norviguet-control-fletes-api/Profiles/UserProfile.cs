using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.User;

namespace norviguet_control_fletes_api.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<UpdateProfileDto, User>();
        }
    }
}
