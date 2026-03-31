using Microsoft.EntityFrameworkCore;
using FamilyMeet.Domain.Entities;

namespace FamilyMeet.EntityFrameworkCore;

public static class FamilyMeetDbContextModelCreatingExtensions
{
    public static void ConfigureFamilyMeet(this ModelBuilder builder)
    {
        builder.Entity<ChatGroup>(b =>
        {
            b.ToTable("ChatGroups");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .ValueGeneratedNever();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.Description)
                .HasMaxLength(500);

            b.Property(x => x.Type)
                .HasConversion<int>();

            b.Property(x => x.CreatorId)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.LastActivityAt);

            b.Property(x => x.IsActive)
                .IsRequired();

            b.Property(x => x.MaxParticipants)
                .IsRequired();

            // Indexes for performance
            b.HasIndex(x => x.CreatorId);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.LastActivityAt);
            b.HasIndex(x => x.IsActive);

            // Relationships
            b.HasMany(x => x.Members)
                .WithOne(x => x.ChatGroup)
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Messages)
                .WithOne(x => x.ChatGroup)
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatGroupMember>(b =>
        {
            b.ToTable("ChatGroupMembers");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .ValueGeneratedNever();

            b.Property(x => x.UserId)
                .IsRequired();

            b.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.ChatGroupId)
                .IsRequired();

            b.Property(x => x.IsCreator)
                .IsRequired();

            b.Property(x => x.JoinedAt)
                .IsRequired();

            b.Property(x => x.LastSeenAt);

            b.Property(x => x.IsActive)
                .IsRequired();

            // Indexes for performance
            b.HasIndex(x => new { x.ChatGroupId, x.UserId }).IsUnique();
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.JoinedAt);
            b.HasIndex(x => x.LastSeenAt);

            // Relationships
            b.HasOne(x => x.ChatGroup)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.ToTable("ChatMessages");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .ValueGeneratedNever();

            b.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(1000);

            b.Property(x => x.SenderId)
                .IsRequired();

            b.Property(x => x.SenderName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.ChatGroupId)
                .IsRequired();

            b.Property(x => x.Type)
                .HasConversion<int>();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.EditedAt);

            b.Property(x => x.IsDeleted)
                .IsRequired();

            b.Property(x => x.DeletedAt);

            b.Property(x => x.ReplyToMessageId);

            // Indexes for performance - CRITICAL for chat performance
            b.HasIndex(x => x.ChatGroupId);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => new { x.ChatGroupId, x.CreatedAt }); // Composite index for message pagination
            b.HasIndex(x => x.SenderId);
            b.HasIndex(x => x.IsDeleted);
            b.HasIndex(x => new { x.ChatGroupId, x.IsDeleted, x.CreatedAt }); // For active messages query

            // Relationships
            b.HasOne(x => x.ChatGroup)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ReplyToMessage)
                .WithMany()
                .HasForeignKey(x => x.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.Attachments)
                .WithOne(x => x.ChatMessage)
                .HasForeignKey(x => x.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChatMessageAttachment>(b =>
        {
            b.ToTable("ChatMessageAttachments");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .ValueGeneratedNever();

            b.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            b.Property(x => x.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            b.Property(x => x.FileSize)
                .IsRequired();

            b.Property(x => x.MimeType)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.UploadedAt)
                .IsRequired();

            b.Property(x => x.ChatMessageId)
                .IsRequired();

            // Indexes for performance
            b.HasIndex(x => x.ChatMessageId);
            b.HasIndex(x => x.UploadedAt);

            // Relationships
            b.HasOne(x => x.ChatMessage)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
