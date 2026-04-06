using System;

namespace afonsoft.FamilyMeet.Chat.Dtos;

/// <summary>
/// DTO para mensagens de chat enviadas via SignalR
/// </summary>
public class ChatMessageSignalRDto
{
    /// <summary>
    /// ID do grupo de chat
    /// </summary>
    public Guid ChatGroupId { get; set; }

    /// <summary>
    /// Conteúdo da mensagem
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da mensagem (0=Text, 1=Image, 2=File, 3=System)
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// ID da mensagem sendo respondida (opcional)
    /// </summary>
    public Guid? ReplyToMessageId { get; set; }

    /// <summary>
    /// Anexos da mensagem (imagens, arquivos)
    /// </summary>
    public MessageAttachmentDto[]? Attachments { get; set; }

    /// <summary>
    /// Metadados adicionais
    /// </summary>
    public object? Metadata { get; set; }
}

/// <summary>
/// DTO para anexos de mensagens
/// </summary>
public class MessageAttachmentDto
{
    /// <summary>
    /// ID do anexo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do arquivo
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// URL do arquivo
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Tamanho do arquivo em bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// MIME type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Largura (para imagens/vídeos)
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Altura (para imagens/vídeos)
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Duração (para áudio/vídeo em segundos)
    /// </summary>
    public int? Duration { get; set; }
}

/// <summary>
/// DTO para status de usuários em grupos
/// </summary>
public class UserGroupStatusDto
{
    /// <summary>
    /// ID do grupo
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Lista de usuários online
    /// </summary>
    public OnlineUserDto[] OnlineUsers { get; set; } = Array.Empty<OnlineUserDto>();

    /// <summary>
    /// Total de usuários no grupo
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Última atualização
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para informações de usuários online
/// </summary>
public class OnlineUserDto
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Status online
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Está digitando
    /// </summary>
    public bool IsTyping { get; set; }

    /// <summary>
    /// Última visualização
    /// </summary>
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// URL do avatar
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Status personalizado
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// DTO para eventos de chamada de vídeo
/// </summary>
public class VideoCallEventDto
{
    /// <summary>
    /// ID da chamada
    /// </summary>
    public Guid CallId { get; set; }

    /// <summary>
    /// ID do grupo
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// ID do chamador
    /// </summary>
    public Guid CallerId { get; set; }

    /// <summary>
    /// Nome do chamador
    /// </summary>
    public string CallerName { get; set; } = string.Empty;

    /// <summary>
    /// ID do destinatário
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// Status da chamada
    /// </summary>
    public VideoCallStatus Status { get; set; }

    /// <summary>
    /// Timestamp do evento
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Dados adicionais
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// DTO para signaling WebRTC
/// </summary>
public class WebRTCSignalDto
{
    /// <summary>
    /// ID da chamada
    /// </summary>
    public Guid CallId { get; set; }

    /// <summary>
    /// ID do remetente
    /// </summary>
    public Guid FromUserId { get; set; }

    /// <summary>
    /// Tipo do sinal (offer, answer, ice-candidate)
    /// </summary>
    public WebRTCSignalType Type { get; set; }

    /// <summary>
    /// Dados do sinal
    /// </summary>
    public object Signal { get; set; } = new object();

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO para notificações em tempo real
/// </summary>
public class RealTimeNotificationDto
{
    /// <summary>
    /// ID da notificação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo da notificação
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Título
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID do usuário remetente
    /// </summary>
    public Guid? FromUserId { get; set; }

    /// <summary>
    /// Nome do remetente
    /// </summary>
    public string? FromUserName { get; set; }

    /// <summary>
    /// URL da imagem (opcional)
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// URL de ação (opcional)
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Metadados adicionais
    /// </summary>
    public object? Metadata { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO para eventos de grupo
/// </summary>
public class GroupEventDto
{
    /// <summary>
    /// ID do grupo
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Tipo do evento
    /// </summary>
    public GroupEventType EventType { get; set; }

    /// <summary>
    /// ID do usuário que acionou o evento
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Dados do evento
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
}

#region Enums

/// <summary>
/// Status de chamadas de vídeo
/// </summary>
public enum VideoCallStatus
{
    /// <summary>
    /// Chamada iniciada
    /// </summary>
    Initiated = 0,

    /// <summary>
    /// Chamada ativa
    /// </summary>
    Active = 1,

    /// <summary>
    /// Chamada encerrada
    /// </summary>
    Ended = 2,

    /// <summary>
    /// Chamada rejeitada
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Chamada falhou
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Chamada em espera
    /// </summary>
    Ringing = 5
}

/// <summary>
/// Tipos de sinal WebRTC
/// </summary>
public enum WebRTCSignalType
{
    /// <summary>
    /// Oferta WebRTC
    /// </summary>
    Offer = 0,

    /// <summary>
    /// Resposta WebRTC
    /// </summary>
    Answer = 1,

    /// <summary>
    /// ICE Candidate
    /// </summary>
    IceCandidate = 2,

    /// <summary>
    /// Descrição local
    /// </summary>
    LocalDescription = 3,

    /// <summary>
    /// Descrição remota
    /// </summary>
    RemoteDescription = 4
}

/// <summary>
/// Tipos de notificação
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Nova mensagem
    /// </summary>
    NewMessage = 0,

    /// <summary>
    /// Chamada de vídeo
    /// </summary>
    VideoCall = 1,

    /// <summary>
    /// Convite para grupo
    /// </summary>
    GroupInvite = 2,

    /// <summary>
    /// Usuário entrou no grupo
    /// </summary>
    UserJoined = 3,

    /// <summary>
    /// Usuário saiu do grupo
    /// </summary>
    UserLeft = 4,

    /// <summary>
    /// Mensagem editada
    /// </summary>
    MessageEdited = 5,

    /// <summary>
    /// Mensagem excluída
    /// </summary>
    MessageDeleted = 6,

    /// <summary>
    /// Sistema
    /// </summary>
    System = 7
}

/// <summary>
/// Tipos de eventos de grupo
/// </summary>
public enum GroupEventType
{
    /// <summary>
    /// Usuário entrou
    /// </summary>
    UserJoined = 0,

    /// <summary>
    /// Usuário saiu
    /// </summary>
    UserLeft = 1,

    /// <summary>
    /// Usuário digitando
    /// </summary>
    UserTyping = 2,

    /// <summary>
    /// Usuário parou de digitar
    /// </summary>
    UserStoppedTyping = 3,

    /// <summary>
    /// Grupo criado
    /// </summary>
    GroupCreated = 4,

    /// <summary>
    /// Grupo atualizado
    /// </summary>
    GroupUpdated = 5,

    /// <summary>
    /// Grupo excluído
    /// </summary>
    GroupDeleted = 6
}

#endregion
