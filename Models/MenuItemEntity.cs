using System;
using Azure;
using Azure.Data.Tables;

namespace CLDV6212_CoffeeNChill_Functions.Models
{
    public class MenuItemEntity : ITableEntity
    {
        // MENU_ITEM_ENTITY is used to shape the menu item storage to be used for the Table Storage
        // The PartitionKey is used to group the items toghether by their category, which makes the querying process within a partition much faster
        // The RowKey uniquely identifies the item entity (SKU) within it category, known as the partition. 
        // In combination with the PartitionKey, it forms the entity's unique PK. 
        // PartitionKey = Catrgory and RowKey = SKU/ID 
        // This design was chosen based upon the way that the staff and customers will query the menu, by always trying to show all the items in a category
        // By grouping, using the category as the PartitionKey, means that Azure Table Storage will store all beverages together, all snacks together. etc.
        // The RowKey (SKU) then just needs to be unqiue within that specific category
        // Two different cateogries might even be able to use the same SKU number, since the PartitionKey and RowKey in combination is what has to be unique, and not just one in isolation. 

        // All of these variables are required by the ITableEntity and will be managed automaitcally by the Table Storage. 
        // = string.Empty ensure that these proerties are guarenteed to have some value assigned, avoiding any build errors or warnings. 
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty; 
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

       // Variables needed for the menu items
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}

// Reference: Microsoft (2024) Azure.Data.Tables ITableEntity Interface.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.itableentity
// (Accessed: 8 September 2026).
