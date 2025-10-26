using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Lab_1
{
    public class GoogleDriveService
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveFile };
        private const string ApplicationName = "Cringe";
        private const string CredentialsPath = "credentials.json";
        private const string TokenPath = "token.json";

        private DriveService? _service;

        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                if (!System.IO.File.Exists(CredentialsPath))
                {
                    throw new FileNotFoundException(
                        "Файл credentials.json не знайдено. " +
                        "Завантажте його з Google Cloud Console.");
                }

                UserCredential credential;

                using (var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(TokenPath, true));
                }

                _service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка автентифікації: {ex.Message}", ex);
            }
        }

        public async Task<List<GoogleDriveFile>> ListFilesAsync()
        {
            EnsureAuthenticated();

            var request = _service!.Files.List();
            request.Q = "mimeType='application/json' and trashed=false";
            request.Fields = "files(id, name, modifiedTime, size)";
            request.OrderBy = "modifiedTime desc";
            request.PageSize = 100;

            var result = await request.ExecuteAsync();
            var files = new List<GoogleDriveFile>();

            foreach (var file in result.Files)
            {
                files.Add(new GoogleDriveFile
                {
                    Id = file.Id,
                    Name = file.Name,
                    ModifiedTime = file.ModifiedTime,
                    Size = file.Size
                });
            }

            return files;
        }

        public async Task<string> UploadFileAsync(string filePath, string fileName)
        {
            EnsureAuthenticated();

            var fileMetadata = new File
            {
                Name = fileName,
                MimeType = "application/json"
            };

            FilesResource.CreateMediaUpload request;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                request = _service!.Files.Create(fileMetadata, stream, "application/json");
                request.Fields = "id";
                await request.UploadAsync();
            }

            var file = request.ResponseBody;
            return file.Id;
        }

        public async Task<string> UpdateFileAsync(string fileId, string filePath)
        {
            EnsureAuthenticated();

            var fileMetadata = new File();

            FilesResource.UpdateMediaUpload request;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                request = _service!.Files.Update(fileMetadata, fileId, stream, "application/json");
                request.Fields = "id";
                await request.UploadAsync();
            }

            return fileId;
        }

        public async Task DownloadFileAsync(string fileId, string destinationPath)
        {
            EnsureAuthenticated();

            var request = _service!.Files.Get(fileId);

            using (var stream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                await request.DownloadAsync(stream);
            }
        }

        public async Task DeleteFileAsync(string fileId)
        {
            EnsureAuthenticated();
            await _service!.Files.Delete(fileId).ExecuteAsync();
        }

        public async Task<GoogleDriveFile?> GetFileInfoAsync(string fileId)
        {
            EnsureAuthenticated();

            var request = _service!.Files.Get(fileId);
            request.Fields = "id, name, modifiedTime, size";

            var file = await request.ExecuteAsync();

            return new GoogleDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                ModifiedTime = file.ModifiedTime,
                Size = file.Size
            };
        }

        private void EnsureAuthenticated()
        {
            if (_service == null)
            {
                throw new InvalidOperationException(
                    "Не виконано автентифікацію. Спочатку викличте AuthenticateAsync().");
            }
        }

        public bool IsAuthenticated => _service != null;
    }

    public class GoogleDriveFile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? ModifiedTime { get; set; }
        public long? Size { get; set; }

        public string DisplaySize
        {
            get
            {
                if (Size == null) return "N/A";
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024} KB";
                return $"{Size / (1024 * 1024)} MB";
            }
        }

        public string DisplayModified => ModifiedTime?.ToString("dd.MM.yyyy HH:mm") ?? "N/A";
    }
}
