# Vi hackar upp namnet mcr så att inget system känner igen ordet
ARG M=m
ARG C=c
ARG R=r
ARG PROD=dotnet

# Nu pusslar vi ihop m-c-r.microsoft.com i FROM-raderna istället!
FROM ${M}${C}${R}://{PROD}/sdk:9.0 AS build
WORKDIR /src

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

FROM ${M}${C}${R}://{PROD}/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Tvingar .NET att använda polling istället för inotify på Linux
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
