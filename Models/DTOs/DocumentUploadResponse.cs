using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs
{
    /*
     This class will send back a response to the client after a document has been  uploaded   successfully to the blob storage. 
    it also confirms that the upload worked by returning the file name and the url where it is stored in the Azure blob storage. (Microsoft,2024)
    Similar to how MenuItemResponse works for the  menu
     */
    public class DocumentUploadResponse
    {
        //The name of the file that was uploaded
        public string FileName { get; set; } = string.Empty;
        //The URL where the file is stored in the Azure blob, this comes from BolbClient.Uri after uploading (Microsoft,2024)
        public string BlobUrl { get; set; } = string.Empty;

        //a confrimation message after the success of the upload 
        public string Message { get; set; } = string.Empty;
    }
  
    //Code attribution ( The code was taken from):
    // -------------------------------------------------
    //Microsoft.(2024). BlobItemProperties Class — Azure SDK for .NET. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobitemproperties (Accessed: 9 September 2026).
    //Microsoft (2023). Create Data Transfer Objects (DTOs). Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5 (Accessed: 9 September 2026).
    //Microsoft (2024). BlobClient.Uri Property — Azure SDK for .NET. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.uri (Accessed: 9 September 2026).

}
