FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY konecta_erp.sln ./
COPY backend/SharedContracts/SharedContracts.csproj backend/SharedContracts/
COPY backend/UserManagementService/UserManagementService.csproj backend/UserManagementService/
RUN dotnet restore backend/UserManagementService/UserManagementService.csproj

COPY backend ./backend
RUN dotnet publish backend/UserManagementService/UserManagementService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5078
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserManagementService.dll"]
