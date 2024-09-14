#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install SkiaSharp native dependencies for Linux
RUN apt-get update && \
    apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libxext6 \
    libxrender1

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WorkflowConfigurator.csproj", "."]
RUN dotnet restore "./WorkflowConfigurator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WorkflowConfigurator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkflowConfigurator.csproj" -c Release -o /app/publish

FROM base AS final


WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowConfigurator.dll"]
