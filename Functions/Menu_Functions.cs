using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CLDV6212_CoffeeNChill_Functions.Models;
using CLDV6212_CoffeeNChill_Functions.Models.DTOs;
using CLDV6212_CoffeeNChill_Functions.Services;
using System.Text.Json;

namespace CLDV6212_CoffeeNChill_Functions.Functions
{
    // The Menu_Functions Class is where the logic of HTTP triggered Menu Functions exists
    // It houses all 5 functions, as specified in the POE Part 1
    // The Menu Functions do not talk direnctly and are kept seperate from the Table storage, establishing seperation of concerns
    // The Menu functions intead proceed through the IMenuTableService, which then communicated with the Table Storage. 

    // Parts of the code in the class were learned, studied and gathered from the references listed below. 

    // <!-- Microsoft Learn, 2024[7] and Microsoft Learn[2] -->
    // All of the Microsoft.AspNETCore.MVC.ObjectResults and there subclasses were learned and taken from Microsoft Learn as well as learned in the PowerPoint slides presented to us on ARC -->
    // These return objectsresults will provide the correct response along with its corresponding status code. 
    // Meaning that the status codes do not have to be generated manually for the response. 
    public class Menu_Functions
    {
        // Creates the MenuTableService object to call the Table Storage Service layer
        private readonly ILogger<Menu_Functions> _logger;
        private readonly IMenuTableService _menuTableService;
        
        public Menu_Functions(ILogger<Menu_Functions> logger, IMenuTableService menuTableService)
        {
            _logger = logger;
            _menuTableService = menuTableService;
        }

        // POST/API/Menu
        // Function 1
        // This function adds a new item to the CoffeeNChill menu
        [Function("CreateMenuItem")]
        public async Task<IActionResult> CreateMenuItem([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequest req)
        {
            MenuItemRequest? item;

            // Try-catch is used to for the JsonExecpetion, as a baldy formatted body will thow an exception
            try
            {
                // <!-- Microsoft Learn, 2024[5] -->
                // Deserialises the JSON into the DTO's object
                item = await req.ReadFromJsonAsync<MenuItemRequest>();
            }
            catch (JsonException)
            {
                // error 400
                return new BadRequestObjectResult(new { error = "Invalid JSON in the request body. Please recheck your JSON"});
            }


            // Basic validation
            // The Category, SKU and Item Name have to be filled in and the Price has to be a real, positive number
            // <!-- Microsoft Learn, 2024[6] and Microsoft Learn, 2026 -->
            if (item == null ||
                string.IsNullOrWhiteSpace(item.Category) ||
                string.IsNullOrWhiteSpace(item.SKU) ||
                string.IsNullOrWhiteSpace(item.Name) ||
                !item.Price.HasValue || 
                item.Price <= 0)
            {
                // error 400
                return new BadRequestObjectResult(new { error = "The Category, SKU, Name and a Price greater than 0 are all required." });
            }

            // Validation to prevent the same, duplicate item from being added to the menu 
            // Allows a clear messages to be returned instead of a generic storage failure message
            var duplicateItem = await _menuTableService.GetMenuItemIfExistsAsync(item.Category, item.SKU);
            if (duplicateItem != null)
            {
                // error 409
                return new ConflictObjectResult(new { error = $"SKU '{item.SKU}' already exists in the category: '{item.Category}'." });
            }

            try {
                var entity = new MenuItemEntity
                {
                    PartitionKey = item.Category,
                    RowKey = item.SKU,
                    Name = item.Name,
                    Description = item.Description ?? string.Empty,
                    Price = item.Price.Value,
                    IsAvailable = item.IsAvailable ?? true
                };

                var created = await _menuTableService.CreateMenuItemAsync(entity);
                var response = MenuItemResponse.FromEntity(created);

                _logger.LogInformation("Created menu item {SKU} in {Category}", response.SKU, response.Category);
                // <!-- Microsoft Learn, 2024[7] and Micrososoft Learn, 2024[2] -->
                // <! Learned from Microsoft Learn and Student Power Points 
                // 201 
                return new CreatedResult($"/api/menu/{response.Category}/{response.SKU}", response);
            }
            catch (Exception ex)
            {
                // Any unexpected occurence gets logged and turns into an error 500, instead of the Function crashing. 
                _logger.LogError(ex, "Something went wrong while creating the menu item.");
                return new ObjectResult(new { error = "Something went wrong while saving the item. Please try again!" }) {StatusCode = 500 };
            }
          }


        // GET/API/menu
        // Function 2
        // This function gets all of the menu items across all of the categories (PartitionKey)
        [Function("GetAllMenuItems")]
        public async Task<IActionResult> GetAllMenuItems([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequest req)
        {
            var items = await _menuTableService.GetAllMenuItemsAsync();
            var response = items.Select(MenuItemResponse.FromEntity);
            // 200
            return new OkObjectResult(response);
        }


        // GET/API/menu/category
        // Function 2
        // This function is used to get all of the items kept an a specific category 
        [Function("GetMenuItemsByCategory")]
        public async Task<IActionResult> GetMenuItemsByCategory([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequest req,
            string category)
        {
            // Validation for the required category
            if (string.IsNullOrWhiteSpace(category))
            {
                // error 400
                return new BadRequestObjectResult(new { error = "The Category is required." });
            }

            var items = await _menuTableService.GetMenuItemsByCategoryAsync(category);
            var response = items.Select(MenuItemResponse.FromEntity);
            // 200
            return new OkObjectResult(response);
        }


        // PUT/API/menu/category and id
        // Function 3
        // This function is used to update items in the CoffeeNChill menu
        [Function("UpdateMenuItem")]
        public async Task<IActionResult> UpdateMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequest req,
            string category, string id)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(id))
            {
                // error 400
                return new BadRequestObjectResult(new { error = "The Category and id are required in the route." });
            }

            MenuItemRequest? request;
            // error handling with try-catch
            try
            {
                // Deserializes the Json to the Object
                // <!-- Microsoft Learn, 2024[5] -->
                request = await req.ReadFromJsonAsync<MenuItemRequest>();
            }
            catch
            {
                // error 400
                return new BadRequestObjectResult(new { error = "Invalid JSON in the request body. Please recheck your JSON" });
            }

            // Validation
            if (request == null || (!request.Price.HasValue && !request.IsAvailable.HasValue))
            {
                // error 400
                return new BadRequestObjectResult(new { error = "Please provide a Price and an isAvailable value to update." });
            }

            // Validation
            if (request.Price.HasValue && request.Price <= 0)
            {
                return new BadRequestObjectResult(new { error = "The Price must be greater than 0." });
            }

            var updated = await _menuTableService.UpdateMenuItemAsync(category, id, request.Price, request.IsAvailable);

            // Validation
            if (updated == null)
            {
                // error 404
                return new NotFoundObjectResult($"The Menu item '{id}' in the category: '{category}' was not found.");
            }
            // 200
            return new OkObjectResult(MenuItemResponse.FromEntity(updated));
        }

        
        // DELETE/API/menu
        // Function 4
        // This function is the delete of CRUD, used to delete items from the canteen menu
        [Function("DeleteMenuItem")]
        public async Task<IActionResult> DeleteMenuItem([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{id}")] HttpRequest req,
            string category, string id)
        {
            // Validation conditional statement 
            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(id))
            {
                // error 400
                return new BadRequestObjectResult(new { error = "The Category and id are required in the route." });
            }

            var deleted = await _menuTableService.DeleteMenuItemAsync(category, id);
            // Validation 
            if (!deleted)
            {
                // error 404
                return new NotFoundObjectResult($"The Menu item '{id}' in the category: '{category}' was not found.");
            }
                // 201
                return new OkObjectResult(new { message = $"The Menu item '{id}' in the category: '{category}' was deleted." });
        }
    }
}

// Reference: Microsoft (2024) Azure Functions HTTP trigger.
// Available at: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger
// (Accessed: 8 September 2026).


// <!-- REFERENCE LIST --> 
// ----------------------------
// <!-- Microsoft Learn. 2024[1]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview>  [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn. 2024[2]. ObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.objectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[3]. TableClient.Query Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.query?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[4]. NotFoundObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.notfoundobjectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[5]. HttpRequestJsonExtensions.ReadFromJsonAsync Method, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequestjsonextensions.readfromjsonasync?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[6]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[7]. Microsoft.AspNetCore.MVC Namespace, MVC and Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn. 2026. Part 9, add Validation to an ASP.NET Core MVC app, ASP.NET and C# [Online]. Available at: <https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/validation?view=aspnetcore-10.0&tabs=visual-studio> [Accessed 29 May 2026]. -->
