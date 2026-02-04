# Stage 1: Build Angular app
FROM node:25.2-alpine AS angular-build
WORKDIR /app/client

# Copy package files and install dependencies
COPY src/Presenters/Monet.WebApp/ClientApp/package*.json ./
RUN npm ci

# Copy Angular source and build
COPY src/Presenters/Monet.WebApp/ClientApp/ ./
RUN npm run build -- --configuration production

# Stage 2: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /src

# Copy solution and project files
COPY . .

# Copy Angular build output into wwwroot
RUN mkdir -p /src/src/Presenters/Monet.WebApp/wwwroot
COPY --from=angular-build /app/client/dist/ClientApp/browser /src/src/Presenters/Monet.WebApp/wwwroot/

# Build and publish with optimizations
WORKDIR /src/src/Presenters/Monet.WebApp
RUN dotnet publish -c Release -o /app/publish \
    /p:SkipClientAppInstall=true \
    /p:EnableCompressionInSingleFile=true \
    /p:DebugType=None \
    /p:DebugSymbols=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create data directory for event store (must be done before USER)
USER root
RUN mkdir -p /app/data && chown -R app:app /app/data
USER app

# Copy published app
COPY --from=dotnet-build /app/publish .

# Configure production settings
ARG APP_VERSION=unknown
ENV Application__Version=$APP_VERSION
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Monet.WebApp.dll"]
