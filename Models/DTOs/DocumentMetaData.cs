using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs

    /*
     This class  holds the basic file infromation when listing documents from the blob storage.
    Instead of sending raw Azure data back to the client , it is packaged into this class (Microsoft ,2023)
     */
{
    public class DocumentMetaData
    {
        //The name of the file 
        public string FileName { get; set; } = string.Empty;

        //this determines the size of the file in bytes,
        public long FileSize { get; set; }

        //this is the date and time when the file was last modified, the question mark represents that the value can be null, which is useful when the file has not been modified yet.(Microsoft,2024)
        public DateTime LastModified { get; set; }

    }

    //References:
    // ----------------------
    //Microsoft (2023). Create Data Transfer Objects (DTOs). Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5 (Accessed: 9 September 2026).
    //Microsoft (2024). BlobItemProperties Class - Azure SDK for .NET. Microsoft Learn.Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobitemproperties (Accessed: 9 September 2026).
}

