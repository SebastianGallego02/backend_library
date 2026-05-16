FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /app

# Copiar los archivos de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código
COPY . .

# Exponer el puerto en el que correrá la app (por defecto 8080 en .NET 8)
EXPOSE 8080

# Comando para desarrollo con hot-reload automático
CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:8080"]
