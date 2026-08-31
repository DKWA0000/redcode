# 1. Bygg appen med .NET 9 SDK från Amazons publika spegling
FROM public.ecr.aws/sam/build-dotnet9:latest AS build
WORKDIR /src

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# 2. Kör appen med .NET 9 Runtime från Amazons officiella Linux-spegling
FROM public.ecr.aws/amazonlinux/amazonlinux:2023
WORKDIR /app

# Installera .NET 9 Runtime direkt inuti Amazon Linux
RUN dnf update -y && dnf install -y dotnet-runtime-9.0 aspnetcore-runtime-9.0 && dnf clean all

COPY --from=build /app .

# Tvingar .NET och Linux att samarbeta på Renders servrar utan inotify-krascher
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
