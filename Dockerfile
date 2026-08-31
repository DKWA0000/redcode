FROM ://microsoft.com AS build
WORKDIR /src

COPY BookApi.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookApi.dll"]
