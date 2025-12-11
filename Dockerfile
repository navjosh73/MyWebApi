# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
ENV DOTNET_SYSTEM_NET_DISABLEIPV6=1
RUN dotnet restore --disable-parallel ./mywebapi.csproj
RUN dotnet publish ./mywebapi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "mywebapi.dll"]
