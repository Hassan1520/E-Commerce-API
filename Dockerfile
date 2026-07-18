# ─────────────────────────────────────────────
# Stage 1: Build
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["E-Commerce.Domain/E-Commerce.Domain.csproj", "E-Commerce.Domain/"]
COPY ["E-Commerce.Application/E-Commerce.Application.csproj", "E-Commerce.Application/"]
COPY ["E-Commerce.Infrastructure/E-Commerce.Infrastructure.csproj", "E-Commerce.Infrastructure/"]
COPY ["E-Commerce.API/E-Commerce.API.csproj", "E-Commerce.API/"]
RUN dotnet restore "E-Commerce.API/E-Commerce.API.csproj"

COPY . .
WORKDIR "/src/E-Commerce.API"
RUN dotnet build "E-Commerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ─────────────────────────────────────────────
# Stage 2: Publish
# ─────────────────────────────────────────────
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "E-Commerce.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ─────────────────────────────────────────────
# Stage 3: Final runtime image
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

RUN adduser --disabled-password --gecos "" appuser

WORKDIR /app

RUN mkdir -p /app/logs && chown -R appuser:appuser /app/logs

COPY --from=publish --chown=appuser:appuser /app/publish .

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "E-Commerce.API.dll"]
