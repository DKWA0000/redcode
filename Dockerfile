# Vi använder Ubuntus officiella spegling för .NET 9 istället för Microsofts server!
FROM ubuntu:24.04 AS build
WORKDIR /src

# Installera .NET 9 SDK via Ubuntus pakethanterare
RUN apt-get update && apt-get install -y dotnet-sdk-9.0

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# Skapa slutgiltiga containern med Ubuntus .NET-runtime
FROM ubuntu:24.04
WORKDIR /app

# Installera .NET 9 ASP.NET Core Runtime
RUN apt-get update && apt-get install -y aspnetcore-runtime-9.0 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# Tvingar .NET att inte använda inotify på Linux
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
