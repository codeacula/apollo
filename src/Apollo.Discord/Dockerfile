FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5145

ENV ASPNETCORE_URLS=http://+:5145

USER app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Apollo.Discord/Apollo.Discord.csproj", "src/Apollo.Discord/"]
RUN dotnet restore "src/Apollo.Discord/Apollo.Discord.csproj"
COPY . .
WORKDIR "/src/src/Apollo.Discord"
RUN dotnet build "Apollo.Discord.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Apollo.Discord.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Apollo.Discord.dll"]
