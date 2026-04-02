using AutoMapper;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet;

public class ChatAutoMapperProfile : Profile
{
    public ChatAutoMapperProfile()
    {
        CreateMap<ChatGroup, ChatGroupDto>()
            .ForMember(dest => dest.ParticipantCount, opt => opt.Ignore())
            .ForMember(dest => dest.MessageCount, opt => opt.Ignore());

        CreateMap<CreateChatGroupDto, ChatGroup>()
            .ConstructUsing(src => new ChatGroup(
                Guid.NewGuid(),
                src.Name,
                src.Description,
                src.IsPublic,
                src.MaxParticipants));

        CreateMap<UpdateChatGroupDto, ChatGroup>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
            .ForMember(dest => dest.LastModificationTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifierId, opt => opt.Ignore());

        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore());

        CreateMap<CreateChatMessageDto, ChatMessage>()
            .ConstructUsing(src => new ChatMessage(
                Guid.NewGuid(),
                src.ChatGroupId,
                Guid.Empty, // Will be set in service
                string.Empty, // Will be set in service
                src.Content,
                src.Type,
                src.ReplyToMessageId));

        CreateMap<UpdateChatMessageDto, ChatMessage>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ChatGroupId, opt => opt.Ignore())
            .ForMember(dest => dest.SenderId, opt => opt.Ignore())
            .ForMember(dest => dest.SenderName, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.CreationTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
            .ForMember(dest => dest.ReplyToMessageId, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<ChatMessage, ChatMessageSignalRDto>()
            .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore());

        CreateMap<ChatMessageDto, ChatMessageSignalRDto>()
            .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore());
    }
}
