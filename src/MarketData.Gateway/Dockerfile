FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MarketData.Gateway/MarketData.Gateway.csproj", "MarketData.Gateway/"]
RUN dotnet restore "MarketData.Gateway/MarketData.Gateway.csproj"
COPY . .
WORKDIR "/src/MarketData.Gateway"
RUN dotnet build "MarketData.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketData.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketData.Gateway.dll"]
