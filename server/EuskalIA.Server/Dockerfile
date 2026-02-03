# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

# Copy csproj and restore
COPY ["server/EuskalIA.Server/EuskalIA.Server.csproj", "server/EuskalIA.Server/"]
RUN dotnet restore "server/EuskalIA.Server/EuskalIA.Server.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/server/EuskalIA.Server"
RUN dotnet build "EuskalIA.Server.csproj" -c Release -o /app/build

# Publish
FROM build-env AS publish
RUN dotnet publish "EuskalIA.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port and start
EXPOSE 80
ENTRYPOINT ["dotnet", "EuskalIA.Server.dll"]
