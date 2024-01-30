FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ./CryptoBank.csproj .
RUN dotnet restore ./CryptoBank.csproj

COPY . .
RUN dotnet build ./CryptoBank.csproj -c Release -o /app/build

RUN dotnet publish ./CryptoBank.csproj -c Release -o /app/publish

FROM base AS final

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CryptoBank.dll"]
