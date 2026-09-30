using System;

namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs
{
    // This class contains the menu item request modeling
    // This is what a client sends in the JSON body when creating or updating an item in the menu
    public class MenuItemRequest
    {
        // Required when creating a new item
        public string? Category { get; set; }
        public string? SKU { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Used by both Create and Update
        public double? Price { get; set; }
        public bool? IsAvailable { get; set; }
    }
}

// Reference: Microsoft (2024) Nullable reference types - C# reference.
// Available at: https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references
// (Accessed: 8 September 2026).
