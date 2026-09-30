using CLDV6212_CoffeeNChill_Functions.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_CoffeeNChill_Functions.Services
{
    /*
     This is an interface that will define what the blob services must  be able  to do 
    , which includes  upload ,list and download */
    public interface IDocumentBlobService
    {
        // this method uploads a file to the staff-docs blob container and returns a DocumentUploadResponse with the file name and blob url (Microsoft,2024)
       Task<DocumentUploadResponse> UploadDocumentAsync(string fileName, Stream fileStream,string contentType );
        
        //this method will list all the  files that are stored  in the staff-docs container and return a list of DocumentMetaData objects with the file infromation 
        Task<IEnumerable<DocumentMetaData>> ListDocumentsAsync();

        //This method will download a document from the staff-docs blob container and returns the DocumentDownloadResults as a Stream of the File content and Content Type to the client (Microsoft,2024)
        Task<DocumentDownloadResult?> DownloadDocumentAsync(string fileName);
    }

    //Code attribution ( The code was taken from) :
    // -------------------------------------------------
    //Microsoft (2024) Azure Blob Storage client library for .NET. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme  (Accessed: 10 September 2026).
    //Microsoft (2024) BlobContainerClient Class - Azure SDK for .NET. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient (Accessed:10 September 2026).
}
