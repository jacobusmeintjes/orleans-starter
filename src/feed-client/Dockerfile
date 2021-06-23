FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["feed-client/feed-client.csproj", "feed-client/"]
RUN dotnet restore "feed-client/feed-client.csproj"
COPY . .
WORKDIR "/src/feed-client"
RUN dotnet build "feed-client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "feed-client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "feed-client.dll"]
