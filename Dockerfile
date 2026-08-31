# 1. Use .Net 9 to build the application
FROM ://microsoft.com AS build
WORKDIR /src

# Copy the project file and restore
COPY BookApi.csproj ./
RUN dotnet restore

# Copy the rest of the code and publish the application
COPY . .
RUN dotnet publish -c Release -o /app

# 2. Create a lightweight container for .NET 9
FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app .

# Render Demands that port 8080 is used for HTTP-trafic
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "BookApi.dll"]
