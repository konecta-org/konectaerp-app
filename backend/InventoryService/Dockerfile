FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY konecta_erp.sln ./
COPY backend/SharedContracts/SharedContracts.csproj backend/SharedContracts/
COPY backend/InventoryService/InventoryService.csproj backend/InventoryService/
RUN dotnet restore backend/InventoryService/InventoryService.csproj

COPY backend ./backend
RUN dotnet publish backend/InventoryService/InventoryService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5020
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryService.dll"]
