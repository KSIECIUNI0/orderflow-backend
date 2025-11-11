FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/OrderFlow.Api/*.csproj src/OrderFlow.Api/
COPY src/OrderFlow.Application/*.csproj src/OrderFlow.Application/
COPY src/OrderFlow.Domain/*.csproj src/OrderFlow.Domain/
COPY src/OrderFlow.Infrastructure/*.csproj src/OrderFlow.Infrastructure/
COPY src/OrderFlow.Worker/*.csproj src/OrderFlow.Worker/
COPY tests/OrderFlow.UnitTests/*.csproj tests/OrderFlow.UnitTests/
COPY tests/OrderFlow.IntegrationTests/*.csproj tests/OrderFlow.IntegrationTests/

RUN dotnet restore

COPY . .

RUN dotnet publish src/OrderFlow.Api -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]
