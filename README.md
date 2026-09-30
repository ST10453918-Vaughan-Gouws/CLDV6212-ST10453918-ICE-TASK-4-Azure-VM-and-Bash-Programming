# CoffeeNChill Canteen Management System - Part 1

___________________________________________________________________________________________________________________________________________________________________________________________________________________

<img width="1588" height="326" alt="Screenshot 2026-09-13 025655" src="https://github.com/user-attachments/assets/7ec02e49-2436-4b58-8d0f-1434b58c2c7b" />

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Project Overview

### CoffeeNChill is our campus canteen's food and beverage station, currently using paper menus, chalkboard pricing, handwritten order slips and recipe documents, scattered across a filing cabinet. 
### During peak lunch times, this management style causes long queues and leads to information being lost.

### Group 7 has been brought on, as the cloud development team, to modernize the canteen's operations into a cloud-enabled microservice system, which will be built incrementally over three parts of the POE.

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Tech Stack

+ Compute and coding - Azure Functions, using the Isolated Worker Model, in .NET 10.0 (LTS)
+ Menu Data Storage - Azure Table Storage, emulated by Azurite
+ Staff Document Storage - Azure Blob Storage, not File Share, emulated by Azurite
+ Local Storage emulation - Azurite (Docker)
+ Containerization - Docker (No Docker Compose in Part 1)
+ Testing - Postman
+ IDE - Visual Studio 2022
+ Version Control - Git / GitHub
  
___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Architecture 

In Part 1, there is no MVC application or UI layer implemented, as this will be added incrementally in later stages of the POE. Every capability is exposed as an isolated HTTP-triggered Azure Function:

```
Client (Postman / browser)
|
Azure Functions (Isolated Worker, .NET 10.0)
|
-> Menu_Functions -> IMenuTableService -> Azure Table Storage (Menu_Items)
-> Staff_Document_Functions -> IDocumentBlobService -> Azure Blob Storage (Staff documents)
|
Azurite (Local Azure storage emulator, Docker container)
```


### Design decision:
- - For the POE, Group 7 has decided to implement a Service Layer separation, that separates the Menu_Functions and Staff_Documents_Functions from the TableClient and BlobServiceClient, meaning that the functions and clients do not talk to each other directly. Each functions calls into a dedicated service class (Learned in PROG6212), MenuTableService and DocumentBlobService, which implements an Interface. This design keeps the storage concerns isolated from the HTTP layer of the Azure Functions.

- - DTO's are used, not the raw entity. MenuItemEntity, which implements the ItableEntity and carries the specific storage fields like the PartitionKey, RowKey and ETag, is never returned directly to the client. Request and response shapes the DTO's inside the Model folder and mapps them explicitly, so that the API contract stays clean regardless of how the data is being stored.

- - Blob Storage is being used over File Share. The staff documents are stored in a Blob container named "staff-docs" and is streamed directly via Azure.Storage.Blobs instead of using Azure File Share.

- - Container Isolation. Azurite and the Azure Functions App runs as two seperate standalone Docker Containers on a shared Docker Network (coffeenchill-net), communicating by the container name rather than 127.0.0.1 - No Docker Compose is used in Part 1, as per the assignment brief.

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Project Structure
(Taken from Claude AI)

```
CLDV6212_CoffeeNChill_Functions/
│
├── .git/                              (excluded via .gitignore)
├── .vs/                               (excluded via .gitignore)
├── bin/                               (excluded via .gitignore)
├── obj/                               (excluded via .gitignore)
│
├── Connected Services/
├── Dependencies/
├── Imports/
├── Properties/
│
├── Functions/
│   ├── Menu_Functions.cs
│   └── Staff_Document_Functions.cs
│
├── Models/
│   ├── DTOs/
│   │   ├── AllowedFileType.cs
│   │   ├── DocumentDownloadResult.cs
│   │   ├── DocumentMetaData.cs
│   │   ├── DocumentUploadResponse.cs
│   │   ├── MenuItemRequest.cs
│   │   └── MenuItemResponse.cs
│   └── MenuItemEntity.cs
│
├── Services/
│   ├── DocumentBlobService.cs
│   ├── IDocumentBlobService.cs
│   ├── IMenuTableService.cs
│   └── MenuTableService.cs
│
├── test_docs/
│   ├── CoffeeNChill Docker .postman_environment.json
│   ├── CoffeeNChill Local.postman_environment.json
│   └── CoffeeNChill_Part 1.postman_collection
│
├── .gitignore
├── CLDV6212_CoffeeNChill_Functions.slnx
├── Dockerfile
├── .dockerignore
├── host.json
├── local.settings.json
├── Program.cs
└── README.md
```

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Prerequisites

Before running this project, make sure to have:
- Visual Studio 2022 or later with the Azurite development workload installed
- .NET 10.0 SDK (LTS)
- Docker Desktop
- Postman 
- A Docker Hub account ( to push and pull the container images)

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Step-by-Step Guide

### 1. Clone the repository 

```
git clone <repository-url>
cd CLDV6212_CoffeeNChill_Functions
```

### 2. Open the solution 

- Double-click on "CLDV6212_CoffeeNChill_Functions.slnx" to open the project in Visual Studio. 
- Let the IDE restore the NuGet packages on loading. 

### 3. Pull and Start Azurite

- Azurite emulates the Azure Table and Blob Storage locally, so no real Azure subscription is needed for the local development. 
- pull the Microsoft Azurite emulator and run the container, with the port numbers for Blob, Queue and Table Storage included.
  
```
docker pull mcr.microsoft.com/azure-storage/azurite

docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite-emulator
mcr.microsoft.com/azure-storage/azurite
```

- Port '10000' -> Blob Storage (staff documents)
- Port '10001' -> Queue Storage (reserved for Part 2)
- Port '10002' -> Table Storage (menu)


* Note on Ports: They were tested, verified and found to be different to the Assessment brief but all ports are still working and listening to its respective Storage posts.
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<img width="830" height="323" alt="Screenshot 2026-09-12 210706" src="https://github.com/user-attachments/assets/0ebff520-b7c1-49a7-a083-b2ff8d40094f" />

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

### 4. Confirm the local.settings.json

```
json 
{
"IsEncrypted":false,
"Values": {
"AzureWebJobsStorage": "UseDevelopmentStorage=true",
"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
 }
}
```

### 5. Run Locally 
- Run the App in Visual Studio (F5 or the green button over the function project). 
- Confirm that the Functions host starts and lists to all 8 endpoints at "http://localhost:7247/api/..."
- Test each one manually before moving on to the containers. 


### 6. Build and Run the Docker Containers
The actual building, running and publishing of the containers and their images

### - Login in to DockerHub:
```
docker login -u <dockerhub-username>

docker login -u vaughank001
```

### - Re-tag the Azurite-Emulator image to a verified docker version:
```
docker tag mcr.microsoft.com/azure-storage/azurite-emulator <dockerhub-username>

docker tag mcr.microsoft.com/azure-storage/azurite vaughank001/coffeenchill-azurite:v1.0
```

### - Build the Functions image:

- Get the Exact File context 
```
cd CLDV6212_CoffeeNChill_Functions
or
cd C:\Users\vaugh\OneDrive\Documents\GitHub\cldv6212-group-1-poe-part-1-st10453918-vaughan-gouws
```

- Build the Functions image with a verified version tag
```
docker build -t <dockerhub-username>/coffeenchill-functions:v1.0 .

docker build -t vaughank001/cofeenchill-functions:v1.0 . 
```

### - Create a shared network so that the two containers can resolve each other by hostname:
```
docker network create coffeenchill-net
```

### - Run Azurite Container on that network:
```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite-emulator --network coffeenchill-net <dockerhub-username>/coffeenchill-azurite:v1.0

docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite-emulator --network coffeenchill-net vaughank001/coffeenchill-azurite:v1.0
```

### - Run the Functions container on the same network, pointing at Azurite by the container name (Azurite Default Connection Account):
```
docker run -d --name coffeenchill-functions --network coffeenchill-net -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite-emulator:10000/devstoreaccount1;QueueEndpoint=http://azurite-emulator:10001/devstoreaccount1;TableEndpoint=http://azurite-emulator:10002/devstoreaccount1;" -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" <dockerhub-username>/coffeenchill-functions:v1.0

docker run -d --name coffeenchill-functions --network coffeenchill-net -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite-emulator:10000/devstoreaccount1;QueueEndpoint=http://azurite-emulator:10001/devstoreaccount1;TableEndpoint=http://azurite-emulator:10002/devstoreaccount1;" -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" vaughank001/coffeenchill-functions:v1.0
```

### - Push both images to the docker hub:
```
docker push <dockerhub-username>/coffeenchill-azurite:v1.0
docker push <dockerhub-username>/coffeenchill-functions:v1.0

docker push vaughank001/coffeenchill-azurite:v1.0
docker push vaughank001/coffeenchill-functions:v1.0
```

### 7. Verify that everything works

Postman: 
Import the Postman Collection and both environments from test_docs/:
- "CoffeeNChill_Part 1.postman_collection.json" 
- "CoffeeNChill Local.postman_environment.json ~ points towards http://localhost:7247/api" for Function testing via Visual Studio locally
- "CoffeeNChill Docker.postman_environment.json ~ points towards http://localhost:7071/api" for Containerized Function testing 

Select whichever environment matches what you're testing, and run all the requests, making sure that they all pass against either target.

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Docker and Docker Hub

- Make sure that you have docker-desktop installed on your machine.
- Ensure the docker engine is running within docker-desktop.
- The container and images can be viewed on docker-desktop.
- Logs can be viewed to verify the docker command results and port activities of the containers.
----

Dockerhub username: vaughank001

- Azure Emulator (Standalone) ~ vaughank001/coffeenchill-azurite:v1.0
- Function App ~ vaughank001/coffeenchill-functions:v1.0

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<img width="1585" height="217" alt="image" src="https://github.com/user-attachments/assets/2389c72f-455f-4eb6-955b-8695a18000c0" />

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


Docker commands:
- **Build and publish command**
```
docker build -t

docker login -u

docker tag <source-image>

docker push

docker pull 
```

- **Network commands**
```
docker network create ...-net

docker network connect ...-net <container-name>

docker network ls

docker network inspect ...-net
```

- **Running containers**
```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name ... --network ...-net <docker-image>

docker run -d --name ... --network ...-net -p 7071:80 -e AzureWebJobsStorage="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite-emulator:10000/devstoreaccount1;QueueEndpoint=http://azurite-emulator:10001/devstoreaccount1;TableEndpoint=http://azurite-emulator:10002/devstoreaccount1;" -e FUNCTIONS_WORKER_RUNTIME="dotnet-isolated" <dockerhub-username><container image>
```

- **Verification commands**
```
docker ps

docker ps -a

docker images
```

- **Stopping, Starting and Removing commands**
```
docker stop <container-name>

docker start <container-name>

docker rm <container-name>

```

___________________________________________________________________________________________________________________________________________________________________________________________________________________
## API ENDPOINT REFERENCE

### Menu (Azure Table Storage)

- | POST | /api/menu | Create a new menu item |
- | GET | /api/menu | Get all items on the menu|
- | GET | /api/menu/category/{category} | Get items on the menu filtered by category |
- | PUT | /api/menu/{category}/{id} | Update an existing item on the menu |
- | DELETE | /api/menu/{category}/{id} | Delete an item from the menu |

### Staff documents (Azure Blob Storage)

- | POST | /api/documents/upload | Upload a staff document |
- | GET | /api/documents | List all staff documents with their metadata |
- | GET | /api/documents/download/{fileName} | Download a specific staff document |

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Testing

All 8 endpoints are covered by a Postman collection stored at "test-docs/CoffeeNChill_Part1.postman_collection_json" using the "{{baseUrl}}" environment variable while the automated test scripts assert on both the status codes and response shapes, including the negative path tests (Negative price or missing file).  

Two environments are provided, so the same collection can be run against either target without editing any of the requests
- ** CoffeeNChill Local ** - {{baseUrl}} = http://localhost:7247/api
- ** CoffeeNChill Docker ** - {{baseUrl}} = http://localhost:7071/api

To run: 
1. Import the collection and both environment files into Postman.
2. Select the appropriate environment from the drop down (Local or Docker).
3. Run the collection via the Collection Runner or send the requests individually.

Some requests passing successfully:
-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<img width="1551" height="897" alt="Postman PUT_update menu item" src="https://github.com/user-attachments/assets/422c32c8-53e5-4dd8-86b9-25e30eca482a" />

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<img width="1542" height="926" alt="Postman GET_api_menu" src="https://github.com/user-attachments/assets/8c708533-6a9f-4eab-a5f0-05a366adad26" />

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<img width="1405" height="875" alt="Screenshot 2026-09-12 185620" src="https://github.com/user-attachments/assets/aefcc5a3-1511-4a46-bbcb-64c9982e082b" />

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Team Roles and Individual Responsibilities

Group 7 Members: 

- - **Njabulo Kunene**
  - | ST10220161 |
  - | Azure Menu Functions & Table Storage |
  - | Project Menu Management and MenuTableServices. MenuItemEntity Model and all 5 Menu HTTP endpoints. Dependency injection wiring in the Program.cs.
---

- - **Rumbidzai Maburutse**
  - | ST10339019 |
  - | Blob Storage & Testing |
  - | Staff Document Functions and StaffDocumentServices. All 3 Document endpoints, which includes the file size and file type validation, Full Postman Collection Testing (all 8 endpoints, environment variables and automated tests)
---

- - **Vaughan Gouws**
  - | ST10453918 |
  - | GitHub, Code Editor, Docker & Documentation |
  - | GitHub Repository set-up and ruleset administration, ReadMe file Documentation, DockerFile, Docker network and standalone container, Docker Hub publishing, video walkthrough, Pull and Merge Request reviewer to main. Code Editor. 
 
___________________________________________________________________________________________________________________________________________________________________________________________________________________
 
## Video Presentation
 
Unlisted Part 1 YouTube Video Link: https://youtu.be/xX7fIvXMurM

___________________________________________________________________________________________________________________________________________________________________________________________________________________
 
 ## Known Limitations
 
- no authentication is implemented yet. All of the endpoints are currently using AuthorisationLevel.anonymous. JWT authentication and role-based access control are introduced in Part 3.
- No orchestration between the containers beyond a shared Docker network - Docker Compose is intentionally excluded from Part 1.
- No order-placement functionality has been implemented yet - this will be introduced in Part 2 via Azure Storage Queues.

___________________________________________________________________________________________________________________________________________________________________________________________________________________
 
## Coming in Part 2 

Part 2 Introduces: 
- Asynchronous order queuing
- A queue-triggered Azure Function for order processing
- Docker Compose Orchestration pulling the published images directly from Docker Hub

___________________________________________________________________________________________________________________________________________________________________________________________________________________

## Code Attribution and References

--------------------------------------------------------------------------------------------------------------------------------
### Code Attribution, References and Comments were added all throughout the project, maintaining code and referencing integrity 
--------------------------------------------------------------------------------------------------------------------------------

Datacamp, 2025. Practical Guide to Containers, Azure Functions and Containerization [Online]. Available at: <https://www.datacamp.com/tutorial/docker-tutorial> [Accessed 9 Septemeber 2026]. -->  

Docker, [n.d.]. CLI Cheat Sheet, Docker [pdf]. [Online]. Available at: <https://docs.docker.com/get-started/docker_cheatsheet.pdf> [Accessed 9 September 2026]. -->

Docker docs, 2026. Explore the Containers view in Docker Desktop, Docker Desktop [Onilne]. Available at: <https://docs.docker.com/desktop/use-desktop/container/> [Accesssed 9 September 2026]. -->

GeeksforGeeks, 2025. How to use Docker Desktop to Deploy Docker Containerized Application, Azure Fubctions and Docker [Online]. Available at: <https://www.geeksforgeeks.org/devops/how-to-use-docker-desktop-to-deploy-docker-containerized-applications/> [Accessed 9 September 2026]. -->

Microsoft Learn. 2024[1]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview>  [Accessed 9 September 2026]. -->

Microsoft Learn. 2024[2]. ObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.objectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[3]. TableClient.Query Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.query?view=azure-dotnet> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[4]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[5]. NotFoundObjectResult Class, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.notfoundobjectresult?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[6]. HttpRequestJsonExtensions.ReadFromJsonAsync Method, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequestjsonextensions.readfromjsonasync?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[7]. Azure Functions Overview, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[8]. Microsoft.AspNetCore.MVC Namespace, MVC and Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc?view=aspnetcore-10.0> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[9]. Work with containers and Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-custom-container?tabs=core-tools%2Cacr%2Cazure-cli2%2Cazure-cli&pivots=container-apps> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[10]. Create your first containerised Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-deploy-container?tabs=acr%2Cbash%2Cazure-cli&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[11]. Azure Blob storage trigger for Azure Functions, Azure Functions & Docker [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp> [Accessed 9 September 2026].

Microsoft Learn, 2024[A]. Azure Table bindings for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api&pivots=programming-language-csharp> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[B]. TableClient.GetEntityIfExists Method, Azure Functions [Onine]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.getentityifexists?view=azure-dotnet> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[C]. Architecture best practices for Azure Functions, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/well-architected/service-guides/azure-functions> [Accessed 9 September 2026]. -->

Microsoft Learn, 2024[D]. Azure Functions Documentation, Azure Functions [Online]. Available at: <https://learn.microsoft.com/en-us/azure/azure-functions/> [Accessed 9 September 2026]. --> 

Microsoft Learn, 2024[a]. BlobDownloadDetails.ContentType Property, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobdownloaddetails.contenttype?view=azure-dotnet> [Accessed 11 September 2026]. -->

Microsoft Learn, 2024[b]. Azure.Storage.Blobs.Models, Azure SDK for .NET [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models?view=azure-dotnet> [Accessed 11 September 2026]. -->

Microsoft Learn, 2024[c]. List<T> Class, .NET API Reference [Online]. Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-10.0> [Accessed 11 September 2026]. -->

Stack Overflow, 2013. Do I need Content-Type: application/octet-stream for file download?. MIME Media Types [Online]. Available at: <https://stackoverflow.com/questions/20508788/do-i-need-content-type-application-octet-stream-for-file-download> [Accessed 11 September 2026]. -->

Stack Overflow, 2023. Send content-length in return statement along with download content value [duplicate], Download.Value [Online]. <https://stackoverflow.com/questions/76720669/send-content-length-in-return-statement-along-with-download-content-value> Available at: [Accessed 11 September 2026]. -->  

___________________________________________________________________________________________________________________________________________________________________________________________________________________

# Project Course Details

- - **Module**: Cloud Development B
- - **Code**: CLDV6212
- - **Project**: Proof of Evidence Part 1
- - **Group 1**- Group 7 

___________________________________________________________________________________________________________________________________________________________________________________________________________________
