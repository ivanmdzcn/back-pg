# Usa la imagen base de .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos .csproj de todos los proyectos
COPY ["API/API.csproj", "API/"]
COPY ["AccesoDatos/AccesoDatos.csproj", "AccesoDatos/"]
COPY ["Dominio/Dominio.csproj", "Dominio/"]
COPY ["Servicios/Servicios.csproj", "Servicios/"]

# Restaurar paquetes para el proyecto principal
RUN dotnet restore "API/API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar el proyecto API
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# AGREGADO: Variables de entorno por defecto
# ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# AGREGADO: Crear usuario no-root por seguridad
RUN adduser --disabled-password --gecos '' apiuser && chown -R apiuser /app
USER apiuser

ENTRYPOINT ["dotnet", "API.dll"]
