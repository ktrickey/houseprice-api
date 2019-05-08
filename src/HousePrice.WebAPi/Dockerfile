FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk-alpine AS build
WORKDIR /src
COPY ["HousePrice.WebAPi/HousePrice.WebAPi.csproj", "HousePrice.WebAPi/"]
RUN dotnet restore "HousePrice.WebAPi/HousePrice.WebAPi.csproj"
COPY . .
WORKDIR /src/HousePrice.WebAPi
RUN dotnet build "HousePrice.WebAPi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HousePrice.WebAPi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HousePrice.WebAPi.dll"]
