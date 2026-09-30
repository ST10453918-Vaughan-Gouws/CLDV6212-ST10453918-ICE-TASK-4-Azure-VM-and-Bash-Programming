namespace CLDV6212_CoffeeNChill_Functions.Models.DTOs
{
    // The Responses that will be provided to the requests being made
    // Modeling the response messages
    public class MenuItemResponse
    {
        public string Category { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; }

        public static MenuItemResponse FromEntity(MenuItemEntity entity) => new()
        {
            Category = entity.PartitionKey,
            SKU = entity.RowKey,
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            IsAvailable = entity.IsAvailable
        };
    }
}
// Reference: Microsoft (2024) Web API design best practices - Azure Architecture Center.
// Available at: https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design
// (Accessed: 8 September 2026).

