using AutoMapper;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.DeliveryNote;

namespace norviguet_control_fletes_api.Profiles
{
    public class DeliveryNoteProfile : Profile
    {
        public DeliveryNoteProfile()
        {
            CreateMap<DeliveryNote, DeliveryNoteDto>()
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.Carrier != null ? src.Carrier.Name : null));
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
