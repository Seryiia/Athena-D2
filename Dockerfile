FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY src/Athena.Medusa/*.csproj src/Athena.Medusa/
COPY src/Athena.Common/*.csproj src/Athena.Common/
COPY src/AthenaBot/*.csproj src/AthenaBot/
COPY src/AthenaBot.Coordinator/*.csproj src/AthenaBot.Coordinator/
COPY src/AthenaBot.Generators/*.csproj src/AthenaBot.Generators/
COPY NuGet.Config ./
RUN dotnet restore src/AthenaBot/

COPY . .
WORKDIR /source/src/AthenaBot
RUN set -xe; \
    dotnet --version; \
    dotnet publish -c Release -o /app --no-restore; \
    mv /app/data /app/data_init; \
    rm -Rf libopus* libsodium* opus.* runtimes/win* runtimes/osx* runtimes/linux-arm* runtimes/linux-mips*; \
    find /app -type f -exec chmod -x {} \; ;\
    chmod +x /app/AthenaBot

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app

RUN set -xe; \
    useradd -m athena; \
    apt-get update; \
    apt-get install -y --no-install-recommends libopus0 libsodium23 libsqlite3-0 curl ffmpeg python3 python3-pip sudo; \
    update-alternatives --install /usr/bin/python python /usr/bin/python3.9 1; \
    echo 'Defaults>athena env_keep+="ASPNETCORE_* DOTNET_* AthenaBot_* shard_id total_shards TZ"' > /etc/sudoers.d/athena; \
    pip3 install --no-cache-dir --upgrade youtube-dl; \
    apt-get purge -y python3-pip; \
    chmod +x /usr/local/bin/youtube-dl; \
    apt-get autoremove -y; \
    apt-get autoclean -y

COPY --from=build /app ./
COPY docker-entrypoint.sh /usr/local/sbin

ENV shard_id=0
ENV total_shards=1
ENV AthenaBot__creds=/app/data/creds.yml

VOLUME [ "/app/data" ]
ENTRYPOINT [ "/usr/local/sbin/docker-entrypoint.sh" ]
CMD dotnet AthenaBot.dll "$shard_id" "$total_shards"
