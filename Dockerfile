# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage 1 - Base
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# Stage 2 - Build
# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CLDV6212_CoffeeNChill_Functions.csproj", "."]


RUN dotnet restore "./CLDV6212_CoffeeNChill_Functions.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./CLDV6212_CoffeeNChill_Functions.csproj" -c $BUILD_CONFIGURATION -o /app/build


#Stage 3 - publish
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CLDV6212_CoffeeNChill_Functions.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


#Stage 4 - Final
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true


# REFERENCES
# (Dockerdocs, 2026), (Cloud Tutor Raven's GreetingAPI demo project) and (Part 1 Assessment brief docker run command) 

