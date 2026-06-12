FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FoodTrust.Api/FoodTrust.Api.csproj", "FoodTrust.Api/"]
COPY ["FoodTrust.Infrastructure/FoodTrust.Infrastructure.csproj", "FoodTrust.Infrastructure/"]
COPY ["FoodTrust.Core/FoodTrust.Core.csproj", "FoodTrust.Core/"]
RUN dotnet restore "FoodTrust.Api/FoodTrust.Api.csproj"

COPY . .
RUN dotnet publish "FoodTrust.Api/FoodTrust.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FoodTrust.Api.dll"]
