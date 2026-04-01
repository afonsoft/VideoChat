using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace FamilyMeet.Domain.Users
{
    public class User : FullAuditedAggregateRoot<Guid>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public string Language { get; set; } = "en";
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Social authentication
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? MicrosoftId { get; set; }

        // User preferences
        public string? Preferences { get; set; } // JSON string for user preferences

        protected User()
        {
        }

        public User(
            Guid id,
            string userName,
            string email,
            string firstName,
            string lastName
        ) : base(id)
        {
            UserName = userName;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            IsActive = true;
            IsOnline = false;
            TwoFactorEnabled = false;
            AccessFailedCount = 0;
            TimeZone = "UTC";
            Language = "en";
        }

        public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? avatar, string timeZone, string language)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Avatar = avatar;
            TimeZone = timeZone;
            Language = language;
        }

        public void SetOnlineStatus(bool isOnline)
        {
            IsOnline = isOnline;
            if (isOnline)
            {
                LastLoginAt = DateTime.UtcNow;
            }
        }

        public void EnableTwoFactor()
        {
            TwoFactorEnabled = true;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
        }

        public void LockOut(DateTime? lockoutEnd)
        {
            LockoutEnd = lockoutEnd;
            AccessFailedCount = 0;
        }

        public void ResetAccessFailedCount()
        {
            AccessFailedCount = 0;
        }

        public void IncrementAccessFailedCount()
        {
            AccessFailedCount++;
        }

        public bool IsLockedOut()
        {
            return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        }

        public void SetRefreshToken(string? refreshToken, DateTime? expiryTime)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiryTime = expiryTime;
        }

        public void ClearRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpiryTime = null;
        }

        public bool IsRefreshTokenValid(string refreshToken)
        {
            return !string.IsNullOrEmpty(RefreshToken) &&
                   RefreshToken == refreshToken &&
                   RefreshTokenExpiryTime.HasValue &&
                   RefreshTokenExpiryTime.Value > DateTime.UtcNow;
        }

        public void LinkGoogleAccount(string? googleId)
        {
            GoogleId = googleId;
        }

        public void UnlinkGoogleAccount()
        {
            GoogleId = null;
        }

        public void LinkFacebookAccount(string? facebookId)
        {
            FacebookId = facebookId;
        }

        public void UnlinkFacebookAccount()
        {
            FacebookId = null;
        }

        public void LinkMicrosoftAccount(string? microsoftId)
        {
            MicrosoftId = microsoftId;
        }

        public void UnlinkMicrosoftAccount()
        {
            MicrosoftId = null;
        }

        public void UpdatePreferences(string? preferences)
        {
            Preferences = preferences;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        public bool HasLinkedSocialAccount()
        {
            return !string.IsNullOrEmpty(GoogleId) ||
                   !string.IsNullOrEmpty(FacebookId) ||
                   !string.IsNullOrEmpty(MicrosoftId);
        }
    }
}
