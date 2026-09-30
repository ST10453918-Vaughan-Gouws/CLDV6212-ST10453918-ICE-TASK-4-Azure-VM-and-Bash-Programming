using CLDV6212_CoffeeNChill_Functions.Models.DTOs;
using CLDV6212_CoffeeNChill_Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CLDV6212_CoffeeNChill_Functions.Functions
{
    /*
     This class is responsible for handling all staff document related HTTP requests for the CoffeeNChill application.
     - It has three main  functions - one for uploading, one for listing and one for downloading files from the staff-docs blob container.
     - It  works together with the IDocumentBlobService to do the actual blob storage work so that this class stays clean and focused only on handling the HTTP requests
     <!-- Microsoft Learn, 2024[a] -->
    */
    public class Staff_Document_Functions
    {
        private readonly ILogger<Staff_Document_Functions> _logger;
        private readonly IDocumentBlobService _documentBlobService;

        /*
          -The following code  defines a list of file types that are allowed to be uploaded to the staff-docs container. 
        - This is important because we dont want people uploading random or dangerous files to our storage.
        - It will  only allow PDFs, Word Documents, Modern Word Docx, PNG images, JPEG images and JPG images for now, as these are the most common file types used for staff documents
         <!-- OWASP Foundation, 2021 -->
        */
        private static readonly HashSet<string> AllowedContentTypes = new HashSet<string>
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/png",
            "image/jpeg",
            "image/jpg"
        };

        /*
         -Using a constructor for the Staff_Document_Functions class.
         -When Azure Functions starts up it will automatically pass in the logger and the blob service for us through dependency injection.
         -This means we dont have to manually create these objects ourselves,Azure handles that for us behind the scenes
         <!-- Microsoft Learn, 2024[b] -->
        */
        public Staff_Document_Functions(ILogger<Staff_Document_Functions> logger, IDocumentBlobService documentBlobService)
        {
            _logger = logger;
            _documentBlobService = documentBlobService;
        }

        /*
         -This function is triggered when someone sends a POST request to /api/documents/upload with a file attached to the request body.
         -It first validates that a file was sent and that the file type is allowed before uploading it to the staff-docs blob container.
         - Everything is wrapped up  in a try catch so that if something goes wrong with the blob storage connection we can log the error and return a proper error response to the client instead of crashing
         <!-- Microsoft Learn, 2024[a] -->
         <!-- Stack Overflow, 2012 -->
        */
        [Function("UploadStaffDocument")]
        public async Task<IActionResult> UploadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req)
        {
            //Uisng a  try catch so that any unexpected errors are caught and logged properly instead of crashing the function
            // <!-- Microsoft Learn, 2024[a] -->
            try
            {
                //first we check if the request actually has any files in it and if there are no files we cannot do anything so we return a 400 bad request
                if (req.Form.Files == null || req.Form.Files.Count == 0)
                {
                    //We then  log a warning so we know someone sent a request without a file
                    _logger.LogWarning("UploadStaffDocument was called but no file was attached to the request.");
                    return new BadRequestObjectResult("No file was found in the request. Please make sure you attach a file before sending.");
                }

                //We try to get the file by looking for a field named "file" first
                //if that doesnt exist we just grab the first file in the request
                var file = req.Form.Files["file"] ?? req.Form.Files[0];
                 
                //This makes sure  that the file actually has some content in it
                //an empty file is useless so we reject it with a 400 bad request
                if (file.Length == 0)
                {
                    _logger.LogWarning("UploadStaffDocument received an empty file: {FileName}", file.FileName);
                    return new BadRequestObjectResult("The file you uploaded appears to be empty. Please upload a valid file with content.");
                }

                //In the following code ,  we check if the file type is one of the allowed types
                //It alos checks the content type of the file which tells us what kind of file it is
                //For example application/pdf means it is a PDF file
                //if the file type is not in our allowed list we reject it with a 400
                // <!-- OWASP Foundation, 2021 -->
                // <!-- Stack Overflow, 2012 -->
                if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.LogWarning("UploadStaffDocument rejected file {FileName} because the file type {ContentType} is not allowed.", file.FileName, file.ContentType);
                    return new BadRequestObjectResult($"The file type '{file.ContentType}' is not allowed. Please upload a PDF, DOC, DOCX, PNG, JPEG or JPG file only.");
                }


                //This  will open the file as a stream so we can send it to blob storage
                //We use the USING  keyword so the stream is automatically closed when done
                // <!-- Microsoft Learn, 2024[c] -->
                using var stream = file.OpenReadStream();

                //This calls the blob service to handle the actual upload to Azure Blob Storage
                //we pass the content type along so it gets stored correctly in blob storage
                //this means when someone downloads the file later it will open correctly
                // <!-- Microsoft Learn, 2024[c] -->
                var response = await _documentBlobService.UploadDocumentAsync(file.FileName, stream, file.ContentType);

                //log a success message so we know the upload worked
                _logger.LogInformation("UploadStaffDocument successfully uploaded file {FileName} to staff-docs.", file.FileName);

                //if everything went well we return a 201 created response
                //this tells the client the file was saved and shows where they can download it
                return new CreatedResult($"/api/documents/download/{response.FileName}", response);
            }
            catch (Exception ex)
            {
                //if something unexpected goes wrong we log the full error details
                //this helps us figure out what went wrong when we check the logs later
                // <!-- Microsoft Learn, 2024[a] -->
                _logger.LogError(ex, "UploadStaffDocument failed while trying to upload a file to staff-docs. Error: {Message}", ex.Message);
                return new ObjectResult("Something went wrong while uploading the file. Please try again later.")
                {
                    StatusCode = 500
                };
            }
        }

        /*
         this function is triggered when someone sends a GET request to
         /api/documents to see all the files stored in the staff-docs container.
         it simply asks the blob service to fetch all the file information
         and returns it back to the client as a JSON list.
         we wrap it in a try catch so that if blob storage is unreachable
         we can log the error and return a proper response
         <!-- Microsoft Learn, 2024[d] -->
        */
        [Function("ListStaffDocuments")]
        public async Task<IActionResult> ListStaffDocuments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
        {
            //Using  a try catch so that any unexpected errors are caught and logged
            // <!-- Microsoft Learn, 2024[d] -->
            try
            {
                //Ask the blob service to go and fetch all the files in the staff-docs container
                //it will return a list of DocumentMetaData objects with the file details
                var documents = await _documentBlobService.ListDocumentsAsync();

                //we log that the list was successfully retrieved
                _logger.LogInformation("ListStaffDocuments successfully retrieved all documents from staff-docs.");

                //Return the list back to the client with a 200 ok response
                //if there are no files in the container the list will just be empty
                return new OkObjectResult(documents);
            }
            catch (Exception ex)
            {
                //if something goes wrong ,log the error with full details , we s can till trace back what happened when checking the logs
                // <!-- Microsoft Learn, 2024[d] -->
                _logger.LogError(ex, "ListStaffDocuments failed while trying to retrieve documents from staff-docs. Error: {Message}", ex.Message);
                return new ObjectResult("Something went wrong while retrieving the documents. Please try again later.")
                {
                    StatusCode = 500
                };
            }
        }

        /*
         -This function is triggered when someone sends a GET request to /api/documents/download/{fileName} where fileName is the name of
         -The file they want to download e.g. /api/documents/download/barista-guide.pdf
         - it  also checks that a file name was provided in the route then tries to find and download that specific file from the staff-docs blob container.
         - We  use DocumentDownloadResult which carries both the file stream and the original content type so the browser knows exactly what kind  of file it is receiving e.g. PDF, PNG or JPG
         <!-- Microsoft Learn, 2024[e] -->
         <!-- Stack Overflow, 2013 -->
        */
        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
            string fileName)
        {
            //Using  a try catch so that any unexpected errors are caught and logged
            // <!-- Microsoft Learn, 2024[e] -->
            try
            {
                //This  checks if the file name was actually provided in the route
                //without a file name we dont know what to download so we return a 400
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.LogWarning("DownloadStaffDocument was called but no file name was provided in the route.");
                    return new BadRequestObjectResult("Please provide the name of the file you want to download.");
                }

                //We  then  ask the blob service to try and find and download the file
                //it returns a DocumentDownloadResult which has both the stream and content type
                //if the file does not exist the service will return null
                // <!-- Microsoft Learn, 2024[e] -->
                var result = await _documentBlobService.DownloadDocumentAsync(fileName);

                //if the result is null it means the file was not found in blob storage
                //Return a 404 not found so the client knows the file doesnt exist
                if (result == null)
                {
                    _logger.LogWarning("DownloadStaffDocument could not find the file {FileName} in staff-docs.", fileName);
                    return new NotFoundObjectResult($"Sorry, the file '{fileName}' could not be found in the staff-docs container.");
                }

                // log that the download was successful
                _logger.LogInformation("DownloadStaffDocument successfully downloaded file {FileName} from staff-docs.", fileName);

                //Return the file as a downloadable stream to the client
                //we use the content type from the result so the browser knows what kind of file it is
                //for example a PDF will open as a PDF and a PNG will display as an image
                //FileDownloadName sets the name the file will have when it downloads on the clients side
                // <!-- Stack Overflow, 2013 -->
                // <!-- Microsoft Learn, 2024[e] -->
                return new FileStreamResult(result.Content, result.ContentType)
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                //if something unexpected goes wrong we log the full error details
                //this helps us figure out what went wrong when we check the logs later
                // <!-- Microsoft Learn, 2024[e] -->
                _logger.LogError(ex, "DownloadStaffDocument failed while trying to download {FileName} from staff-docs. Error: {Message}", fileName, ex.Message);
                return new ObjectResult("Something went wrong while downloading the file. Please try again later.")
                {
                    StatusCode = 500
                };
            }
        }
    }

    // <!-- REFERENCE LIST -->
    // ----------------------------------
    // <!-- Microsoft Learn, 2024[a]. Azure Functions HTTP trigger, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[b]. Dependency injection in .NET Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-dependency-injection> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[c]. BlobClient.UploadAsync Method - Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.uploadasync> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[d]. BlobContainerClient.GetBlobsAsync Method - Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient.getblobsasync> [Accessed 11 September 2026]. -->
    // <!-- Microsoft Learn, 2024[e]. FileStreamResult Class - ASP.NET Core [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filestreamresult> [Accessed 11 September 2026]. -->
    // <!-- Stack Overflow, 2013. Do I need Content-Type: application/octet-stream for file download?. MIME Media Types [Online]. Available at: <https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download> [Accessed 11 September 2026]. -->
    // <!-- Stack Overflow, 2012. How to validate file type in ASP.NET [Online]. Available at: <https://stackoverflow.com/questions/9732098/how-to-validate-file-type-before-upload> [Accessed 11 September 2026]. -->
    // <!-- OWASP Foundation, 2021. File Upload Cheat Sheet. OWASP [Online]. Available at: <https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html> [Accessed 11 September 2026]. -->
}