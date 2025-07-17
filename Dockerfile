FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SharedConfigApp.csproj", "."]
RUN dotnet restore "./SharedConfigApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./SharedConfigApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SharedConfigApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create mount point for Azure Files
RUN mkdir -p /mnt/config

ENTRYPOINT ["dotnet", "SharedConfigApp.dll"]