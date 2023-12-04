#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

RUN groupadd -r appuser -g 433 && \
    useradd -u 431 -r -g appuser -s /sbin/nologin -c "Docker image user" appuser

RUN mkdir /app/data

EXPOSE 8080
#Volume per configurazione esterna
VOLUME /app/data 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hfs.Serve.Core/Hfs.Serve.Core.csproj", "Hfs.Serve.Core/"]
COPY ["Hfs.Client/Hfs.Client.csproj", "Hfs.Client/"]
RUN dotnet restore "./Hfs.Serve.Core/./Hfs.Serve.Core.csproj"
COPY . .
WORKDIR "/src/Hfs.Serve.Core"
RUN dotnet build "./Hfs.Serve.Core.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hfs.Serve.Core.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod -R 1777 /app/data
USER appuser
ENTRYPOINT ["dotnet", "Hfs.Serve.Core.dll"]