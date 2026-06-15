FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Sentinal.slnx .
COPY src/Sentinal.Api/Sentinal.Api.csproj src/Sentinal.Api/
COPY src/Sentinal.Application/Sentinal.Application.csproj src/Sentinal.Application/
COPY src/Sentinal.Domain/Sentinal.Domain.csproj src/Sentinal.Domain/
COPY src/Sentinal.Infrastructure/Sentinal.Infrastructure.csproj src/Sentinal.Infrastructure/
COPY src/Sentinal.Tests/Sentinal.Tests.csproj src/Sentinal.Tests/

RUN dotnet restore src/Sentinal.Api/Sentinal.Api.csproj

COPY . .
RUN dotnet publish src/Sentinal.Api/Sentinal.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Sentinal.Api.dll"]