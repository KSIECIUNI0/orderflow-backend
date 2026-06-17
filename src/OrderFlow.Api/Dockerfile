FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore src/OrderFlow.Api/OrderFlow.Api.csproj
RUN dotnet publish src/OrderFlow.Api/OrderFlow.Api.csproj \
    -c Release \
    -o /app/out \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]
