using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace UploadImageToCloudinary.Service
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IOptions<Credentials> configuration)
        {
            var cloudinarySettings = configuration.Value;
            var account = new Account(
                  cloudinarySettings.CloudName,
                  cloudinarySettings.ApiKey,
                  cloudinarySettings.ApiSecret
          );
            _cloudinary = new Cloudinary(account);
        }
        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folderName)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folderName // Specify the folder name here
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
            }
            // Replace '/' with '-' in publicId
            uploadResult.PublicId = uploadResult.PublicId.Replace("/", "-");
            return uploadResult;
        }
        public async Task<List<string>> GetAllImagesAsync()
        {
            var listResult = new List<string>();
            var nextCursor = string.Empty;

            do
            {
                var result = await _cloudinary.ListResourcesAsync(new ListResourcesParams()
                {
                    Type = "upload",
                    ResourceType = ResourceType.Image,
                    MaxResults = 500,
                    NextCursor = nextCursor
                });

                if (result.Resources != null)
                {
                    foreach (var resource in result.Resources)
                    {
                        listResult.Add(resource.SecureUrl.ToString());
                    }
                }

                nextCursor = result.NextCursor;
            } while (!string.IsNullOrEmpty(nextCursor));

            return listResult;
        }
        public async Task<string> GetImageByKeyAsync(string publicId)
        {
            try
            {
                // Replace '-' with '/' in publicId
                publicId = publicId.Replace("-", "/");

                var resource = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId));

                if (resource == null || string.IsNullOrEmpty(resource.SecureUrl))
                {
                    return null;
                }

                return resource.SecureUrl;
            }
            catch (Exception ex)
            {
                // Log the exception
                return null;
            }
        }
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            // Replace '-' with '/' in publicId
            publicId = publicId.Replace("-", "/");
            var deletionParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deletionParams);
        }
        //delete folder and all image insight
        public async Task<DeleteFolderResult> DeleteFolderAsync(string folderName)
        {
            return await _cloudinary.DeleteFolderAsync(folderName);
        }

    }
}
