using AutoMapper;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Models.Profiles
{
    public class DeliveryNoteProfile : Profile
    {
        public DeliveryNoteProfile()
        {
            CreateMap<DeliveryNote, DeliveryNoteDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier.Name))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order.Status.ToString()));
            CreateMap<CreateDeliveryNoteDto, DeliveryNote>();
            CreateMap<UpdateDeliveryNoteDto, DeliveryNote>();
            CreateMap<UpdateDeliveryNoteStatusDto, DeliveryNote>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseDeliveryNoteStatus(src.Status)));
        }
        private static DeliveryNoteStatus ParseDeliveryNoteStatus(string? status)
        {
            return Enum.TryParse<DeliveryNoteStatus>(status, true, out var parsed) ? parsed : DeliveryNoteStatus.Pending;
        }
    }
}
