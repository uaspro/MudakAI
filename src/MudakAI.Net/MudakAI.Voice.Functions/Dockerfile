#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# Install FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install audio codecs
RUN apt-get update && \
    apt-get install -y libopus0=1.3.1-0.1 libsodium23=1.0.18-1 && \
    apt-get clean

RUN ln -s /usr/lib/x86_64-linux-gnu/libopus.so.0.8.0 /usr/lib/x86_64-linux-gnu/libopus.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libopus.so.0.8.0 /usr/lib/x86_64-linux-gnu/opus.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libsodium.so.23.3.0 /usr/lib/x86_64-linux-gnu/libsodium.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libsodium.so.23.3.0 /usr/lib/x86_64-linux-gnu/libsodium

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MudakAI.Voice.Functions/MudakAI.Voice.Functions.csproj", "MudakAI.Voice.Functions/"]
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