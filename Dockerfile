ARG REGISTRY=://microsoft.com
ARG REPO_SDK=dotnet/sdk:9.0
ARG REPO_RUNTIME=dotnet/aspnet:9.0

# 1. Bygg appen
FROM ${REGISTRY}/${REPO_SDK} AS build
WORKDIR /src

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# 2. Kör appen
FROM ${REGISTRY}/${REPO_RUNTIME}
WORKDIR /app
COPY --from=build /app .

# 🌟 Tvingar .NET att använda polling istället för inotify på Linux! Detta löser kraschen helt.
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
