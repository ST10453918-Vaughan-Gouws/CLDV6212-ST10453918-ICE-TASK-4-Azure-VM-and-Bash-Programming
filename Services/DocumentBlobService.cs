using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CLDV6212_CoffeeNChill_Functions.Models.DTOs;

namespace CLDV6212_CoffeeNChill_Functions.Services
{
    /*
     This class implements the IDocumentBlobService interface.
     It handles all the blob storage operations for the staff-docs container.
     It connects to Azure Blob Storage using the BlobServiceClient (Microsoft, 2024)
    */
    public class DocumentBlobService : IDocumentBlobService
    {
        private readonly BlobContainerClient _containerClient;

        //the name of the container where all staff documents are stored
        private const string ContainerName = "staffdocs";

        /*
         this constructor runs when the service is first created
         it gets a reference to the staff-docs container from blob storage
         we dont create the container here anymore because constructors
         cannot run async code - we do it inside the upload method instead
         <!-- Microsoft Learn, 2024[b] -->
        */
        public DocumentBlobService(BlobServiceClient blobServiceClient)
        {
            //get a reference to the staff-docs container
            _containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        }

        /*
         this method uploads a file to the staff-docs blob container
         it takes the file name, file stream and content type as parameters
         it also cleans the file name to remove spaces and special characters
         returns a DocumentUploadResponse with the file name and blob url
         <!-- Microsoft Learn, 2024[c] -->
        */
        public async Task<DocumentUploadResponse> UploadDocumentAsync(string fileName, Stream fileStream, string contentType)
        {
            //create the container if it doesnt already exist
            //we do this here instead of the constructor so it runs asynchronously
            // <!-- Microsoft Learn, 2024[c] -->
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            //clean the file name by replacing spaces with hyphens
            //blob storage does not allow spaces or special characters in file names
            // <!-- Stack Overflow, 2012 -->
            var cleanFileName = fileName.Replace(" ", "-").ToLower();

            //get a reference to the blob using the cleaned file name
            var blobClient = _containerClient.GetBlobClient(cleanFileName);

            //set the content type so the file is served correctly
            //e.g. application/pdf means it will open as a PDF
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            //upload the file stream to blob storage
            await blobClient.UploadAsync(fileStream, uploadOptions);

            //return the response with the cleaned file name and where it is stored
            return new DocumentUploadResponse
            {
                FileName = cleanFileName,
                BlobUrl = blobClient.Uri.ToString(),
                Message = $"{cleanFileName} was uploaded successfully to staff-docs"
            };
        }

        /*
         this method lists all files stored in the staff-docs blob container
         it loops through all the blobs and returns their basic info
         using the DocumentMetaData class we created
         <!-- Microsoft Learn, 2024[d] -->
        */
        public async Task<IEnumerable<DocumentMetaData>> ListDocumentsAsync()
        {
            // Has to create a client container as Azurite Docker container starts with zero containers everytime it refreshes or starts. 
            // This code will create the container if is does not exist. 
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            
            //create a list to store the file information
            var documents = new List<DocumentMetaData>();

            //loop through all the blobs in the container
            // <!-- Microsoft Learn, 2024[d] -->
            await foreach (BlobItem blob in _containerClient.GetBlobsAsync())
            {
                //add each files information to the list
                documents.Add(new DocumentMetaData
                {
                    FileName = blob.Name,
                    FileSize = blob.Properties.ContentLength ?? 0,
                    LastModified = blob.Properties.LastModified?.DateTime ?? DateTime.MinValue
                });
            }

            return documents;
        }

        /*
         this method downloads a specific file from the staff-docs container
         it returns a DocumentDownloadResult which carries both the file
         stream and the original content type so the browser knows what
         kind of file it is receiving e.g. PDF, PNG or JPG
         returns null if the file does not exist
         <!-- Microsoft Learn, 2024[a] -->
         <!-- Stack Overflow, 2013 -->
        */
        public async Task<DocumentDownloadResult?> DownloadDocumentAsync(string fileName)
        {
            //get a reference to the specific blob we want to download
            var blobClient = _containerClient.GetBlobClient(fileName);

            //check if the file exists before trying to download it
            bool exists = await blobClient.ExistsAsync();
            if (!exists)
            {
                return null;
            }

            //download the file and return it as a stream
            // <!-- Microsoft Learn, 2024[a] -->
            // <!-- Stack Overflow, 2023 -->
            var download = await blobClient.DownloadStreamingAsync();

            //get the file stream and content type out of the download results
            var fileStream = download.Value.Content;
            var fileContentType = download.Value.Details.ContentType;

            //if the content type is empty we default to a generic file download
            // <!-- Stack Overflow, 2013 -->
            if (string.IsNullOrEmpty(fileContentType))
            {
                fileContentType = "application/octet-stream";
            }

            //shape the result object so the function can send it back to the client
            var result = new DocumentDownloadResult
            {
                Content = fileStream,
                ContentType = fileContentType
            };

            return result;
        }
    }

    //Code attribution ( The code was taken from) :
    //Microsoft (2024) Azure Blob Storage client library for .NET. Microsoft Learn.
    //    Available at: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme
    //    (Accessed: 10 September 2026).
    //Microsoft (2024) BlobContainerClient Class - Azure SDK for .NET. Microsoft Learn.
    //    Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient
    //    (Accessed: 10 September 2026).
    //Microsoft (2024) BlobClient.UploadAsync Method - Azure SDK for .NET. Microsoft Learn.
    //    Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.uploadasync
    //    (Accessed: 10 September 2026).
    //Microsoft (2024) BlobContainerClient.GetBlobsAsync Method - Azure SDK for .NET. Microsoft Learn.
    //    Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient.getblobsasync
    //    (Accessed: 10 September 2026).

    // <!-- REFERENCE LIST -->
    // ----------------------------------
    // <!-- Microsoft Learn, 2024[a]. BlobDownloadDetails.ContentType Property, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobdownloaddetails.contenttype?view=azure-dotnet> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[b]. Dependency injection in .NET Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-dependency-injection> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[c]. BlobContainerClient.CreateIfNotExistsAsync Method [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient.createifnotexistsasync> [Accessed 12 September 2026]. -->
    // <!-- Microsoft Learn, 2024[d]. BlobContainerClient.GetBlobsAsync Method [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient.getblobsasync> [Accessed 11 September 2026]. -->
    // <!-- Stack Overflow, 2013. Do I need Content-Type: application/octet-stream for file download?. MIME Media Types [Online]. Available at: <https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download> [Accessed 11 September 2026]. -->
    // <!-- Stack Overflow, 2012. How to validate file type in ASP.NET [Online]. Available at: <https://stackoverflow.com/questions/9732098/how-to-validate-file-type-before-upload> [Accessed 11 September 2026]. -->
    // <!-- Stack Overflow, 2023. Send content-length in return statement along with download content value [duplicate], Download.Value [Online]. Available at: <https://stackoverflow.com/questions/76720669/send-content-length-in-return-statement-along-with-download-content-value> [Accessed 11 September 2026]. -->
}