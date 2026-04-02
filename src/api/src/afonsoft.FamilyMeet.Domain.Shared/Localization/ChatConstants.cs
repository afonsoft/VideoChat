namespace afonsoft.FamilyMeet.Localization;

public static class ChatConstants
{
    public const string DbTablePrefix = "AppChat";

    public const string DbSchema = null;

    public static class ChatGroup
    {
        public const int MaxNameLength = 128;
        public const int MaxDescriptionLength = 500;
        public const int DefaultMaxParticipants = 100;
        public const int MaxMaxParticipants = 1000;
    }

    public static class ChatMessage
    {
        public const int MaxContentLength = 4000;
        public const int MaxSenderNameLength = 128;
    }

    public static class ChatParticipant
    {
        public const int MaxUserNameLength = 128;
    }
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    System = 3
}
