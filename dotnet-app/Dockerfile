# ./dotnet-app/Dockerfile
# Multi-stage: build (SDK) -> runtime (ASP.NET)
# Mantém a imagem final pequena e sem toolchain de build.

# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia apenas os csproj primeiro para otimizar cache de restore
COPY *.csproj ./
RUN dotnet restore

# Copia o restante do código
COPY . ./

# Permite alterar a configuração via arg (Release por padrão)
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Segurança: cria usuário não-root
# (Debian-based image: useradd disponível)
RUN useradd -m appuser
COPY --from=build /app/publish ./
RUN chown -R appuser:appuser /app

# Porta exposta pelo Kestrel no container
EXPOSE 5000

# Variáveis padrão (o docker-compose pode sobrescrever)
ENV ASPNETCORE_URLS=http://+:5000 \
    DOTNET_EnableDiagnostics=0 \
    OTEL_EXPORTER_OTLP_PROTOCOL=grpc

# Executa como não-root
USER appuser

# Inicia a aplicação
# Ajuste o nome do DLL se seu .csproj tiver outro <AssemblyName>
ENTRYPOINT ["dotnet", "poc.dll"]
