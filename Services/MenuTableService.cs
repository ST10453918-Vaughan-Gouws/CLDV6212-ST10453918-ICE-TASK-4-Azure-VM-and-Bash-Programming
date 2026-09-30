using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using CLDV6212_CoffeeNChill_Functions.Models;

namespace CLDV6212_CoffeeNChill_Functions.Services
{
    // The Service Layer Class implements the IMenuTableService Interface while following the Seperation of Concerns
    // Follows the principle by keeping the table storage code seperate from the Azure Function code
    // This service is what actually communicates with the Azure Table Storage. 
    // The Menu Function class just calls the service and gets back the plain MenuItemEntity Objects. 
    
    public class MenuTableService : IMenuTableService
    {
        private readonly TableClient _tableClient;
        private const string TableName = "MenuItems";

        // Makes sure that the tableClient actually exists before trying to read or write 
        public MenuTableService(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient(TableName);
            _tableClient.CreateIfNotExists();
        }

        // The method adds a brand new menu item to the table
        public async Task<MenuItemEntity> CreateMenuItemAsync(MenuItemEntity entity)
        {
            await _tableClient.AddEntityAsync(entity);
            return entity;
        }

        // The method returns every item in the menu, that is stored in the table
        public async Task<IEnumerable<MenuItemEntity>> GetAllMenuItemsAsync()
        {
            // The QueryAsync with no filter returns all of the entities in the table
            // <!-- Microsoft Learn, 2024[3] -->
            var items = new List<MenuItemEntity>();
            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>())
            {
                items.Add(item);
            }
            return items;
        }

        // Returns only the menu items that is found to belong to a single category (The PartitionKey)
        public async Task<IEnumerable<MenuItemEntity>> GetMenuItemsByCategoryAsync(string category)
        {
            
            // <!-- Microsoft Learn, 2024[3] -->
            // Filters the PartionKey in order to query the Table Storage for the specific category. 
            var items = new List<MenuItemEntity>();
            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>(e => e.PartitionKey == category))
            {
                items.Add(item);
            }
            return items;
        }

        // This method checks whether a menu items exsists
        // If the item does exists, it will be returned, otherwise, it will return null if it does not exists.  
        public async Task<MenuItemEntity?> GetMenuItemIfExistsAsync(string category, string id)
        {
            // <!-- Microsoft Learn, 2024[B] -->
            // try-catch for error handling
            try
            {
                var response = await _tableClient.GetEntityIfExistsAsync<MenuItemEntity>(category, id);

                if (response.HasValue)
                {
                    return response.Value;
                }
                return null;
            }
            catch (RequestFailedException ex)  
            {
                // 404 (not found) if the item is not found 
                if (ex.Status == 404)
                {
                    return null;
                }
                throw; 
            }
        }

        public async Task<MenuItemEntity?> UpdateMenuItemAsync(string category, string id, double? price, bool? isAvailable)
        {
            var existing = await GetMenuItemIfExistsAsync(category, id);
            if (existing == null)
            {
                // If there is nothing to update - the calling function will turn this into a 404 (not found)
                return null;
            }

            if (price.HasValue)
            {
                existing.Price = price.Value;
            }

            if (isAvailable.HasValue)
            {
                existing.IsAvailable = isAvailable.Value;
            }

            // The TableUpdateMode.Replace overwrites the whole entity with the updated values
            // <!-- Microsoft Learn, 2024[E] -->
            await _tableClient.UpdateEntityAsync(existing, existing.ETag, TableUpdateMode.Replace);
            return existing;
        }

        // This method deletes a menu item if it exists. It will return a false, so that the menu function can turn it into a clean 404 (not found) instead of crashing.  
        // Boolean 
        public async Task<bool> DeleteMenuItemAsync(string category, string id)
        {
            var existing = await GetMenuItemIfExistsAsync(category, id);
            if (existing == null)
            {
                return false;
            }

            await _tableClient.DeleteEntityAsync(category, id);
            return true;
        }
    }
}

// Reference: Microsoft (2024) Get started with Azure Table client library for .NET.
// Available at: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme
// (Accessed: 8 September 2026)


// <!-- REFERENCE LIST -->
// -------------------------------
// <!-- Microsoft Learn, 2024[3]. TableClient.Query Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.query?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[A]. Azure Table bindings for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[B]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[C]. Architecture best practices for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[D]. Azure Functions Documentation, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/> [Accessed 9 September 2026]. --> 
// <!-- Microsoft Learn, 2024[E]. TableUpdateMode Enum, Azure.Data.Tables [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableupdatemode?view=azure-dotnet> [Accessed 9 Septemeber 2026]. -->


