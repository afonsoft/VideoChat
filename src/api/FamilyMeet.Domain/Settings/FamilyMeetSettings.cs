using System;
using System.Collections.Generic;
using Volo.Abp.Settings;

namespace FamilyMeet.Domain.Settings
{
    public static class FamilyMeetSettings
    {
        public const string GroupName = "FamilyMeet";

        // Authentication Settings
        public static class Authentication
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".Authentication";
            
            public const string GoogleClientId = GroupName + ".GoogleClientId";
            public const string GoogleClientSecret = GroupName + ".GoogleClientSecret";
            public const string JwtExpirationMinutes = GroupName + ".JwtExpirationMinutes";
            public const string JwtSecretKey = GroupName + ".JwtSecretKey";
            public const string JwtIssuer = GroupName + ".JwtIssuer";
            public const string JwtAudience = GroupName + ".JwtAudience";
        }

        // File Storage Settings
        public static class FileStorage
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".FileStorage";
            
            public const string Provider = GroupName + ".Provider"; // Local, Azure, AWS, etc.
            public const string LocalStoragePath = GroupName + ".LocalStoragePath";
            public const string AzureStorageConnectionString = GroupName + ".AzureStorageConnectionString";
            public const string AzureStorageContainerName = GroupName + ".AzureStorageContainerName";
            public const string AwsAccessKeyId = GroupName + ".AwsAccessKeyId";
            public const string AwsSecretAccessKey = GroupName + ".AwsSecretAccessKey";
            public const string AwsRegion = GroupName + ".AwsRegion";
            public const string AwsBucketName = GroupName + ".AwsBucketName";
            public const string MaxFileSize = GroupName + ".MaxFileSize";
            public const string AllowedExtensions = GroupName + ".AllowedExtensions";
        }

        // Video Call Settings
        public static class VideoCall
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".VideoCall";
            
            public const string MaxParticipants = GroupName + ".MaxParticipants";
            public const string EnableRecording = GroupName + ".EnableRecording";
            public const string RecordingStoragePath = GroupName + ".RecordingStoragePath";
            public const string MaxCallDurationMinutes = GroupName + ".MaxCallDurationMinutes";
            public const string EnableScreenSharing = GroupName + ".EnableScreenSharing";
            public const string EnableChat = GroupName + ".EnableChat";
            public const string EnableFileTransfer = GroupName + ".EnableFileTransfer";
        }

        // Notification Settings
        public static class Notification
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".Notification";
            
            public const string EmailProvider = GroupName + ".EmailProvider";
            public const string SmtpHost = GroupName + ".SmtpHost";
            public const string SmtpPort = GroupName + ".SmtpPort";
            public const string SmtpUsername = GroupName + ".SmtpUsername";
            public const string SmtpPassword = GroupName + ".SmtpPassword";
            public const string EnableEmailNotifications = GroupName + ".EnableEmailNotifications";
            public const string EnablePushNotifications = GroupName + ".EnablePushNotifications";
            public const string PushNotificationApiKey = GroupName + ".PushNotificationApiKey";
        }

        // Security Settings
        public static class Security
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".Security";
            
            public const string EnableTwoFactorAuthentication = GroupName + ".EnableTwoFactorAuthentication";
            public const string PasswordPolicyMinLength = GroupName + ".PasswordPolicyMinLength";
            public const string PasswordPolicyRequireUppercase = GroupName + ".PasswordPolicyRequireUppercase";
            public const string PasswordPolicyRequireLowercase = GroupName + ".PasswordPolicyRequireLowercase";
            public const string PasswordPolicyRequireNumbers = GroupName + ".PasswordPolicyRequireNumbers";
            public const string PasswordPolicyRequireSpecialChars = GroupName + ".PasswordPolicyRequireSpecialChars";
            public const string MaxLoginAttempts = GroupName + ".MaxLoginAttempts";
            public const string AccountLockoutDurationMinutes = GroupName + ".AccountLockoutDurationMinutes";
        }

        // Audit Settings
        public static class Audit
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".Audit";
            
            public const string EnableAuditLogging = GroupName + ".EnableAuditLogging";
            public const string AuditLogRetentionDays = GroupName + ".AuditLogRetentionDays";
            public const string EnableEntityChangeTracking = GroupName + ".EnableEntityChangeTracking";
            public const string EnableRequestLogging = GroupName + ".EnableRequestLogging";
            public const string EnableSecurityLogging = GroupName + ".EnableSecurityLogging";
        }

        // Cache Settings
        public static class Cache
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".Cache";
            
            public const string DefaultExpirationMinutes = GroupName + ".DefaultExpirationMinutes";
            public const string UserCacheExpirationMinutes = GroupName + ".UserCacheExpirationMinutes";
            public const string GroupCacheExpirationMinutes = GroupName + ".GroupCacheExpirationMinutes";
            public const string MessageCacheExpirationMinutes = GroupName + ".MessageCacheExpirationMinutes";
        }

        // Rate Limiting Settings
        public static class RateLimiting
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".RateLimiting";
            
            public const string EnableRateLimiting = GroupName + ".EnableRateLimiting";
            public const string MaxRequestsPerMinute = GroupName + ".MaxRequestsPerMinute";
            public const string MaxRequestsPerHour = GroupName + ".MaxRequestsPerHour";
            public const string MaxRequestsPerDay = GroupName + ".MaxRequestsPerDay";
        }

        // General Settings
        public static class General
        {
            public const string GroupName = FamilyMeetSettings.GroupName + ".General";
            
            public const string ApplicationName = GroupName + ".ApplicationName";
            public const string ApplicationVersion = GroupName + ".ApplicationVersion";
            public const string DefaultLanguage = GroupName + ".DefaultLanguage";
            public const string DefaultTimeZone = GroupName + ".DefaultTimeZone";
            public const string MaintenanceMode = GroupName + ".MaintenanceMode";
            public const string MaintenanceMessage = GroupName + ".MaintenanceMessage";
        }
    }

    public class FamilyMeetSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            // Authentication Settings
            context.Add(
                new SettingDefinition(
                    FamilyMeetSettings.Authentication.GoogleClientId,
                    string.Empty,
                    displayName: L("DisplayName:FamilyMeet.Authentication.GoogleClientId"),
                    description: L("Description:FamilyMeet.Authentication.GoogleClientId"),
                    group: FamilyMeetSettings.Authentication.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Authentication.GoogleClientSecret,
                    string.Empty,
                    displayName: L("DisplayName:FamilyMeet.Authentication.GoogleClientSecret"),
                    description: L("Description:FamilyMeet.Authentication.GoogleClientSecret"),
                    group: FamilyMeetSettings.Authentication.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Authentication.JwtExpirationMinutes,
                    "60",
                    displayName: L("DisplayName:FamilyMeet.Authentication.JwtExpirationMinutes"),
                    description: L("Description:FamilyMeet.Authentication.JwtExpirationMinutes"),
                    group: FamilyMeetSettings.Authentication.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Authentication.JwtSecretKey,
                    "FamilyMeetSecretKey123456789",
                    displayName: L("DisplayName:FamilyMeet.Authentication.JwtSecretKey"),
                    description: L("Description:FamilyMeet.Authentication.JwtSecretKey"),
                    group: FamilyMeetSettings.Authentication.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                )
            );

            // File Storage Settings
            context.Add(
                new SettingDefinition(
                    FamilyMeetSettings.FileStorage.Provider,
                    "Local",
                    displayName: L("DisplayName:FamilyMeet.FileStorage.Provider"),
                    description: L("Description:FamilyMeet.FileStorage.Provider"),
                    group: FamilyMeetSettings.FileStorage.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.FileStorage.LocalStoragePath,
                    "./uploads",
                    displayName: L("DisplayName:FamilyMeet.FileStorage.LocalStoragePath"),
                    description: L("Description:FamilyMeet.FileStorage.LocalStoragePath"),
                    group: FamilyMeetSettings.FileStorage.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.FileStorage.MaxFileSize,
                    "10485760", // 10MB
                    displayName: L("DisplayName:FamilyMeet.FileStorage.MaxFileSize"),
                    description: L("Description:FamilyMeet.FileStorage.MaxFileSize"),
                    group: FamilyMeetSettings.FileStorage.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                ),
                new SettingDefinition(
                    FamilyMeetSettings.FileStorage.AllowedExtensions,
                    ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.txt",
                    displayName: L("DisplayName:FamilyMeet.FileStorage.AllowedExtensions"),
                    description: L("Description:FamilyMeet.FileStorage.AllowedExtensions"),
                    group: FamilyMeetSettings.FileStorage.GroupName,
                    isVisibleToClients: false,
                    isLocalizable: false
                )
            );

            // Video Call Settings
            context.Add(
                new SettingDefinition(
                    FamilyMeetSettings.VideoCall.MaxParticipants,
                    "50",
                    displayName: L("DisplayName:FamilyMeet.VideoCall.MaxParticipants"),
                    description: L("Description:FamilyMeet.VideoCall.MaxParticipants"),
                    group: FamilyMeetSettings.VideoCall.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                ),
                new SettingDefinition(
                    FamilyMeetSettings.VideoCall.EnableRecording,
                    "true",
                    displayName: L("DisplayName:FamilyMeet.VideoCall.EnableRecording"),
                    description: L("Description:FamilyMeet.VideoCall.EnableRecording"),
                    group: FamilyMeetSettings.VideoCall.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                ),
                new SettingDefinition(
                    FamilyMeetSettings.VideoCall.MaxCallDurationMinutes,
                    "240", // 4 hours
                    displayName: L("DisplayName:FamilyMeet.VideoCall.MaxCallDurationMinutes"),
                    description: L("Description:FamilyMeet.VideoCall.MaxCallDurationMinutes"),
                    group: FamilyMeetSettings.VideoCall.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                )
            );

            // Security Settings
            context.Add(
                new SettingDefinition(
                    FamilyMeetSettings.Security.EnableTwoFactorAuthentication,
                    "false",
                    displayName: L("DisplayName:FamilyMeet.Security.EnableTwoFactorAuthentication"),
                    description: L("Description:FamilyMeet.Security.EnableTwoFactorAuthentication"),
                    group: FamilyMeetSettings.Security.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Security.PasswordPolicyMinLength,
                    "8",
                    displayName: L("DisplayName:FamilyMeet.Security.PasswordPolicyMinLength"),
                    description: L("Description:FamilyMeet.Security.PasswordPolicyMinLength"),
                    group: FamilyMeetSettings.Security.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Security.MaxLoginAttempts,
                    "5",
                    displayName: L("DisplayName:FamilyMeet.Security.MaxLoginAttempts"),
                    description: L("Description:FamilyMeet.Security.MaxLoginAttempts"),
                    group: FamilyMeetSettings.Security.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                )
            );

            // Audit Settings
            context.Add(
                new SettingDefinition(
                    FamilyMeetSettings.Audit.EnableAuditLogging,
                    "true",
                    displayName: L("DisplayName:FamilyMeet.Audit.EnableAuditLogging"),
                    description: L("Description:FamilyMeet.Audit.EnableAuditLogging"),
                    group: FamilyMeetSettings.Audit.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                ),
                new SettingDefinition(
                    FamilyMeetSettings.Audit.AuditLogRetentionDays,
                    "90",
                    displayName: L("DisplayName:FamilyMeet.Audit.AuditLogRetentionDays"),
                    description: L("Description:FamilyMeet.Audit.AuditLogRetentionDays"),
                    group: FamilyMeetSettings.Audit.GroupName,
                    isVisibleToClients: true,
                    isLocalizable: true
                )
            );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<FamilyMeetResource>(name);
        }
    }
}
