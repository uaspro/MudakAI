#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# Install FFmpeg and audio codecs
RUN apt-get update && \
    apt-get install -y ffmpeg libopus-dev libsodium-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MudakAI.Voice.Functions/MudakAI.Voice.Functions.csproj", "MudakAI.Voice.Functions/"]
COPY ["MudakAI.Connectors.Azure.Blob/MudakAI.Connectors.Azure.Blob.csproj", "MudakAI.Connectors.Azure.Blob/"]
COPY ["MudakAI.Connectors.Discord/MudakAI.Connectors.Discord.csproj", "MudakAI.Connectors.Discord/"]
RUN dotnet restore "MudakAI.Voice.Functions/MudakAI.Voice.Functions.csproj"
COPY . .
WORKDIR "/src/MudakAI.Voice.Functions"
RUN dotnet build "MudakAI.Voice.Functions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MudakAI.Voice.Functions.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true