#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Hfs.Server.Core/Hfs.Server.Core.csproj", "Hfs.Server.Core/"]
RUN dotnet restore "Hfs.Server.Core/Hfs.Server.Core.csproj"
COPY . .
WORKDIR "/src/Hfs.Server.Core"
RUN dotnet build "Hfs.Server.Core.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hfs.Server.Core.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hfs.Server.Core.dll"]