#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ENV DOTNET_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY eridu-presence-host/. eridu-presence-host/
COPY eridu-presence-model/. eridu-presence-model/
COPY eridu-presence.sln eridu-presence.sln
RUN dotnet restore

RUN dotnet build  -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS deploy
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eridu-presence-host.dll"]
