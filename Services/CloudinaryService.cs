using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SkyLegends.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly IWebHostEnvironment _environment;

        public CloudinaryService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _environment = environment;

            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
                ?? configuration["Cloudinary:CloudName"];
            var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
                ?? configuration["Cloudinary:ApiKey"];
            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
                ?? configuration["Cloudinary:ApiSecret"];

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
            }
        }

        public bool IsConfigured => _cloudinary != null;

        /// <summary>
        /// Uploads an image. Uses Cloudinary in production, local filesystem in development.
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            if (_cloudinary != null)
            {
                return await UploadToCloudinaryAsync(imageFile);
            }

            // Fallback: save locally (development)
            return await SaveLocallyAsync(imageFile);
        }

        private async Task<string> UploadToCloudinaryAsync(IFormFile imageFile)
        {
            await using var stream = imageFile.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, stream),
                Folder = "skylegends/posters",
                Transformation = new Transformation()
                    .Width(1200).Height(1200).Crop("limit").Quality("auto")
            };

            var result = await _cloudinary!.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {result.Error.Message}");
            }

            return result.SecureUrl.ToString();
        }

        private async Task<string> SaveLocallyAsync(IFormFile imageFile)
        {
            var fileName = Path.GetFileName(imageFile.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "img", "posters");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/img/posters/{uniqueFileName}";
        }
    }
}
