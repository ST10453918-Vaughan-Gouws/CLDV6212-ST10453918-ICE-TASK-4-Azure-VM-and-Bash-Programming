using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs
{
    // This class is a DTO class, meaning these values in this class will be presented to the user/staff/client, as they are interested in seeing the data modeled and stored in the DTO's
    // The class will carry both the file content and it's content type whenever a document is downloaded from the staff-docs blob container. 
    // Without this class, a stream only will be returned, which causes the lose of the MIME type, which is needed by the Function layer in order to serve the file back correcly
    // It is also needed for validation purposese 
    // A Pdf should come back as application/pdf and not as a generic download document. 


    // <!-- Microsoft Learn, 2024[a] and Microsoft Learn, 2024[b] -->
    // <!-- Anthropic, 2026 -->
    // Parts of the code was learned and taken from Microsoft Learn
    // The code planning and idea was consolidated using Claude, in order to indentify gaps in the BlobDocument Logic and improve the staff document services. 

    public class DocumentDownloadResult
    {
        // This variable contains the content of the file, which is streamed from the blob storage
        public Stream Content { get; set; } = Stream.Null;

        // <!-- Stack Overflow, 2013 --> 
        // <!-- Part of this code was taken from Stack Overflow -->
        // <!-- https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download -->
        // <!-- https://stackoverflow.com/users/400547/jon-hanna -->
        public string ContentType { get; set; } = "application/octet-stream";
    }
}

// <!-- REFERENCE LIST --> 
// ---------------------------------
// <!-- Microsoft Learn, 2024[a]. BlobDownloadDetails.ContentType Property, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobdownloaddetails.contenttype?view=azure-dotnet> [Accessed 11 September 2026]. -->
// <!-- Microsoft Learn, 2024[b]. Azure.Storage.Blobs.Models, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models?view=azure-dotnet> [Accessed 11 September 2026]. -->
// <!-- Stack Overflow, 2013. Do I need Content-Type: application/octet-stream for file download?. MIME Media Types [Online]. Available at: <https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download> [Accessed 11 September 2026]. -->
