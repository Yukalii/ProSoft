# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY EasyLogServer.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2 — Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV LOG_DIR=/var/logs/easysave
ENV PORT=8080
ENTRYPOINT ["dotnet", "EasyLogServer.dll"]