FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
ENV IMPORT_DATA_ROOT "/transaction_data"
ENV POSTCODE_DATA "/postcodedata/ukpostcodes.csv"
ENV connectionString "mongodb://mongo-houseprice:27017"
WORKDIR /app
EXPOSE 49457
EXPOSE 44382

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY HousePrice.Api.ImportFileWatcher/HousePrice.Api.ImportFileWatcher.csproj HousePrice.Api.ImportFileWatcher/
COPY HousePrice.Api.Services/HousePrice.Api.Services.csproj HousePrice.Api.Services/
RUN dotnet restore  HousePrice.Api.ImportFileWatcher/HousePrice.Api.ImportFileWatcher.csproj
COPY . .
WORKDIR /src/HousePrice.Api.ImportFileWatcher
RUN dotnet build HousePrice.Api.ImportFileWatcher.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish HousePrice.Api.ImportFileWatcher.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
VOLUME /transactiondata
VOLUME /postcodedata
ENTRYPOINT ["dotnet", "HousePrice.WebAPi.dll"]
