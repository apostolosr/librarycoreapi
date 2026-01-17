# Development Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 10.0.2
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj and restore dependencies
COPY ["LibraryCoreApi.csproj", "./"]
RUN dotnet restore "LibraryCoreApi.csproj"

# Copy everything else
COPY . .

# Expose ports
EXPOSE 8080

# Set environment variables for development
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true

# Enable hot reload with dotnet watch
ENTRYPOINT ["dotnet", "watch", "run", "--no-launch-profile"]
