using Microsoft.EntityFrameworkCore;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Shared.Enums;

namespace FamilyChat.EntityFrameworkCore;

public class FamilyChatDbContext : DbContext
{
    public FamilyChatDbContext(DbContextOptions<FamilyChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatGroup> ChatGroups { get; set; } = null!;
    public DbSet<ChatGroupMember> ChatGroupMembers { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<ChatMessageAttachment> ChatMessageAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureFamilyChat();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    public async Task SeedDataAsync()
    {
        // Verificar se já existe dados
        if (await ChatGroups.AnyAsync())
        {
            return; // Database já foi populado
        }

        // Criar grupos de exemplo
        var groups = new List<ChatGroup>
        {
            new ChatGroup("Geral", "Grupo para conversas gerais", GroupType.Chat, Guid.NewGuid())
            {
                Description = "Bem-vindo ao grupo geral! Use este espaço para conversas informais."
            },
            new ChatGroup("Trabalho", "Discussões sobre projetos de trabalho", GroupType.Chat, Guid.NewGuid())
            {
                Description = "Espaço para discussões profissionais e alinhamento de equipe."
            },
            new ChatGroup("Vídeo Conferência", "Sala de videoconferência principal", GroupType.VideoCall, Guid.NewGuid(), 10)
            {
                Description = "Use esta sala para videoconferências com até 10 participantes."
            }
        };

        await ChatGroups.AddRangeAsync(groups);
        await SaveChangesAsync();

        // Adicionar membros aos grupos
        var members = new List<ChatGroupMember>();
        var userId = Guid.NewGuid();

        foreach (var group in groups)
        {
            members.Add(new ChatGroupMember(userId, "Usuário Demo", true)); // Criador
            members.Add(new ChatGroupMember(Guid.NewGuid(), "Maria Silva", false));
            members.Add(new ChatGroupMember(Guid.NewGuid(), "João Santos", false));
            members.Add(new ChatGroupMember(Guid.NewGuid(), "Ana Costa", false));
        }

        await ChatGroupMembers.AddRangeAsync(members);
        await SaveChangesAsync();

        // Adicionar mensagens de exemplo
        var messages = new List<ChatMessage>();
        var generalGroup = groups.First(g => g.Name == "Geral");
        var workGroup = groups.First(g => g.Name == "Trabalho");

        messages.AddRange(new[]
        {
            new ChatMessage("Olá pessoal! Sejam bem-vindos ao FamilyChat! 🎉", userId, "Usuário Demo", generalGroup.Id, MessageType.System),
            new ChatMessage("Obrigado! Estou animado para testar as videoconferências.", Guid.NewGuid(), "Maria Silva", generalGroup.Id, MessageType.Text),
            new ChatMessage("As funcionalidades de chat estão incríveis!", Guid.NewGuid(), "João Santos", generalGroup.Id, MessageType.Text),
            new ChatMessage("Concordo! A interface é muito intuitiva.", Guid.NewGuid(), "Ana Costa", generalGroup.Id, MessageType.Text),

            new ChatMessage("Reunião de alinhamento semanal - sexta-feira 14h", userId, "Usuário Demo", workGroup.Id, MessageType.System),
            new ChatMessage("Confirmo minha presença.", Guid.NewGuid(), "Maria Silva", workGroup.Id, MessageType.Text),
            new ChatMessage("Vou preparar a apresentação do sprint.", Guid.NewGuid(), "João Santos", workGroup.Id, MessageType.Text),
            new ChatMessage("Ótimo! Vamos revisar as métricas também.", Guid.NewGuid(), "Ana Costa", workGroup.Id, MessageType.Text)
        });

        await ChatMessages.AddRangeAsync(messages);
        await SaveChangesAsync();
    }
}
