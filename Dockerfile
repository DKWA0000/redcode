# 1. Bygg appen med .NET 9 SDK från Amazon ECR
FROM public.ecr.aws/sam/build-dotnet9:latest AS build
WORKDIR /src

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# 2. Kör appen med .NET 9 Runtime från GitHub Container Registry
FROM ghcr.io/fluent-cms/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Tvingar .NET att använda polling istället för inotify på Linux
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
