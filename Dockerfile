# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY Maono.Domain/Maono.Domain.csproj Maono.Domain/
COPY Maono.Application/Maono.Application.csproj Maono.Application/
COPY Maono.Infrastructure/Maono.Infrastructure.csproj Maono.Infrastructure/
COPY Maono.Api/Maono.Api.csproj Maono.Api/
RUN dotnet restore Maono.Api/Maono.Api.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish Maono.Api/Maono.Api.csproj -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Maono.Api.dll"]
