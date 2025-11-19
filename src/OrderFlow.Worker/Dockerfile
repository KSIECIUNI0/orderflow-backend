FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "OrderFlow.Worker/OrderFlow.Worker.csproj"

RUN dotnet publish "OrderFlow.Worker/OrderFlow.Worker.csproj" \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Worker.dll"]
