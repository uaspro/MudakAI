#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

# Install FFmpeg, curl, and unzip
RUN apt-get update && \
    apt-get install -y ffmpeg curl unzip && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Upgrade libstdc++6 from Debian Trixie (GLIBCXX_3.4.32 required by libdave)
RUN echo "deb http://deb.debian.org/debian trixie main" > /etc/apt/sources.list.d/trixie.list && \
    apt-get update && \
    apt-get install -y -t trixie libstdc++6 && \
    rm /etc/apt/sources.list.d/trixie.list && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install audio codecs and libdave (pinned to Bullseye versions required by Discord.Net)
RUN curl -fSL -o /tmp/libopus0.deb http://deb.debian.org/debian/pool/main/o/opus/libopus0_1.3.1-0.1_amd64.deb && \
    curl -fSL -o /tmp/libsodium23.deb http://deb.debian.org/debian/pool/main/libs/libsodium/libsodium23_1.0.18-1_amd64.deb && \
    curl -fSL -o /tmp/libdave.zip https://github.com/discord/libdave/releases/download/v1.1.1/cpp/libdave-Linux-X64-boringssl.zip && \
    dpkg -i /tmp/libopus0.deb /tmp/libsodium23.deb && \
    unzip /tmp/libdave.zip -d /tmp/libdave && \
    cp /tmp/libdave/lib/libdave.so /usr/lib/x86_64-linux-gnu/libdave.so && \
    rm /tmp/libopus0.deb /tmp/libsodium23.deb && \
    rm -rf /tmp/libdave /tmp/libdave.zip

RUN ln -s /usr/lib/x86_64-linux-gnu/libopus.so.0.8.0 /usr/lib/x86_64-linux-gnu/libopus.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libopus.so.0.8.0 /usr/lib/x86_64-linux-gnu/opus.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libsodium.so.23.3.0 /usr/lib/x86_64-linux-gnu/libsodium.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libsodium.so.23.3.0 /usr/lib/x86_64-linux-gnu/libsodium && \
    ln -s /usr/lib/x86_64-linux-gnu/libdave.so /usr/lib/x86_64-linux-gnu/dave.so && \
    ln -s /usr/lib/x86_64-linux-gnu/libdave.so /usr/lib/x86_64-linux-gnu/libdave && \
    ldconfig

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