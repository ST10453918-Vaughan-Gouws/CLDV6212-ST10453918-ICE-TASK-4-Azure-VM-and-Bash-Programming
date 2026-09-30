FROM mcr.microsoft.com/azure-storage/azurite

EXPOSE 10000 10001 10002

CMD ["azurite", "--location", "/data", "--blobHost", "0.0.0.0", "--queueHost", "0.0.0.0", "--tableHost", "0.0.0.0", "--loose", "--skipApiVersionCheck"]

