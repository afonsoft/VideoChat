namespace afonsoft.FamilyMeet.Localization;

public static class ChatPermissions
{
    public const string GroupName = "Chat";

    public static class Groups
    {
        public const string Default = GroupName;
        public static class ChatGroups
        {
            public const string Default = GroupName + ".ChatGroups";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
            public const string Manage = Default + ".Manage";
        }

        public static class ChatMessages
        {
            public const string Default = GroupName + ".ChatMessages";
            public const string Send = Default + ".Send";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
            public const string View = Default + ".View";
        }

        public static class ChatParticipants
        {
            public const string Default = GroupName + ".ChatParticipants";
            public const string Join = Default + ".Join";
            public const string Leave = Default + ".Leave";
            public const string Kick = Default + ".Kick";
            public const string Ban = Default + ".Ban";
            public const string Manage = Default + ".Manage";
        }
    }
}
