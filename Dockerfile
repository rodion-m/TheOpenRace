FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OpenRace/OpenRace.csproj", "OpenRace/"]
RUN dotnet restore "OpenRace/OpenRace.csproj"
COPY . .
WORKDIR "/src/OpenRace"
RUN dotnet build "OpenRace.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenRace.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenRace.dll"]
