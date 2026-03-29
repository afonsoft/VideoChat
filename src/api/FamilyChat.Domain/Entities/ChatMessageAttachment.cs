namespace SimpleConnect.Domain.Entities;

public class ChatMessageAttachment
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string MimeType { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    
    public Guid ChatMessageId { get; private set; }
    public ChatMessage ChatMessage { get; private set; } = null!;
    
    public ChatMessageAttachment()
    {
        Id = Guid.NewGuid();
        UploadedAt = DateTime.UtcNow;
    }
    
    public ChatMessageAttachment(string fileName, string fileUrl, long fileSize, string mimeType)
        : this()
    {
        FileName = fileName;
        FileUrl = fileUrl;
        FileSize = fileSize;
        MimeType = mimeType;
    }
}
