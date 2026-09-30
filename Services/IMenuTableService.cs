using System.Collections.Generic;
using System.Threading.Tasks;
using CLDV6212_CoffeeNChill_Functions.Models;

namespace CLDV6212_CoffeeNChill_Functions.Services
{
    // This is the TableServiceInterface that specifies the general behaviour of the TableService class
    // Interface implemented by MenuTableService
    // Sepeeration of Concerns

    // Parts of the code were learned, studied and implemented from the references listed below 
    public interface IMenuTableService
    {
        Task<MenuItemEntity> CreateMenuItemAsync(MenuItemEntity entity);
        Task<IEnumerable<MenuItemEntity>> GetAllMenuItemsAsync();
        Task<IEnumerable<MenuItemEntity>> GetMenuItemsByCategoryAsync(string category);
        Task<MenuItemEntity?> GetMenuItemIfExistsAsync(string category, string sku);
        Task<MenuItemEntity?> UpdateMenuItemAsync(string category, string id, double? price, bool? isAvailable);
        Task<bool> DeleteMenuItemAsync(string category, string id);
    }
}

// Reference: Microsoft (2024) Dependency injection - .NET.
// Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
// (Accessed: 8 September 2026).


// <!-- REFERENCE LIST -->
// -------------------------------
// <!-- Microsoft Learn, 2024[A]. Azure Table bindings for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[B]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[C]. Architecture best practices for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[D]. Azure Functions Documentation, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/> [Accessed 9 September 2026]. --> 

