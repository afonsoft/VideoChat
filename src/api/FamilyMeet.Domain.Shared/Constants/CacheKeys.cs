namespace FamilyMeet.Domain.Shared.Constants;

public static class CacheKeys
{
    private const string Prefix = "FamilyMeet";

    public static string ChatGroups(Guid groupId) => $"{Prefix}:ChatGroups:{groupId}";
    public static string UserGroups(Guid userId) => $"{Prefix}:UserGroups:{userId}";
    public static string GroupMessages(Guid groupId) => $"{Prefix}:Messages:{groupId}";
    public static string GroupMembers(Guid groupId) => $"{Prefix}:Members:{groupId}";
    public static string UserInfo(Guid userId) => $"{Prefix}:User:{userId}";
    public static string OnlineUsers => $"{Prefix}:OnlineUsers";
    public static string CallParticipants(Guid groupId) => $"{Prefix}:Call:{groupId}:Participants";
    public static string CallInfo(Guid groupId) => $"{Prefix}:Call:{groupId}:Info";
}
