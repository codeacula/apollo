FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5144

ENV ASPNETCORE_URLS=http://+:5144

USER app

# Build the client
FROM node:alpine AS client-build
WORKDIR /src
COPY ./src/Client/package*.json ./src/Client/
RUN cd ./src/Client && npm ci
COPY ./src/Client ./src/Client

# Build to ../Apollo.API/wwwroot as configured in vite.config.ts
RUN cd ./src/Client && npm run build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Apollo.API/Apollo.API.csproj", "src/Apollo.API/"]
RUN dotnet restore "src/Apollo.API/Apollo.API.csproj"
COPY . .
COPY --from=client-build /src/src/Apollo.API/wwwroot ./src/Apollo.API/wwwroot
WORKDIR "/src/src/Apollo.API"
RUN dotnet build "Apollo.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Apollo.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Apollo.API.dll"]
