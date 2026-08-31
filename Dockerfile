# 1. Använd .NET 9 SDK för att bygga och kompilera appen
FROM ://microsoft.com AS build
WORKDIR /src

# Kopiera projektfilen och återställ paket
COPY BookApi.csproj ./
RUN dotnet restore

# Kopiera resten av koden och publicera den
COPY . .
RUN dotnet publish -c Release -o /app

# 2. Skapa slutgiltiga containern med en lättvikts-runtime för .NET 9
FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app .

# Render kräver att appen lyssnar på port 8080 för HTTP-trafik
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Starta appen (Pekas mot din BookApi.dll)
ENTRYPOINT ["dotnet", "BookApi.dll"]
