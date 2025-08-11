using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Seller;

namespace norviguet_control_fletes_api.Profiles
{
    public class SellerProfile : Profile
    {
        public SellerProfile()
        {
            CreateMap<Seller, SellerDto>();
            CreateMap<CreateSellerDto, Seller>();
            CreateMap<UpdateSellerDto, Seller>();
        }
    }
}
