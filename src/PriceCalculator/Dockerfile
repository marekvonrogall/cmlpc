FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY PriceCalculator.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .

COPY ./data /app/data

EXPOSE 80

ENTRYPOINT ["dotnet", "PriceCalculator.dll"]
