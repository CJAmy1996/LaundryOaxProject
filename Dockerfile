FROM node:22-bookworm-slim AS web
WORKDIR /src/client
COPY client/package*.json ./
RUN npm ci --no-fund
COPY client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LaundryOaxWebAPI.csproj ./
RUN dotnet restore LaundryOaxWebAPI.csproj
COPY . ./
COPY --from=web /src/wwwroot ./wwwroot
RUN dotnet publish LaundryOaxWebAPI.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LaundryOaxWebAPI.dll"]
