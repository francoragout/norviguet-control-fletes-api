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
            CreateMap<UpdateUserRoleDto, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => ParseUserRole(src.Role)));
            CreateMap<UpdateUserAccountDto, User>();
        }

        private static UserRole ParseUserRole(string? role)
        {
            return Enum.TryParse<UserRole>(role, true, out var parsed) ? parsed : UserRole.Pending;
        }
    }
}
