using Microsoft.EntityFrameworkCore;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Localization;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace afonsoft.FamilyMeet.EntityFrameworkCore;

public static class ChatEfCoreMappingExtensions
{
    public static void ConfigureChat(this ModelBuilder builder)
    {
        builder.Entity<ChatGroup>(b =>
        {
            b.ToTable(ChatConstants.DbTablePrefix + "ChatGroups", ChatConstants.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(ChatConstants.ChatGroup.MaxNameLength);

            b.Property(x => x.Description)
                .HasMaxLength(ChatConstants.ChatGroup.MaxDescriptionLength);

            b.Property(x => x.MaxParticipants)
                .IsRequired();

            b.Property(x => x.IsActive)
                .IsRequired();

            b.Property(x => x.LastMessageAt);

            // Indexes
            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.IsPublic);
            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => x.LastMessageAt);
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.ToTable(ChatConstants.DbTablePrefix + "ChatMessages", ChatConstants.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ChatGroupId)
                .IsRequired();

            b.Property(x => x.SenderId)
                .IsRequired();

            b.Property(x => x.SenderName)
                .IsRequired()
                .HasMaxLength(ChatConstants.ChatMessage.MaxSenderNameLength);

            b.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(ChatConstants.ChatMessage.MaxContentLength);

            b.Property(x => x.Type)
                .IsRequired();

            b.Property(x => x.IsEdited)
                .IsRequired();

            b.Property(x => x.EditedAt);

            b.Property(x => x.ReplyToMessageId);

            b.Property(x => x.IsDeleted)
                .IsRequired();

            b.Property(x => x.DeletedAt);

            // Foreign keys
            b.HasOne<ChatGroup>()
                .WithMany()
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<ChatMessage>()
                .WithMany()
                .HasForeignKey(x => x.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(x => x.ChatGroupId);
            b.HasIndex(x => x.SenderId);
            b.HasIndex(x => x.Type);
            b.HasIndex(x => x.IsDeleted);
            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => new { x.ChatGroupId, x.CreationTime });
            b.HasIndex(x => new { x.ChatGroupId, x.IsDeleted, x.CreationTime });
        });

        builder.Entity<ChatParticipant>(b =>
        {
            b.ToTable(ChatConstants.DbTablePrefix + "ChatParticipants", ChatConstants.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ChatGroupId)
                .IsRequired();

            b.Property(x => x.UserId)
                .IsRequired();

            b.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(ChatConstants.ChatParticipant.MaxUserNameLength);

            b.Property(x => x.IsOnline)
                .IsRequired();

            b.Property(x => x.LastSeenAt);

            b.Property(x => x.IsMuted)
                .IsRequired();

            b.Property(x => x.IsBanned)
                .IsRequired();

            b.Property(x => x.BannedUntil);

            b.Property(x => x.IsCreator)
                .IsRequired();

            // Foreign keys
            b.HasOne<ChatGroup>()
                .WithMany()
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            b.HasIndex(x => x.ChatGroupId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.IsOnline);
            b.HasIndex(x => x.IsBanned);
            b.HasIndex(x => new { x.ChatGroupId, x.UserId }).IsUnique();
        });
    }
}
