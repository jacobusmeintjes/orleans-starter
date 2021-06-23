FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY ./publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "feed-siloclient.dll"]
