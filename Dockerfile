# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["ContaFlow.API/ContaFlow.API.csproj", "ContaFlow.API/"]
RUN dotnet restore "ContaFlow.API/ContaFlow.API.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/ContaFlow.API"
RUN dotnet publish "ContaFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Imagen final de producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContaFlow.API.dll"]
