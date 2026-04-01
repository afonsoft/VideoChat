using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using FamilyMeet.Settings;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace FamilyMeet.Application.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder = "uploads");
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "uploads");
        Task<byte[]> DownloadFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<List<string>> GetFilesAsync(string folder = "uploads");
        Task<bool> FileExistsAsync(string filePath);
        string GetFileUrl(string filePath);
    }

    public class FileStorageService : ITransientDependency, IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingManager _settingManager;
        private readonly string _storagePath;

        public FileStorageService(
            IConfiguration configuration,
            ISettingManager settingManager)
        {
            _configuration = configuration;
            _settingManager = settingManager;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            
            // Ensure storage directory exists
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0)
            {
                throw new UserFriendlyException("No file provided.");
            }

            // Validate file size
            var maxFileSize = await GetMaxFileSizeAsync();
            if (file.Length > maxFileSize)
            {
                throw new UserFriendlyException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB.");
            }

            // Validate file extension
            var allowedExtensions = await GetAllowedExtensionsAsync();
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new UserFriendlyException($"File extension '{fileExtension}' is not allowed.");
            }

            // Generate unique file name
            var fileName = GenerateUniqueFileName(file.FileName);
            var folderPath = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(folderPath, fileName);

            // Ensure folder exists
            Directory.CreateDirectory(folderPath);

            // Save file
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            return GetRelativePath(filePath);
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "uploads")
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                throw new UserFriendlyException("No file data provided.");
            }

            // Validate file size
            var maxFileSize = await GetMaxFileSizeAsync();
            if (fileBytes.Length > maxFileSize)
            {
                throw new UserFriendlyException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB.");
            }

            // Validate file extension
            var allowedExtensions = await GetAllowedExtensionsAsync();
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new UserFriendlyException($"File extension '{fileExtension}' is not allowed.");
            }

            // Generate unique file name
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var folderPath = Path.Combine(_storagePath, folder);
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Ensure folder exists
            Directory.CreateDirectory(folderPath);

            // Save file
            await File.WriteAllBytesAsync(filePath, fileBytes);

            return GetRelativePath(filePath);
        }

        public async Task<byte[]> DownloadFileAsync(string filePath)
        {
            var fullPath = GetFullPath(filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new UserFriendlyException("File not found.");
            }

            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetFilesAsync(string folder = "uploads")
        {
            var folderPath = Path.Combine(_storagePath, folder);
            
            if (!Directory.Exists(folderPath))
            {
                return new List<string>();
            }

            var files = new List<string>();
            var filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in filePaths)
            {
                files.Add(GetRelativePath(filePath));
            }

            return files;
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = GetFullPath(filePath);
            return File.Exists(fullPath);
        }

        public string GetFileUrl(string filePath)
        {
            return $"/api/files/{filePath}";
        }

        private async Task<long> GetMaxFileSizeAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.FileStorage.MaxFileSize);
            if (long.TryParse(setting, out var size))
            {
                return size;
            }
            
            var configValue = _configuration["FileStorage:MaxFileSize"];
            if (long.TryParse(configValue, out size))
            {
                return size;
            }

            return 10 * 1024 * 1024; // Default 10MB
        }

        private async Task<List<string>> GetAllowedExtensionsAsync()
        {
            var setting = await _settingManager.GetOrNullAsync(FamilyMeetSettings.FileStorage.AllowedExtensions);
            if (!string.IsNullOrEmpty(setting))
            {
                return setting.Split(',').Select(ext => ext.Trim().ToLowerInvariant()).ToList();
            }
            
            var configValue = _configuration["FileStorage:AllowedExtensions"];
            if (!string.IsNullOrEmpty(configValue))
            {
                return configValue.Split(',').Select(ext => ext.Trim().ToLowerInvariant()).ToList();
            }

            // Default allowed extensions
            return new List<string>
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".txt", ".rtf", ".csv",
                ".zip", ".rar", ".7z", ".tar", ".gz",
                ".mp3", ".wav", ".ogg", ".flac",
                ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm"
            };
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            
            // Sanitize file name
            fileNameWithoutExtension = SanitizeFileName(fileNameWithoutExtension);
            
            return $"{fileNameWithoutExtension}_{timestamp}_{random}{extension}";
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "_");
            }
            
            // Remove multiple underscores
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, "_+", "_");
            
            // Remove leading/trailing underscores
            fileName = fileName.Trim('_');
            
            return fileName;
        }

        private string GetFullPath(string relativePath)
        {
            return Path.Combine(_storagePath, relativePath);
        }

        private string GetRelativePath(string fullPath)
        {
            return Path.GetRelativePath(_storagePath, fullPath).Replace("\\", "/");
        }

        public async Task<string> GetStorageProviderAsync()
        {
            return await _settingManager.GetOrNullAsync(FamilyMeetSettings.FileStorage.Provider) 
                   ?? _configuration["FileStorage:Provider"] 
                   ?? "Local";
        }
    }

    // Azure Blob Storage Service (for future implementation)
    public class AzureBlobStorageService : IFileStorageService
    {
        // Implementation for Azure Blob Storage
        public Task<string> UploadFileAsync(IFormFile file, string folder = "uploads")
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "uploads")
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public Task<byte[]> DownloadFileAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public Task<List<string>> GetFilesAsync(string folder = "uploads")
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }

        public string GetFileUrl(string filePath)
        {
            throw new NotImplementedException("Azure Blob Storage not implemented yet.");
        }
    }

    // AWS S3 Storage Service (for future implementation)
    public class AwsS3StorageService : IFileStorageService
    {
        // Implementation for AWS S3 Storage
        public Task<string> UploadFileAsync(IFormFile file, string folder = "uploads")
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "uploads")
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public Task<byte[]> DownloadFileAsync(string filePath)
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public Task<List<string>> GetFilesAsync(string folder = "uploads")
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }

        public string GetFileUrl(string filePath)
        {
            throw new NotImplementedException("AWS S3 Storage not implemented yet.");
        }
    }
}
