FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ClangenUpdateApi/ClangenUpdateApi.csproj", "ClangenUpdateApi/"]
RUN dotnet restore "ClangenUpdateApi/ClangenUpdateApi.csproj"
COPY . .
WORKDIR "/src/ClangenUpdateApi"
RUN dotnet build "ClangenUpdateApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClangenUpdateApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClangenUpdateApi.dll"]
