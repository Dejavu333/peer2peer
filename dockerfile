FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

env TRACKER_SERVER_IP=""

# Copy everything
COPY ./ ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /App
COPY --from=build-env /App/out ./
EXPOSE 3000
ENTRYPOINT ["dotnet", "./peer2peer.dll"]

# docker build -t test-peer-image .
# docker network create testnetwork
# docker run -it --name peer1cont --network testnetwork test-peer-image
# docker run -it --name peer2cont --network testnetwork test-peer-image
# docker network inspect testnetwork