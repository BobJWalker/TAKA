# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS base
USER app
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src
COPY ["TAKA.Web.csproj", "."]
RUN dotnet restore "./TAKA.Web.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./TAKA.Web.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./TAKA.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
ENV TAKA_INSTANCE_URL=blah
ENV TAKA_INSTANCE_API_KEY=blah
ENV TAKA_SUMOLOGIC_URL=blah
ENV TAKA_SECRET_PHRASE=blah
ENV TAKA_ENVIRONMENT="Production"
ENV TAKA_OPEN_FEATURE_CLIENT_ID="blah"
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT="Production"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TAKA.Web.dll"]