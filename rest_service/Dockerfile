# Use the official .NET 8.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["rest_service/rest_service.csproj", "rest_service/"]
COPY ["shared/shared.csproj", "shared/"]

# Restore dependencies
RUN dotnet restore "rest_service/rest_service.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/rest_service"
RUN dotnet build "rest_service.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "rest_service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "rest_service.dll"]