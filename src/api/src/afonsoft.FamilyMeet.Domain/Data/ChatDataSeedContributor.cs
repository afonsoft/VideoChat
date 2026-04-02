using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Data;

public class ChatDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;
    private readonly IRepository<ChatMessage, Guid> _chatMessageRepository;
    private readonly IRepository<ChatParticipant, Guid> _chatParticipantRepository;
    private readonly ILogger<ChatDataSeedContributor> _logger;

    public ChatDataSeedContributor(
        IRepository<ChatGroup, Guid> chatGroupRepository,
        IRepository<ChatMessage, Guid> chatMessageRepository,
        IRepository<ChatParticipant, Guid> chatParticipantRepository,
        ILogger<ChatDataSeedContributor> logger)
    {
        _chatGroupRepository = chatGroupRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatParticipantRepository = chatParticipantRepository;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Seeding chat data...");

        await SeedChatGroupsAsync();
        await SeedChatMessagesAsync();
        await SeedChatParticipantsAsync();

        _logger.LogInformation("Chat data seeded successfully!");
    }

    private async Task SeedChatGroupsAsync()
    {
        // Check if groups already exist
        var existingGroups = await _chatGroupRepository.GetCountAsync();
        if (existingGroups > 0)
        {
            _logger.LogInformation("Chat groups already exist, skipping seed");
            return;
        }

        var groups = new[]
        {
            new ChatGroup(
                Guid.NewGuid(),
                "General Discussion",
                "General chat for all family members",
                true,
                100
            ),
            new ChatGroup(
                Guid.NewGuid(),
                "Family Events",
                "Planning and discussion about family events",
                true,
                50
            ),
            new ChatGroup(
                Guid.NewGuid(),
                "Kids Corner",
                "Chat for the younger family members",
                true,
                25
            ),
            new ChatGroup(
                Guid.NewGuid(),
                "Private - Parents Only",
                "Private discussions for parents only",
                false,
                10
            ),
            new ChatGroup(
                Guid.NewGuid(),
                "Photo Sharing",
                "Share family photos and memories",
                true,
                75
            )
        };

        foreach (var group in groups)
        {
            await _chatGroupRepository.InsertAsync(group);
        }

        _logger.LogInformation($"Created {groups.Length} chat groups");
    }

    private async Task SeedChatMessagesAsync()
    {
        // Get all groups
        var groups = await _chatGroupRepository.GetListAsync();

        if (groups.Count == 0)
        {
            _logger.LogWarning("No groups found, cannot seed messages");
            return;
        }

        // Check if messages already exist
        var existingMessages = await _chatMessageRepository.GetCountAsync();
        if (existingMessages > 0)
        {
            _logger.LogInformation("Chat messages already exist, skipping seed");
            return;
        }

        var sampleMessages = new[]
        {
            new { GroupIndex = 0, Sender = "Admin", Content = "Welcome to the General Discussion group!", Type = MessageType.System },
            new { GroupIndex = 0, Sender = "Mom", Content = "Hello everyone! How was your day?", Type = MessageType.Text },
            new { GroupIndex = 0, Sender = "Dad", Content = "Great! Just finished work. How about you?", Type = MessageType.Text },
            new { GroupIndex = 0, Sender = "Grandma", Content = "I made cookies today! 🍪", Type = MessageType.Text },

            new { GroupIndex = 1, Sender = "Admin", Content = "Welcome to Family Events planning!", Type = MessageType.System },
            new { GroupIndex = 1, Sender = "Aunt Mary", Content = "When is the next family reunion?", Type = MessageType.Text },
            new { GroupIndex = 1, Sender = "Uncle John", Content = "I was thinking next month, what do you all think?", Type = MessageType.Text },

            new { GroupIndex = 2, Sender = "Admin", Content = "Welcome to Kids Corner! 🎈", Type = MessageType.System },
            new { GroupIndex = 2, Sender = "Tommy", Content = "Who wants to play games online?", Type = MessageType.Text },
            new { GroupIndex = 2, Sender = "Sarah", Content = "Me! I love games!", Type = MessageType.Text },

            new { GroupIndex = 3, Sender = "Admin", Content = "Private parents discussion started", Type = MessageType.System },
            new { GroupIndex = 3, Sender = "Mom", Content = "We need to discuss the kids' school schedules", Type = MessageType.Text },
            new { GroupIndex = 3, Sender = "Dad", Content = "Agreed. Let's plan for next week", Type = MessageType.Text },

            new { GroupIndex = 4, Sender = "Admin", Content = "Photo sharing group created! 📸", Type = MessageType.System },
            new { GroupIndex = 4, Sender = "Grandpa", Content = "Check out these old family photos I found!", Type = MessageType.Image },
            new { GroupIndex = 4, Sender = "Cousin Lisa", Content = "These are amazing! Thanks for sharing!", Type = MessageType.Text }
        };

        foreach (var messageData in sampleMessages)
        {
            if (messageData.GroupIndex < groups.Count)
            {
                var group = groups[messageData.GroupIndex];
                var message = new ChatMessage(
                    Guid.NewGuid(),
                    group.Id,
                    Guid.NewGuid(), // Random user ID for demo
                    messageData.Sender,
                    messageData.Content,
                    messageData.Type
                );

                await _chatMessageRepository.InsertAsync(message);

                // Update group's last message time
                group.SetLastMessageTime(message.CreationTime);
                await _chatGroupRepository.UpdateAsync(group);
            }
        }

        _logger.LogInformation($"Created {sampleMessages.Length} sample chat messages");
    }

    private async Task SeedChatParticipantsAsync()
    {
        // Get all groups
        var groups = await _chatGroupRepository.GetListAsync();

        if (groups.Count == 0)
        {
            _logger.LogWarning("No groups found, cannot seed participants");
            return;
        }

        // Check if participants already exist
        var existingParticipants = await _chatParticipantRepository.GetCountAsync();
        if (existingParticipants > 0)
        {
            _logger.LogInformation("Chat participants already exist, skipping seed");
            return;
        }

        var familyMembers = new[]
        {
            new { Name = "Mom", IsCreator = true },
            new { Name = "Dad", IsCreator = false },
            new { Name = "Grandma", IsCreator = false },
            new { Name = "Grandpa", IsCreator = false },
            new { Name = "Aunt Mary", IsCreator = false },
            new { Name = "Uncle John", IsCreator = false },
            new { Name = "Cousin Lisa", IsCreator = false },
            new { Name = "Tommy", IsCreator = false },
            new { Name = "Sarah", IsCreator = false },
            new { Name = "Baby Emma", IsCreator = false }
        };

        foreach (var group in groups)
        {
            // Add creator as first participant
            var creatorParticipant = new ChatParticipant(
                Guid.NewGuid(),
                group.Id,
                Guid.NewGuid(),
                "Admin",
                true
            );
            creatorParticipant.IsOnline = true;
            creatorParticipant.LastSeenAt = DateTime.UtcNow;
            await _chatParticipantRepository.InsertAsync(creatorParticipant);

            // Add family members to public groups
            if (group.IsPublic)
            {
                foreach (var member in familyMembers)
                {
                    var participant = new ChatParticipant(
                        Guid.NewGuid(),
                        group.Id,
                        Guid.NewGuid(),
                        member.Name,
                        member.IsCreator && group.Name.Contains("Private")
                    );
                    participant.IsOnline = false;
                    participant.LastSeenAt = DateTime.UtcNow.AddHours(-new Random().Next(1, 24)); // Random last seen
                    await _chatParticipantRepository.InsertAsync(participant);
                }
            }
        }

        _logger.LogInformation($"Created chat participants for {groups.Count} groups");
    }
}
