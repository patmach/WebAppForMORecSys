# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY WebAppForMORecSys/*.csproj ./WebAppForMORecSys/
RUN dotnet restore

# copy everything else and build app
COPY WebAppForMORecSys/. ./WebAppForMORecSys/
WORKDIR /source/WebAppForMORecSys
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p Logs
COPY Logs/. ./Logs/
RUN apt-get update
RUN apt-get install curl -y
#Sleep to make sure RS started and first request can be sent
ENTRYPOINT sleep 60 && echo 'RS train started by sending first request' && curl -s -o /dev/null http://rs:5000/ \
    && echo 'RS ready to be used' && dotnet WebAppForMORecSys.dll
