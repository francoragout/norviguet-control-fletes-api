using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.Seller;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class SellerProfile: Profile
    {
        public SellerProfile()
        {
            CreateMap<Seller, SellerDto>();
            CreateMap<SellerCreateDto, Seller>();
            CreateMap<SellerUpdateDto, Seller>();
        }
    }
}
