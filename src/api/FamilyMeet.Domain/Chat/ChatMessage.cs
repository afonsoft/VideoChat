using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace FamilyMeet.Domain.Chat
{
    public class ChatMessage : FullAuditedAggregateRoot<Guid>
    {
        public Guid ChatGroupId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text"; // Text, Image, File, System
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid? ReplyToMessageId { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? EditedByUserId { get; set; }
        public Guid? DeletedByUserId { get; set; }
        public string Reactions { get; set; } = string.Empty; // JSON string for reactions
        public string Metadata { get; set; } = string.Empty; // JSON string for additional metadata

        protected ChatMessage()
        {
        }

        public ChatMessage(
            Guid id,
            Guid chatGroupId,
            Guid userId,
            string content,
            string messageType = "Text"
        ) : base(id)
        {
            ChatGroupId = chatGroupId;
            UserId = userId;
            Content = content;
            MessageType = messageType;
            IsEdited = false;
            IsDeleted = false;
        }

        public ChatMessage(
            Guid id,
            Guid chatGroupId,
            Guid userId,
            string content,
            string fileUrl,
            string fileName,
            long fileSize,
            string messageType = "File"
        ) : base(id)
        {
            ChatGroupId = chatGroupId;
            UserId = userId;
            Content = content;
            FileUrl = fileUrl;
            FileName = fileName;
            FileSize = fileSize;
            MessageType = messageType;
            IsEdited = false;
            IsDeleted = false;
        }

        public void EditContent(string newContent, Guid editedByUserId)
        {
            if (string.IsNullOrEmpty(newContent))
            {
                throw new ArgumentException("Content cannot be empty", nameof(newContent));
            }

            Content = newContent;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
            EditedByUserId = editedByUserId;
        }

        public void SoftDelete(Guid deletedByUserId)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }

        public void SetReplyToMessage(Guid replyToMessageId)
        {
            ReplyToMessageId = replyToMessageId;
        }

        public void ClearReplyToMessage()
        {
            ReplyToMessageId = null;
        }

        public void AddReaction(string reactionJson)
        {
            Reactions = reactionJson;
        }

        public void UpdateReactions(string reactionsJson)
        {
            Reactions = reactionsJson;
        }

        public void UpdateMetadata(string metadataJson)
        {
            Metadata = metadataJson;
        }

        public bool IsTextMessage()
        {
            return MessageType == "Text";
        }

        public bool IsImageMessage()
        {
            return MessageType == "Image";
        }

        public bool IsFileMessage()
        {
            return MessageType == "File";
        }

        public bool IsSystemMessage()
        {
            return MessageType == "System";
        }

        public bool HasAttachment()
        {
            return !string.IsNullOrEmpty(FileUrl) && (IsImageMessage() || IsFileMessage());
        }

        public bool CanBeEdited()
        {
            return !IsDeleted && (IsTextMessage() || MessageType == "System") && 
                   DateTime.UtcNow.Subtract(CreationTime).TotalHours < 24; // Can edit within 24 hours
        }

        public bool CanBeDeleted()
        {
            return !IsDeleted;
        }

        public bool IsFromUser(Guid userId)
        {
            return UserId == userId;
        }
    }
}
