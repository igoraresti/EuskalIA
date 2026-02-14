# Euskal IA - Aprendizaje de Euskera con IA

Euskal IA es una plataforma moderna de aprendizaje del idioma euskera, inspirada en Duolingo pero potenciada por inteligencia artificial para la generación adaptativa de ejercicios y tutoría conversacional.

## Arquitectura

- **Frontend**: Aplicación móvil multiplataforma desarrollada con **React Native (Expo)** y TypeScript.
- **Backend**: API REST desarrollada en **.NET 9** con Entity Framework Core.
- **Base de Datos**: SQLite (alojada localmente para facilitar el despliegue).
- **IA**: Motor de generación de ejercicios integrado en el backend.

## Estructura del Proyecto

- `/app`: Código fuente de la aplicación móvil (Expo).
- `/server`: Código fuente del servidor .NET API.
- `start.bat`: Script de inicio unificado para Windows.
- `start.sh`: Script de inicio unificado para Mac/Linux.

## Requisitos Previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js (v20+)](https://nodejs.org/)
- [Docker](https://www.docker.com/) (opcional, para despliegue en contenedores)

## Instalación y Ejecución Local

### 1. Servidor (.NET)

Desde la raíz del proyecto:
```bash
cd server/EuskalIA.Server
dotnet run
```
El servidor estará disponible en `http://localhost:5235`. La base de datos se creará automáticamente como `euskalia.db`.

### 2. Aplicación Móvil (Expo)

Desde la raíz del proyecto:
```bash
cd app
npm install
npm run web  # Para probar en el navegador
# O
npx expo start # Para probar en iOS/Android con Expo Go
```

## Tests

### Backend (.NET)
Para ejecutar los tests del servidor:
```bash
cd server/EuskalIA.Tests
dotnet test
```

### Frontend (React Native)
Para ejecutar los tests unitarios de la aplicación:
```bash
cd app
npm test
```

### End-to-End (Playwright)
Para ejecutar los tests de integración visual:
```bash
cd app
npx playwright test
```
Para ver la interfaz gráfica de los tests:
```bash
npx playwright test --ui
```

## Despliegue con Docker

Para desplegar el servidor fácilmente en cualquier entorno (incluyendo Windows):

1. Construir la imagen:
   ```bash
   docker build -t euskalia-server -f server/EuskalIA.Server/Dockerfile .
   ```
2. Ejecutar el contenedor:
   ```bash
   docker run -d -p 5235:80 --name euskalia-api euskalia-server
   ```

## Contribución

Este proyecto ha sido desarrollado como una base funcional para una aplicación de aprendizaje de idiomas. El código está estructurado para ser fácilmente extensible con nuevos tipos de ejercicios y modelos de IA reales.

---
Desarrollado para @igoraresti
