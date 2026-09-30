using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using Azure.Storage.Blobs;
using CLDV6212_CoffeeNChill_Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddSingleton(sp =>
{
    // read the connection string directly from environment variables
    // this is more reliable than using IConfiguration for Azure Functions
    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
        ?? "UseDevelopmentStorage=true";
    return new BlobServiceClient(connectionString);
});

builder.Services.AddSingleton<IMenuTableService, MenuTableService>();
// this registers the BlobServiceClient so the app knows how to connect to Azure Blob Storage using the connection string from settings
// <!-- Microsoft Learn, 2024[a] -->
builder.Services.AddSingleton(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
        ?? "UseDevelopmentStorage=true";
    return new TableServiceClient(connectionString);
});

// this registers our DocumentBlobService so that whenever IDocumentBlobService is needed Azure will automatically inject DocumentBlobService into the constructor for us
// <!-- Microsoft Learn, 2024[b] -->
builder.Services.AddSingleton<IDocumentBlobService, DocumentBlobService>();

builder.Build().Run();

// Reference: Microsoft (2024) Dependency injection in .NET Azure Functions.
// Available at: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-dependency-injection
// (Accessed: 8 September 2026).


// <!-- REFERENCE LIST --> 
// ----------------------------
// <!-- Datacamp, 2025. Practical Guide to Containers, Azure Functions and Containerization [Online]. Available at: <https://www.datacamp.com/tutorial/docker-tutorial> [Accessed 9 Septemeber 2026]. -->  
// <!-- Docker, [n.d.]. CLI Cheat Sheet, Docker [pdf]. [Online]. Available at: <https://docs.docker.com/get-started/docker_cheatsheet.pdf> [Accessed 9 September 2026]. -->
// <!-- Docker docs, 2026. Explore the Containers view in Docker Desktop, Docker Desktop [Onilne]. Available at: <https://docs.docker.com/desktop/use-desktop/container/> [Accesssed 9 September 2026]. -->
// <!-- GeeksforGeeks, 2025. How to use Docker Desktop to Deploy Docker Containerized Application, Azure Fubctions and Docker [Online]. Available at: <https://www.geeksforgeeks.org/devops/how-to-use-docker-desktop-to-deploy-docker-containerized-applications/> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn. 2024[1]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview>  [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn. 2024[2]. ObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.objectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[3]. TableClient.Query Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.query?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[4]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[5]. NotFoundObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.notfoundobjectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[6]. HttpRequestJsonExtensions.ReadFromJsonAsync Method, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequestjsonextensions.readfromjsonasync?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[7]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[8]. Microsoft.AspNetCore.MVC Namespace, MVC and Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[9]. Work with containers and Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-custom-container?tabs=core-tools%2Cacr%2Cazure-cli2%2Cazure-cli&pivots=container-apps> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[10]. Create your first containerised Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-deploy-container?tabs=acr%2Cbash%2Cazure-cli&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[11]. Azure Blob storage trigger for Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[A]. Azure Table bindings for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[B]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[C]. Architecture best practices for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions> [Accessed 9 September 2026]. -->
// <!-- Microsoft Learn, 2024[D]. Azure Functions Documentation, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/> [Accessed 9 September 2026]. --> 
// <!-- Microsoft Learn, 2024[a]. BlobDownloadDetails.ContentType Property, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobdownloaddetails.contenttype?view=azure-dotnet> [Accessed 11 September 2026]. -->
// <!-- Microsoft Learn, 2024[b]. Azure.Storage.Blobs.Models, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models?view=azure-dotnet> [Accessed 11 September 2026]. -->
// <!-- Microsoft Learn, 2024[c]. List<T> Class, .NET API Reference [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-10.0> [Accessed 11 September 2026]. -->
// <!-- Microsoft Learn. 2026. Part 9, add Validation to an ASP.NET Core MVC app, ASP.NET and C# [Online]. Available at: <https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/validation?view=aspnetcore-10.0&tabs=visual-studio> [Accessed 29 May 2026]. -->
// <!-- Stack Overflow, 2013. Do I need Content-Type: application/octet-stream for file download?. MIME Media Types [Online]. Available at: <https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download> [Accessed 11 September 2026]. -->
// <!-- Stack Overflow, 2023. Send content-length in return statement along with download content value [duplicate], Download.Value [Online]. <https://stackoverflow.com/questions/76720669/send-content-length-in-return-statement-along-with-download-content-value> Available at: [Accessed 11 September 2026]. -->  
// <!-- Microsoft Learn, 2024[a]. BlobServiceClient Class - Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobserviceclient> [Accessed 11 September 2026]. -->
// <!-- Microsoft Learn, 2024[b]. Dependency injection in .NET Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-dependency-injection> [Accessed 11 September 2026]. -->

