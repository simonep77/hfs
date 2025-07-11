#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN mkdir /app/data

#Volume per configurazione esterna
VOLUME /app/data 

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Hfs.Serve.Core/Hfs.Serve.Core.csproj", "Hfs.Serve.Core/"]
COPY ["Hfs.Client/Hfs.Client.csproj", "Hfs.Client/"]
COPY . .
WORKDIR "/src/Hfs.Serve.Core"

RUN dotnet publish "./Hfs.Serve.Core.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod -R 1666 /app/data
USER $APP_UID
ENTRYPOINT ["dotnet", "Hfs.Serve.Core.dll"]