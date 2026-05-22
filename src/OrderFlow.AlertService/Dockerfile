FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "OrderFlow.AlertService/OrderFlow.AlertService.csproj"

RUN dotnet publish "OrderFlow.AlertService/OrderFlow.AlertService.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.AlertService.dll"]