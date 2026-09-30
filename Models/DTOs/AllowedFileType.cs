using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs
{
    // ALLOWED FILE TYPE Model DTO class
    // This class, as per discussion and rubric consolidation, will hold the list of content types that the staff are allowed to upload to the staff documents. 
    // This design and class structure was used in Semester 1, PROG6221, which is why I decided to use the type and style of MIME validation in this Cloud Part 1.
    public class AllowedFileType
    {
        //<!-- Microsoft Learn, 2024[c] -->
        public static readonly List<string> ContentTypes = new List<string>
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/png",
            "image/jpeg",
            "image/jpg"
        };

        // Public method to check if a given content type is currently on the allowed list
        // The method will return true or false based on the content type being uploaded.
        public static bool IsAllowed(string contentType)
        {
            return ContentTypes.Contains(contentType);
        }
    }
}

// <!-- REFERENCES -->
// -------------------------------------------
// <!-- Anthropic. 2026. Claude (Claude Sonnet 4.6). [AI Assistant]. Available at: <https://claude.ai/> [Accessed 10 September 2026]. -->
// <!-- Microsoft Learn, 2024[c]. List<T> Class, .NET API Reference [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-10.0> [Accessed 11 September 2026]. -->

