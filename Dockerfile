# Rabbithole ships as ONE image: ASP.NET Core serves the API and the built React SPA from wwwroot.
# Keep this in step with Rabbithole.AppHost (PublishWithContainerFiles → ./wwwroot).

# 1. Build the SPA
FROM node:26-alpine AS web
WORKDIR /src/rabbithole.web
COPY rabbithole.web/package.json rabbithole.web/package-lock.json ./
RUN npm ci
COPY rabbithole.web/ ./
RUN npm run build

# 2. Publish the API
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY .editorconfig ./
COPY Rabbithole.Core/Rabbithole.Core.csproj Rabbithole.Core/
COPY Rabbithole.Domain/Rabbithole.Domain.csproj Rabbithole.Domain/
RUN dotnet restore Rabbithole.Core/Rabbithole.Core.csproj
COPY Rabbithole.Core/ Rabbithole.Core/
COPY Rabbithole.Domain/ Rabbithole.Domain/
RUN dotnet publish Rabbithole.Core/Rabbithole.Core.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# 3. Runtime: API + SPA in one container
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web /src/rabbithole.web/dist ./wwwroot
RUN mkdir -p /app/Logs && chown -R $APP_UID /app/Logs
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
VOLUME /app/Logs
ENTRYPOINT ["dotnet", "Rabbithole.Core.dll"]
