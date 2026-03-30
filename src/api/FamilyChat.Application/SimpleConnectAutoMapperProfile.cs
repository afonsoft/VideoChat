using AutoMapper;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Shared.ValueObjects;

namespace FamilyChat.Application;

public class FamilyChatAutoMapperProfile : Profile
{
    public FamilyChatAutoMapperProfile()
    {
        CreateMap<ChatGroup, FamilyChatGroupDto>()
            .ForMember(dest => dest.CreatorName, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentParticipantsCount, opt => opt.MapFrom(src => src.Members.Count(m => m.IsActive)))
            .ForMember(dest => dest.ActiveCallParticipantsCount, opt => opt.MapFrom(src => src.ActiveCallParticipants.Count));

        CreateMap<ChatGroupMember, ChatGroupMemberDto>()
            .ForMember(dest => dest.IsInCall, opt => opt.Ignore());

        CreateMap<CallParticipant, CallParticipantDto>();

        CreateMap<ChatMessage, FamilyChatMessageDto>();

        CreateMap<ChatMessageAttachment, ChatMessageAttachmentDto>();

        CreateMap<CreateChatGroupDto, ChatGroup>()
            .ConstructUsing(src => new ChatGroup(src.Name, src.Description, src.Type, src.CreatorId, src.MaxParticipants));

        CreateMap<UpdateChatGroupDto, ChatGroup>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<SendMessageDto, ChatMessage>()
            .ConstructUsing(src => new ChatMessage(src.Content, src.SenderId, "User", src.ChatGroupId, src.Type));

        CreateMap<EditMessageDto, ChatMessage>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content));
    }
}
