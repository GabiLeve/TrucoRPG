FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/TrucoRPG.API/TrucoRPG.API.csproj"

RUN dotnet publish "src/TrucoRPG.API/TrucoRPG.API.csproj" \
    -c Release \
    -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TrucoRPG.API.dll"]